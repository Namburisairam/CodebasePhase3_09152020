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
using EventManagement.BusinessLogic.Business;
using EventManagement.Filter;
using TransportERP.Base;
using System.IO;
using CommonHelpers;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class AttendesController : BaseController
    {        
        AttendesLogic attendBuisness = new AttendesLogic();
        // GET: Attendes
        public async Task<ActionResult> Index()
        {
            return View(await attendBuisness.GetAttendes());
        }
        // GET: Attendes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attende attende =await attendBuisness.GetAttende(id.Value);
            if (attende == null)
            {
                return HttpNotFound();
            }
            return View(attende);
        }

        // GET: Attendes/Create
        public async Task<ActionResult> Create(int? id)
        {
            var attende =await attendBuisness.GetAttende(id);
            EventLogic eventLogic = new EventLogic();
            ViewBag.Events = await eventLogic.GetAllEvents();
            return View(attende);
        }

        // POST: Attendes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Attende attende,List<int> events, HttpPostedFileBase thumbnail)
        {
            try
            {
                string fileName = "";
                if (thumbnail != null)
                    fileName = Guid.NewGuid().ToString() + ".png";
                if (attende.ID == 0)
                    attende.AddedBY = CurrentUserSession.UserID;
                attende.Thumbnail = fileName;
                await attendBuisness.AddUpdateAttendee(attende,events);

                if (thumbnail != null)
                    thumbnail.SaveAs(Path.Combine(ImageSavePath, fileName));

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Helpers.LogError("Atted create error", ex);
                EventLogic eventLogic = new EventLogic();
                ViewBag.Events = await eventLogic.GetAllEvents();
                ViewBag.Error = Literals.ErrorMessage;
                return View(attende);
            }
        }



        // GET: Attendes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attende attende = await attendBuisness.GetAttende(id.Value);
            if (attende == null)
            {
                return HttpNotFound();
            }
            return View(attende);
        }

        // POST: Attendes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            //Attende attende = await db.Attendes.FindAsync(id);
            //db.Attendes.Remove(attende);
            //await db.SaveChangesAsync();
            
            return RedirectToAction("Index");
        }


    }
}
