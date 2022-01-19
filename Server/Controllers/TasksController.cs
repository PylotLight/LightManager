using BencodeNET.Parsing;
using BencodeNET.Torrents;
using LightManager.Server.Context;
using LightManager.Shared.Models;
using LightManager.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
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
        private readonly ISettingsService _appSettings;
        private readonly TaskDBContext _context;

        public TasksController(ILogger<TasksController> logger, ISettingsService appSettings, TaskDBContext context)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _appSettings = appSettings;
            _context = context;
        }


        [HttpGet]
        public async Task<List<TaskItem>> GetTasks()
        {
            return await GetTasksInternal();
        }

        [HttpPost]
        public async Task<RDTorrentInfo> SelectFiles(List<RDTorrentInfo> torrentInfo)
        {
            return await SelectFilesInternal(torrentInfo);
        }

        [HttpPost]
        public async Task<List<RDTorrentInfo>> AddSelected(List<TaskItem> tasks)
        {
            return await AddSelectedInternal(tasks);
        }

        private async Task<List<RDTorrentInfo>> AddSelectedInternal(List<TaskItem> tasks)
        {
            //Add selected items to DL list then return item select UI in modal when interactive? (sonnar interactive vs quick add)
            try
            {
                List<RDTorrentInfo> infos = new List<RDTorrentInfo>();
                _logger.LogInformation($"Loading {tasks.Count} tasks");
                foreach (var task in tasks)
                {
                    if (task.Downloaded is false) { continue; }
                    //submit to RD and get response, save info to db/log/service? local file for now.. should be db though ;p
                    
                    RDAddTorrent SelectedTorrentResponse = await AddSelectedTorrent(task);
                    _logger.LogInformation($"Added Torrent to RD");

                    RDTorrentInfo torrentReturnData = await HandleSelectedTorrent(SelectedTorrentResponse);
                    _logger.LogInformation($"Sucessfully selected {torrentReturnData.files.Count} Files with torrent status of {torrentReturnData.status}");

                    while (torrentReturnData.status != "downloaded")
                    {
                        _logger.LogInformation($"{torrentReturnData.filename} is not ready yet, trying again in 60 seconds");
                        await Task.Delay(TimeSpan.FromSeconds(60));
                        var UpdatedInfoResponse = await RDApi(new APIObject() { id = torrentReturnData.id, Method = API.Info });
                        torrentReturnData = await UpdatedInfoResponse.Content.ReadFromJsonAsync<RDTorrentInfo>();
                        _logger.LogInformation($"Torrent Progress: {torrentReturnData.progress}");
                    }

                    var links = await GetDownloadLinks(torrentReturnData);
                    await DownloadFiles(links);
                    await ArchiveTaskRecord(task); //delete/remove torrent/mag file and submit to db after finished downloading.

                }

                _logger.LogInformation("All selected files have been added.");
                return infos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                //return null;
                throw new Exception("Download has failed");
            }


        }

        private async Task ArchiveTaskRecord(TaskItem task)
        {
            _logger.LogInformation($"Saving {task.Filename} to DB");
            await _context.AddAsync(task);
            await _context.SaveChangesAsync();
            string TorrentPath = Path.Combine((await _appSettings.ReadSettingsAsync()).ImportPath, task.Filename);
            _logger.LogInformation($"Deleting {task.Filename}");
            System.IO.File.Delete(TorrentPath);
        }

        //Add Torrent file and return for file selection
        private async Task<RDAddTorrent> AddSelectedTorrent(TaskItem task)
        {
            //submit torrent data to api and return an RD object
            APIObject apiData = new APIObject() { data = task.MagnetLink, Method = API.AddMagnet };
            var AddTorrentResponse = await RDApi(apiData);
            RDAddTorrent AddedContent = await AddTorrentResponse.Content.ReadFromJsonAsync<RDAddTorrent>();
            AddTorrentResponse.EnsureSuccessStatusCode();
            _logger.LogInformation($"{task.Filename} was added to RD");

            return AddedContent;
        }
        //Get Torrent info?
        private async Task<RDTorrentInfo> HandleSelectedTorrent(RDAddTorrent addTorrent)
        {//Add torrent and select episode files and return dl links to unrestrict if ready, otherwise return proggress
            //Get list of files and status of torrent
            var files = await RDApi(new APIObject() { id = addTorrent.id, Method = API.Info });
            //var t = await files.Content.ReadAsStringAsync();
            RDTorrentInfo torrentFileInfo = await files.Content.ReadFromJsonAsync<RDTorrentInfo>();
            var formats = (await _appSettings.ReadSettingsAsync()).SupportedFormats;
            var fileList = torrentFileInfo.files.Where(x => formats.Contains(x.path.Split('.').Last())).Select(x => x.id).ToArray();
            //var fileList2 = torrentFileInfo.files.Where(x => x.path.Contains(".mkv")).Select(x => x.id).ToArray();
            if (fileList.Length != 0)
            {
                _logger.LogInformation($"Selecting {fileList.Length} {string.Join(',', formats)} Files");
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

        private async Task<List<RDUnrestrictedJson>> GetDownloadLinks(RDTorrentInfo torrentInfo)
        {
            //fix with an injected service?
            List<RDUnrestrictedJson> Links = new List<RDUnrestrictedJson >();
            foreach (var link in torrentInfo.links)
            {
                var responseMessage = await RDApi(new APIObject() { data = HttpUtility.UrlEncode(link), Method = API.UnrestrictLink });
                RDUnrestrictedJson RDUnrestrictedLinks = await responseMessage.Content.ReadFromJsonAsync<RDUnrestrictedJson>();
                if (!string.IsNullOrEmpty(RDUnrestrictedLinks.download))
                {
                    Links.Add(RDUnrestrictedLinks);
                    //get new DL links
                }
            }
            return Links;
        }

        private async Task DownloadFiles(List<RDUnrestrictedJson> unrestrictedLinks)
        {
            string exportpath = (await _appSettings.ReadSettingsAsync()).ExportPath;
            foreach (var unrestrictedLink in unrestrictedLinks)
            {
                using var request = new HttpRequestMessage(new HttpMethod("GET"), unrestrictedLink.download);
                HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                //Confirm OK
                response.EnsureSuccessStatusCode();
                //Create filestream
                string fileDLPath = Path.Combine(exportpath, unrestrictedLink.filename);
                using FileStream fileStream = new FileStream(fileDLPath, FileMode.Create, FileAccess.Write, FileShare.None,4096,true);

                await response.Content.CopyToAsync(fileStream);
                //System.IO.File.Move(unrestrictedLink.filename, fileDLPath); 
                //archive torrent/magnet file?                                                                                         
                //System.IO.File.Move(RDUnrestrictedLinks.filename, $"/volume1/Media/Downloads/{RDUnrestrictedLinks.filename}"); //move the download itself or just download to this location to start with.
                _logger.LogInformation("Downloaded: " + unrestrictedLink.filename);
            }
        }

        private async Task<List<TaskItem>> GetTasksInternal()
        {
            //var settingsController = new SettingsController();
            //var currentsettings = await _appSettings.ReadSettings();
            List<TaskItem> tasks = new List<TaskItem>();
            var files = new DirectoryInfo((await _appSettings.ReadSettingsAsync()).ImportPath).GetFiles();
            //var files2 = files (currentsettings.ImportPath);
            foreach (var file in files)
            {
                tasks.Add(new TaskItem()
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
            _logger.LogInformation($"API Request: {apiData.Method}");
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
            _logger.LogInformation($"{apiData.Method} API Request to: {requestURL}");
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