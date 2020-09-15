using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Mvc;
using EventManagement.Filter;
using EventManagement.Models;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class HomeController : BaseController
    {
        public ActionResult download()
        {


            return View();
        }
        public ActionResult DownloadsF(int id)
        {
            FileDownlod obj = new FileDownlod();
            //////int CurrentFileID = Convert.ToInt32(FileID); 
            DataAccess.DataBase.Model.Entities db = new DataAccess.DataBase.Model.Entities();
            var photo = db.Photos.Where(x => x.EventID == id).ToList();
            var filesCol = obj.GetFile(photo).ToList();
            using (var memoryStream = new MemoryStream())
            {
                using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    for (int i = 0; i < filesCol.Count; i++)
                    {
                        ziparchive.CreateEntryFromFile(filesCol[i].FilePath, filesCol[i].FileName);
                    }
                }
                return File(memoryStream.ToArray(), "application/zip", "Attachments.zip");
            }
        }

        public ActionResult Index()
        {

            using (EventManagement.DataAccess.DataBase.Model.Entities obj = new EventManagement.DataAccess.DataBase.Model.Entities())
            {
                TempData["attendee_count"] = obj.Attendes.Count();
                TempData["event_count"] = obj.Events.Count();
                return View();
            }
            //return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
};