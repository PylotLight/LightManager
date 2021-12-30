using LightManager.Shared.Models;
using System.Text.Json;

namespace LightManager.Server.Services
{
    public interface IAppSettingsService
    {
        //string ImportPath { get; }

        AppSettings ReadSettings();
        
        void WriteSettings(AppSettings appSettings);
    }
}
