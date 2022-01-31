using LightManager.Server.Context;
using LightManager.Server.Services;
using LightManager.Shared.Models;
using LightManager.Shared.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddScoped<AuthenticationStateProvider,
    ServerAuthenticationStateProvider>();
builder.Services.AddScoped<SignOutSessionStateManager>();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
//var connectionstring = builder.Configuration.GetConnectionString("default");
using (var context = new TaskDBContext(new DbContextOptions<TaskDBContext>()))
{
    //The line below clears and resets the databse.
    //context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    context.Database.MigrateAsync();
}
builder.Services.AddDbContext<TaskDBContext>(x => x.UseSqlite());
builder.Services.AddLogging();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<ITasksService, TasksService>();

builder.Services.AddHttpClient<TasksService>();

string CorsOrigins = "CorsOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsOrigins,
        builder => builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseCors(CorsOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
});


app.MapRazorPages();
app.MapControllers();
//app.MapFallbackToFile("index.html");
app.MapFallbackToPage("/_Host");

app.Run();
