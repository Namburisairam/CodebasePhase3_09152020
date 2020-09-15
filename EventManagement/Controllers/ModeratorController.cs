using System.Threading.Tasks;
using System.Web.Mvc;
using EventManagement.BusinessLogic.Business;
using EventManagement.Filter;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true, UserRole = CommonHelpers.enUserRole.Moderator)]
    public class ModeratorController : BaseController
    {
        EventLogic eventLogic = new EventLogic();
        // GET: Events
        public async Task<ActionResult> Index()
        {
            return View(await eventLogic.GetAllEvents());
        }
        /// <summary>
        /// Get events data for mentioned event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Event(int id)
        {
            ViewBag.EventComments = await eventLogic.GetComments(id);
            var eventdt = await eventLogic.GetEventById(id);
            return View();
        }
        /// <summary>
        /// Approve comments 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JsonResult> ApproveComment(int id)
        {
            var result = await eventLogic.ApproveComment(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}