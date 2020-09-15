using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.BusinessLogic.Business
{
    public class RecentMessage
    {
        public bool IsAdmin { get; set; }

        public string UserName { get; set; }

        public string TextMessage { get; set; }

        public int ChannelID { get; set; }

        public int AttendeeID { get; set; }

        public string Photo { get; set; }

        public int UnreadMessageCount { get; set; }

        public DateTime LastMessageSentDate { get; set; }
        public string type { get; set; }
    }
}

