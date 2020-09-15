using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CommonHelpers;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Models;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using EventManagement.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using QRCoder;
using TransportERP.Base;

namespace EventManagement.Controllers
{

    [AuthFilter(IsWebCall = true, UserRole = CommonHelpers.enUserRole.EventManager)]
    public class EventManagerController : BaseController
    {
        Entities db = new Entities();
        EventLogic eventLogic = new EventLogic();
        AttendesLogic attendesLogic = new AttendesLogic();
        SponsorsLogic sponserlogic = new SponsorsLogic();
        ActivityLogic activitylogic = new ActivityLogic();

        // GET: Events
        public async Task<ActionResult> Index()
        {
            return View(await eventLogic.GetAllEvents(CurrentUserSession.AttendesID));
        }

        // GET: Events/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = await eventLogic.GetEventById(id.Value);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }
        /// <summary>
        /// To get all information specific to evet
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: Events/Create
        public async Task<ActionResult> Create(int? id)
        {
            if (id == null || id == 0)
                return RedirectToAction("Index");
            ViewBag.ManagerUserList = new SelectList(await new AuthLogic().GetAllUser(), "Id", "FirstName");
            var @event = await eventLogic.GetEventById(id ?? 0);

            var attendData = await attendesLogic.GetAttendes();
            List<Attende> attendee = new List<Attende>();
            attendee.AddRange(attendData.Where(x => x.AttendesEvents.Any(y => y.EventID == id)));
            attendee.AddRange(attendData.Where(x => !x.AttendesEvents.Any(y => y.EventID == id)));
            ViewBag.Attendes = attendee;

            ViewBag.Activities = (eventLogic.GetEeventActivities(id));
            ViewBag.EventComments = await eventLogic.GetComments(id);
            ViewBag.QrCodes = await eventLogic.GetEventQrCodes(id);
            var sponserData = await sponserlogic.GetSponsors();
            List<Sponsor> sponsor = new List<Sponsor>();
            sponsor.AddRange(sponserData.Where(x => x.SponsorsEvents.Any(y => y.EventID == id)));
            sponsor.AddRange(sponserData.Where(x => !x.SponsorsEvents.Any(y => y.EventID == id)));
            ViewBag.Sponsers = sponsor;

            ViewBag.Themes = (await eventLogic.GetEeventThemes(id));

            //ViewBag.Sponsers = await sponserlogic.GetSponsors();
            ///
            //for activities
            ViewBag.ActivityTypeid = await eventLogic.ActivitiesType();
            ViewBag.url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.ReturnImagePath());
            ///            
            //qrcode
            //floormap
            //photos
            //sponosors
            //eventThem
            return View(@event);
        }
        /// <summary>
        /// Create new activity in specific event
        /// </summary>
        /// <param name="activite"></param>
        /// <param name="thumbnail"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateActivity(Activite activite, HttpPostedFileBase thumbnail)
        {
            activite.CreateON = DateTime.Now;
            string fileName = string.Empty;
            if (thumbnail != null)
                fileName = Guid.NewGuid().ToString() + ".png";
            activite.Thumbnail = fileName;

            await eventLogic.AddUpdateActivity(activite, null, CurrentUserSession.AttendesID.Value);

            if (thumbnail != null)
                thumbnail.SaveAs(Path.Combine(ImageSavePath, fileName));
            return RedirectToAction("Create", "Events", new { Id = activite.EventID });
        }

        public async Task<ActionResult> CreateActivity(int? id)
        {
            var activity = await activitylogic.GetActivity(id);
            return View(activity);
        }

        public async Task<ActionResult> CreateTheme(Theme theme)
        {
            await eventLogic.AddUpdateTheme(theme);
            return RedirectToAction("Create", "Events", new { id = theme.EventID });
        }

        /// <summary>
        /// Updates theme data for specific event
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="SplashScreem"></param>
        /// <param name="AppBackground"></param>
        /// <param name="Button"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateTheme(Theme theme, HttpPostedFileBase SplashScreem, HttpPostedFileBase AppBackground, HttpPostedFileBase Button)
        {
            try
            {
                string splashScrFileName = string.Empty;
                string appBgFileName = string.Empty;
                string buttonFileName = string.Empty;

                if (SplashScreem != null)
                {
                    splashScrFileName = Guid.NewGuid().ToString() + ".png";
                }
                if (Button != null)
                {
                    appBgFileName = Guid.NewGuid().ToString() + ".png";
                    buttonFileName = Guid.NewGuid().ToString() + ".png";
                }
                theme.SplashScreemURL = splashScrFileName;
                theme.AppBackgroundURL = appBgFileName;
                theme.ButtonURL = buttonFileName;
                await eventLogic.AddUpdateTheme(theme);

                if (SplashScreem != null)
                {
                    SplashScreem.SaveAs(Path.Combine(ImageSavePath, splashScrFileName));
                }
                if (AppBackground != null)
                {
                    AppBackground.SaveAs(Path.Combine(ImageSavePath, appBgFileName));
                }
                if (Button != null)
                {
                    Button.SaveAs(Path.Combine(ImageSavePath, buttonFileName));
                }
                return RedirectToAction("Create", "Events", new { id = theme.EventID });
            }
            catch (Exception ex)
            {
                Helpers.LogError("Theme create error", ex);
                //EventLogic eventLogic = new EventLogic();
                //ViewBag.Events = await eventLogic.GetAllEvents();
                ViewBag.Error = Literals.ErrorMessage;
                return View(theme);
            }
        }

        /// <summary>
        /// Approve comments based on id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JsonResult> ApproveComment(int id)
        {
            var result = await eventLogic.ApproveComment(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add attendees to specific event or remove attendees from specific event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="attendeeId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddRemoveAttendee(int eventId, int attendeeId)
        {
            try
            {
                await eventLogic.AddRemoveAttendeEvent(eventId, attendeeId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Helpers.LogError("attende remove ", ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Add or remove Sponsors from specific event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="sponsorId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddRemoveSponsor(int eventId, int sponsorId)
        {
            try
            {
                await eventLogic.AddRemoveSponsorEvent(eventId, sponsorId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Helpers.LogError("sponsor remove ", ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Add or remove exhibitor from specific event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exhibitorId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddRemoveExhibitor(int eventId, int exhibitorId)
        {
            try
            {
                await eventLogic.AddRemoveExhibitorEvent(eventId, exhibitorId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Helpers.LogError("sponsor remove ", ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GenerateQrCode(int id, enQRCodeFor type, string url)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(string.Format("Event:{0},URL:{1}", id, url), QRCodeGenerator.ECCLevel.Q);
            QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            var fileName = Guid.NewGuid().ToString() + ".png";
            //if (enQRCodeFor.Event == type)
            //    await eventLogic.EventQrCode(id, fileName, CurrentUserSession.UserID ?? 0);
            //else
            //    await eventLogic.ActivityQrCode(id, fileName, CurrentUserSession.UserID ?? 0);

            qrCodeImage.Save(Path.Combine(ImageSavePath, fileName));
            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> EventPhotosPartialView(int id)
        {
            var eventPhotos = await eventLogic.EventPhotos(id);
            return PartialView("_EventPhotosPartialView", eventPhotos);
        }
        //public async Task<ActionResult> SaveEventImages(int Id)
        //{
        //    try
        //    {
        //        if (Request.Files.Count > 0)
        //        {
        //            string pathrefer = Request.UrlReferrer.ToString();
        //            string Serverpath = ImageSavePath;
        //            var postedFile = Request.Files[0];
        //            string file;
        //            //In case of IE
        //            if (Request.Browser.Browser.ToUpper() == "IE")
        //            {
        //                string[] files = postedFile.FileName.Split(new char[] { '\\' });
        //                file = files[files.Length - 1];
        //            }
        //            else // In case of other browsers
        //            {
        //                file = postedFile.FileName;
        //            }
        //            string ext = Path.GetExtension(ImageSavePath + "\\" + file);
        //            file = Guid.NewGuid() + ext; // Creating a unique name for the file 
        //            await eventLogic.SaveEventImages(Id, file, CurrentUserSession.UserID ?? 0);
        //            postedFile.SaveAs(Path.Combine(ImageSavePath, file));
        //        }
        //        else
        //        {
        //            throw new Exception("No file to upload");
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        Helpers.LogError("Event Image upload failed", exp);
        //        throw new Exception(exp.Message);
        //    }
        //    return Json(true, JsonRequestBehavior.AllowGet);
        //}

        // POST: Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Saves images posted in an event
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="AlbumID"></param>
        /// <param name="AlbumDescription"></param>
        /// <returns></returns>
        public async Task<ActionResult> SaveEventImages(int Id, int? AlbumID, string AlbumDescription)
        {
            try
            {
                if (Request.Files.Count > 0)
                {
                    //string pathrefer = Request.UrlReferrer.ToString();
                    //string Serverpath = ImageSavePath;
                    var postedFile = Request.Files[0];
                    string file;
                    //In case of IE
                    if (Request.Browser.Browser.ToUpper() == "IE")
                    {
                        string[] files = postedFile.FileName.Split(new char[] { '\\' });
                        file = files[files.Length - 1];
                    }
                    else // In case of other browsers
                    {
                        file = postedFile.FileName;
                    }
                    string ext = Path.GetExtension(ImageSavePath + "\\" + file);
                    file = Guid.NewGuid() + ext; // Creating a unique name for the file 
                    await eventLogic.SaveEventImagesFromWeb(Id, file, CurrentUserSession.AttendesID ?? 0, AlbumID, AlbumDescription);

                    postedFile.SaveAs(Path.Combine(ImageSavePath, file));
                    string jsonkeysfilepath = HttpRuntime.AppDomainAppPath + @"\" + GoogleDriveSync.jsonkeysfile;
                    string ename = eventLogic.GetEventName(Id);
                    GoogleDriveSync.CallSaveApi(jsonkeysfilepath, ename, Path.Combine(ImageSavePath, file));
                }
                else
                {
                    throw new Exception("No file to upload");
                }
            }
            catch (Exception exp)
            {
                Helpers.LogError("Event Image upload failed", exp);
                throw new Exception(exp.Message);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Create new event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Event @event)
        {
            try
            {
                if (@event.ID == 0)
                    return RedirectToAction("Index");
                var id = await eventLogic.AddUpdateEvent1(@event);
                return RedirectToAction("Create", "Events", new { Id = id });
            }
            catch (Exception ex)
            {
                Helpers.LogError("Event Error", ex);
                ViewBag.error = Literals.ErrorMessage;
                return View(@event);
            }
        }


        public async Task<ActionResult> EventsExcelFile(HttpPostedFileBase file)
        {
            List<EventExcelDto> eventData = new List<EventExcelDto>();
            List<EventActivitiesDTO> eventActivitiesData = new List<EventActivitiesDTO>();
            if (file != null)
            {
                if (Path.GetExtension(file.FileName) == ".xlsx")
                {
                    Stream fs = file.InputStream;
                    //ExcelPackage package = new ExcelPackage(fs);
                    using (var package = new ExcelPackage(fs))
                    {
                        ExcelWorksheet workSheet = package.Workbook.Worksheets[1];
                        var start = workSheet.Dimension.Start;
                        var end = workSheet.Dimension.End;
                        for (int row = start.Row + 1; row <= end.Row; row++)
                        { // Row by row...
                            string name = workSheet.Cells[row, 1].Text;
                            string galacticId = workSheet.Cells[row, 2].Text;
                            eventData.Add(new EventExcelDto
                            {
                                GalacticId = galacticId.ToSafeInteger(),
                                Name = name,
                            });
                        }
                    }
                    using (var package = new ExcelPackage(fs))
                    {
                        ExcelWorksheet workSheet = package.Workbook.Worksheets[2];
                        var start = workSheet.Dimension.Start;
                        var end = workSheet.Dimension.End;
                        for (int row = start.Row + 1; row <= end.Row; row++)
                        { // Row by row...

                            string galacticId = workSheet.Cells[row, 1].Text;
                            string Address = workSheet.Cells[row, 2].Text;
                            string Description = workSheet.Cells[row, 3].Text;
                            string StartTime = workSheet.Cells[row, 4].Text;
                            string EndTime = workSheet.Cells[row, 5].Text;
                            eventActivitiesData.Add(new EventActivitiesDTO
                            {
                                GalacticEventId = galacticId.ToSafeInteger(),
                                Address = Address,
                                Description = Description,
                                StartTime = StartTime.ToSafeDate(),
                                EndTime = EndTime.ToSafeDate()
                            });
                        }
                    }
                    await eventLogic.SaveEventExcelFileData(eventData, eventActivitiesData, CurrentUserSession.AttendesID.Value);
                }
                return RedirectToAction("Index");
            }

            //save of event work comes after this.
            return RedirectToAction("Index");
        }
        // GET: Events/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = await eventLogic.GetEventById(id ?? 0);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            //need to confirm if event need to be dleted or closed or disabled

            //Event @event = await db.Events.FindAsync(id);
            //db.Events.Remove(@event);
            //await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Save floormapdetails of specific event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="PhotoWidth"></param>
        /// <param name="PhotoHeight"></param>
        /// <param name="FloorName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<ActionResult> SaveFloorMap(int eventId, int PhotoWidth, int PhotoHeight, string FloorName, HttpPostedFileBase file)
        {
            string fileName = string.Empty;
            if (file != null)
                fileName = Guid.NewGuid().ToString() + ".png";

            await eventLogic.SaveFloorMap(eventId, fileName, PhotoWidth, PhotoHeight, FloorName);

            if (file != null)
                file.SaveAs(Path.Combine(ImageSavePath, fileName));
            return RedirectToAction("Create", new { Id = eventId });
        }
        public async Task<ActionResult> CreateQrCode(FormCollection form)
        {
            if (form.Count > 0)
            {
                var eventId = form["EventID"].ToSafeInteger();

                var description = form["Description"];
                var qrCode = await eventLogic.EventQrCode(eventId, string.Empty, CurrentUserSession.AttendesID.Value, description, form["WebUrl"]);
                var qrCodeImage = GenerateEventQrCode(eventId, form["WebUrl"], enQRCodeFor.Event, qrCode);
                await eventLogic.UpdateEventQrCode(qrCode, qrCodeImage);
                return RedirectToAction("Create", new { Id = eventId });
            }
            return RedirectToAction("Index");
        }
        private string GenerateEventQrCode(int id, string url, enQRCodeFor type, int qrId)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(string.Format("{2}:{0},URL:{1},QRId:{3}", id, url, type.ToString(), qrId), QRCodeGenerator.ECCLevel.Q);
            QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            var fileName = Guid.NewGuid().ToString() + ".png";
            qrCodeImage.Save(Path.Combine(ImageSavePath, fileName));
            return fileName;
        }
        /// <summary>
        /// Delete existing theme data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> DeleteTheme(int id)
        {
            var eventid = await eventLogic.DeleteThee(id);
            return RedirectToAction("Create", new { id = eventid });
        }
        /// <summary>
        /// Delete existing photos
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        public JsonResult DeleteEventPhotos(Int32? photoId)
        {
            try
            {
                if (photoId.HasValue)
                {
                    eventLogic.DeleteEventPhotos(photoId.Value);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }

            catch (Exception ex)
            {
                Helpers.LogError("photo remove ", ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Generate Qrcode for given activity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> ActivityQrCode(int id)
        {
            try
            {

                var codeid = await eventLogic.ActivityQrCode(id, string.Empty, CurrentUserSession.AttendesID.Value);
                var qCode = GenerateEventQrCode(id, string.Empty, enQRCodeFor.Activity, codeid);
                await eventLogic.UpdateActivityQrCode(codeid, qCode);
                return Json(qCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Helpers.LogError("Activity QrCode:", ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> FloorMapComment(FloorRegionMapping file)
        {
            await eventLogic.SaveFloorDescription(file);
            return RedirectToAction("Create", new { id = file.EventID });
        }
        public async Task<ActionResult> DeleteFloorMapComment(int id)
        {
            var eid = await eventLogic.FloorMapDeleteComment(id);
            return RedirectToAction("Create", new { id = eid });
        }
        //public async Task<ActionResult> FloorMapComments(int id)
        //{
        //    var data = await eventLogic.GetFloorMapComments(id);
        //    return PartialView("_FloorMapComments", data);
        //}
        FloorMapResponse fmr = new FloorMapResponse();
        /// <summary>
        /// get floormapdetails for specific ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> FloorMapDetails(int id)
        {
            var floormap = await db.FloorMappings.FindAsync(id);
            var region = db.FloorRegionMappings.Where(x => x.FMid == floormap.ID).ToList();
            fmr.canvas.fmid = floormap.ID;
            fmr.canvas.src = floormap.PhotoURL;
            fmr.canvas.width = floormap.PhotoWidth.ToString();
            fmr.canvas.height = floormap.PhotoWidth.ToString();
            foreach (var item in region)
            {
                fmr.Pin.Add(new FloorMapResponse.FloorMapPin
                {
                    content = db.FloorMapLocations.Find(item.floorLocationID)?.Location,
                    coords = new FloorMapResponse.Coords { lat = item.X.ToString(), @long = item.Y.ToString() }
                });
            }

            ViewBag.jstring = ConvertListbasedJsonToNodebased(fmr);
            var floorMapEvent = db.FloorMappings.FirstOrDefault(x => x.ID == id);
            ViewBag.FloorLocations = floorMapEvent.Event.FloorMapLocations;
            return View(fmr);
        }

        private const string canvaskey = "canvas";
        private const string parentjsonkey = "example_image1";
        private string ConvertListbasedJsonToNodebased(FloorMapResponse _flormap)
        {
            string _jsonstring = string.Empty;
            Dictionary<string, Object> respodata = new Dictionary<string, object>
            {
                { canvaskey, _flormap.canvas }
            };
            for (int i = 0; i < _flormap.Pin.Count; i++)
            {
                respodata.Add(i.ToString(), _flormap.Pin[i]);
            }
            Dictionary<string, Dictionary<string, Object>> respo = new Dictionary<string, Dictionary<string, Object>>
            {
                { parentjsonkey, respodata }
            };
            _jsonstring = JsonConvert.SerializeObject(respo);
            return _jsonstring;
        }
        /// <summary>
        /// Create an album at event level
        /// </summary>
        /// <param name="albumName"></param>
        /// <param name="eventID"></param>
        /// <param name="AlbumDescription"></param>
        /// <returns></returns>
        public ActionResult CreateAlbum(string albumName, int eventID, string AlbumDescription)
        {
            db.PhotoAlbums.Add(new PhotoAlbum()
            {
                AlbumName = albumName,
                eventID = eventID,
                description = AlbumDescription
            });
            db.SaveChanges();
            return Redirect(Url.Action("Create", "EventManager", new { Id = eventID }) + "#Photos");
        }
        /// <summary>
        /// Delete existing albums
        /// </summary>
        /// <param name="albumID"></param>
        /// <returns></returns>
        public ActionResult DeleteAlbum(int albumID)
        {
            db.PhotoAlbums.Remove(db.PhotoAlbums.FirstOrDefault(x => x.ID == albumID));
            db.SaveChanges();
            return Json(true);
        }
        /// <summary>
        /// Add bookmarks to specific activites for given attendee
        /// </summary>
        /// <param name="attendeeID"></param>
        /// <param name="activities"></param>
        /// <returns></returns>
        public ActionResult AddBookMark(int attendeeID, List<int> activities)
        {

            if (activities == null)
            {
                activities = new List<int>();
            }
            var deletedActivities = db.BookMarks.Where(x => x.AttendesID == attendeeID && !activities.Any(y => y == x.ActivityID));
            db.BookMarks.RemoveRange(deletedActivities);

            foreach (var activityID in activities)
            {
                if (!db.BookMarks.Any(x => x.ActivityID == activityID && x.AttendesID == attendeeID))
                {
                    db.BookMarks.Add(
                        new BookMark()
                        {
                            AttendesID = attendeeID,
                            ActivityID = activityID
                        });
                }
            }

            db.SaveChanges();

            return Json(true);
        }
    }
}