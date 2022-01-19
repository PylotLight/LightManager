using LightManager.Server.Context;
using LightManager.Shared.Models;
using LightManager.Shared.Services;
using System.Text;
using System.Text.Json;

namespace LightManager.Server.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ILogger<SettingsService> _logger;
        private readonly TaskDBContext _context;

        public SettingsService(ILogger<SettingsService> logger, TaskDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        //public string ImportPath => (await ReadSettingsInternal()).ImportPath;

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

        public async Task<AppSettings> ReadSettingsAsync()
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
                AppSettings storedSettings = await JsonSerializer.DeserializeAsync<AppSettings>(fs);

                return storedSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
                return null;
            }
        }

        public async Task<FolderObject> ReadFolderObjectAsync(string folderPath)
        {
            try
            {
                DirectoryInfo currentFolder = new DirectoryInfo(folderPath);
                DirectoryInfo[] folders = currentFolder.GetDirectories("*", SearchOption.TopDirectoryOnly);

                //with the parent directory object if the parent path is root then set root otherwise split.
                DirectoryObject parentObject = new DirectoryObject()
                {
                    Name = "..",
                    Path = currentFolder.Parent is null ? System.IO.Path.Combine(currentFolder.FullName) : System.IO.Path.Combine(currentFolder.Parent.FullName)
                };
                return new FolderObject()
                {
                    ParentPath = parentObject.Path,
                    CurrentPath = currentFolder.FullName,
                    DirectoryObjects = folders.Select(f => new DirectoryObject()
                    {
                        Name = f.Name,
                        Path = f.FullName
                    }).Prepend(parentObject)
                };
                //return t;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return new FolderObject()
            {
                ParentPath = "Failed to get folder info"
            };
        }

        public async Task WriteSettingsAsync(AppSettings appSettings)
        {
            using var fs = new FileStream("userConfig.json", FileMode.OpenOrCreate);
            await JsonSerializer.SerializeAsync(fs, appSettings);
        }

		public async Task<byte[]> ExportDBAsync()
		{
            var DBData = _context.Task.ToList();
            var ByteExport = JsonSerializer.SerializeToUtf8Bytes(DBData);
            //var str = Encoding.Default.GetString(ByteExport);

            //var tt = JsonSerializer.Deserialize<List<TaskItem>>(str);
            return ByteExport;
        }
	}
}
