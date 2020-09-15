using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EventManagement.DataAccess.DataBase.Model;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    public class UserEventsController : BaseController
    {
        Entities db = new Entities();
        // GET: UserEvents
        public ActionResult Create(int? id, int eventID)
        {
            var group = id == null ? new GroupInfo() : db.GroupInfoes.Find(id);
            ViewBag.eventAttendees = db.Attendes.Where(x => x.AttendesEvents.Any(y => y.EventID == eventID));
            return View(group);
        }
        /// <summary>
        /// Add attendees to groups
        /// </summary>
        /// <param name="groupInfo"></param>
        /// <param name="attendees"></param>
        /// <returns></returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GroupInfo groupInfo, List<int> attendees)
        {
            if (groupInfo.ID > 0)
            {
                var selectedGroup = db.GroupInfoes.Find(groupInfo.ID);
                selectedGroup.GroupName = groupInfo.GroupName;
                selectedGroup.GroupDescription = groupInfo.GroupDescription;
                if (attendees == null)
                {
                    var removeAttendees = db.AttendeeGroups.Where(x => x.GroupID == groupInfo.ID);
                    db.AttendeeGroups.RemoveRange(removeAttendees);
                }
                else
                {
                    var removeAttendees = db.AttendeeGroups.Where(x => x.GroupID == groupInfo.ID && !attendees.Contains(x.AttendeeID.Value)).ToList();
                    db.AttendeeGroups.RemoveRange(removeAttendees);

                    foreach (var attendeeID in attendees)
                    {
                        if (!db.AttendeeGroups.Where(x => x.GroupID == groupInfo.ID).Any(x => x.AttendeeID == attendeeID))
                        {
                            db.AttendeeGroups.Add(new AttendeeGroup() { AttendeeID = attendeeID, GroupID = groupInfo.ID });
                        }
                    }
                }
            }
            else
            {
                if (attendees != null && attendees.Count > 0)
                {
                    groupInfo.AttendeeGroups = attendees.Select(x => new AttendeeGroup() { AttendeeID = x }).ToList();
                }
                groupInfo.CreatedBy = CurrentUserSession.AttendesID;
                groupInfo.CreatedDate = DateTime.Now;
                db.GroupInfoes.Add(groupInfo);
            }
            db.SaveChanges();
            return Redirect(Url.Action("Create", "Events", new { Id = groupInfo.EventID }) + "#UserGroups");
        }
        /// <summary>
        /// Delete existing group
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public ActionResult Delete(int ID, int eventID)
        {
            var group = db.GroupInfoes.Find(ID);
            //db.GroupInfoes.Remove(group);
            db.DeleteGroups(ID, eventID);
            db.SaveChanges();
            return Redirect(Url.Action("Create", "Events", new { Id = eventID }) + "#UserGroups");
        }
    }
}