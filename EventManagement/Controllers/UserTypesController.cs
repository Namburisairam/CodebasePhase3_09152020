using System;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class UserTypesController : BaseController
    {
        private Entities db = new Entities();

        // GET: UserTypes
        public async Task<ActionResult> Index()
        {
            return View(await db.UserPackageTypes.ToListAsync());
        }

        // GET: UserTypes/Details/5
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

        // GET: UserTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Add new package type
        /// </summary>
        /// <param name="userPackageType"></param>
        /// <param name="thumbnail"></param>
        /// <returns></returns>
        // POST: UserTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,Name")] UserPackageType userPackageType, HttpPostedFileBase thumbnail)
        {
            if (ModelState.IsValid)
            {
                //string docName = "";
                string fileName = string.Empty;
                if (thumbnail != null)
                    fileName = Guid.NewGuid().ToString() + ".png";
                userPackageType.iconimage = fileName;

                if (thumbnail != null)
                    thumbnail.SaveAs(Path.Combine(ImageSavePath, fileName));
                db.UserPackageTypes.Add(userPackageType);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(userPackageType);
        }

        // GET: UserTypes/Edit/5
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

        // POST: UserTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Name")] UserPackageType userPackageType, HttpPostedFileBase thumbnail)
        {
            if (ModelState.IsValid)
            {
                //string docName = "";
                string fileName = string.Empty;
                if (thumbnail != null)
                    fileName = Guid.NewGuid().ToString() + ".png";
                userPackageType.iconimage = fileName;

                if (thumbnail != null)
                    thumbnail.SaveAs(Path.Combine(ImageSavePath, fileName));
                db.UserPackageTypes.Add(userPackageType);
                db.Entry(userPackageType).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(userPackageType);
        }

        // GET: UserTypes/Delete/5
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
        /// Delete existing packagetype
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // POST: UserTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UserPackageType userPackageType = await db.UserPackageTypes.FindAsync(id);
            db.UserPackageTypes.Remove(userPackageType);
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
