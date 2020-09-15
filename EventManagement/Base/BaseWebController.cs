using System.Web.Mvc;
using TransportERP.Base;

namespace EventManagement.Base
{
    public class BaseWebController : BaseController
    {
        // GET: BaseWeb
        public ActionResult Index()
        {
            return View();
        }
    }
}