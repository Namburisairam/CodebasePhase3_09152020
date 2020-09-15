using CommonHelpers;
using EventManagement.Base;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Classes;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EventManagement.Areas.API.Controllers
{
    public class LoginController : BaseAPIController
    {
        CommonLogic commonLogic = new CommonLogic();

        public ActionResult IsValidAppVersion(string appVersion, string platform)
        {
            bool isValidAppVersion = commonLogic.IsValidAppVersion(appVersion,platform);

            return Json(isValidAppVersion, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ApplicationCloseTime(string token)
        {
            int attendeeId = CurrentUserSession.Attende.ID;

            commonLogic.ApplicationCloseTime(attendeeId);
            return Json(true);
        }

        [HttpPost]
        public ActionResult ApplicationStartTime(string token)
        {
            int attendeeId = CurrentUserSession.Attende.ID;

            commonLogic.ApplicationStartTime(attendeeId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

    }
}