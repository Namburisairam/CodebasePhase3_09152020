using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.BusinessLogic.Models
{
    public class CMSChannelMessages
    {
        public long id { get; set; }
        public int channelid { get; set; }
        public int userid { get; set; }
        public string sendername { get; set; }
        public string message { get; set; }
        public System.DateTime sentdate { get; set; }
        public string type { get; set; }
    }
}
