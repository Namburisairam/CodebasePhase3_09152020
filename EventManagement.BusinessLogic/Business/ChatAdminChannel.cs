using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.BusinessLogic.Business
{
    public class ChatAdminChannel
    {
        public int ChannelId { get; set; }
        
        public string EventName { get; set; }
        
        public int ToUserId { get; set; }
        
        public string ChannelName { get; set; }

        public int UnreadMessageCount { get; set;}

        public string ChannelLastMessage { get; set; }

        public DateTime ChannelLatestTime { get; set; }

    }

}
