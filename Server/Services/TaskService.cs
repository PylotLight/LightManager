using LightManager.Shared.Models;
using System.Text.Json;

namespace LightManager.Server.Services
{
    public class TaskService : ITaskService
    {
        public List<TaskItem> GetTasks(string importpath)
        {
            //var settingsController = new SettingsController();
            //var currentsettings = await _appSettings.ReadSettings();
            List<TaskItem> tasks = new List<TaskItem>();
            var files = new DirectoryInfo(importpath).GetFiles();
            //var files2 = files (currentsettings.ImportPath);
            foreach (var file in files)
            {
                tasks.Add(new TaskItem()
                {
                    Filename = file.Name,
                    LastModified = file.LastWriteTime,
                    //MagnetLink = await GetMagnet(file)
                });

            }
            return tasks;
        }
    }
}
