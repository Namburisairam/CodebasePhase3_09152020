using System.Linq;
using System.Web.Mvc;
using EventManagement.Base;
using EventManagement.BusinessLogic.Business;
using EventManagement.Filter;

namespace EventManagement.Areas.API.Controllers
{
    [AuthFilter]
    public class EventVideoController : BaseAPIController
    {
        public ActionResult GetEventVideoByID(string Token, int eventID)
        {
            EventVideoManager eventVideoManager = new EventVideoManager();
            return Json(eventVideoManager.GetEventVideos(eventID).Select(x => new
            {
                x.ID,
                x.VideoDescription,
                x.VideoPath
            }), JsonRequestBehavior.AllowGet);
        }
    }
}