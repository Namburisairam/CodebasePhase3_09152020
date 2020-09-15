using EventManagement.Base;
using EventManagement.BusinessLogic.Business;
using EventManagement.Filter;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EventManagement.Areas.API.Controllers
{
    [AuthFilter]
    public class QRCodesController : BaseAPIController
    {
        // GET: API/QRCodes
        [HttpGet]
        public async Task<ActionResult> QRCodeScanEntry(string Token, int QRCodeId)
        {
            QRCodeLogic businessLogic = new QRCodeLogic();
            var response = await businessLogic.RecordQRCodeScanHistory(CurrentUserSession.AttendesID.Value, QRCodeId);
            return Json(new { data = response }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> QrCodeScanHistory(string Token)
        {
            QRCodeLogic businessLogic = new QRCodeLogic();
            var data = await businessLogic.GetAttendeScanHistory(CurrentUserSession.AttendesID.Value);
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
    }
}