using LightManager.Server.Services;
using LightManager.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace LightManager.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SettingsController : ControllerBase
    {
        //private static readonly string[] Summaries = new[]
        //{
        //"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        //};

        private readonly ILogger<TasksController> _logger;
        private readonly ISettingsService _appsettings;

        public SettingsController(ILogger<TasksController> logger, ISettingsService appSettings)
        {
            _logger = logger;
            _appsettings = appSettings;
        }
        //private IOptionsMonitor<AppSettings> _appSettingsMonitor;
        //private IOptionsSnapshot<AppSettings> _appSettingsSnapshot;
        //private IOptions<AppSettings> _settings;
        //public SettingsController(IOptions<AppSettings> settings, IOptionsSnapshot<AppSettings> optionsSnapshot, IOptionsMonitor<AppSettings> optionsMonitor)
        //{
        //    _appSettingsMonitor = optionsMonitor;
        //    _appSettingsSnapshot = optionsSnapshot;
        //    _settings = settings;
        //}

        //public AppSettings GetAppSettings()
        //{
        //    return new AppSettings
        //    {
        //        ImportPath = GetImportPath(),
        //        ExportPath = "/Data/Imported",
        //    };
        //}

        //public string GetImportPath()
        //{
        //    //Get from stored config/json/yml file.
        //    return "/Data/Downloaded";
        //}

        [HttpGet]
        public FolderObject GetFolder(string Path)
        {
            try
            {
                DirectoryInfo currentFolder = new DirectoryInfo(Path);
                //if (parent.Parent is not null and parent.parent)
                //{
                //    parent = Directory.GetParent(Path);
                //}
                //IEnumerable<string> directoryList = Directory.EnumerateDirectories(Path);
                DirectoryInfo[] folders = currentFolder.GetDirectories("*", SearchOption.TopDirectoryOnly);

                //with the parent directory object if the parent path is root then set root otherwise split.
                DirectoryObject parentObject = new DirectoryObject()
                {
                    Name = "..",//currentFolder.Parent is null ? ". ." : currentFolder.Parent.Name,
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


        [HttpGet]
        public async Task<AppSettings> ReadSettings()
        {
            return _appsettings.ReadSettings();
        }

        

        [HttpPost]
        public async Task SaveSettings(AppSettings settings)
        {
            _appsettings.WriteSettings(settings);
            //Ok();
        }

       



    }
}

//{
//    "parent": "/volume1/",
//  "directories": [
//    {
//        "type": "folder",
//      "name": ".stfolder",
//      "path": "/volume1/Media/.stfolder/",
//      "size": 0,
//      "lastModified": "2021-12-07T11:40:24.073174Z"
//    },
//    {
//        "type": "folder",
//      "name": "Anime",
//      "path": "/volume1/Media/Anime/",
//      "size": 0,
//      "lastModified": "2021-11-25T13:36:53.639834Z"
//    },
//    {
//        "type": "folder",
//      "name": "Music",
//      "path": "/volume1/Media/Music/",
//      "size": 0,
//      "lastModified": "2021-12-07T12:04:01.553403Z"
//    },
//    {
//        "type": "folder",
//      "name": "TV Shows",
//      "path": "/volume1/Media/TV Shows/",
//      "size": 0,
//      "lastModified": "2021-12-06T11:20:21.743296Z"
//    }
//  ],
//  "files": []
//}