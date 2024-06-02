using AJKIOT.Api.Controllers;
using AJKIOT.Api.Data;
using AJKIOT.Api.Hubs;
using AJKIOT.Api.Models;
using AJKIOT.Api.Repositories;
using AJKIOT.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MQTTnet.AspNetCore;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory"));
});
// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IIotDeviceRepository, IotDeviceRepository>();
builder.Services.AddScoped<IIotDeviceService, IotDeviceService>();
builder.Services.AddSingleton<ITemplateService, TemplateService>();
builder.Services.AddSingleton<IMessageBus, MessageBus>();
builder.Services.AddSingleton<ConnectionMapping>();
builder.Services.AddSingleton<DeviceIdStore>();
// JSON
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT
var jwtSettings = builder.Configuration.GetSection("JwtTokenSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ClockSkew = TimeSpan.Zero,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.GetValue<string>("ValidIssuer"),
        ValidAudience = jwtSettings.GetValue<string>("ValidAudience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("SymmetricSecurityKey")!))
    };
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "AJKIOT API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Email
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<IEmailSender, EmailSenderService>(serviceProvider =>
{
    var smtpSettings = serviceProvider.GetRequiredService<IOptions<SmtpSettings>>().Value;
    var logger = serviceProvider.GetRequiredService<ILogger<EmailSenderService>>();
    var templateService = serviceProvider.GetRequiredService<ITemplateService>();
    return new EmailSenderService(smtpSettings, logger, templateService);
});

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.SupportedProtocols = new[] { "json" };
});
// MQTT 
builder.Services.AddMqttServer(mqttServer =>
{
    mqttServer.WithEncryptedEndpointBoundIPAddress(IPAddress.Any)
              .WithEncryptedEndpointPort(8883)
              .WithoutDefaultEndpoint();
}).AddConnections();

builder.Services.AddSingleton<IDeviceData, DeviceData>();
builder.Services.AddSingleton<MqttController>();

// Endpoint filters
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});


var securedMqttEndpoint = builder.Configuration.GetSection("SecureMqtt").GetValue<bool>("IsSecure")!;


// Load the server certificate from the generated PFX file
var serverCertificate = new X509Certificate2(
    Path.Combine(Directory.GetCurrentDirectory(), "MqttCert", "server-cert.pfx"),
    "AjkCertPass!3300"
);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 8883, listenOptions =>
    {


        listenOptions.UseHttps(httpsOptions =>
        {
            httpsOptions.ServerCertificate = serverCertificate;
            if (securedMqttEndpoint)
            {
                // Load the CA certificate from PEM file
                var caCertificate = new X509Certificate2(
                    Path.Combine(Directory.GetCurrentDirectory(), "MqttCert", "ca-cert.pem")
                );
                httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
                {
                    Console.WriteLine("---------------------------------------------------");
                    try
                    {
                        // Build the client certificate chain
                        chain!.ChainPolicy.ExtraStore.Add(caCertificate);
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                        chain.ChainPolicy.VerificationFlags |= X509VerificationFlags.IgnoreRootRevocationUnknown;
                        chain.ChainPolicy.VerificationFlags |= X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown;

                        Console.WriteLine($"Client certificate: {cert.Subject}");
                        var valid = chain.Build(cert);
                        Console.WriteLine($"Certificate chain valid: {valid}");

                        // Extract the root CA certificate from the client certificate chain
                        X509Certificate2 clientRootCa = null!;
                        foreach (var element in chain.ChainElements)
                        {
                            if (element.Certificate.Subject == element.Certificate.Issuer)
                            {
                                clientRootCa = element.Certificate;
                                //Console.WriteLine($"Client Root CA Found: {clientRootCa.Subject}");
                                break;
                            }
                        }

                        if (clientRootCa == null)
                        {
                            Console.WriteLine("Unable to extract root CA certificate from client certificate chain.");
                            return false;
                        }

                        // Compare the root CA certificates of both client and server
                        bool caMatch = clientRootCa.Thumbprint == caCertificate.Thumbprint;
                        Console.WriteLine($"CA Certificate Thumbprint: {caCertificate.Thumbprint}");
                        Console.WriteLine($"Client Root CA Thumbprint: {clientRootCa.Thumbprint}");
                        Console.WriteLine($"Certificates have the same root CA: {caMatch}");

                        // Return the result of the chain validation and root CA matching
                        return valid && caMatch;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception during certificate validation: {ex}");
                        return false;
                    }
                };
            }

        });

        listenOptions.UseMqtt();
    });
    options.Listen(IPAddress.Any, 7253, listenOptions =>
    {
        listenOptions.UseHttps(httpsOptions =>
        {
            httpsOptions.ServerCertificate = serverCertificate;
        });
    });
    options.Listen(IPAddress.Any, 5217);
});


var app = builder.Build();
// Migrations
using (var serviceScope = app.Services.CreateScope())
{
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Checking for pending migrations...");
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation("Applying pending migrations...");
            dbContext.Database.Migrate();
            logger.LogInformation("Migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations to apply.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying migrations.");
    }
}

// Middleware 
// app.UseHttpsRedirection();
app.UseCors(builder =>
{
    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();

});
app.UseWebSockets();
app.UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHub<NotificationHub>("/notificationHub");
        endpoints.MapMqtt("/mqtt");
    });
app.UseSwagger();
app.UseSwaggerUI();
app.UseMqttServer(server =>
{
    var mqttController = app.Services.GetRequiredService<MqttController>();
    server.ValidatingConnectionAsync += mqttController.ValidateConnection;
    server.ClientConnectedAsync += mqttController.OnClientConnected;
    server.InterceptingPublishAsync += mqttController.OnInterceptingPublish;
    server.ClientDisconnectedAsync += mqttController.OnClientDisconnected;
});

async Task InitializeDeviceIdStore(IServiceProvider services)
{
    var deviceIdStore = services.GetRequiredService<DeviceIdStore>();
    var deviceService = services.GetRequiredService<IIotDeviceService>();

    var deviceIds = deviceService.GetAllowedDevicesAsync();

    await deviceIdStore.LoadDeviceIdsAsync(deviceIds);
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await InitializeDeviceIdStore(services);
}

app.Run();
