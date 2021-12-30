using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightManager.Shared.Models
{
    public class DLFileSelection
    {
        public bool Selected { get; set; }
        public RDTorrentInfo torrentInfo { get; set; }
        //Filename, file contents/episode count
    }
}
