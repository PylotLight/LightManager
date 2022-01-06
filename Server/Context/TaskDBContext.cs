using LightManager.Shared.Models;
using Microsoft.EntityFrameworkCore;


namespace LightManager.Server.Context
{
    public class TaskDBContext : DbContext
    {
        public TaskDBContext(DbContextOptions<TaskDBContext> contextOptions) : base (contextOptions)
        {

        }

        public DbSet<TaskItem> Task { get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();


            var connectionstring = configuration.GetConnectionString("default");
            optionsBuilder.UseSqlite(connectionstring);
        }

       

    }
}
