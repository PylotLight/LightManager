using LightManager.Server.Context;
using LightManager.Server.Services;
using LightManager.Shared.Models;
using LightManager.Shared.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


//builder.Services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
var connectionstring = builder.Configuration.GetConnectionString("default");
using (var context = new TaskDBContext(new DbContextOptions<TaskDBContext>()))
{

    //The line below clears and resets the databse.
    //context.Database.EnsureDeleted();

    // Create the database if it does not exist
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
app.MapRazorPages();
app.MapControllers();
//app.MapFallbackToFile("index.html");
app.MapFallbackToPage("/_Host");

app.Run();
