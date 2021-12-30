using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightManager.Shared.Models
{
    public class RDAddTorrent
    {
        public string id { get; set; }
        public string uri { get; set; }
    }

    public class RDTorrentInfo
    {
        public string id { get; set; }
        public string filename { get; set; }
        public string original_filename { get; set; }
        public string hash { get; set; }
        public long bytes { get; set; }
        public long original_bytes { get; set; }
        public string host { get; set; }
        public int split { get; set; }
        public int progress { get; set; }
        public string status { get; set; }
        public DateTime added { get; set; }
        public IList<File> files { get; set; }
        public IList<string> links { get; set; }
    }

    public class File
    {
        public int id { get; set; }
        public string path { get; set; }
        public long bytes { get; set; }
        public int selected { get; set; }
    }

    public class RDUnrestrictedJson
    {
        public string id { get; set; }
        public string filename { get; set; }
        public string mimeType { get; set; }
        public long filesize { get; set; }
        public string link { get; set; }
        public string host { get; set; }
        public string host_icon { get; set; }
        public int chunks { get; set; }
        public int crc { get; set; }
        public string download { get; set; }
        public int streamable { get; set; }
    }

    public enum API
    { AddMagnet, SelectFiles, UnrestrictLink, Info }

    public class APIObject
    {
        public API Method { get; set; }
        public string id { get; set; }
        public string data { get; set; }
        
        //public string link { get; set; }
    }
}
