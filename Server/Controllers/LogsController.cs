using BencodeNET.Parsing;
using BencodeNET.Torrents;
using LightManager.Server.Context;
using LightManager.Server.Services;
using LightManager.Shared;
using LightManager.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;

namespace LightManager.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LogsController : ControllerBase
    {
        //private static readonly string[] Summaries = new[]
        //{
        //"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        //};        

        //public TasksController(ILogger<TasksController> logger)
        //{
        //    _logger = logger;
        //}


        private readonly HttpClient _httpClient;
        private readonly ILogger<TasksController> _logger;
        private readonly ISettingsService _appSettings;
        private readonly TaskDBContext _context;

        public LogsController(ILogger<TasksController> logger, ISettingsService appSettings, TaskDBContext context)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _appSettings = appSettings;
            _context = context;
        }


        [HttpGet]
        public async Task<List<TaskItem>> GetLogs()
        {
            return await GetLogsInternal();
        }

        private async Task<List<TaskItem>> GetLogsInternal()
        {
            //var settingsController = new SettingsController();
            //var currentsettings = await _appSettings.ReadSettings();
            var logs = _context.Task.ToList();

            //foreach (var file in logs)
            //{
            //    tasks.Add(new TaskItem()
            //    {
            //        Filename = file.Name,
            //        LastModified = file.LastWriteTime,
            //        MagnetLink = await GetMagnet(file)
            //    });

            //}
            return logs;
        }

        private async Task<RDTorrentInfo> SelectFilesInternal(List<RDTorrentInfo> torrentInfo)
        {
            //var settingsController = new SettingsController();
            //var currentsettings = await settingsController.ReadSettings();
            //List<TaskItem> tasks = new List<TaskItem>();
            //var files = new DirectoryInfo(currentsettings.ImportPath).GetFiles();
            //var files2 = files (currentsettings.ImportPath);
            foreach (var file in torrentInfo)
            {
                var t = file.files.Where(x => x.selected == 1).ToList();
                //{
                //    //Go download torrent
                //    continue;
                //}

                //return ;
                //if(file.path.Contains(".mkv"))
            }

            return new RDTorrentInfo();
        }

        //Helper Function
        private async Task<string> GetMagnet(FileInfo file)
        {
            //var fileData = await new StreamReader(filePath).ReadToEndAsync();
            var parser = new BencodeParser();
            //var stream = new StreamReader(file.FullName).BaseStream;
            string magnetLink;
            //var torrent = new Torrent();
            //var t = new BencodeNET.IO.BencodeReader(fileData.BaseStream);
            //torrent.GetMagnetLink()
            try
            {
                if (file.Extension.ToLower() == ".torrent")
                {
                    // torrent;
                    Torrent torrent = parser.Parse<Torrent>(file.FullName);
                    magnetLink = HttpUtility.UrlEncode(torrent.GetMagnetLink());
                    return magnetLink;
                }

                magnetLink = HttpUtility.UrlEncode(await file.OpenText().ReadToEndAsync());
                return magnetLink;
            }
            catch { return null; }


        }

        private async Task<HttpResponseMessage> RDApi(APIObject apiData)
        {
            string method;
            if (string.IsNullOrEmpty(apiData.data)) { method = "GET"; }
            else { method = "POST"; }
            string requestURL = apiData.Method switch
            {
                API.AddMagnet => "https://api.real-debrid.com/rest/1.0/torrents/addMagnet",
                API.SelectFiles => $"https://api.real-debrid.com/rest/1.0/torrents/selectFiles/{apiData.id}",
                API.UnrestrictLink => $"https://api.real-debrid.com/rest/1.0/unrestrict/link",
                API.Info => $"https://api.real-debrid.com/rest/1.0/torrents/info/{apiData.id}"
            };

            using var request = new HttpRequestMessage(new HttpMethod(method), requestURL);
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {Environment.GetEnvironmentVariable("APIKey")}");
            request.Content = apiData.Method switch
            {
                API.AddMagnet => new StringContent($"magnet={apiData.data}"),
                API.UnrestrictLink => new StringContent($"link={apiData.data}"),
                API.SelectFiles => new StringContent($"files={apiData.data}"),
                _ => null
            };
            if (request.Content != null)
            {
                string MediaTypeHeader = "application/x-bittorrent";
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeHeader);
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            //var t = response.Content.ReadAsStringAsync();
            return response;
        }

        //[HttpGet]
        //public async Task<List<RDTorrentInfo>> GetSelectItems()
        //{
        //    //Return to the UI a list of items wo download links to be submitted for DL and displayed in tasks UI perhaps?
        //    return await GetSelectItemsInternal();
        //}

        //private Task<List<RDTorrentInfo>> GetSelectItemsInternal()
        //{
        //    throw new NotImplementedException();
        //}

        //[HttpGet]
        //public async Task<List<RDTorrentInfo>> GetAvailableItems()
        //{
        //    //Get list of all items in RD for DL, use this to check on stuff ready for DL added recently?
        //    return await GetAvailableItemsInternal();
        //}

        //private Task<List<RDTorrentInfo>> GetAvailableItemsInternal()
        //{
        //    throw new NotImplementedException();
        //}

        //Helper Function



    }
}