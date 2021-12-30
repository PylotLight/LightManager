using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightManager.Shared.Models
{
    public class DLTask
    {
        public bool Download { get; set; }
        public string? Filename { get; set; }
        public string? Description { get; set; }
        public DateTime LastModified { get; set; }
        public string? MagnetLink { get; set; }
        //Filename, file contents/episode count
    }
}
