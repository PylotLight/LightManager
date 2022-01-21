using BencodeNET.Parsing;
using BencodeNET.Torrents;
using LightManager.Server.Context;
using LightManager.Shared.Models;
using LightManager.Shared.Services;
using System.Text;
using System.Text.Json;
using System.Web;

namespace LightManager.Server.Services
{
    public partial class TasksService : ITasksService
    {
        private readonly ILogger<SettingsService> _logger;
        private readonly TaskDBContext _context;
        private readonly ISettingsService _settingsService;
        private readonly HttpClient _httpClient;

        public TasksService(ILogger<SettingsService> logger, TaskDBContext context, ISettingsService settingsService, HttpClient httpClient)
        {
            _logger = logger;
            _context = context;
            _settingsService = settingsService;
            _httpClient = httpClient;
        }

        public async Task<List<TaskItem>> GetTasks()
        {
            List<TaskItem> tasks = new List<TaskItem>();
            var files = new DirectoryInfo((await _settingsService.ReadSettingsAsync()).ImportPath).GetFiles();
            foreach (var file in files)
            {
                tasks.Add(new TaskItem()
                {
                    Filename = file.Name,
                    LastModified = file.LastWriteTime,
                    MagnetLink = await GetMagnet(file)
                });

            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return tasks;
        }

        public async Task AddTorrents(List<TaskItem> taskItems)
        {
            try
            {
                List<RDTorrentInfo> infos = new List<RDTorrentInfo>();
                _logger.LogInformation($"Loading {taskItems.Count} tasks");
                foreach (var task in taskItems)
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
                //return infos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                //return null;
                throw new Exception("Download has failed");
            }
        }

        public async Task DeleteFiles(List<TaskItem> tasks)
        {
            foreach (var task in tasks)
            {
                string TorrentPath = Path.Combine((await _settingsService.ReadSettingsAsync()).ImportPath, task.Filename);
                _logger.LogInformation($"Deleting {task.Filename}");
               
                System.IO.File.Delete(TorrentPath);
            }
        }

    }
}