using System;
using System.Collections.Generic;
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
    public class ExhibitorsController : BaseController
    {
        Exhibitors exhibitorLogic = new Exhibitors();
        EventLogic eventlogic = new EventLogic();
        // GET: Exhibitors
        public async Task<ActionResult> Index()
        {
            return View(await exhibitorLogic.GetExhibitors());
        }

        // GET: Exhibitors/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exhibitor exhibitor = await exhibitorLogic.GetExhibitorsById(id.Value);
            if (exhibitor == null)
            {
                return HttpNotFound();
            }
            return View(exhibitor);
        }
        /// <summary>
        /// get all exhibitors mapped to specific event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        // GET: Exhibitors/Create
        public async Task<ActionResult> Create(int? id)
        {
            var exhibitor = await exhibitorLogic.GetExhibitorsById(id);
            //EventLogic eventLogic = new EventLogic();
            ViewBag.Events = await eventlogic.GetAllEvents();
           // Entities Db = new Entities();
            //ViewBag.Userpackage = await Db.UserPackageTypes.ToListAsync();
            return View(exhibitor);
        }

        /// <summary>
        /// Add new exhibitors for event
        /// </summary>
        /// <param name="exhibitor"></param>
        /// <param name="events"></param>
        /// <param name="thumbnail"></param>
        /// <returns></returns>
        // POST: Exhibitors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(Exhibitor exhibitor, List<int> events, HttpPostedFileBase thumbnail)
        {
            try
            {
                // TODO: Add insert logic here
                string fileName = string.Empty;
                if (thumbnail != null)
                    fileName = Guid.NewGuid().ToString() + ".png";
                exhibitor.Thumbnail = fileName;
                await exhibitorLogic.AddUpdateExhibitors(exhibitor, events);
                if (thumbnail != null)
                    thumbnail.SaveAs(Path.Combine(ImageSavePath, fileName));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Helpers.LogError("Exhibitor Error", ex);
                //EventLogic eventLogic = new EventLogic();
                ViewBag.Events = await eventlogic.GetAllEvents();
                ViewBag.Error = Literals.ErrorMessage;
                return View(exhibitor);
            }
        }


        // GET: Exhibitors/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Exhibitors/Delete/5
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
        /// Delete existing exhibitors data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> DeleteExhibitor(int id)
        {
            Entities db = new Entities();
            //Exhibitor exhibitor = await db.Exhibitors.FindAsync(id);
            //var exhbrec = db.ExhibitorsEvents.Where(x => x.ExhibitorsID == exhibitor.ID).ToList();
            //foreach (var item in exhbrec)
            //{
            //    db.ExhibitorsEvents.Remove(item);
            //    await db.SaveChangesAsync();
            //}
            //db.Exhibitors.Remove(exhibitor);
            db.DeleteExhibitor(id);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<string> CreateExhibitorQrCode(int exhibitorID)
        {
            QRCodeLogic qRCodeLogic = new QRCodeLogic();
            var exhibitor = await exhibitorLogic.GetExhibitorsById(exhibitorID);
            return qRCodeLogic.GenerateExhibitorQRCode(ImageSavePath, exhibitorID, CurrentUserSession.AttendesID.Value, exhibitor.WebsiteURL);
        }
    }
}
