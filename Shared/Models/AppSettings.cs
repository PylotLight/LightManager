using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightManager.Shared.Models
{
    public class AppSettings
    {
        public string? ImportPath { get; set; }
        public string? ExportPath { get; set; }
        public string? RDAPIKey { get; set; }
        public List<string> SupportedFormats { get; set; } = new List<string>() { "mkv", "mp4" };
    }
}
