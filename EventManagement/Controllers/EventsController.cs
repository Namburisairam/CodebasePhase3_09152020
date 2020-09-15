using CommonHelpers;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Classes;
using EventManagement.BusinessLogic.Models;
using EventManagement.BusinessLogic.Models.APIDTOOjbects;
using EventManagement.BusinessLogic.Models.Enums;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using EventManagement.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class EventsController : BaseController
    {
        Entities db = new Entities();
        EventLogic eventLogic = new EventLogic();
        AttendesLogic attendesLogic = new AttendesLogic();
        SponsorsLogic sponserlogic = new SponsorsLogic();
        ActivityLogic activitylogic = new ActivityLogic();
        //SocialForumLogic sc = new SocialForumLogic();
        //Speakers speakers = new Speakers();
        Exhibitors exhibitorsLogic = new Exhibitors();
        VendorsLogic vendorsLogic = new VendorsLogic();
        Notifications notifications = new Notifications();

        CommonLogic commonLogic = new CommonLogic();

        // GET: Events
        public async Task<ActionResult> Index()
        {
            //await eventLogic.GetScheduledActivityTokens();
            ViewBag.ManagerUserList = AuthLogic.GetManagers();
            //byte[] bytes = System.IO.File.ReadAllBytes(@"D:\Waqar.mp4");
            //System.IO.File.WriteAllBytes(@"D:\myfile.txt", bytes);
            //sc.MemoryPostedFile(@"D:\c.mp4");
            return View(await eventLogic.GetAllEvents());
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
            ViewBag.eventID = id ?? 0;
            var attendData = await attendesLogic.GetAttendes();
            List<Attende> attendee = new List<Attende>();
            attendee.AddRange(attendData.Where(x => x.AttendesEvents.Any(y => y.EventID == id)));
            //attendee.AddRange(attendData.Where(x => !x.AttendesEvents.Any(y => y.EventID == id)));
            ViewBag.Attendes = attendee;
            List<Theme> thi = new List<Theme>();
            var thidata = db.Themes.Where(x => x.EventID == id).ToList();
            thi.AddRange(thidata.Where(x => x.EventID == id));
            ViewBag.th = thi;
            ViewBag.a = (eventLogic.GetEeventAttendes(id));
            ViewBag.Activities = (eventLogic.GetEeventActivities(id));
            ViewBag.Groups = eventLogic.GetGroups(id.Value);
            ViewBag.EventComments = await eventLogic.PendingComments(id);
            ViewBag.EventAllComments = await eventLogic.GetComments(id);
            ViewBag.DeleteRequest = await eventLogic.GetRequest(id);
            ViewBag.QrCodes = await eventLogic.GetEventQrCodes(id);
            ViewBag.Documents = eventLogic.GetDocuments(id);
            ViewBag.Speakers = await attendesLogic.GetAttendeeForSpecificEvent(id.Value, checkAttendeeValidity: new SpeakerValidator());

            ViewBag.DocumentsTypes = Enum.GetValues(typeof(ScreenTypes)).Cast<ScreenTypes>().Select((x, idx) => new SelectListItem() { Text = x.ToString(), Value = ((int)x).ToString() }).ToList();

            ViewBag.PackageTypes = eventLogic.UserPackageTypes();

            var sponserData = await sponserlogic.GetSponsors();
            List<Sponsor> sponsor = new List<Sponsor>();
            sponsor.AddRange(sponserData.Where(x => x.SponsorsEvents.Any(y => y.EventID == id)));
            sponsor.AddRange(sponserData.Where(x => !x.SponsorsEvents.Any(y => y.EventID == id)));
            ViewBag.Sponsers = sponsor;

            var exhibitorsData = await exhibitorsLogic.GetExhibitors();
            List<Exhibitor> exhibitor = new List<Exhibitor>();
            exhibitor.AddRange(exhibitorsData.Where(x => x.ExhibitorsEvents.Any(y => y.EventID == id)));
            exhibitor.AddRange(exhibitorsData.Where(x => !x.ExhibitorsEvents.Any(y => y.EventID == id)));
            ViewBag.Exhibitors = exhibitor;

            var vendorsData = await vendorsLogic.GetVendor();
            List<Vendor> Vendors = new List<Vendor>();
            Vendors.AddRange(vendorsData.Where(x => x.VendorsEvents.Any(y => y.EventID == id)));
            Vendors.AddRange(vendorsData.Where(x => !x.VendorsEvents.Any(y => y.EventID == id)));
            ViewBag.Vendors = Vendors;

            var notifydata = await eventLogic.GetAllNotificationHistory(id);
            List<Notification> notify = new List<Notification>();
            notify.AddRange(notifydata.Where(x => x.EventID == id));
            // notify.AddRange(notifydata.Where(x => !x.EventID == id));
            ViewBag.notify = notify;
            //Event Photo
            var PhotoData = await eventLogic.GetAllEventPhoto(id);
            List<Photo> EventPhotos = new List<Photo>();
            EventPhotos.AddRange(PhotoData.Where(x => x.EventID == id));
            ViewBag.EPhotos = PhotoData;
            ViewBag.url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.ReturnImagePath());

            //Attendee Photo

            var activitydata = await activitylogic.GetActivities();
            List<Activite> Activity = new List<Activite>();
            Activity.AddRange(activitydata.Where(x => x.EventID == id));
            // notify.AddRange(notifydata.Where(x => !x.EventID == id));
            ViewBag.act = Activity;

            ViewBag.Themes = (await eventLogic.GetEeventThemes(id));

            //ViewBag.Sponsers = await sponserlogic.GetSponsors();
            ///
            //for activities
            ViewBag.ActivityTypeid = await eventLogic.ActivitiesType();
            ViewBag.Question = await eventLogic.GetEventQuestions(id);
            ViewBag.Widgets = @event.WidgetMappings.OrderBy(x => x.YLocation).Select(x => new
            {
                x.ID,
                x.WidgetID,
                WidgetName = x.Widget?.Name,
                x.Width,
                x.Height,
                x.XLocation,
                x.YLocation,
                x.DisplayName,
                x.ExternalLink,
                x.isExternalLink,
                x.ImagePath,
                x.widgetTypeID,
                x.FontSize,
                x.Color,
                x.Justification
            });
            ViewBag.timeZones = TimeZoneInfo.GetSystemTimeZones();
            //ViewBag.eventFormWaiverKeys = eventLogic.GetFormWaiverTemplateFieldMappings(id.Value).Select(x => new
            //{
            //    x.ID,
            //    x.FormKey,
            //    x.Value
            //});
            ViewBag.FormWaiverValues = db.WaiverFormMappingValues.Select(x => new { x.ID, x.Name });
            ViewBag.ActivityFormWaivers = db.Activites.Where(x => x.WaiverFormTemplatePath != null).Select(x => new ActivityWaiverDoc()
            {

                ID = x.ID,
                WaiverFormTemplatePath = x.WaiverFormTemplatePath,
                Name = x.Name

            }).ToList();

            ViewBag.Eventinformation = db.EventInformations.Where(x => x.EventID == @event.ID);
            ///            
            //qrcode
            //floormap
            //photos
            //sponosors
            //eventThem

            return View(@event);
        }

        public async Task<ActionResult> onDocumentsDropDownChange(ScreenTypes screenType, int eventID)
        {
            var dropdownResult = await eventLogic.getDocumentDropDownValuesByType(screenType, eventID);
            return Json(dropdownResult?.Select(x => new { id = x.Key, value = x.Value }).ToList(), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Upload new documents 
        /// </summary>
        /// <param name="screenType"></param>
        /// <param name="documentFile"></param>
        /// <param name="selectedID"></param>
        /// <param name="eventID"></param>
        /// <param name="documentName"></param>
        /// <returns></returns>
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> uploadDocuments(ScreenTypes screenType, HttpPostedFileBase documentFile, int? selectedID, int eventID, string documentName)
        {
            string fileName = "";
            if (documentFile != null)
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(documentFile.FileName);

            await this.eventLogic.uploadDocuments(screenType, selectedID, eventID, fileName, documentName);
            if (documentFile != null)
                documentFile.SaveAs(Path.Combine(DocumentsSavePath, fileName));

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Documents", "Insert", userId, $"The file with the name: {fileName} is uploaded for the event id: {eventID} by {userName}.");

            return Redirect(Url.Action("Create", new { Id = eventID }) + "#documents");
        }
        /// <summary>
        /// Download each document from list of documents
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        
        public async Task<ActionResult> DownloadDocument(int documentID)
        {
            var document = await this.eventLogic.getDocumentByID(documentID);
            string fullDocumentPath = Path.Combine(DocumentsSavePath, document.FilePath);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Documents", "Select", userId, $"The file with the name: {fullDocumentPath} is downloaded by {userName}.");

            return File(fullDocumentPath, System.Net.Mime.MediaTypeNames.Application.Octet, document.FilePath);
        }
        /// <summary>
        /// Add Event information
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <param name="aboutEventIconFile"></param>
        /// <param name="helpDeskIconFile"></param>
        /// <param name="accomodationsIconFile"></param>
        /// <param name="thingsToKnowIconFile"></param>
        /// <param name="arrivalInfoIconFile"></param>
        /// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        //public async Task<ActionResult> AddEventInfo(EventInfo eventInfo, HttpPostedFileBase aboutEventIconFile, HttpPostedFileBase helpDeskIconFile, HttpPostedFileBase accomodationsIconFile, HttpPostedFileBase thingsToKnowIconFile, HttpPostedFileBase arrivalInfoIconFile)
        //{
        //    //string aboutEventIconfileName = "";
        //    //if (aboutEventIconFile != null)
        //    //    aboutEventIconfileName = Guid.NewGuid().ToString() + Path.GetExtension(aboutEventIconFile.FileName);

        //    string aboutEventIconfileName = this.eventLogic.createGUIDFileNameFromFile(aboutEventIconFile);

        //    string helpDeskIconfileName = this.eventLogic.createGUIDFileNameFromFile(helpDeskIconFile);

        //    string accomodationsIconFileName = this.eventLogic.createGUIDFileNameFromFile(accomodationsIconFile);

        //    string thingsToKnowIconFileName = this.eventLogic.createGUIDFileNameFromFile(thingsToKnowIconFile);
        //    string arrivalInfoIconFileName = this.eventLogic.createGUIDFileNameFromFile(arrivalInfoIconFile);

        //    eventInfo.AboutEventIcon = aboutEventIconfileName;
        //    eventInfo.HelpDeskIcon = helpDeskIconfileName;
        //    eventInfo.AccomodationsIcon = accomodationsIconFileName;
        //    eventInfo.ThingsToKnowIcon = thingsToKnowIconFileName;
        //    eventInfo.ArrivalInfoIcon = arrivalInfoIconFileName;

        //    await this.eventLogic.AddEventInfo(eventInfo);

        //    if (aboutEventIconFile != null)
        //        aboutEventIconFile.SaveAs(Path.Combine(ImageSavePath, aboutEventIconfileName));

        //    if (helpDeskIconFile != null)
        //        helpDeskIconFile.SaveAs(Path.Combine(ImageSavePath, helpDeskIconfileName));

        //    if (accomodationsIconFile != null)
        //        accomodationsIconFile.SaveAs(Path.Combine(ImageSavePath, accomodationsIconFileName));

        //    if (thingsToKnowIconFile != null)
        //        thingsToKnowIconFile.SaveAs(Path.Combine(ImageSavePath, thingsToKnowIconFileName));

        //    if (arrivalInfoIconFile != null)
        //        arrivalInfoIconFile.SaveAs(Path.Combine(ImageSavePath, arrivalInfoIconFileName));

        //    return Redirect(Url.Action("Create", new { Id = eventInfo.ID }) + "#eventInfo");
        //}
        /// <summary>
        ///  Create new activity in specific event
        /// </summary>
        /// <param name="activite"></param>
        /// <param name="thumbnail"></param>
        /// <param name="FloorMapLocation"></param>
        /// <returns></returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> CreateActivity(Activite activite, HttpPostedFileBase thumbnail, string FloorMapLocation)
        {
            try
            {
                activite.CreateON = DateTime.Now;
                string fileName = "";
                if (thumbnail != null)

                    fileName = Guid.NewGuid().ToString() + ".png";
                activite.Thumbnail = fileName;

                if (activite.ID == 0)
                {
                    int userId = CurrentUserSession.Attende.ID;
                    string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
                    commonLogic.SaveLog("Activities", "Insert", userId, $"The Activity with the id: {activite.ID} for the event : {activite.EventID} is created by {userName}.");
                }


                await eventLogic.AddUpdateActivity(activite, FloorMapLocation, CurrentUserSession.AttendesID.Value);

                if (thumbnail != null)
                    thumbnail.SaveAs(Path.Combine(ImageSavePath, fileName));
                // return RedirectToAction("Create", "Events", new { Id = activite.EventID });

                

                return Redirect(Url.Action("Create", new { Id = activite.EventID }) + "#Activities");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ActionResult> CreateActivity(int? id)
        {
            var activity = await activitylogic.GetActivity(id);
            return View(activity);
        }
        public async Task<ActionResult> CreateTheme(Theme theme)
        {
            await eventLogic.AddUpdateTheme(theme);
            //return RedirectToAction("Create", "Events", new { id = theme.EventID });
            if (theme.ID == 0)
            {
                int userId = CurrentUserSession.Attende.ID;
                string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
                commonLogic.SaveLog("Theme", "Insert", userId, $"The Theme with the id: {theme.ID} for the event : {theme.EventID} is created by {userName}.");
            }

            return Redirect(Url.Action("Create", new { id = theme.EventID }) + "#EventTheme");
        }
        /// <summary>
        /// Create new theme 
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
                string splashScrFileName = "";
                string appBgFileName = "";
                string buttonFileName = "";

                if (SplashScreem != null)
                {
                    splashScrFileName = Guid.NewGuid().ToString() + ".png";
                }
                if (Button != null)
                {
                    appBgFileName = Guid.NewGuid().ToString() + ".png";
                }
                if (Button != null)
                {
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
                //return RedirectToAction("Create", "Events", new { id = theme.EventID });
                return Redirect(Url.Action("Create", new { id = theme.EventID }) + "#EventTheme");

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

        public async Task<JsonResult> CheckDuplicateActivity(string name)
        {
            var result = await eventLogic.CheckActiviteDuplicate(name);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> ApproveComment(int id, int eventid)
        {
            var result = await eventLogic.ApproveComment(id);
            var file = eventLogic.GetSocialName(id);
            if (file != null || file != "")
            {
                string ename = eventLogic.GetEventName(eventid);
                string jsonkeysfilepath = HttpRuntime.AppDomainAppPath + @"\" + GoogleDriveSync.jsonkeysfile;
                GoogleDriveSync.CallSaveApi(jsonkeysfilepath, ename, Path.Combine(ImageSavePath, file));
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Approve photos posted for specific event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="eventid"></param>
        /// <returns></returns>
        public async Task<JsonResult> ApproveEventPhoto(int id, int eventid)
        {
            var result = await eventLogic.ApproveEventPhoto(id);
            var file = eventLogic.GetPhotoName(id);
            string ename = eventLogic.GetEventName(eventid);
            string jsonkeysfilepath = HttpRuntime.AppDomainAppPath + @"\" + GoogleDriveSync.jsonkeysfile;
            GoogleDriveSync.CallSaveApi(jsonkeysfilepath, ename, Path.Combine(ImageSavePath, file));
            return Json(result, JsonRequestBehavior.AllowGet);
            //return Redirect(Url.Action("Create", new { id = eventid }) + "#ApprovePhotos");
        }
        /// <summary>
        /// Delete posted comments 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeleteComment(int id)
        {
            var result = await eventLogic.DeleteComment(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult FetchActivities(int galEventid, int id, int eventID)
        //{

        //    var res = eventLogic.FetchActivities(galEventid, id, eventID);

        //    return Json(true, JsonRequestBehavior.AllowGet);
        //}

        public async Task<JsonResult> FetchActivities(int galEventid, int id, int eventID)
        {
            await eventLogic.BookmarkedActivities(galEventid, eventID);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> FetchEventInfo(int id)
        {
            var res = eventLogic.FetchEventInfo(id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> FetchAttendeeFromGalactic(int eventid, int mainid, bool isSpeakerRequest = false)
        {
            //AuthLogic al = new AuthLogic();
            //var records = al.GetProgramAttendees(eventid);
            //foreach (var item in records.ProgramAttendees.Where(x => (isSpeakerRequest && x.AttendeeType.ToLower() == "speaker") || (!isSpeakerRequest && x.AttendeeType.ToLower() != "speaker")))
            //{
            //    var checkExist = db.Attendes.Any(x => x.Email == item.Email);
            //    if (!checkExist)
            //    {
            //        Attende at = new Attende();
            //        at.FirstName = item.FirstName;
            //        at.Lastname = item.LastName;
            //        at.Thumbnail = attendesLogic.GenerateImageFromName(item.FirstName, item.LastName, ImageSavePath); // "noimage.png";
            //        at.Email = item.Email;
            //        at.LinkedinURL = item.LinkedInURL;
            //        at.IsSpeaker = item.AttendeeType.ToLower() == "speaker";
            //        db.Attendes.Add(at);
            //        AttendesEvent ae = new AttendesEvent();
            //        ae.AttendesID = at.ID;
            //        ae.EventID = mainid;
            //        db.AttendesEvents.Add(ae);
            //        await db.SaveChangesAsync();
            //    }
            //}
            AuthLogic al = new AuthLogic();
            var records = al.GetProgramAttendees(eventid);
            Func<ProgramAttendee, bool> validator = x => (isSpeakerRequest && x.AttendeeType.ToLower() == "speaker") || (!isSpeakerRequest && x.AttendeeType.ToLower() != "speaker");
            await eventLogic.AddResyncedAttendees(records, validator, mainid, ImageSavePath);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Sync user groups from galactic DB
        /// </summary>
        /// <param name="eventid"></param>
        /// <param name="mainid"></param>
        /// <returns></returns>
        public async Task<ActionResult> ResyncUserGroups(int eventid, int mainid)
        {
            AuthLogic al = new AuthLogic();
            UserGroups userGroups = new UserGroups();
            var gpsAttendees = al.GetProgramAttendees(eventid);
            await eventLogic.AddResyncedAttendees(gpsAttendees, x => true, mainid, ImageSavePath); // might be a bad way, we need to relook into this
            userGroups.AddResyncedGroups(gpsAttendees.ProgramAttendees, mainid, CurrentUserSession.AttendesID.Value);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> CheckEventExist(int id)
        {
            var res = await eventLogic.CheckEvent(id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        //public async Task<JsonResult> FloorMapDetails()
        //{
        //    try
        //    {
        //        FloorMapResponse fmp = new FloorMapResponse();
        //        var floormap = await db.FloorMappings.FindAsync(34);
        //        if (floormap != null)
        //        {
        //            fmp.canvas = new Canvas();
        //            fmp.canvas.height = floormap.PhotoHeight.Value.ToString();
        //            fmp.canvas.width = floormap.PhotoWidth.Value.ToString();
        //            fmp.canvas.src = floormap.PhotoURL.ToString();
        //            fmp.Pin = new List<FloorMapPin>();

        //            var regions = db.FloorRegionMappings.Where(x => x.FMid == floormap.ID).ToList();
        //            if (regions != null && regions.Count > 0)
        //            {
        //                foreach (var indreg in regions)
        //                {
        //                    fmp.Pin.Add(new FloorMapPin
        //                    {
        //                        coords = new Coords
        //                        {
        //                            _lat = indreg.X.Value.ToString(),
        //                            _long = indreg.Y.ToString()
        //                        },
        //                        Message = indreg.Description.Trim()
        //                    });
        //                }
        //            }
        //        }

        //        return Json(fmp, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;

        //    }


        //}
        //FloorMapResponse fmr = new FloorMapResponse();
        /// <summary>
        /// Give Floormapdetails based on ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> FloorMapDetails(int id)
        {
            FloorDirectionsLogic floorDirectionsLogic = new FloorDirectionsLogic();
            List<FloorRegionMapping> regions = floorDirectionsLogic.GetFloorMappings(id);
            var floorMap = db.FloorMappings.Find(id);
            ViewBag.FloorImage = floorMap?.PhotoURL;
            ViewBag.FloorLocations = floorMap?.Event.FloorMapLocations;
            //fmr.canvas.fmid = floormap.ID;
            //fmr.canvas.src = floormap.PhotoURL;
            //fmr.canvas.width = floormap.PhotoWidth.ToString();
            //fmr.canvas.height = floormap.PhotoWidth.ToString();
            //foreach (var item in region)
            //{
            //    fmr.Pin.Add(new FloorMapResponse.FloorMapPin
            //    {
            //        content = db.FloorMapLocations.Find(item.floorLocationID)?.Location,
            //        coords = new Coords { lat = (item.X - 16).ToString(), @long = (item.Y).ToString() }
            //    });
            //}
            //ViewBag.jstring = ConvertListbasedJsonToNodebased(fmr);
            //var floorMapEvent = db.FloorMappings.FirstOrDefault(x => x.ID == id);
            //ViewBag.FloorLocations = floorMapEvent.Event.FloorMapLocations;
            //ViewBag.Activities = floorMapEvent.Event.Activites;
            //ViewBag.Sponsors = floorMapEvent.Event.SponsorsEvents.Select(x => x.Sponsor).ToList();
            //ViewBag.Exhibitors = floorMapEvent.Event.ExhibitorsEvents.Select(x => x.Exhibitor).ToList();

            return View(regions);
        }

        //public async Task<JsonResult> FloorMapDetails1(int id)
        //{
        //    var floormap = await db.FloorMappings.FindAsync(id);
        //    var region = db.FloorRegionMappings.Where(x => x.FMid == floormap.ID).ToList();

        //    fmr.canvas.src = floormap.PhotoURL;
        //    fmr.canvas.width = floormap.PhotoWidth.ToString();
        //    fmr.canvas.height = floormap.PhotoWidth.ToString();
        //    foreach (var item in region)
        //    {
        //        fmr.Pin.Add(new FloorMapResponse.FloorMapPin
        //        {
        //            content = item.Description,
        //            coords = new Coords { lat = (item.X - 16).ToString(), @long = (item.Y).ToString() }
        //        });
        //    }

        //    string jstring = ConvertListbasedJsonToNodebased(fmr);
        //    return Json(jstring, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public async Task<ActionResult> PassValues(string jsond, int? fmid)
        {

            var values = ConvertNodebasedToListbasedJson(jsond);
            var region = db.FloorRegionMappings.Where(x => x.FMid == fmid).ToList();
            var delregions = db.FloorRegionMappings.RemoveRange(region);
            var status = await db.SaveChangesAsync();

            foreach (var item in values.Pin)
            {
                FloorRegionMapping fm = new FloorRegionMapping
                {
                    FMid = fmid.Value,
                    X = Convert.ToDecimal(item.coords.lat) + 16, // 16 is for correction
                    Y = Convert.ToDecimal(item.coords.@long),// 16 is for correction
                    Description = item.content
                };

                var floorLocation = db.FloorMapLocations.FirstOrDefault(x => x.Location == item.content);
                fm.floorLocationID = floorLocation?.ID;
                fm.EventID = floorLocation.eventID;
                db.FloorRegionMappings.Add(fm);
                await db.SaveChangesAsync();
            }

            return Redirect(Url.Action("FloorMapDetails", new { id = fmid }));

        }

        public void SaveFloorMapMappings(FloorMapBox[] floorMapBoxes, int fMID, int eventID)
        {
            FloorDirectionsLogic floorDirectionsLogic = new FloorDirectionsLogic();
            floorDirectionsLogic.SaveFloorMapMappings(floorMapBoxes, fMID, eventID);
        }


        //json keys
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
        public FloorMapResponse fmp;


        private FloorMapResponse ConvertNodebasedToListbasedJson(string postedjson)
        {
            FloorMapResponse _flormap = new FloorMapResponse();
            Dictionary<string, Dictionary<string, Object>> respo = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Object>>>(postedjson);
            if (respo != null && respo.Keys.Contains(parentjsonkey))
            {
                Dictionary<string, Object> respodata = respo[parentjsonkey];
                if (respodata != null && respodata.Keys.Contains(canvaskey))
                {
                    string canvasjsonstring = respodata[canvaskey].ToString();
                    canvasjsonstring = canvasjsonstring.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                    FloorMapResponse.Canvas _canvas = JsonConvert.DeserializeObject<FloorMapResponse.Canvas>(canvasjsonstring);
                    if (_canvas != null)
                        _flormap.canvas = _canvas;
                }
                string[] keys = respodata.Keys.ToArray();
                if (keys != null && keys.Length > 0)
                {
                    foreach (string indkey in keys)
                    {
                        if (!indkey.Equals(canvaskey))
                        {
                            string pinjsonstring = respodata[indkey].ToString();
                            pinjsonstring = pinjsonstring.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                            FloorMapResponse.FloorMapPin IndPin = JsonConvert.DeserializeObject<FloorMapResponse.FloorMapPin>(pinjsonstring);
                            if (IndPin != null)
                                _flormap.Pin.Add(IndPin);
                        }
                    }
                }
            }

            return _flormap;
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
        /// <summary>
        /// Add or remove vendor from specific event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddRemoveVendor(int eventId, int vendorId)
        {
            try
            {
                await eventLogic.AddRemoveVendorEvent(eventId, vendorId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Helpers.LogError("sponsor remove ", ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Add new or update existing boothno for exhibitor
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exhibitorId"></param>
        /// <param name="boothNo"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddUpdateExhibitorboothnumber(int eventId, int exhibitorId, string boothNo)
        {
            try
            {
                await eventLogic.AddUpdateExhibitorboothnumber(eventId, exhibitorId, boothNo);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Helpers.LogError("Add booth number ", ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Add new or update existing booth details for exhibitor
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="exhibitorId"></param>
        /// <param name="ExhibitorBoothNo"></param>
        /// <param name="ExhibitorStartTime"></param>
        /// <param name="ExhibitorEndTime"></param>
        /// <param name="FloorMapLocation"></param>
        /// <returns></returns>

        public async Task<ActionResult> AddUpdateExhibitorboothdetails(int EventID, int exhibitorId, string ExhibitorBoothNo, DateTime? ExhibitorStartTime, DateTime? ExhibitorEndTime, string FloorMapLocation, int? ExhibitorPackageType)
        {
            int? floorMapLocationID = eventLogic.GetFloorMapLocationID(FloorMapLocation, EventID);
            await eventLogic.AddUpdateExhibitorboothdetails(EventID, exhibitorId, ExhibitorBoothNo, ExhibitorStartTime, ExhibitorEndTime, floorMapLocationID, ExhibitorPackageType);
            return Redirect(Url.Action("Create", new { id = EventID }) + "#Exhibitors");
        }
        /// <summary>
        /// Add new or update existing booth details for Sponsor
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="sponsorId"></param>
        /// <param name="SponsorBoothNo"></param>
        /// <param name="SponsorStartTime"></param>
        /// <param name="SponsorEndTime"></param>
        /// <param name="FloorMapLocation"></param>
        /// <returns></returns>

        public async Task<ActionResult> AddUpdateSponsorboothdetails(int EventID, int sponsorId, string SponsorBoothNo, DateTime? SponsorStartTime, DateTime? SponsorEndTime, string FloorMapLocation, int? SponsorPackageType)
        {
            int? floorMapLocationID = eventLogic.GetFloorMapLocationID(FloorMapLocation, EventID);
            await eventLogic.AddUpdateSponsorboothdetails(EventID, sponsorId, SponsorBoothNo, SponsorStartTime, SponsorEndTime, floorMapLocationID, SponsorPackageType);
            return Redirect(Url.Action("Create", new { id = EventID }) + "#Sponsors");
        }
        /// <summary>
        /// Add new or update existing booth details for Vendor
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="vendorId"></param>
        /// <param name="VendorBoothNo"></param>
        /// <param name="VendorStartTime"></param>
        /// <param name="VendorEndTime"></param>
        /// <param name="FloorMapLocation"></param>
        /// <returns></returns>
        public async Task<ActionResult> AddUpdateVendorboothdetails(int EventID, int vendorId, string VendorBoothNo, DateTime? VendorStartTime, DateTime? VendorEndTime, string FloorMapLocation, int? VendorPackageType)
        {
            int? floorMapLocationID = eventLogic.GetFloorMapLocationID(FloorMapLocation, EventID);
            await eventLogic.AddUpdateVendorboothdetails(EventID, vendorId, VendorBoothNo, VendorStartTime, VendorEndTime, floorMapLocationID, VendorPackageType);
            return Redirect(Url.Action("Create", new { id = EventID }) + "#Vendors");
        }
        /// <summary>
        /// Add or remove Speaker from specific event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="speakerID"></param>
        /// <returns></returns>

        //public async Task<JsonResult> AddRemoveSpeaker(int eventId, int speakerID)
        //{
        //    try
        //    {
        //        await eventLogic.AddRemoveSpeakerEvent(eventId, speakerID);
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Helpers.LogError("speaker remove ", ex);
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //}


        public async Task<ActionResult> GenerateQrCode(int id, enQRCodeFor type, string url)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(string.Format("Event:{0},URL:{1}", id, url), QRCodeGenerator.ECCLevel.Q);
            QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            var fileName = Guid.NewGuid().ToString() + ".png";

            qrCodeImage.Save(Path.Combine(ImageSavePath, fileName));
            return Json(fileName, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Gets specific photos
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> EventPhotosPartialView(int id)
        {
            var eventPhotos = await eventLogic.EventPhotos(id);
            return PartialView("_EventPhotosPartialView", eventPhotos);
        }
        /// <summary>
        /// Adds new images to specific event
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
                    if (string.Equals(Request.Browser.Browser, "IE", StringComparison.CurrentCultureIgnoreCase))
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

        // POST: Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        /// <summary>
        /// Creates new event
        /// </summary>
        /// <param name="event"></param>
        /// <param name="EventImage"></param>
        /// <param name="LeftMenuLogo"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(Event @event, HttpPostedFileBase EventImage, HttpPostedFileBase LeftMenuLogo, string latitude, string longitude)
        {
            try
            {
                string fileName = "";
                string leftMenuLogoFileName = "";
                if (EventImage != null)
                    fileName = Guid.NewGuid().ToString() + ".png";
                if (LeftMenuLogo != null)
                {
                    leftMenuLogoFileName = Guid.NewGuid().ToString() + ".png";
                }

                @event.EventImage = fileName;
                @event.LeftMenuLogo = leftMenuLogoFileName;
                if (latitude != null && longitude != null)
                {
                    @event.latitude = Convert.ToDouble(latitude);
                    @event.longitude = Convert.ToDouble(longitude);
                }

                int userId = CurrentUserSession.Attende.ID;
                string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
                int eventId = @event.ID;

                var id = await eventLogic.AddUpdateEvent(@event);
                string jsonkeysfilepath = HttpRuntime.AppDomainAppPath + @"\" + GoogleDriveSync.jsonkeysfile;
                GoogleDriveSync.CallApi(jsonkeysfilepath, @event.EventName);
                if (EventImage != null)
                    EventImage.SaveAs(Path.Combine(ImageSavePath, fileName));
                if (LeftMenuLogo != null)
                {
                    LeftMenuLogo.SaveAs(Path.Combine(ImageSavePath, leftMenuLogoFileName));
                }

                if (eventId == default)
                {
                    commonLogic.SaveLog("Event", "Insert", userId, $"The new event with id: {id} is created by {userName}.");
                }
                else
                {
                    commonLogic.SaveLog("Event", "Update", userId, $"The event with id: {id} is updated by {userName}.");
                }
                
                return RedirectToAction("Create", "Events", new { Id = id });
            }
            catch (GalacticEventExistsException ex)
            {
                Helpers.LogError("Event Error", ex);
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
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
        /// Save floormap mapping detials for specific event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="PhotoWidth"></param>
        /// <param name="PhotoHeight"></param>
        /// <param name="FloorName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<ActionResult> SaveFloorMap(int eventId, int PhotoWidth, int PhotoHeight, string FloorName, HttpPostedFileBase file)
        {
            string fileName = "";
            if (file != null)
                fileName = Guid.NewGuid().ToString() + ".png";

            await eventLogic.SaveFloorMap(eventId, fileName, PhotoWidth, PhotoHeight, FloorName);

            if (file != null)
                file.SaveAs(Path.Combine(ImageSavePath, fileName));
            //return RedirectToAction("Create",new { Id = eventId});
            return Redirect(Url.Action("Create", new { Id = eventId }) + "#FloorMap");
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
                return Redirect(Url.Action("Create", new { Id = eventId }) + "#QrCode");
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
        /// Delete existing theme
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> DeleteTheme(int id)
        {
            var eventid = await eventLogic.DeleteThee(id);
            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Theme", "Delete", userId, $"The theme with the id: {id} for the event id: {eventid} is deleted by {userName}.");
            return Redirect(Url.Action("Create", new { Id = eventid }) + "#EventTheme");

        }
        /// <summary>
        /// Delete photos 
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteEventPhotos(Int32? photoId)
        {
            try
            {
                if (photoId.HasValue)
                {
                    eventLogic.DeleteEventPhotos(photoId.Value);
                    int userId = CurrentUserSession.Attende.ID;
                    string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
                    commonLogic.SaveLog("Photos", "Delete", userId, $"The photo with the id: {photoId.Value} is deleted by {userName}.");

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
        /// <summary>
        /// Add comments for existing floormap
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<ActionResult> FloorMapComment(FloorRegionMapping file)
        {
            await eventLogic.SaveFloorDescription(file);
            //return RedirectToAction("Create", new { id = file.EventID });
            return Redirect(Url.Action("Create", new { id = file.EventID }) + "#FloorMap");
        }
        /// <summary>
        /// delete existing floormap comments
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> DeleteFloorMapComment(int id)
        {
            var eid = await eventLogic.FloorMapDeleteComment(id);
            //return RedirectToAction("Create", new { id = eid });

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("FloorMap", "Delete", userId, $"The floormap comment with the id: {id} is deleted by {userName}.");

            return Redirect(Url.Action("Create", new { id = eid }) + "#FloorMap");
        }
        /// <summary>
        /// Delete floormap
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> deletefloormap(int id)
        {
            var eid = await eventLogic.deletefloormap(id);
            //return RedirectToAction("Create", new { id = eid });
            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("FloorMap", "Delete", userId, $"The floormap with the id: {id} is deleted by {userName}.");

            return Redirect(Url.Action("Create", new { id = eid }) + "#FloorMap");
        }
        public ActionResult BatchDownload(int id)
        {
            FileDownlod obj = new FileDownlod();
            //////int CurrentFileID = Convert.ToInt32(FileID); 

            var photo = db.Photos.Where(x => x.EventID == id).ToList();
            var filename = photo.Select(x => x.Event.EventName).FirstOrDefault();
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
                return File(memoryStream.ToArray(), "application/zip", filename + ".zip");
            }
        }
        public async Task<ActionResult> FloorMapComments(int id)
        {
            var data = await eventLogic.GetFloorMapComments(id);
            return PartialView("_FloorMapComments", data);
        }
        //public class APSNotification
        //{
        //    [DataMember(Name = "aps")]
        //    public APSBody APS { get; set; }

        //    [DataContract(Name = "apsBody")]
        //    public class APSBody
        //    {
        //        [DataMember(Name = "badge")]
        //        public int Badge { get; set; }

        //        [DataMember(Name = "alert")]
        //        public string Alert { get; set; }

        //        [DataMember(Name = "sound")]
        //        public string Sound { get; set; }
        //    }

        //    [DataMember(Name = "device_tokens")]
        //    public List<string> DeviceTokens { get; set; }




        //    public string ToJsonString()
        //    {
        //        var ms = new MemoryStream();
        //        var ser = new DataContractJsonSerializer(typeof(APSNotification));
        //        ser.WriteObject(ms, this);
        //        ms.Seek(0, SeekOrigin.Begin);
        //        var sr = new StreamReader(ms);
        //        var result = sr.ReadToEnd();
        //        ms.Close();
        //        ms.Dispose();

        //        return result;
        //    }
        //}
        /// <summary>
        /// send notification to all attendees mapped
        /// </summary>
        /// <param name="NotiText"></param>
        /// <param name="title"></param>
        /// <param name="Attendes"></param>
        /// <param name="eid"></param>
        /// <param name="chk"></param>
        /// <returns></returns>
        public async Task<ActionResult> SendNotification(string NotiText, string title, List<string> Attendes, int eid, bool chk)
        {
            Entities db = new Entities();
            try
            {
                var notificationID = await notifications.MaintainNotificationHistory(eid, title, NotiText, CurrentUserSession.AttendesID.Value);


                if (chk == true)
                {
                    var userSessions = db.UserSessions.Where(x => x.IsActive == true && x.Platform != "Web" && !string.IsNullOrEmpty(x.DeviceToken) && x.Attende.AttendesEvents.Any(y => y.EventID == eid)).ToList();

                    userSessions = userSessions.Distinct(new UserDeviceTokenComparer()).ToList();

                    List<int> attedees = new List<int>();
                    foreach (var session in userSessions)
                    {
                        if (session.AttendesID != null)
                        {
                            if (session.IsActive == true && !string.IsNullOrEmpty(session.DeviceToken))
                            {
                                int attend = session.AttendesID.Value;

                                int status = await eventLogic.SendPushNotification(session.DeviceToken, title, NotiText, eid, attend, attend, notificationID);

                                if (status == 1 && !attedees.Contains(attend))
                                {
                                    attedees.Add(attend);
                                }
                            }
                        }
                    }

                    foreach (int user in attedees)
                    {
                        await eventLogic.SendEmail(user, title, NotiText);
                    }
                }
                else
                {
                    if (Attendes != null)
                    {
                        var str = Attendes[0].Replace("[", "");
                        str = str.Replace("]", "");
                        str = str.Replace('"', ' ').Trim();
                        //string[] AttenderArr = str.Split(',');
                        int[] array = str.Split(',').Select(int.Parse).ToArray();
                        foreach (var items in array)
                        {
                            if (db.UserSessions.Any(x => x.AttendesID == items))
                            {
                                List<int> statuses = new List<int>();
                                var deviceTokens = eventLogic.GetDeviceTokensByUserid(items, eid);
                                foreach (var device in deviceTokens)
                                {
                                    if (device.IsActive != false)
                                    {
                                        var status = await eventLogic.SendPushNotification(device.DeviceToken, title, NotiText, eid, items, CurrentUserSession.AttendesID.Value, notificationID);

                                        statuses.Add(status);
                                    }
                                }

                                if (statuses.Any(s => s == 1))
                                {
                                    await eventLogic.SendEmail(items, title, NotiText);
                                }

                            }
                            //eventLogic.SendEmailforAttendeeNotification(title, NotiText, db.Attendes.Where(x => x.ID == items).Select(y => y.Email).ToString());
                        }
                    }

                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);

            }
        }
        /// <summary>
        /// Send notifications to all the attendees mapped
        /// </summary>
        /// <param name="NotiText"></param>
        /// <param name="title"></param>
        /// <param name="Attendes"></param>
        /// <param name="eid"></param>
        /// <returns></returns>
        public async Task<ActionResult> SendNotificationAttendes(string NotiText, string title, List<int> Attendes, int eid)
        {
            Entities db = new Entities();
            try
            {
                if (Attendes != null)
                {
                    var notificationID = await notifications.MaintainNotificationHistory(eid, title, NotiText, CurrentUserSession.AttendesID.Value);
                    foreach (var items in Attendes)
                    {
                        if (db.UserSessions.Any(x => x.AttendesID == items))
                        {
                            var deviceTokens = eventLogic.GetDeviceTokensByUserid(items, eid);
                            foreach (var device in deviceTokens)
                            {
                                if (device.IsActive != false)
                                {
                                    await eventLogic.sendPushNotificationWithEmail(device.DeviceToken, title, NotiText, eid, device.AttendesID.Value, CurrentUserSession.AttendesID.Value, notificationID);
                                    //eventLogic.SendNotificationFromFirebaseCloud(device.DeviceToken, title, NotiText);
                                    //await eventLogic.MaintainNotificationHistory(eid, items, title, NotiText, CurrentUserSession.AttendesID.Value);
                                }
                            }
                        }
                    }
                }
                return Redirect(Url.Action("Create", new { id = eid }) + "#Notification");
            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);

            }
        }
        /// <summary>
        /// Update existing theme details
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="SplashScreem"></param>
        /// <returns></returns>
        public async Task<ActionResult> UpdateTheme(Theme theme, HttpPostedFileBase SplashScreem)
        {
            string splashScrFileName = string.Empty;
            var oldtheme = db.Themes.Where(x => x.EventID == theme.EventID).FirstOrDefault();

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            if (oldtheme == null)
            {
                if (SplashScreem != null)
                {
                    splashScrFileName = Guid.NewGuid().ToString() + ".png";
                    theme.SplashScreemURL = splashScrFileName;
                }
                db.Themes.Add(theme);
                                
                commonLogic.SaveLog("Theme", "Create", userId, $"The theme for the event id: {theme.EventID} is Created by {userName}.");

            }
            else
            {
                if (SplashScreem != null)
                {
                    splashScrFileName = Guid.NewGuid().ToString() + ".png";
                    oldtheme.SplashScreemURL = splashScrFileName;
                }

                oldtheme.ButtonBackgroundColor = theme.ButtonBackgroundColor;
                oldtheme.ButtonForegroundColor = theme.ButtonForegroundColor;
                oldtheme.LabelForegroundColor = theme.LabelForegroundColor;
                oldtheme.HeadingForegroundColor = theme.HeadingForegroundColor;
                oldtheme.MenuText = theme.MenuText;
                oldtheme.MenuBackground = theme.MenuBackground;
                oldtheme.ContentBackground = theme.ContentBackground;
                oldtheme.LinkText = theme.LinkText;

                commonLogic.SaveLog("Theme", "Update", userId, $"The theme for the event id: {theme.EventID} is updated by {userName}.");
            }
            await db.SaveChangesAsync();
            if (SplashScreem != null)
            {
                SplashScreem.SaveAs(Path.Combine(ImageSavePath, splashScrFileName));
            }
            return Redirect(Url.Action("Create", new { id = theme.EventID }) + "#EventTheme");
        }
        /// <summary>
        /// Delete existing events
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> DeleteEvents(int id)
        {
            //Event even = await db.Events.FindAsync(id);
            //db.Events.Remove(even);
            //await db.SaveChangesAsync();
            //db.DeleteEvent(id);
            db.DeleteEventS(id);

            int userId = CurrentUserSession.Attende.ID;            
            commonLogic.SaveLog("Event", "Delete", userId , $"The event id: {id} is deleted by {userId}.");

            return RedirectToAction("Index");
        }

        //public async Task<ActionResult> DuplicateEvent(int id)
        //{
        //    db.DuplicateEvent(id);

        //    int userId = CurrentUserSession.Attende.ID;
        //    string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
        //    commonLogic.SaveLog("Event", "Insert", userId, $"The event id: {id} is replicated by {userName}.");
        //    return RedirectToAction("Index");
        //}

        public ActionResult DuplicateEvent(FormCollection collection)
        {
            int replicateEventId = Convert.ToInt32(collection["replicateEventId"]);
            ReplicateEvent replicateEventData = new ReplicateEvent
            {
                EventInformation = Convert.ToBoolean(collection["eventInformationCheckBox"].Split(',')[0]),
                EventLevelSurvey = Convert.ToBoolean(collection["eventLevelSurveyCheckBox"].Split(',')[0]),
                EventLocationDetails = Convert.ToBoolean(collection["eventLocationDetailsCheckBox"].Split(',')[0]),
                WidgetMappings = Convert.ToBoolean(collection["WidgetMappingsCheckBox"].Split(',')[0]),
                Attendees = Convert.ToBoolean(collection["attendeesCheckBox"].Split(',')[0]),
                Theme = Convert.ToBoolean(collection["themeCheckBox"].Split(',')[0]),
                Sponsor = Convert.ToBoolean(collection["sponsorCheckBox"].Split(',')[0]),
                Exhibitors = Convert.ToBoolean(collection["exhibitorsCheckBox"].Split(',')[0]),
                Vendors = Convert.ToBoolean(collection["vendorsCheckbox"].Split(',')[0]),
                Activities = Convert.ToBoolean(collection["activitiesCheckBox"].Split(',')[0]),
                ActivitiesSurvey = Convert.ToBoolean(collection["activitiesSurveyCheckBox"].Split(',')[0]),
                FloorMap = Convert.ToBoolean(collection["floorMapCheckBox"].Split(',')[0])
            };

            eventLogic.ReplicateEvent(replicateEventId, replicateEventData);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// used for mapping data maked on map to specific event
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="mapcoordinates"></param>
        /// <param name="LocationInfo"></param>
        /// <returns></returns>
        public async Task<ActionResult> addmap(int EventID, string mapcoordinates, string LocationInfo, string EventMarkerName, string Webpage, HttpPostedFileBase Markerimage1, HttpPostedFileBase Markerimage2, string WebpageName)
        {
            await eventLogic.SaveLocationDetails(EventID, mapcoordinates, LocationInfo, EventMarkerName, Webpage, Markerimage1, Markerimage2, ImageSavePath, WebpageName);
            return Redirect(Url.Action("Create", new { id = EventID }) + "#Maps");
        }

        public async Task<ActionResult> RemoveMarker(int EventID, string mapcoordinates1, string LocationInfo1)
        {
            await eventLogic.RemoveMarker(EventID, mapcoordinates1, LocationInfo1);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Event", "Delete", userId, $"The marker with coordinates : {mapcoordinates1} for the event id: {EventID} is deleted by {userName}.");

            return Redirect(Url.Action("Create", new { id = EventID }) + "#Maps");
        }

        /// <summary>
        /// Save changes made to survey
        /// </summary>
        /// <param name="id"></param>
        [HttpPost]
        public void SaveSurveyChanges(int id)
        {
            var Questionstatus = db.Questions.Find(id);
            if (Questionstatus.HideorShowQuestion == false)
                Questionstatus.HideorShowQuestion = true;
            else if (Questionstatus.HideorShowQuestion == true)
                Questionstatus.HideorShowQuestion = false;
            db.SaveChangesAsync();
        }
        /// <summary>
        /// Save changes made for widgets for specific event
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="widgetsData"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult saveWidgetChanges(int eventID, string widgetsData)
        {
            var widgetMapping = JsonConvert.DeserializeObject<List<WidgetMapping>>(widgetsData);

            eventLogic.AddOrUpdateWidgets(widgetMapping, eventID);

            return Redirect(Url.Action("Create", new { Id = eventID }) + "#homeScreenConfig");
        }
        /// <summary>
        /// Add new widget
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="widgetImageFile"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSaveWidget(WidgetMapping widget, HttpPostedFileBase widgetImageFile)
        {
            string widgetImageFileName = this.eventLogic.createGUIDFileNameFromFile(widgetImageFile);
            if (widgetImageFile != null)
                widget.ImagePath = widgetImageFileName;
            eventLogic.AddUpdateWidget(widget);
            if (widgetImageFile != null)
                widgetImageFile.SaveAs(Path.Combine(ImageSavePath, widgetImageFileName));
            return Redirect(Url.Action("Create", new { Id = widget.EventID }) + "#homeScreenConfig");
        }

        /// <summary>
        /// mapping activities to attendees
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

        //public ActionResult AddSpeakerToEvent(int speakerID, List<int> activities)
        //{

        //    if (activities == null)
        //    {
        //        activities = new List<int>();
        //    }
        //    var deletedSpeakers = db.SpeakerActivities.Where(x => x.SpeakerID == speakerID && !activities.Any(y => y == x.ActivityID));
        //    db.SpeakerActivities.RemoveRange(deletedSpeakers);

        //    foreach (var activityID in activities)
        //    {
        //        if (!db.SpeakerActivities.Any(x => x.ActivityID == activityID && x.SpeakerID == speakerID))
        //        {
        //            db.SpeakerActivities.Add(
        //                new SpeakerActivity()
        //                {
        //                    SpeakerID = speakerID,
        //                    ActivityID = activityID
        //                });
        //        }
        //    }

        //    db.SaveChanges();

        //    return Json(true);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAlbum(string albumName, int eventID, string AlbumDescription)
        {
            db.PhotoAlbums.Add(new PhotoAlbum()
            {
                AlbumName = albumName,
                eventID = eventID,
                description = AlbumDescription
            });
            db.SaveChanges();
            return Redirect(Url.Action("Create", new { Id = eventID }) + "#Photos");
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

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Photos", "Delete", userId, $"The album id: {albumID} is deleted by {userName}.");
            return Json(true);
        }
        /// <summary>
        /// Get attendees mapped to specific group
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="isGroup"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetGroupAttendees(int groupID, bool isGroup)
        {

            ActivityLogic activityLogic = new ActivityLogic();
            if (!isGroup)
            {
                var attendees = await activityLogic.getActivityAttendees(groupID);
                return Json(attendees.Select(x => new
                {
                    x.ID,
                    x.Email,
                    x.InstagramURL,
                    x.FirstName,
                    IsSessionsAvailable = x.UserSessions.Count != 0,
                    x.Lastname
                }), JsonRequestBehavior.AllowGet);
            }
            return Json(db.AttendeeGroups.Where(x => x.GroupID == groupID).Select(x => x.Attende).Select(x => new
            {
                x.ID,
                x.Email,
                x.InstagramURL,
                x.FirstName,
                IsSessionsAvailable = x.UserSessions.Count != 0,
                x.Lastname
            }), JsonRequestBehavior.AllowGet);

        }
        public async Task<ActionResult> DeleteActivity(int ActivityID, int EventID)
        {
            db.DeleteActivity(ActivityID);
            await db.SaveChangesAsync();

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Activities", "Delete", userId, $"The activity with id : {ActivityID} for the event id: {EventID} is deleted by {userName}.");
            return Json(true);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAllEventActivities(int eventID)
        {
            var eventActivities = db.Activites.Where(x => x.EventID == eventID).ToList();
            foreach (var activity in eventActivities)
            {
                db.DeleteActivity(activity.ID);
            }
            await db.SaveChangesAsync();

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Activities", "Delete", userId, $"All The activities for the event id: {eventID} is deleted by {userName}.");

            return Redirect(Url.Action("Create", new { Id = eventID }) + "#Activities");
        }

        public async Task<ActionResult> DeleteDocument(int id)
        {
            EventLogic eventLogic = new EventLogic();
            var EventId = db.Documents.FirstOrDefault(x => x.ID == id).EventID;
            await eventLogic.DeleteDocument(id);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Documents", "Delete", userId, $"The document with id : {id} for the event id: {EventId} is deleted by {userName}.");

            return Redirect(Url.Action("Create", new { Id = EventId }) + "#documents");
        }

        [HttpPost]
        public ActionResult UploadFormWaiverTemplate(int eventID, int activityID, HttpPostedFileBase waiverFormTemplate)
        {
            string waiverFormTemplateName = string.Empty;
            if (waiverFormTemplate != null)
            {
                waiverFormTemplateName = Guid.NewGuid().ToString() + ".pdf";
                waiverFormTemplate.SaveAs(Path.Combine(DocumentsSavePath, waiverFormTemplateName));
            }
                
            eventLogic.UploadFormWaiverTemplate(waiverFormTemplateName, eventID, DocumentsSavePath, activityID);
            return Redirect(Url.Action("Create", new { Id = eventID }) + "#waiverForm");
        }

        //[HttpPost]
        //public ActionResult ProcessPDF(int eventID, int activityID)
        //{
        //    eventLogic.ProcessFormWaiverTemplate(DocumentsSavePath, activityID);
        //    return Redirect(Url.Action("Create", new { Id = eventID }) + "#waiverForm");
        //}

        public ActionResult GetPDFConfig(int activityID)
        {
            var eventFormWaiverKeys = eventLogic.GetFormWaiverTemplateFieldMappings(activityID).Select(x => new
            {
                x.ID,
                x.FormKey,
                x.Value
            });
            return Json(eventFormWaiverKeys);
        }

        [HttpPost]
        public ActionResult SaveWaiverFormMappingData(int eventID, int activityID, Dictionary<int, int> mappingData)
        {
            var status = eventLogic.SaveWaiverFormMappingData(mappingData, activityID);
            eventLogic.SaveFormWaiverMappingsAndSendEmails(DocumentsSavePath, activityID, mappingData);
            return Json(status);
        }

        [HttpPost]
        public async Task<ActionResult> AddRemoveSponsorActivities(int sponsorID, List<int> activityIDs)
        {
            await eventLogic.AddRemoveSponsorActivity(sponsorID, activityIDs);
            return Json(true);
        }

        [HttpPost]
        public async Task<ActionResult> AddRemoveExhibitorActivities(int exhibitorID, List<int> activityIDs)
        {
            await eventLogic.AddRemoveExhibitorActivity(exhibitorID, activityIDs);
            return Json(true);
        }

        [HttpPost]
        public async Task<ActionResult> AddRemoveVendorActivities(int vendorID, List<int> activityIDs)
        {
            await eventLogic.AddRemoveVendorActivity(vendorID, activityIDs);
            return Json(true);
        }

        [HttpGet]
        public ActionResult DownloadWaiverFormDocument(string filePath)
        {
            string fullDocumentPath = Path.Combine(DocumentsSavePath, filePath);
            return File(fullDocumentPath, System.Net.Mime.MediaTypeNames.Application.Octet, filePath);
        }

        [HttpPost]
        public ActionResult DeleteFormWaiverTemplate(int eventID, int activityID)
        {
            eventLogic.DeleteWaiverForm(activityID);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Activities", "Delete", userId, $"The waiver template for the activity :{activityID} for the event id: {eventID} is deleted by {userName}.");

            return Redirect(Url.Action("Create", new { Id = eventID }) + "#waiverForm");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddEventInformation(List<EventInformation> eventInformation, List<HttpPostedFileBase> images, int EventID)
        {
            try
            {
                string aboutEventIconfileName;
                for (var i = 0; i < eventInformation.Count; i++)
                {
                    if (images[i] != null)
                    {
                        aboutEventIconfileName = this.eventLogic.createGUIDFileNameFromFile(images[i]);
                    }
                    else
                    {
                        aboutEventIconfileName = string.Empty;
                    }

                    if (eventInformation[i].HideSection == null)
                        eventInformation[i].HideSection = false;
                    if (eventInformation[i].IsExternalLink == null)
                        eventInformation[i].IsExternalLink = false;

                    eventInformation[i].IconSection = aboutEventIconfileName;
                    eventLogic.AddEventInformation(eventInformation[i], aboutEventIconfileName, EventID);

                    if (images[i] != null)
                        images[i].SaveAs(Path.Combine(ImageSavePath, aboutEventIconfileName));
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Redirect(Url.Action("Create", new { Id = EventID }) + "#eventInfo");
        }
        [HttpPost]
        public ActionResult DeleteEventInfo(int id)
        {
            eventLogic.DeleteEventInfos(id);
            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("EventInformation", "Delete", userId, $"The event information id: {id} is deleted by {userName}.");
            return Redirect(Url.Action("Create", new { Id = id }) + "#eventInfo");
        }

        public async Task<ActionResult> GetEventinformation(int id)
        {
            try
            {
                List<EventInformation> eventinfo = await eventLogic.geteventinformation(id);
                return Json(eventinfo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult AddEventVideo(EventVideo eventVideo)
        {
            EventVideoManager eventVideoManager = new EventVideoManager();
            eventVideoManager.AddVideo(eventVideo);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Videos", "Insert", userId, $"The video with id :{eventVideo.ID} for the event id: {eventVideo.EventID} is created by {userName}.");

            return Redirect(Url.Action("Create", new { Id = eventVideo.EventID }) + "#eventVideos");
        }
        [HttpPost]
        public ActionResult DeleteVideo(int id, int eventID)
        {
            EventVideoManager eventVideoManager = new EventVideoManager();
            eventVideoManager.DeleteVideo(id);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Videos", "Delete", userId, $"The video with id :{id} for the event id: {eventID} is deleted by {userName}.");

            return Redirect(Url.Action("Create", new { Id = eventID }) + "#eventVideos");
        }
    }

}

