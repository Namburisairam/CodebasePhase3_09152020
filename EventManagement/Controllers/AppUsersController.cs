using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using CommonHelpers;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Classes;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class AppUsersController : BaseController
    {
        private Entities db = new Entities();
        AttendesLogic attendBuisness = new AttendesLogic();

        // GET: AppUsers
        public async Task<ActionResult> Index()
        {
            return View(await attendBuisness.GetAttendes(checkAttendeeValidity: new AdminValidator()));
        }

        // GET: AppUsers/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    AppUser appUser = await db.AppUsers.FindAsync(id);
        //    if (appUser == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(appUser);
        //}

        // GET: AppUsers/Create
        public async Task<ActionResult> Create(int? id)
        {
            var adminUser = id != null ? db.Attendes.Find(id) : new Attende() { AdminInfo = new AdminInfo() };
            EventLogic eventLogic = new EventLogic();
            ViewBag.Events = await eventLogic.GetAllEvents();
            return View(adminUser);
        }

        // POST: AppUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Attende appUser, AdminInfo adminInfo, List<int> events)
        {
            try
            {
                ChatLogic chatLogic = new ChatLogic();
                if (appUser.ID > 0)
                {
                    var update = await db.Attendes.FindAsync(appUser.ID);
                    update.FirstName = appUser.FirstName;
                    update.Lastname = appUser.Lastname;
                    update.Email = appUser.Email.Trim();
                    update.AdminInfo.UserRole = adminInfo.UserRole;
                    update.AdminInfo.Password = adminInfo.Password;
                    update.AdminInfo.PhoneNumber = adminInfo.PhoneNumber;

                    if (events == null)
                    {
                        var removeAll = db.AttendesEvents.Where(x => x.AttendesID == appUser.ID).ToList();
                        db.AttendesEvents.RemoveRange(removeAll);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        var removeEvent = db.AttendesEvents.Where(x => x.AttendesID == appUser.ID && !events.Contains(x.EventID));
                        if (removeEvent.Any())
                        {
                            db.AttendesEvents.RemoveRange(removeEvent);
                            await db.SaveChangesAsync();
                        }
                        foreach (var item in events)
                        {
                            if (!db.AttendesEvents.Any(x => x.EventID == item && x.AttendesID == appUser.ID))
                            {
                                db.AttendesEvents.Add(new AttendesEvent
                                {
                                    EventID = item,
                                    AttendesID = appUser.ID
                                });
                                chatLogic.AddNewAdminToEventChannels(appUser, item);
                            }
                        }
                    }
                    await db.SaveChangesAsync();
                }
                else
                {
                    if (db.Attendes.Any(x => string.Equals(x.Email.Trim(), appUser.Email.Trim(), StringComparison.CurrentCultureIgnoreCase)))
                    {
                        EventLogic eventLogic = new EventLogic();
                        ViewBag.Events = await eventLogic.GetAllEvents();
                        ModelState.AddModelError("Email", "Email already exists. Please try any other Email");
                        ModelState.AddModelError(String.Empty, "Failed");
                        return View(appUser);
                    }
                    appUser.AdminInfo = adminInfo;
                    appUser.Email = appUser.Email.Trim();
                    appUser.IsAdmin = true;
                    if (events != null && events.Count > 0)
                        appUser.AttendesEvents = events.Select(x => new AttendesEvent { EventID = x }).ToList();

                    if (chatLogic.getAdminGroupID() != null)
                    {
                        appUser.AttendeeGroups.Add(new AttendeeGroup() { GroupID = chatLogic.getAdminGroupID() });
                    }
                    db.Attendes.Add(appUser);
                    await db.SaveChangesAsync();
                    if (events != null)
                    {
                        foreach (var eventID in events)
                        {
                            chatLogic.AddNewAdminToEventChannels(appUser, eventID);
                        }
                    }
                }
                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException ex)
            {
                Helpers.LogError("AppUser Error", ex);
                ModelState.AddModelError(String.Empty, ex.EntityValidationErrors.ElementAt(0).ValidationErrors.ElementAt(0).ErrorMessage);
                EventLogic eventLogic = new EventLogic();
                ViewBag.Events = await eventLogic.GetAllEvents();
                ViewBag.error = Literals.ErrorMessage;
                return View(appUser);
            }
        }

        //// GET: AppUsers/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    AppUser appUser = await db.AppUsers.FindAsync(id);
        //    if (appUser == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(appUser);
        //}

        // POST: AppUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Id,FirstName,LastName,Email,Password,PhoneNumber,UserRole")] AppUser appUser)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(appUser).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View(appUser);
        //}

        // GET: AppUsers/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attende appUser = await db.Attendes.FindAsync(id);
            if (appUser == null)
            {
                return HttpNotFound();
            }
            return View(appUser);
        }

        public async Task<ActionResult> DeleteUser(int id)
        {
            Attende appUser = await db.Attendes.FindAsync(id);
            //db.Attendes.Remove(appUser);
            db.DeleteAttendee(id);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        //// POST: AppUsers/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    AppUser appUser = await db.AppUsers.FindAsync(id);
        //    db.AppUsers.Remove(appUser);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
