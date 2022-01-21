using LightManager.Shared.Models;

namespace LightManager.Shared.Services
{
    public interface ITasksService
    {
        Task<List<TaskItem>> GetTasks();
        Task AddTorrents(List<TaskItem> taskItems);
        Task DeleteFiles(List<TaskItem> tasks);
    }
}
