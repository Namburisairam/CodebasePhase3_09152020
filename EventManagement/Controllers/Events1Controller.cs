using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EventManagement.DataAccess.DataBase.Model;
using CommonHelpers;

namespace EventManagement.Controllers
{
    public class Events1Controller : Controller
    {
        private Event_ManagementEntities db = new Event_ManagementEntities();

        // GET: Events1
        public async Task<ActionResult> Index()
        {
            var events = db.Events.Include(@ => @.AppUser);
            return View(await events.ToListAsync());
        }

        // GET: Events1/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = await db.Events.FindAsync(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // GET: Events1/Create
        public ActionResult Create()
        {
            ViewBag.ManagerUserId = new SelectList(db.AppUsers.Where(a => a.UserRole == Convert.ToInt32(enUserRole.EventManager)), "Id", "FirstName");
            return View();
        }

        // POST: Events1/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,ModeratePost,GalacticEventId,EventName,Message,ManagerUserId,EnableThemes,EnableActivity,EnableQrCode,EnableAttendees,EnableFloormap,EnablePhotos,EnableSponsor,EnableComments,EnableGoogleApi,allow_bookmark,EventImage,EnableSocialScreen,EnableSearchScreen")] Event @event)
        {
            if (ModelState.IsValid)
            {
                db.Events.Add(@event);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ManagerUserId = new SelectList(db.AppUsers.Where(a => a.UserRole == Convert.ToInt32(enUserRole.EventManager)), "Id", "FirstName", @event.ManagerUserId);
            return View(@event);
        }

        // GET: Events1/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = await db.Events.FindAsync(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            ViewBag.ManagerUserId = new SelectList(db.AppUsers, "Id", "FirstName", @event.ManagerUserId);
            return View(@event);
        }

        // POST: Events1/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,ModeratePost,GalacticEventId,EventName,Message,ManagerUserId,EnableThemes,EnableActivity,EnableQrCode,EnableAttendees,EnableFloormap,EnablePhotos,EnableSponsor,EnableComments,EnableGoogleApi,allow_bookmark,EventImage,EnableSocialScreen,EnableSearchScreen")] Event @event)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@event).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ManagerUserId = new SelectList(db.AppUsers, "Id", "FirstName", @event.ManagerUserId);
            return View(@event);
        }

        // GET: Events1/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = await db.Events.FindAsync(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Events1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Event @event = await db.Events.FindAsync(id);
            db.Events.Remove(@event);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
