using System;

namespace EventManagement.BusinessLogic.Models
{
    public  class FloorMap
    {
        public int ID { get; set; }
        
        public string FloorName { get; set; }

        public Nullable<int> PhotoHeight { get; set; }
        
        public Nullable<int> PhotoWidth { get; set; }
        
        public string PhotoURL { get; set; }
        //public FloorRegionMapping FloorRegionMappings { get; set; }
        //public Activite Activities { get; set; }
        //public Sponsor Sponsors { get; set; }
        //public Exhibitor Exhibitors { get; set; }
        //public Vendor Vendors { get; set; }
        
        public object MarkedLocations { get; set; }
    }
}
