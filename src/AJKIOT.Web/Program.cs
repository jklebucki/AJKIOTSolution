using AJKIOT.Web;
using AJKIOT.Web.Services;
using AJKIOT.Web.Settings;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddSingleton(o => new ApiSettings(
    builder.Configuration.GetSection("ApiSettings").GetValue<string>("ApiBaseUrl")!, 
    builder.Configuration.GetSection("ApiSettings").GetValue<string>("ApiScheme")!));
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var settings = scope.ServiceProvider.GetRequiredService<ApiSettings>();
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri($"{settings.ApiScheme}://{settings.ApiBaseUrl}/") });
}

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddApiAuthorization();
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();
await builder.Build().RunAsync();
