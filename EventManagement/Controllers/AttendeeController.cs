using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CommonHelpers;
using EventManagement.BusinessLogic.Business;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class AttendeeController : BaseController
    {
        AttendesLogic attendBuisness = new AttendesLogic();
        EventLogic eventLogic = new EventLogic();

        CommonLogic commonLogic = new CommonLogic();

        // GET: Attendes
        public async Task<ActionResult> Index()
        {

            return View(await attendBuisness.GetAttendes());
        }
        /// <summary>
        /// Get the details of all attendees
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns information for given attendee</returns>
        // GET: Attendes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attende attende = await attendBuisness.GetAttendee(id.Value);
            if (attende == null)
            {
                return HttpNotFound();
            }
            return View(attende);
        }
        /// <summary>
        /// Deletes attendee information for given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> DeleteAttendee(int id)
        {
            Entities db = new Entities();
            var Attende = await db.Attendes.FindAsync(id);
            //var eventrec = db.AttendesEvents.Where(x => x.AttendesID == Attende.ID).ToList();
            //foreach(var item in eventrec)
            //{
            //    db.AttendesEvents.Remove(item);
            //    await db.SaveChangesAsync();
            //}
            //var Notirec = db.Notifications.Where(x => x.AttendesID == Attende.ID).ToList();
            //foreach (var item in Notirec)
            //{
            //    db.Notifications.Remove(item);
            //    await db.SaveChangesAsync();
            //}

            //var channels = db.ChannelUsers.Where(x=> x.AttendeeID== Attende.ID).ToList();
            //db.ChannelUsers.RemoveRange(channels);
            //db.BookMarks.RemoveRange(db.BookMarks.Where(x => x.AttendesID == Attende.ID));

            //await db.SaveChangesAsync();

            //db.Attendes.Remove(Attende);

            db.DeleteAttendee(id);
            await db.SaveChangesAsync();

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Attendee", "Delete", userId, $"The attendee : {Attende.Email} is deleted by {userName}.");

            return Attende.IsSpeaker ? RedirectToAction("Index", "Speakers") : RedirectToAction("Index");
        }
        public async Task<ActionResult> DeletePic(int id)
        {
            var result = await attendBuisness.DeletePicture(id, ImageSavePath);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Attendee", "Delete", userId, $"The attendee picture id: {id} is deleted by {userName}.");

            return RedirectToAction("Index");
        }
        // GET: Attendes/Create
        public async Task<ActionResult> Create(int? id)
        {
            var attende = await attendBuisness.GetAttendee(id);
            ViewBag.Events = await eventLogic.GetAllEvents();
            return View(attende);
        }

        // POST: Attendes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(Attende attende, List<int> events, HttpPostedFileBase thumbnail)
        {
            try
            {
                bool isUploaded;
                if (await attendBuisness.CheckIfEmailAlreadyExists(attende.Email, attende.ID))
                {
                    EventLogic eventLogic = new EventLogic();
                    ViewBag.Events = await eventLogic.GetAllEvents();
                    ModelState.AddModelError("Email", "Email already exists.");
                    ModelState.AddModelError(String.Empty, "Failed");
                    return attende.IsSpeaker ? View("~/Views/Speakers/Create.cshtml", attende) : View(attende);
                }
                string fileName = string.Empty;

                if (thumbnail != null)
                {
                    isUploaded = true;
                    fileName = Guid.NewGuid().ToString() + ".png";
                }
                else
                {
                    Entities db = new Entities();
                    var imagecheck = await db.Attendes.FindAsync(attende.ID);
                    if (attende.IsUploadedImage == false)
                    {
                        fileName = attendBuisness.GenerateImageFromName(attende.FirstName, attende.Lastname, ImageSavePath);  //"noimage.png";
                        isUploaded = false;
                    }
                    else
                    {
                        isUploaded = true;
                        fileName = string.Empty;
                    }
                    //if(imagecheck?.Thumbnail == null)
                    //{
                    //fileName = attendBuisness.GenerateImageFromName(attende.FirstName, attende.Lastname, ImageSavePath);  //"noimage.png";
                    //isUploadedImage = false;
                    //}
                }
                if (attende.ID == 0)
                {
                    attende.AddedBY = CurrentUserSession.AttendesID;
                }
                attende.Thumbnail = fileName;
                await attendBuisness.AddUpdateAttendee(attende, events, isUploaded);

                if (thumbnail != null)
                    thumbnail.SaveAs(Path.Combine(ImageSavePath, fileName));

                return attende.IsSpeaker ? RedirectToAction("Index", "Speakers") : RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Helpers.LogError("Attendee create error", ex);
                EventLogic eventLogic = new EventLogic();
                ViewBag.Events = await eventLogic.GetAllEvents();
                ViewBag.Error = Literals.ErrorMessage;
                return View(attende);
            }
        }

        public async Task<ActionResult> CheckIfEmailAlreadyExists(string email, int attendeeID)
        {
            var isEmailExists = await attendBuisness.CheckIfEmailAlreadyExists(email, attendeeID);
            return Json(isEmailExists, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Pictures()
        {
            Entities db = new Entities();
            var requested = db.Attendes.Where(x => x.Imageapprove == true).ToList();

            return View(requested);
        }

        // GET: Attendes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attende attende = await attendBuisness.GetAttendee(id.Value);
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
        public string GenerateAttendeeQRCode(int attendeeId)
        {
            QRCodeLogic qr = new QRCodeLogic();
            var qrPath = qr.GenerateAttendeeQRCode(ImageSavePath, attendeeId, CurrentUserSession.AttendesID.Value);
            return qrPath;
        }
    }
}
