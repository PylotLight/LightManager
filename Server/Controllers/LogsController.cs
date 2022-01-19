using LightManager.Server.Context;
using LightManager.Shared.Models;
using LightManager.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace LightManager.Server.Controllers
{
	[ApiController]
    [Route("api/[controller]/[action]")]
    public class LogsController : ControllerBase
    {

        private readonly ILogger<TasksController> _logger;
        private readonly ISettingsService _appSettings;
        private readonly TaskDBContext _context;

        public LogsController(ILogger<TasksController> logger, ISettingsService appSettings, TaskDBContext context)
        {
            _logger = logger;
            _appSettings = appSettings;
            _context = context;
        }


        [HttpGet]
        public async Task<List<TaskItem>> GetLogs()
        {
            return await GetLogsInternal();
        }

        private async Task<List<TaskItem>> GetLogsInternal()
        {
            var logs = _context.Task.ToList();

            return logs;
        }
    }
}