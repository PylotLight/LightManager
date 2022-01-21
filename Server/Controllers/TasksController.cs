using BencodeNET.Parsing;
using BencodeNET.Torrents;
using LightManager.Server.Context;
using LightManager.Shared.Models;
using LightManager.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Web;

namespace LightManager.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TasksController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TasksController> _logger;
        private readonly ISettingsService _appSettings;
        private readonly ITasksService _taskService;
        private readonly TaskDBContext _context;

        public TasksController(ILogger<TasksController> logger, ISettingsService appSettings, TaskDBContext context, ITasksService tasksService)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _appSettings = appSettings;
            _context = context;
            _taskService = tasksService;
        }


        [HttpGet]
        public async Task<List<TaskItem>> GetTasks()
        {
            return await _taskService.GetTasks();
        }

        [HttpPost]
        public async Task AddTorrents(List<TaskItem> tasks)
        {
            await _taskService.AddTorrents(tasks);
        }

        [HttpPost]
        public async Task DeleteFiles(List<TaskItem> tasks)
        {
            await _taskService.DeleteFiles(tasks);
        }
    }
}