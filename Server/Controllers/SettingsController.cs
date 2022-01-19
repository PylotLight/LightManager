using LightManager.Shared.Models;
using LightManager.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace LightManager.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SettingsController : ControllerBase
    {
        private readonly ILogger<TasksController> _logger;
        private readonly ISettingsService _appsettings;

        public SettingsController(ILogger<TasksController> logger, ISettingsService appSettings)
        {
            _logger = logger;
            _appsettings = appSettings;
        }

        //GET
        [HttpGet]
        public async Task<FolderObject> GetFolder(string Path)
        {
           return await _appsettings.ReadFolderObjectAsync(Path);
        }

        [HttpGet]
        public async Task<AppSettings> ReadSettings()
        {
            return await _appsettings.ReadSettingsAsync();
        }

        [HttpGet]
        public async Task<byte[]> GetDBExport()
        {
            return await _appsettings.ExportDBAsync();
        }

        //POST
        [HttpPost]
        public async Task SaveSettings(AppSettings settings)
        {
            await _appsettings.WriteSettingsAsync(settings);
            //Ok();
        }
    }
}