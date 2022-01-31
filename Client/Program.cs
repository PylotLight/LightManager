using LightManager.Client;
using LightManager.Client.Services;
using LightManager.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<ITasksService, TasksService>();

builder.Services.AddHttpClient("LightManager.ServerAPI", client =>
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("LightManager.ServerAPI"));
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    //options.ProviderOptions.DefaultAccessTokenScopes.Add("api://96e84d1d-bb18-431d-ae50-5133793f4abd/API.Read");
    //options.ProviderOptions.LoginMode = "redirect";
});



await builder.Build().RunAsync(); 