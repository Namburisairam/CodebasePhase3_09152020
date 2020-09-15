using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Models;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class WidgetImageController : BaseController
    {
        WidgetImageService widgetImageService = new WidgetImageService();
        // GET: WidgetImage
        public ActionResult Index(int eventId)
        {
            var db = new Entities();
            IEnumerable<WidgetMapping> imageWidgets = widgetImageService.GetImageWidgets(eventId).ToList();
            var @event = db.Events.Find(eventId);
            ViewBag.widgetImage = @event.WidgetImage;
            ViewBag.zoomPercentage = @event.WidgetImageBoxesZoomPercentage ?? 100;
            ViewBag.offsetX = @event.WidgetImageBoxesOffsetX ?? 0;
            ViewBag.offsetY = @event.WidgetImageBoxesOffsetY ?? 0;
            return View(imageWidgets);
        }

        [HttpPost]
        public void CropWidgetImages(WidgetBox[] rectangles, int eventId, float zoomPercentage, float boxOffsetX, float boxOffsetY)
        {
            widgetImageService.CropWidgetImages(rectangles, eventId, ImageSavePath, zoomPercentage, boxOffsetX, boxOffsetY);
            //return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadWidgetImage(int eventID, HttpPostedFileBase file)
        {
            string fileName = string.Empty;
            if (file != null)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            }
            else
            {
                throw new Exception("Image is required");
            }
            widgetImageService.UploadEventWidget(eventID, fileName);
            file.SaveAs(Path.Combine(ImageSavePath, fileName));
            return RedirectToAction("Index", new { eventId = eventID });
        }
    }

}