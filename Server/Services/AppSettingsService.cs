using LightManager.Shared.Models;
using System.Text.Json;

namespace LightManager.Server.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        //public string ImportPath => (await ReadSettingsInternal()).ImportPath;
        AppSettings IAppSettingsService.ReadSettings()
        {
            return ReadSettingsInternal();
        }

        void IAppSettingsService.WriteSettings(AppSettings appSettings)
        {
            WriteSettingsInternal(appSettings);
        }

        private AppSettings ReadSettingsInternal()
        {
            try
            {
                using var fs = new FileStream("userConfig.json", FileMode.OpenOrCreate) ?? throw new ArgumentException("Could not open file.. I think");
                AppSettings storedSettings = JsonSerializer.Deserialize<AppSettings>(fs);

                return storedSettings;
            }
            catch (JsonException)
            {
                var newsettings = new AppSettings();
                WriteSettingsInternal(newsettings);
                return newsettings;
            }
        }

        private void WriteSettingsInternal(AppSettings settings)
        {
            using var fs = new FileStream("userConfig.json", FileMode.Open);
            JsonSerializer.Serialize(fs, settings);
        }

    }
}
