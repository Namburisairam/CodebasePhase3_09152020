using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.BusinessLogic.Models
{
    public class ReplicateEvent
    {
        public bool EventInformation { get; set; }

        public bool EventLevelSurvey { get; set; }

        public bool EventLocationDetails { get; set; }

        public bool WidgetMappings { get; set; }

        public bool Attendees { get; set; }

        public bool Theme { get; set; }

        public bool SocialForum { get; set; }

        public bool Documents { get; set; }

        public bool Sponsor { get; set; }

        public bool Exhibitors { get; set; }

        public bool Vendors { get; set; }

        public bool Photos { get; set; }

        public bool Activities { get; set; }

        public bool ActivitiesSurvey { get; set; }

        public bool FloorMap { get; set; }
    }
}
