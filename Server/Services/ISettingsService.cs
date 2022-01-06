using LightManager.Shared.Models;
using System.Text.Json;

namespace LightManager.Server.Services
{
    public interface ISettingsService
    {
        AppSettings ReadSettings();
        
        void WriteSettings(AppSettings appSettings);
    }
}
