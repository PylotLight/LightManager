using LightManager.Shared.Models;
using System.Text.Json;

namespace LightManager.Client.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ILogger<SettingsService> _logger;
        public SettingsService(ILogger<SettingsService> logger)
        {
            _logger = logger;
        }

        //public string ImportPath => (await ReadSettingsInternal()).ImportPath;
        AppSettings ISettingsService.ReadSettings()
        {
            return ReadSettingsInternal();
        }

        void ISettingsService.WriteSettings(AppSettings appSettings)
        {
            //WriteSettingsInternal(appSettings);
        }

        private AppSettings ReadSettingsInternal()
        {
            return new AppSettings();
        }

        private void WriteSettingsInternal(AppSettings settings)
        {
           
        }

    }
}
