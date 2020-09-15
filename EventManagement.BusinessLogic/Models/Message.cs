namespace EventManagement.BusinessLogic.Models
{
    public class Message
    {
        public int SenderID { get; set; }
        public int ChannelId { get; set; }
        public int EventId { get; set; }
        public string MessageText { get; set; }
        public string AttendeeName { get; set; }
        public string NotificationType { get; set; }
        public bool status { get; set; }
        public string type { get; set; }
    }
}
