using System;

namespace EventManagement.BusinessLogic.Models
{
    public class Comment
    {
        public int ID { get; set; }

        public int AttendeeId { get; set; }
        
        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public string Userpic { get; set; }
        
        public string CommentDesc { get; set; }
        
        public DateTime? CommentedON { get; set; }
    }
}
