using System;

namespace EventManagement.BusinessLogic.Models
{
    public  class SocialCommentDTO
    {
        public int ID { get; set; }
        
        public int PostID { get; set; }
        
        public int AttendesID { get; set; }
        
        public string Comments { get; set; }
        
        public Nullable<System.DateTime> ApprovedON { get; set; }
        
        public Nullable<int> ApprovedBY { get; set; }
        
        public Nullable<System.DateTime> CommentedON { get; set; }
        
        public int ReplyerID { get; set; }
    }
}
