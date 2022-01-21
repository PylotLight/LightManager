using LightManager.Shared.Models;
using LightManager.Shared.Services;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace LightManager.Client.Services
{
	public class TasksService : ITasksService
    {
        private readonly HttpClient _httpClient;

        public TasksService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TaskItem>> GetTasks()
        {
            return await _httpClient.GetFromJsonAsync<List<TaskItem>>($"api/Tasks/GetTasks");
        }

        public async Task AddTorrents(List<TaskItem> tasks)
        {
            var downloadTasks = tasks.Where(x => x.Downloaded == true).ToList();
            await _httpClient.PostAsJsonAsync("api/Tasks/AddTorrents", downloadTasks);
        }

        public async Task DeleteFiles(List<TaskItem> tasks)
        {
            var deletetasks = tasks.Where(x => x.Downloaded == true).ToList();
            await _httpClient.PostAsJsonAsync("api/Tasks/DeleteFiles", deletetasks);
            
        }
    }
}
