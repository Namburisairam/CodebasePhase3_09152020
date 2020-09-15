using EventManagement.Areas.API.Models;
using System;

namespace EventManagement.BusinessLogic.Models
{
    public class SponsorsDTO : BaseModel
    {
        public int ID { get; set; }
        
        public string Description { get; set; }
        
        public string Thumbnail { get; set; }
        
        public string DocURL { get; set; }
        
        public new Nullable<bool> Status { get; set; }
        
        public string Name { get; set; }

    }
}
