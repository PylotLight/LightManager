using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightManager.Shared.Models
{
    public class Notification
    {
        public string Description { get; set; }
        public enum Severity { Error,Log,Debug,Message }
    }
}
