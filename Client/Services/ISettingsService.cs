using LightManager.Shared.Models;
using System.Text.Json;

namespace LightManager.Client.Services
{
    public interface ISettingsService
    {

        //AppSettings? settings ();
        //private FolderObject? folderObject = new FolderObject();

        AppSettings ReadSettings();
        
        void WriteSettings(AppSettings appSettings);
    }
}
