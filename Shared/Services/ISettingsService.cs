using LightManager.Shared.Models;

namespace LightManager.Shared.Services
{
    public interface ISettingsService
    {
        Task<AppSettings> ReadSettingsAsync();
        Task <FolderObject> ReadFolderObjectAsync(string folderPath);
        
        Task WriteSettingsAsync(AppSettings appSettings);

        Task<byte[]> ExportDBAsync();
    }
}
