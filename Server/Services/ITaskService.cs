using LightManager.Shared.Models;
using System.Text.Json;

namespace LightManager.Server.Services
{
    public interface ITaskService
    {
        List<TaskItem> GetTasks(string importPath);
        
        //void WriteSettings(AppSettings appSettings);
    }
}
