using BencodeNET.Parsing;
using BencodeNET.Torrents;
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
    public class TasksController : ControllerBase
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
        private readonly IAppSettingsService _appSettings;

        public TasksController(ILogger<TasksController> logger, IAppSettingsService appSettings)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _appSettings = appSettings;
        }


        [HttpGet]
        public async Task<List<DLTask>> GetTasks()
        {
            return await GetTasksInternal();
        }

        [HttpPost]
        public async Task<RDTorrentInfo> SelectFiles(List<RDTorrentInfo> torrentInfo)
        {
            return await SelectFilesInternal(torrentInfo);
        }




        [HttpPost]
        public async Task<List<RDTorrentInfo>> AddSelected(List<DLTask> tasks)
        {
            return await AddSelectedInternal(tasks);
        }

        private async Task<List<RDTorrentInfo>> AddSelectedInternal(List<DLTask> tasks)
        {
            //Add selected items to DL list then return item select UI in modal when interactive? (sonnar interactive vs quick add)
            try
            {
                List<RDTorrentInfo> infos = new List<RDTorrentInfo>();
                foreach (var task in tasks)
                {
                    if (task.Download is false) { continue; }
                    //submit to RD and get response, save info to db/log/service? local file for now.. should be db though ;p

                    RDAddTorrent data = await AddSelectedTorrent(task);
                    //Get file Ids and select them for next func?
                    //infos.Add(await HandleSelectedTorrent(data));
                    RDTorrentInfo torrentReturnData = await HandleSelectedTorrent(data);
                    if (torrentReturnData.status == "downloaded")
                    {
                        //start downloading to server, need dl location and move location
                        await DownloadFiles(torrentReturnData);
                        //var file = await RDApi(new APIObject() {data= torrentReturnData.links });
                        //Console.WriteLine(torrentReturnData);
                        //unrestrict and dl

                    }
                    //await WaitForTorrent();
                    //add to torrents to check on later?
                    //Get the unrestricted links
                    //Check if ready for download
                    //Spawn thread to check for downloads that arents ready
                    //otherwise grab and download
                    //move file to selected location with proper torrent filename
                    //delete/move torrent file, or grab mag link and store in db and del torrent file?
                    //log downloaded and moved files.

                }

                _logger.LogInformation("All selected files have been added, file selection required.");
                return infos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                //return null;
                throw new Exception("Add selected has failed");
            }


        }
        //Add Torrent file and return for file selection
        private async Task<RDAddTorrent> AddSelectedTorrent(DLTask task)
        {
            //submit torrent data to api and return an RD object
            APIObject apiData = new APIObject() { data = task.MagnetLink, Method = API.AddMagnet };
            var AddTorrentResponse = await RDApi(apiData);
            RDAddTorrent AddedContent = await AddTorrentResponse.Content.ReadFromJsonAsync<RDAddTorrent>();
            AddTorrentResponse.EnsureSuccessStatusCode();
            _logger.LogInformation($"{task.Description} was added to RD");

            return AddedContent;
        }
        //Get Torrent info?
        private async Task<RDTorrentInfo> HandleSelectedTorrent(RDAddTorrent addTorrent)
        {//Add torrent and select episode files and return dl links to unrestrict if ready, otherwise return proggress
            //Get list of files and status of torrent
            var files = await RDApi(new APIObject() { id = addTorrent.id, Method = API.Info });
            //var t = await files.Content.ReadAsStringAsync();
            RDTorrentInfo torrentFileInfo = await files.Content.ReadFromJsonAsync<RDTorrentInfo>();

            var fileList = torrentFileInfo.files.Where(x => x.path.Contains(".mkv")).Select(x => x.id).ToArray();
            if (fileList.Length != 0)
            {
                //submit file selection
                var result = await RDApi(new APIObject() { id = torrentFileInfo.id, data = string.Join(',', fileList), Method = API.SelectFiles });
                //get update of torrent after selecting files, need to grab links if available or wait if unavailable.
                files = await RDApi(new APIObject() { id = addTorrent.id, Method = API.Info });
                torrentFileInfo = await files.Content.ReadFromJsonAsync<RDTorrentInfo>();
                //Check if ready:
                if (torrentFileInfo.status == "downloaded")
                {
                    return torrentFileInfo;

                }

                //else loop and check api until its ready and return that.

            }

            //Grab file info and if multifile do something to select file IDs and cont or return to select them.
            //Now need a whole new table/UI for displaying possible multiple files.


            //With the returned submitted ID, youll need to select the files here, or return them to UI for selection?
            //int fileNum = addTorrent.files.Where(x => x.path.Contains(".mkv")).Select(x => x.id).SingleOrDefault();
            //2-3
            //HttpResponseMessage RDSelectResponse = await RDApi(_httpClient, RDAddContent.id, fileNum.ToString());
            //string RDSelectObject = await RDSelectResponse.Content.ReadAsStringAsync();

            return torrentFileInfo;
        }

        private async Task DownloadFiles(RDTorrentInfo torrentInfo)
        {
            //fix with an injected service?

            foreach (var link in torrentInfo.links)
            {
                var responseMessage = await RDApi(new APIObject() { data = HttpUtility.UrlEncode(link), Method = API.UnrestrictLink });
                RDUnrestrictedJson RDUnrestrictedLinks = await responseMessage.Content.ReadFromJsonAsync<RDUnrestrictedJson>();
                if (!string.IsNullOrEmpty(RDUnrestrictedLinks.download))
                {
                    //get new DL links
                    using var request = new HttpRequestMessage(new HttpMethod("GET"), RDUnrestrictedLinks.download);
                    HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    //Confirm OK
                    response.EnsureSuccessStatusCode();
                    //Update
                    //AppSettings settings = _appSettings.ReadSettings();

                    //Create filestream
                    string fileDLPath = _appSettings.ReadSettings().ExportPath + RDUnrestrictedLinks.filename;
                    using FileStream fileStream = new FileStream(RDUnrestrictedLinks.filename, FileMode.Create, FileAccess.Write, FileShare.None);

                    await response.Content.CopyToAsync(fileStream);
                    System.IO.File.Move(RDUnrestrictedLinks.filename, $"{_appSettings.ReadSettings().ExportPath}/{RDUnrestrictedLinks.filename}"); //archive torrent/magnet file?
                    //System.IO.File.Move(RDUnrestrictedLinks.filename, $"/volume1/Media/Downloads/{RDUnrestrictedLinks.filename}"); //move the download itself or just download to this location to start with.
                    _logger.LogInformation("Downloaded: " + RDUnrestrictedLinks.filename);

                }
            }
        }

        private async Task<List<DLTask>> GetTasksInternal()
        {
            //var settingsController = new SettingsController();
            //var currentsettings = await _appSettings.ReadSettings();
            List<DLTask> tasks = new List<DLTask>();
            var files = new DirectoryInfo(_appSettings.ReadSettings().ImportPath).GetFiles();
            //var files2 = files (currentsettings.ImportPath);
            foreach (var file in files)
            {
                tasks.Add(new DLTask()
                {
                    Filename = file.Name,
                    LastModified = file.LastWriteTime,
                    MagnetLink = await GetMagnet(file)
                });

            }
            return tasks;
        }

        private async Task<RDTorrentInfo> SelectFilesInternal(List<RDTorrentInfo> torrentInfo)
        {
            //var settingsController = new SettingsController();
            //var currentsettings = await settingsController.ReadSettings();
            //List<DLTask> tasks = new List<DLTask>();
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