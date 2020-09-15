using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EventManagement.BusinessLogic.Business;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class PackageTypeController : BaseController
    {
        private Entities db = new Entities();
        CommonLogic commonLogic = new CommonLogic();

        // GET: PackageType
        public async Task<ActionResult> Index()
        {
            return View(await db.UserPackageTypes.ToListAsync());
        }

        // GET: PackageType/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserPackageType userPackageType = await db.UserPackageTypes.FindAsync(id);
            if (userPackageType == null)
            {
                return HttpNotFound();
            }
            return View(userPackageType);
        }

        // GET: PackageType/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PackageType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Create new package
        /// </summary>
        /// <param name="userPackageType"></param>
        /// <param name="iconimage"></param>
        /// <returns></returns>
        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserPackageType userPackageType, HttpPostedFileBase iconimage)
        {
            if (ModelState.IsValid)
            {
                string fileName = string.Empty;
                if (iconimage != null)
                    fileName = Guid.NewGuid().ToString() + ".png";
                userPackageType.iconimage = fileName;

                if (iconimage != null)
                    iconimage.SaveAs(Path.Combine(ImageSavePath, fileName));
                db.UserPackageTypes.Add(userPackageType);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(userPackageType);
        }
        /// <summary>
        /// Edit existing package
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: PackageType/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserPackageType userPackageType = await db.UserPackageTypes.FindAsync(id);
            if (userPackageType == null)
            {
                return HttpNotFound();
            }
            return View(userPackageType);
        }

        // POST: PackageType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit(UserPackageType userPackageType, HttpPostedFileBase thumbnail)
        {
            if (ModelState.IsValid)
            {
                string fileName = string.Empty;
                if (thumbnail != null)
                    fileName = Guid.NewGuid().ToString() + ".png";
                userPackageType.iconimage = fileName;

                if (thumbnail != null)
                    thumbnail.SaveAs(Path.Combine(ImageSavePath, fileName));
                db.Entry(userPackageType).State = EntityState.Modified;
                await db.SaveChangesAsync();

                int userId = CurrentUserSession.Attende.ID;
                string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
                commonLogic.SaveLog("PackageType", "Update", userId, $"The user package with the ID : {userPackageType.ID} is updated by {userName}.");

                return RedirectToAction("Index");
            }
            return View(userPackageType);
        }

        // GET: PackageType/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserPackageType userPackageType = await db.UserPackageTypes.FindAsync(id);
            if (userPackageType == null)
            {
                return HttpNotFound();
            }
            return View(userPackageType);
        }
        /// <summary>
        /// Delete package
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // POST: PackageType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UserPackageType userPackageType = await db.UserPackageTypes.FindAsync(id);
            db.UserPackageTypes.Remove(userPackageType);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeletePackage(int id)
        {
            UserPackageType userPackageType = await db.UserPackageTypes.FindAsync(id);
            var sponsorpackages = db.SponsorsEvents.Where(x => x.packagetype == id).ToList();
            //db.Sponsors.Where(x => x.Packagetype == id).ToList();
            foreach (var x in sponsorpackages)
            {
                x.packagetype = null;
                await db.SaveChangesAsync();
            }
            var exhibitorpackages = db.ExhibitorsEvents.Where(x => x.packagetype == id).ToList();
            foreach (var x in exhibitorpackages)
            {
                x.packagetype = null;
                await db.SaveChangesAsync();
            }
            var Vendorpackages = db.VendorsEvents.Where(x => x.packagetype == id).ToList();
            foreach (var x in Vendorpackages)
            {
                x.packagetype = null;
                await db.SaveChangesAsync();
            }
            db.UserPackageTypes.Remove(userPackageType);
            await db.SaveChangesAsync();

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("PackageType", "Delete", userId, $"The package with the name : {userPackageType.Name} is deleted by {userName}.");

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
