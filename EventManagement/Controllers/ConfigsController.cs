using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using CommonHelpers;
using EventManagement.BusinessLogic.Business;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class ConfigsController : BaseController
    {
        Configs logic = new Configs();
        Entities db = new Entities();
        CommonLogic commonLogic = new CommonLogic();

        // GET: Configs
        public async Task<ActionResult> Index()
        {
            var record = await logic.GetAllConfig();
            return View(record);
        }

        public async Task<ActionResult> Details(int? id)
        {

            return View();
        }

        //public ActionResult Create()
        //{
        //    return View();
        //}
        public async Task<ActionResult> Create(int? id)
        {
            var rec = await logic.GetConfigByID(id);

            return View(rec);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Config config)
        {
            try
            {
                int userId = CurrentUserSession.Attende.ID;
                string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
                if (config.ID == 0)
                {
                    commonLogic.SaveLog("Support Mail", "Insert", userId, $"The config support mail : {config.SupportEmail} is created by {userName}.");
                }
                else
                {
                    commonLogic.SaveLog("Support Mail", "Update", userId, $"The config support mail : {config.SupportEmail} is updated by {userName}.");
                }

                await logic.AddUpdateConfig(config);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Helpers.LogError("Configuration Error", ex);
                ViewBag.error = Literals.ErrorMessage;
                return View(config);
            }

        }

        // GET: Configs/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            return View();
        }

        // POST: Configs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeleteConfig(int id)
        {
            Config config = await db.Configs.FindAsync(id);
            db.Configs.Remove(config);

            int userId = CurrentUserSession.Attende.ID;
            commonLogic.SaveLog("Support Mail", "Delete", userId, $"The config support mail : {config.SupportEmail} is deleted by {userId}.");

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}
