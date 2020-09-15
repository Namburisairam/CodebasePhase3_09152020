using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.BusinessLogic.Models;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Business
{
    public class WidgetImageService : BaseLogic
    {
        public IEnumerable<WidgetMapping> GetImageWidgets(int eventID)
        {
            return Db.WidgetMappings.Where(x => x.EventID == eventID && x.widgetTypeID == 1).OrderBy(x => x.YLocation).ThenBy(x => x.XLocation);
        }

        public void CropWidgetImages(WidgetBox[] rectangles, int eventID, string ImageSavePath, float zoomPercentage, float boxOffsetX, float boxOffsetY)
        {

            Db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
            var @event = Db.Events.FirstOrDefault(x => x.ID == eventID);

            foreach (var rect in rectangles)
            {
                WidgetMapping widgetMapping = Db.WidgetMappings.Find(rect.BoxID);
                Bitmap bmpImage = new Bitmap(Path.Combine(ImageSavePath, @event.WidgetImage));  // @"D:\MobileMetrics\Galactic\Event Application\CMS App\Dev\UI\EventManagementPortal-master\EventManagement\img\john-baker-anwxFFJ7tHw-unsplash (1).jpg"
                var croppedImage = bmpImage.Clone(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height), bmpImage.PixelFormat);
                var guid = Guid.NewGuid();
                var fileName = guid + ".png";
                croppedImage.Save(Path.Combine(ImageSavePath, fileName)); //$@"D:\CropedImages\{guid}.jpg");
                bmpImage.Dispose();
                croppedImage.Dispose();
                widgetMapping.ImagePath = fileName;

                // need to remove them from DB

                //widgetMapping.WidgetImageX = rect.x;
                //widgetMapping.WidgetImageY = rect.y;
                //widgetMapping.WidgetImageWidth = rect.width;
                //widgetMapping.WidgetImageHeight = rect.height;
            }
            @event.WidgetImageBoxesZoomPercentage = zoomPercentage;
            @event.WidgetImageBoxesOffsetX = boxOffsetX;
            @event.WidgetImageBoxesOffsetY = boxOffsetY;
            Db.SaveChanges();
        }

        public void resetWidgetImageCoordinates(int eventID)
        {
            var @event = Db.Events.Find(eventID);
            @event.WidgetImageBoxesZoomPercentage = 100;
            @event.WidgetImageBoxesOffsetX = 0;
            @event.WidgetImageBoxesOffsetY = 0;
            Db.SaveChanges();
        }

        public void UploadEventWidget(int eventID, string fileName)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction())
            {
                try
                {
                    var @event = Db.Events.Find(eventID);
                    @event.WidgetImage = fileName;
                    resetWidgetImageCoordinates(eventID);
                    Db.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
        }
    }
}
