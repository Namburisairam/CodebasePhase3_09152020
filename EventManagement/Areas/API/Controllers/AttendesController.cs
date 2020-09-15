using CommonHelpers;
using EventManagement.Base;
using EventManagement.BusinessLogic;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Models;
using EventManagement.Filter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace EventManagement.Areas.API.Controllers
{
    /// <summary>
    /// Attendees Controller
    /// </summary>
    [AuthFilter]
    public class AttendesController : BaseAPIController
    {
        EventLogic eventLogic = new EventLogic();
        /// <summary>
        /// Get Attendees
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="term"></param>
        /// <returns>Returns Attendees data based on term</returns>

        // GET: API/Attendes
        [HttpGet]
        public async Task<ActionResult> Index(string Token, string term = "")
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            var data = await attendesBusiness.GetAttendes(term);
            return Json(new
            {
                data = data.Select(x => new
                {
                    x.ID,
                    x.AddedON,
                    x.Description,
                    x.Email,
                    x.FirstName,
                    x.LinkedinURL,
                    x.InstagramURL,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                    x.TwitterURL,
                    x.Status,
                    x.FacebookURL,
                })
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// User Profile 
        /// </summary>
        /// <param name="Token"></param>
        /// <returns>Returns User Profile data</returns>
        [HttpGet]
        public async Task<ActionResult> Userprofile(string Token)
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            var x = await attendesBusiness.GetAttendesById(CurrentUserSession.AttendesID.Value);
            return Json(new
            {
                data = new
                {
                    x.AddedON,
                    x.Description,
                    x.Email,
                    x.FirstName,
                    x.Lastname,
                    x.InstagramURL,
                    x.LinkedinURL,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                    x.TwitterURL,
                    x.Status,
                    x.FacebookURL,
                }
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Updates profile of attendee
        /// </summary>
        /// <param name="token"></param>
        /// <param name="attende"></param>
        /// <param name="Base64Image"></param>
        /// <returns>Returns status of update</returns>

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> UpdateUserProfile(string token, DataAccess.DataBase.Model.Attende attende)
        {
            //attende.ID = CurrentUserSession.AttendesID.Value;
            AttendesLogic attendesBusiness = new AttendesLogic();
            //await ChangePhotos(token, Base64Image);
            await ChangeProfilePicture(token, attende.ID);
            attende.IsAdmin = IsAdmin;
            attendesBusiness.UpdateUserProfile(attende, CurrentUserSession.AttendesID.Value, ImageSavePath);
            await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "User profile", "Updated User Profile", "Updating User Details");
            return Json(true);
        }

        /// <summary>
        /// Change Photos to base64
        /// </summary>
        /// <param name="token"></param>
        /// <param name="base64String"></param>
        /// <returns>Returns status of conversion</returns>

        public async Task<ActionResult> ChangePhotos(string token, string base64String)
        {
            if (base64String == "")
                throw new APIException("Null image file.");
            AttendesLogic alogic = new AttendesLogic();
            var uniquename = Guid.NewGuid();  // await alogic.UniqueAttendeeID(CurrentUserSession.AttendesID.Value);
            string name = uniquename + ".png";

            var result = alogic.ChangePhoto(CurrentUserSession.AttendesID.Value, name);
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            image.Save(Path.Combine(ImageSavePath, name));
            string jsonkeysfilepath = HttpRuntime.AppDomainAppPath + @"\" + GoogleDriveSync.jsonkeysfile;

            GoogleDriveSync.CallSaveApi(jsonkeysfilepath, "Profile Pictures", Path.Combine(ImageSavePath, name));
            return Json(new { Data = "Success" }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ChangeProfilePicture(string token, int id)
        {
            AttendesLogic attendesLogic = new AttendesLogic();
            var uniquename = Guid.NewGuid();
            string name = uniquename + ".png";
            await attendesLogic.ChangeProfilePicture(id, ImageSavePath, name);
            return Json(new { Data = "Success" }, JsonRequestBehavior.AllowGet);
        }

        public async Task<string> RemovePicture(string token, int id)
        {
            AttendesLogic attendesLogic = new AttendesLogic();
            string filename = await attendesLogic.GenerateAttendeeImageFromName(id, ImageSavePath, false);
            return imageToUrlConvertionHandler(filename);
            //Helpers.ImageToBase64(MvcCustom.APiImagePath(filename));
        }

        /// <summary>
        /// Returns attendee data
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="id"></param>
        /// <returns>Gives attendee data based on attendee Id provided</returns>

        [HttpGet]
        public async Task<ActionResult> AttendeById(string Token, int id)
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            var x = await attendesBusiness.GetAttendesById(id);
            return Json(new
            {
                data = new
                {
                    x.AddedON,
                    Bio = x.Description,
                    x.Email,
                    x.FirstName,
                    x.Lastname,
                    x.TitleName,
                    x.CompanyName,
                    x.Website,
                    x.InstagramURL,
                    x.LinkedinURL,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),  // Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                    x.TwitterURL,
                    x.Status,
                    x.FacebookURL,
                    x.EnableMessaging,
                    x.EnableAlertEmails,
                    x.EnableAttendeeMessagingEmails,
                }
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Attendee Events
        /// </summary>
        /// <param name="Token"></param>
        /// <returns>Returns all the attendee events</returns>
        [HttpGet]
        public async Task<ActionResult> GetAttendeeEvents(string Token)
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            var data = await attendesBusiness.GetAttendeEvents(CurrentUserSession.AttendesID.Value);
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Attendee image Upload
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="file"></param>
        /// <returns>Status of uploaded image</returns>

        [HttpPost]
        public ActionResult UploadAttendeeImage(string Token, HttpPostedFileBase file)
        {
            string fileName = "";
            if (file != null)
                fileName = Guid.NewGuid().ToString() + ".png";
            //sponsor.Thumbnail = fileName;
            //await sponsorsLogic.AddUpdateSponsor(sponsor, events);
            if (file != null)
                file.SaveAs(Path.Combine(ImageSavePath, fileName));
            return Json(new { data = "Image Upload Failed" });
        }
        /// <summary>
        /// Adding activity to personal Agenda for loggedin Attendee
        /// </summary>
        /// <param name="token"></param>
        /// <param name="activityId"></param>
        /// <returns>Returns Status</returns>
        [HttpGet]
        public async Task<ActionResult> BookmarkActivity(string token, int activityId)
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            await attendesBusiness.BookMarkActivity(CurrentUserSession.AttendesID.Value, activityId);
            await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Bookmarking Activity", "Bookmarking Activity", "Activity has been bookmarked successfully");
            return Json(new { data = new { Message = "Success" } }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Returns all the activities bookmarked for Loggedin Attendee
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetAllBookMarks(string token)
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            var data = await attendesBusiness.GetBookMarksByAttendeId(CurrentUserSession.AttendesID.Value);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Returns attendees for given event
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="EventId"></param>
        /// <returns>Returns attendees data</returns>
        [HttpGet]
        public async Task<ActionResult> GetAttendeeForSpecificEvent(string Token, int EventId)
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            var data = await attendesBusiness.GetAttendeeForSpecificEvent(EventId);
            return Json(data.Select(x => new
            {
                x.ID,
                x.FirstName,
                x.Description,
                Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                x.FacebookURL,
                x.TwitterURL,
                x.InstagramURL,
                x.Status,
                x.Email,
                x.LinkedinURL,
                x.Lastname,
                x.TitleName,
                x.CompanyName
            }), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get speakers data along with attendees when show speakers with attendees toggle is turned on
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="EventId"></param>
        /// <returns> Attendee Information</returns>
        public async Task<ActionResult> GetAttendeeWithSpeakerForSpecificEvent(string Token, int EventId)
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            var data = await attendesBusiness.GetAttendeeWithSpeakerForSpecificEvent(EventId);
            return Json(data.Select(x => new
            {
                x.ID,
                x.FirstName,
                x.Description,
                Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                x.FacebookURL,
                x.TwitterURL,
                x.InstagramURL,
                x.Status,
                x.Email,
                x.LinkedinURL,
                x.Lastname,
                x.TitleName,
                x.CompanyName,
                x.IsSpeaker,
                x.EnableMessaging
            }), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// preferences for loggedin attendee
        /// </summary>
        /// <param name="Token"></param>
        /// <returns> returns preferences data </returns>
        public async Task<ActionResult> GetPreferences(String Token)
        {
            AttendesLogic attendesLogic = new AttendesLogic();
            var data = await attendesLogic.GetPreferences(CurrentUserSession.AttendesID.Value);
            return Json(data.Select(x => new Preferences
            {
                AttendeeID = x.ID,
                EnableMessaging = x.EnableMessaging,
                EnableAlertEmails = x.EnableAlertEmails,
                EnableAttendeeMessagingEmails = x.EnableAttendeeMessagingEmails
            }), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Saves preferences for loggedin attendee
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="EnableMessaging"></param>
        /// <param name="EnableAlertEmails"></param>
        /// <param name="EnableAttendeeMessagingEmails"></param>
        /// <returns>Returns saved preferences data</returns>
        public async Task<ActionResult> SavePreferences(String Token, bool EnableMessaging = false, bool EnableAlertEmails = false, bool EnableAttendeeMessagingEmails = false)
        {
            AttendesLogic attendesLogic = new AttendesLogic();
            var data = await attendesLogic.SavePreferences(CurrentUserSession.AttendesID.Value, EnableMessaging, EnableAlertEmails, EnableAttendeeMessagingEmails);
            await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Preferences", "Changing Preferences", "Preferences has been changed successfully");
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetAttendeeProfilePic(string Token, int id)
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            var attendee = await attendesBusiness.GetAttendesById(id);
            return Json(compressedImageConvertionHandler(attendee.Thumbnail), JsonRequestBehavior.AllowGet);
        }

    }
}