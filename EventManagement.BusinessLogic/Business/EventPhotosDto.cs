using System.Collections.Generic;

namespace EventManagement.BusinessLogic.Business
{
    public class EventPhotosDto
    {
        public int ID { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; internal set; }
        
        public bool? Status { get; internal set; }
        
        public string Url { get; internal set; }
        
        public int? PhotoHeight { get; set; }
        
        public int? PhotoWidth { get; set; }
     
        public IEnumerable<FloorRegionComments> Comments { get; set; }
    }

    public class FloorRegionComments
    {
        public int X { get; set; }
        
        public int Y { get; set; }
        
        public int Height { get; set; }
        
        public int Width { get; set; }
        
        public string Description { get; set; }
        
        public string Regioncolor { get; set; }
    }
}