using AJKIOT.Api.Controllers;
using AJKIOT.Api.Data;
using AJKIOT.Api.Hubs;
using AJKIOT.Api.Middleware;
using AJKIOT.Api.Models;
using AJKIOT.Api.Repositories;
using AJKIOT.Api.Services;
using AJKIOT.Api.Utils;
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
    var certificate = new X509Certificate2(Path.Combine(Environment.CurrentDirectory, "MqttCert", "ajksoftware.pl.pfx"), "AjkCertPass!5500");
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
              .WithEncryptedEndpointBoundIPAddress(System.Net.IPAddress.Any)
              .WithEncryptedEndpointPort(8883)
              .WithoutDefaultEndpoint();
    //mqttServer.WithoutDefaultEndpoint();
})
//.AddMqttWebSocketServerAdapter()
.AddConnections();
builder.Services.AddSingleton<MqttController>();

// Endpoint filters
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});


builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(new IPAddress([172, 16, 90, 151]), 8883, listenOptions =>
    {

        listenOptions.UseHttps(httpsOptions =>
        {
            // Ładowanie certyfikatu serwera
            var serverCert = PemKeyUtils.LoadCertificate("MqttCert/server.crt", "MqttCert/server.key");
            httpsOptions.ServerCertificate = serverCert;
            httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;

            // Ładowanie certyfikatu Root CA
            var rootCaCertificate = new X509Certificate2("MqttCert/ca.crt");

            // Weryfikacja certyfikatu klienta
            httpsOptions.ClientCertificateValidation = (clientCert, chain, sslPolicyErrors) =>
            {
                if (clientCert == null)
                {
                    return false; // Certyfikat klienta jest null
                }

                // Dodanie Root CA do polityki łańcucha
                chain.ChainPolicy.ExtraStore.Add(rootCaCertificate);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // Dostosuj w razie potrzeby
                chain.ChainPolicy.VerificationTime = DateTime.Now;

                // Sprawdzenie, czy łańcuch certyfikatów jest prawidłowy
                bool isValid = chain.Build(clientCert);

                // Upewnienie się, że Root CA jest zaufany
                if (isValid && chain.ChainElements[^1].Certificate.Thumbprint == rootCaCertificate.Thumbprint)
                {
                    return true; // Certyfikat jest ważny i zaufany
                }

                return false; // Certyfikat nie jest ważny lub zaufany
            };
        });

        listenOptions.UseMqtt();
    });
    options.Listen(new IPAddress([172, 16, 90, 151]), 1883, l => l.UseMqtt()); // MQTT over TCP
    //options.ListenAnyIP(5000); // HTTP for WebSocket connection
});
//builder.WebHost.ConfigureKestrel((context, options) =>
//{
//    var endpointConfiguration = context.Configuration.GetSection("Kestrel:Endpoints");

//    foreach (var endpoint in endpointConfiguration.GetChildren())
//    {
//        var address = endpoint["Address"];
//        var port = endpoint.GetValue<int>("Port");
//        options.ListenAnyIP(port, listenOptions =>
//        {
//            if (endpoint.Key == "MqttServer")
//            {
//                listenOptions.UseMqtt(); // Configure for MQTT
//            }
//            else if (endpoint.Key == "HttpsServer")
//            {
//                listenOptions.UseHttps(); // Configure for HTTPS
//            }
//        });
//    }
//});

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
