using System;
using System.Collections.Generic;

namespace EventManagement.BusinessLogic.Models.APIDTOOjbects
{
    public class GPSAPILogin
    {
        public GPSAPILogin()
        {
            UserEvents = new List<APIUserEvents>();
        }

        public string Token { get; set; }

        public string Email { get; set; }

        public bool IsAuthenticated { get; set; }

        public string Message { get; set; }

        public bool HasPassword { get; set; }

        public string Unique_Participant_ID { get; set; }

        public bool IsValid { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string AttendeeType { get; set; }

        public List<APIUserEvents> UserEvents { get; set; }
    }


    public class GPSAPILogin1
    {
        public string Token { get; set; }

        public string Email { get; set; }

        public bool IsAuthenticated { get; set; }

        public int UserID { get; set; }

        public string UserFullName { get; set; }

        public bool isAdmin { get; set; }

        public string UserIcon { get; set; }

        public string Message { get; set; }

        public string Password { get; set; }

        public bool HasPassword { get; set; }

        public bool IsValid { get; set; }

        public List<Object> UserEvents { get; set; }
    }

    public class APIUserEvents
    {
        public string EventName { get; set; }

        public int GalacticEventID { get; set; }
    }

    public class UserActivity
    {
        public int ActivityID { get; set; }

        public string ActivityName { get; set; }

        public string ActivityDate { get; set; }

        public string ActivityStartTime { get; set; }

        public string ActivityEndTime { get; set; }
    }


    public class EventActivitiesGAPI
    {
        public string Token { get; set; }

        public int? GalacticEventID { get; set; }

        public List<UserActivity> ProgramActivities { get; set; }
    }


    public class GetPaxActivities
    {
        public string Token { get; set; }

        public int GalacticEventID { get; set; }

        public String Email { get; set; }

        public List<UserActivity> PaxActivities { get; set; }
    }

    public class ProgramAttendee
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string LinkedInURL { get; set; }

        public string AttendeeType { get; set; }

    }

    public class GPSAttendees
    {
        public string Token { get; set; }

        public List<ProgramAttendee> ProgramAttendees { get; set; }

    }

    public class EventInfos
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string ClientName { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string Destination { get; set; }

        public string WebURL { get; set; }
    }

    public class GetEventInfo
    {
        public string Token { get; set; }

        public string GalacticEventID { get; set; }

        public EventInfos EventInfos { get; set; }

    }

}
