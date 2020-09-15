using System;
using System.Collections.Generic;
using System.Linq;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.BusinessLogic.Models.APIDTOOjbects;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Business
{
    public class UserGroups : BaseLogic
    {
        public void AddResyncedGroups(List<ProgramAttendee> programAttendees, int eventID, int UserID)
        {
            var groupedAttendees = programAttendees.GroupBy(x => x.AttendeeType);

            foreach (var item in groupedAttendees)
            {
                var groupName = !string.IsNullOrEmpty(item.Key) ? item.Key : "Attendee";
                GroupInfo groupInfo;
                List<AttendeeGroup> attendeeGroups = new List<AttendeeGroup>();
                if (!Db.GroupInfoes.Where(x => x.EventID == eventID).Any(x => x.GroupName == groupName))
                {
                    groupInfo = new GroupInfo
                    {
                        GroupName = groupName,
                        CreatedDate = DateTime.Now,
                        EventID = eventID,
                        GroupDescription = groupName,
                        CreatedBy = UserID
                    };
                    Db.GroupInfoes.Add(groupInfo);
                    Db.SaveChanges();
                }
                else
                {
                    groupInfo = Db.GroupInfoes.FirstOrDefault(x => x.GroupName == groupName);
                }
                foreach (var attendee in item)
                {
                    if (!Db.AttendeeGroups.Any(x => x.Attende.Email == attendee.Email && x.GroupID == groupInfo.ID))
                    {
                        var dbAttendee = Db.Attendes.FirstOrDefault(x => x.Email == attendee.Email);
                        if (dbAttendee == null)
                        {
                            throw new Exception("Please resync the attendees before syncing the groups");
                        }
                        Db.AttendeeGroups.Add(
                            new AttendeeGroup()
                            {
                                AttendeeID = dbAttendee.ID,
                                GroupID = groupInfo.ID
                            });
                    }
                }
                Db.SaveChanges();
            }
        }

    }
}
