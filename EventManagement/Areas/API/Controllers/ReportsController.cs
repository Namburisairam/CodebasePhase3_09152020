using System.Threading.Tasks;
using System.Web;
using EventManagement.Base;
using EventManagement.BusinessLogic.Business;


namespace EventManagement.Areas.API.Controllers
{
    public class ReportsController : BaseAPIController
    {

        ReportsLogic reportsLogic = new ReportsLogic();
        EventLogic eventLogic = new EventLogic();
        public async Task Appdownloads(string Token)
        {
            await reportsLogic.Appdownloads(CurrentUserSession.AttendesID.Value);
        }

        //public void downloadImage()
        //{
        //    //eventLogic.DownloadFile("9255d1f8-3d63-4bd8-a845-c96fd512f6d2.jpg");
        //}
    }
}