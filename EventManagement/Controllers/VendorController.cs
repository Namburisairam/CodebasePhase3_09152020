using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
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
    public class VendorController : BaseController
    {
        VendorsLogic VendorLogic = new VendorsLogic();
        EventLogic eventlogic = new EventLogic();
        CommonLogic commonLogic = new CommonLogic();

        // GET: Vendor
        public async Task<ActionResult> Index()
        {
            return View(await VendorLogic.GetVendor());
        }

        // GET: Vendor/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendor sponsor = await VendorLogic.GetVendorById(id.Value);
            if (sponsor == null)
            {
                return HttpNotFound();
            }
            return View(sponsor);
        }

        /// <summary>
        /// Get all vendors 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: Vendor/Create
        public async Task<ActionResult> Create(int? id)
        {
            var vendor = await VendorLogic.GetVendorById(id);
           // EventLogic eventLogic = new EventLogic();
            ViewBag.Events = await eventlogic.GetAllEvents();
            //Entities Db = new Entities();
            //ViewBag.Userpackage = await Db.UserPackageTypes.ToListAsync();
            return View(vendor);
        }

        /// <summary>
        /// Create new vendor and map to specific events
        /// </summary>
        /// <param name="vendor"></param>
        /// <param name="events"></param>
        /// <param name="thumbnail"></param>
        /// <returns></returns>
        // POST: Vendor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(Vendor vendor, List<int> events, HttpPostedFileBase thumbnail)
        {
            try
            {//string docName = "";
                string fileName = string.Empty;
                if (string.IsNullOrEmpty(vendor.Name))
                {
                    ModelState.AddModelError("Name", "Name is Required");
                    //EventLogic eventLogic = new EventLogic();
                    ViewBag.Events = await eventlogic.GetAllEvents();
                    Entities Db = new Entities();
                    ViewBag.Userpackage = await Db.UserPackageTypes.ToListAsync();
                    return View(vendor);
                }
                if (thumbnail != null)
                    fileName = Guid.NewGuid().ToString() + ".png";
                vendor.Thumbnail = fileName;
                await VendorLogic.AddUpdateVendor(vendor, events);
                if (thumbnail != null)
                    thumbnail.SaveAs(Path.Combine(ImageSavePath, fileName));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Helpers.LogError("Vendor Error", ex);
                //EventLogic eventLogic = new EventLogic();
                ViewBag.Events = await eventlogic.GetAllEvents();
                ViewBag.Error = Literals.ErrorMessage;
                return View(vendor);
            }
        }

        // GET: Vendor/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Vendor/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        /// <summary>
        /// Delete existing vendor
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        
        public async Task<ActionResult> DeleteVendor(int id)
        {
            Entities db = new Entities();
            Vendor vendor = await db.Vendors.FindAsync(id);
            //var venrec = db.VendorsEvents.Where(x => x.VendorsID == vendor.ID).ToList();
            //foreach (var item in venrec)
            //{
            //    db.VendorsEvents.Remove(item);
            //    await db.SaveChangesAsync();
            //}
            //db.Vendors.Remove(vendor);
            db.DeleteVendor(id);
            await db.SaveChangesAsync();

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Vendor", "Delete", userId, $"The vendor with the name: {vendor.Name} is deleted by {userName}.");


            return RedirectToAction("Index");
        }

        public async Task<string> CreateVendorQrCode(int vendorID)
        {
            QRCodeLogic qRCodeLogic = new QRCodeLogic();
            var vendor = await VendorLogic.GetVendorById(vendorID);
            return qRCodeLogic.GenerateVendorQRCode(ImageSavePath, vendorID, CurrentUserSession.AttendesID.Value, vendor.WebsiteURL);
        }
    }
}
