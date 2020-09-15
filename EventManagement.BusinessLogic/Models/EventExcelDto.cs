using System;

namespace EventManagement.BusinessLogic.Models
{
    public class EventExcelDto
    {
        public string Name { get; set; }
        
        public int GalacticId { get; set; }
        
        public string EventGalacticId { get; set; }
    }

    public class EventActivitiesDTO
    {
        public string Description { get; set; }
        
        public string Address { get; set; }
        
        public DateTime StartTime { get; set; }
       
        public DateTime EndTime { get; set; }
        
        public int GalacticEventId { get; set; }
    }
}
