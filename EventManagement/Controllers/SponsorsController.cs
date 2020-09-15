using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CommonHelpers;
using EventManagement.BusinessLogic.Business;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using QRCoder;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class SponsorsController : BaseController
    {
        SponsorsLogic sponsorsLogic = new SponsorsLogic();
        EventLogic eventlogic = new EventLogic();
        CommonLogic commonLogic = new CommonLogic(); 


        // GET: Sponsors
        public async Task<ActionResult> Index()
        {
            return View(await sponsorsLogic.GetSponsors());
        }

        // GET: Sponsors/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sponsor sponsor = await sponsorsLogic.GetSponsorById(id.Value);
            if (sponsor == null)
            {
                return HttpNotFound();
            }
            return View(sponsor);
        }
        /// <summary>
        /// Get all sponsors 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: Sponsors/Create
        public async Task<ActionResult> Create(int? id)
        {
            //Entities Db = new Entities();
            var sponsor = await sponsorsLogic.GetSponsorById(id);
           // EventLogic eventLogic = new EventLogic();
            ViewBag.Events = await eventlogic.GetAllEvents();
            //ViewBag.Userpackage = await Db.UserPackageTypes.ToListAsync();
            return View(sponsor);
        }

        /// <summary>
        /// Create new sponsor in specific event
        /// </summary>
        /// <param name="sponsor"></param>
        /// <param name="events"></param>
        /// <param name="thumbnail"></param>
        /// <returns></returns>
        // POST: Sponsors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(Sponsor sponsor, List<int> events, HttpPostedFileBase thumbnail)
        {
            try
            {
                //string docName = "";
                string fileName = string.Empty;
                if (thumbnail != null)
                    fileName = Guid.NewGuid().ToString() + ".png";
                sponsor.Thumbnail = fileName;
                await sponsorsLogic.AddUpdateSponsor(sponsor, events);
                if (thumbnail != null)
                    thumbnail.SaveAs(Path.Combine(ImageSavePath, fileName));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Helpers.LogError("Sponsor Error", ex);
                //EventLogic eventLogic = new EventLogic();
                ViewBag.Events = await eventlogic.GetAllEvents();
                ViewBag.Error = Literals.ErrorMessage;
                return View(sponsor);
            }
        }

        public async Task<ActionResult> CreateSponsorQrCode(int sponsorId)
        {
            try
            {
                if (sponsorId > 0)
                {
                    Entities Db = new Entities();
                    var record = await Db.Sponsors.FindAsync(sponsorId);
                    var url = record.DocURL;
                    var qrCode = await sponsorsLogic.SponsorQrCode(sponsorId, string.Empty, CurrentUserSession.AttendesID.Value, "", url);
                    var qrCodeImage = GenerateSponsorQrCode(sponsorId, url, enQRCodeFor.Sponsor, qrCode);
                    await sponsorsLogic.UpdateSponsorQrCode(qrCode, qrCodeImage);
                    return Json(qrCodeImage, JsonRequestBehavior.AllowGet);
                }
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Helpers.LogError("Sponsors QrCode:", ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }


        }
        private string GenerateSponsorQrCode(int id, string url, enQRCodeFor type, int qrId)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(string.Format("{2}:{0},URL:{1},QRId:{3}", id, url, type.ToString(), qrId), QRCodeGenerator.ECCLevel.Q);
            QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            var fileName = Guid.NewGuid().ToString() + ".png";
            qrCodeImage.Save(Path.Combine(ImageSavePath, fileName));
            return fileName;
        }
        /// <summary>
        /// Delete existing sponsors
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> DeleteSponser(int id)
        {

            Entities db = new Entities();
            Sponsor Appsponsor = await db.Sponsors.FindAsync(id);
            //var Sponsrec = db.SponsorsEvents.Where(x => x.SponsorID == Appsponsor.ID).ToList();
            //var qr = db.QRCodes.Where(x => x.SponsorID == Appsponsor.ID).ToList();

            //foreach (var code in qr)
            //{
            //    db.QRCodes.Remove(code);
            //    await db.SaveChangesAsync();
            //}
            //foreach (var item in Sponsrec)
            //{
            //    db.SponsorsEvents.Remove(item);
            //    await db.SaveChangesAsync();
            //}
            //db.Sponsors.Remove(Appsponsor);
            db.DeleteSponsor(id);
            await db.SaveChangesAsync();
            
            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Sponsor", "Delete", userId, $"The Sponsor with the name: {Appsponsor.Name} is deleted by {userName}.");


            return RedirectToAction("Index");
        }
        // GET: Sponsors/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sponsor sponsor = await sponsorsLogic.GetSponsorById(id.Value);
            if (sponsor == null)
            {
                return HttpNotFound();
            }
            return View(sponsor);
        }

        // POST: Sponsors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            //delete code here check for dependecies
            return RedirectToAction("Index");
        }

    }
}
