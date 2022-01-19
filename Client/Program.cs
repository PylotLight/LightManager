using LightManager.Client.Services;
using LightManager.Shared.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

//var serverHost = string.IsNullOrEmpty(builder.Configuration["SERVER_HOST"]) ?
//    builder.HostEnvironment.BaseAddress :
//    builder.Configuration["SERVER_HOST"];

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(serverHost) });


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ISettingsService, SettingsService>();

await builder.Build().RunAsync();
