using System;

namespace EventManagement.BusinessLogic.Models
{
    public class RecentConversations
    {
        public String UserName { get; set; }
        
        public String TextMessage { get; set; }
        
        public String ChannelID { get; set; }
        
        public int? AttendeeID { get; set; }
        
        public bool IsAdmin { get; set; }
        
        public string Photo { get; set; }
        
        public DateTime? CreatedDateTime { get; set; }
        
        public string Company { get; set; }
        
        public string JobTitle { get; set; }
    }

}
