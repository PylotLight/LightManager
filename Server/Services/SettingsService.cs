using LightManager.Shared.Models;
using System.Text.Json;

namespace LightManager.Server.Services
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
            WriteSettingsInternal(appSettings);
        }

        private AppSettings ReadSettingsInternal()
        {
            try
            {
                //TODO: Test for permission for config file storage location to new volume.

                string configfile = "userConfig.json";

                if (!System.IO.File.Exists(configfile))
                {
                    return new AppSettings() { ExportPath = "/", ImportPath = "/" };
                }

                using var fs = new FileStream("userConfig.json", FileMode.Open);
                AppSettings storedSettings = JsonSerializer.Deserialize<AppSettings>(fs);

                return storedSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
                //var newsettings = new AppSettings() { ExportPath="/", ImportPath="/" };
                //WriteSettingsInternal(newsettings);
                return null;
            }
        }

        private void WriteSettingsInternal(AppSettings settings)
        {
            using var fs = new FileStream("userConfig.json", FileMode.OpenOrCreate);
            JsonSerializer.Serialize(fs, settings);
        }

    }
}
