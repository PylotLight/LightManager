using LightManager.Shared.Models;
using LightManager.Shared.Services;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace LightManager.Client.Services
{
	public class SettingsService : ISettingsService
    {
        private readonly ILogger<SettingsService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _iJSRuntime;
        public SettingsService(ILogger<SettingsService> logger, HttpClient httpClient, IJSRuntime jSRuntime)
        {
            _logger = logger;
            _httpClient = httpClient;
            _iJSRuntime = jSRuntime;
        }

        //public string ImportPath => (await ReadSettingsInternal()).ImportPath;

        public async Task<AppSettings> ReadSettingsAsync()
        {
            return await _httpClient.GetFromJsonAsync<AppSettings>("api/Settings/ReadSettings");
        }

        public async Task WriteSettingsAsync(AppSettings appSettings)
        {
            await _httpClient.PostAsJsonAsync("api/Settings/SaveSettings", appSettings);
        }

        public async Task<FolderObject> ReadFolderObjectAsync(string folderPath)
        {
            return await _httpClient.GetFromJsonAsync<FolderObject>($"api/Settings/GetFolder?Path={folderPath}");
        }

		public async Task<byte[]> ExportDBAsync()
		{
            byte[] DBExport = await _httpClient.GetByteArrayAsync("api/Settings/GetDBExport");
            await _iJSRuntime.InvokeVoidAsync("BlazorDownloadFile", DateTime.Now.ToShortDateString() + "-RDexport.bin", "application/octet-stream", DBExport);
            return DBExport;
        }

		//async Task DownloadText()
		//{
		//    // Generate a text file
		//    byte[] file = System.Text.Encoding.UTF8.GetBytes("Hello world!");
		//    await JSRuntime.InvokeVoidAsync("BlazorDownloadFile", "file.txt", "text/plain", file);
		//}
	}
}
