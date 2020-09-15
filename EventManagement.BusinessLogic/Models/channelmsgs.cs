using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.BusinessLogic.Models
{
   public class channelmsgs
    {
        public int channelId { get; set; }
        public int FromuserId { get; set; }
        public int TouserId { get; set; }
        public List<channelmessageobj> messages { get; set; }
        public string strmessage { get; set; }
        public string LoginUserFcmToken { get; set; }
        public List<Base64IMG> ImageBase64 { get; set; }
        public string ImgExt { get; set; }
        public string Image { get; set; }
        public sbyte msgtype { get; set; }
    }
   public class Base64IMG
   {
       public int? Id { get; set; }
       public string Base64 { get; set; }
       public string FileName { get; set; }
   }
   public class channelmessageobj
   {
       public long _id { get; set; }
       public string text { get; set; }
       public user user { get; set; }
       public long messageId { get; set; }
       public bool Isread { get; set; }
       public DateTime createdAt { get; set; }
       public int ChannelId { get; set; }
       public sbyte? MsgType { get; set; }
       public string image { get; set; }
       public DateTime? logoutTime { get; set; }
   }

   public class user
   {
       public int _id { get; set; }
   }
}
