using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightManager.Shared.Models
{
    public class LogData
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public DateTime Created { get; set; }
    }
}
