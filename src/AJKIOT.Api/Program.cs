using AJKIOT.Api.Controllers;
using AJKIOT.Api.Data;
using AJKIOT.Api.Hubs;
using AJKIOT.Api.Middleware;
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
builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseNpgsql(connectionString));

// Services
builder.Services.AddSingleton<IWebSocketManager, MyWebSocketManager>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IIotDeviceRepository, IotDeviceRepository>();
builder.Services.AddScoped<IIotDeviceService, IotDeviceService>();
builder.Services.AddSingleton<ITemplateService, TemplateService>();
builder.Services.AddSingleton<IMessageBus, MessageBus>();
builder.Services.AddSingleton<ConnectionMapping>();
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

// CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// SignalR
builder.Services.AddSignalR();
// MQTT 
builder.Services.AddMqttServer(mqttServer =>
{
    var certificate = new X509Certificate2(Path.Combine(Environment.CurrentDirectory, "MqttCert", "server-cert.pfx"), "AjkCertPass!3300");
    mqttServer.WithEncryptionCertificate(certificate)
              .WithClientCertificate((object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) =>
              {
                  if (cert == null)
                  {
                      Console.WriteLine("Client certificate not found.");
                      return false;
                  }
                  if (sslPolicyErrors != SslPolicyErrors.None)
                  {
                      Console.WriteLine($"SSL policy errors: {sslPolicyErrors}");
                      return false;
                  }
                  Console.WriteLine($"Client certificate: {cert.Subject}");
                  return cert.GetPublicKey().SequenceEqual(certificate.GetPublicKey());
              })
              .WithEncryptedEndpointBoundIPAddress(IPAddress.Any)
              .WithEncryptedEndpointPort(8883)
              .WithoutDefaultEndpoint();
})
.AddConnections();
builder.Services.AddSingleton<MqttController>();

// Endpoint filters
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});


builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Parse("172.16.90.151"), 8883, listenOptions =>
    {
        // Load the CA certificate from PEM file
        var caCertificate = new X509Certificate2(
            Path.Combine(Directory.GetCurrentDirectory(), "MqttCert", "ca-cert.pem")
        );

        // Load the server certificate from the generated PFX file
        var serverCertificate = new X509Certificate2(
            Path.Combine(Directory.GetCurrentDirectory(), "MqttCert", "server-cert.pfx"),
            "AjkCertPass!3300"
        );

        listenOptions.UseHttps(httpsOptions =>
        {
            httpsOptions.ServerCertificate = serverCertificate;
            httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
            httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
            {
                Console.WriteLine("---------------------------------------------------");
                try
                {
                    // Build the client certificate chain
                    chain.ChainPolicy.ExtraStore.Add(caCertificate);
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                    chain.ChainPolicy.VerificationFlags |= X509VerificationFlags.IgnoreRootRevocationUnknown;
                    chain.ChainPolicy.VerificationFlags |= X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown;

                    Console.WriteLine($"Client certificate: {cert.Subject}");
                    var valid = chain.Build(cert);
                    Console.WriteLine($"Certificate chain valid: {valid}");

                    // Log client chain elements
                    //foreach (var element in chain.ChainElements)
                    //{
                    //    Console.WriteLine($"Client Certificate Chain Element: {element.Certificate.Subject}");
                    //}

                    // Extract the root CA certificate from the client certificate chain
                    X509Certificate2 clientRootCa = null;
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

                    // Log the details of the client root CA
                    //Console.WriteLine($"Client Root CA Details:");
                    //Console.WriteLine($"Subject: {clientRootCa.Subject}");
                    //Console.WriteLine($"Issuer: {clientRootCa.Issuer}");
                    //Console.WriteLine($"Thumbprint: {clientRootCa.Thumbprint}");

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
        });

        listenOptions.UseMqtt();
    });
});


var app = builder.Build();

// Middleware 
//app.UseHttpsRedirection();
app.UseCors("AllowAll");
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
app.Run();
