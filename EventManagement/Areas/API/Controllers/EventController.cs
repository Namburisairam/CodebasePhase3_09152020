using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using CommonHelpers;
using EventManagement.Base;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Business.Email_Notifier;
using EventManagement.BusinessLogic.Models;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using Newtonsoft.Json;
using Twilio;
using Twilio.Jwt.AccessToken;
using Twilio.Rest.Chat.V1.Service.Channel;
using Twilio.Rest.Chat.V1.Service.User;
using static EventManagement.BusinessLogic.Business.EventLogic;
using ChannelResource = Twilio.Rest.Chat.V1.Service.ChannelResource;

namespace EventManagement.Areas.API.Controllers
{
    [AuthFilter]

    public class EventController : BaseAPIController
    {
        EventLogic eventLogic = new EventLogic();
        AttendesLogic attendesBusiness = new AttendesLogic();
        SponsorsLogic SponsorBusiness = new SponsorsLogic();
        /// <summary>
        /// To get all the messages
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="chid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetMessages(string Token, string chid)
        {
            // Load Twilio configuration from Web.config
            var accountSid = "ACe20c78fc2bb903d344e7ca98bcab2d86";
            const string authToken = "3712d9b600dac77588b17bf7a23d55f7";
            TwilioClient.Init(accountSid, authToken);

            var messages = MessageResource.Read(
                pathServiceSid: "IS20947bdf7e214bd087b6492358cfcee2",
                pathChannelSid: chid
            );

            foreach (var record in messages)
            {
                Console.WriteLine(record.Sid);
            }
            return Json(new
            {
                Channels = messages
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To create new message
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="chid"></param>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<ActionResult> CreateMessages(string Token, string chid, string msg, string from)
        {
            // Load Twilio configuration from Web.config
            var accountSid = "ACe20c78fc2bb903d344e7ca98bcab2d86";
            const string authToken = "3712d9b600dac77588b17bf7a23d55f7";
            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                    body: msg,
                    from: from,
                   pathServiceSid: "IS20947bdf7e214bd087b6492358cfcee2",
                    pathChannelSid: chid
                );

            Console.WriteLine(message.Sid);


            return Json(new
            {
                Channels = message
            }, JsonRequestBehavior.AllowGet);
        }
        //[HttpGet]
        //public async Task<ActionResult> GetAllUser(string Token)
        //{
        //    // Load Twilio configuration from Web.config
        //    var accountSid = "ACe20c78fc2bb903d344e7ca98bcab2d86";
        //    const string authToken = "3712d9b600dac77588b17bf7a23d55f7";

        //    TwilioClient.Init(accountSid, authToken);

        //    var users = UserResource.Read(
        //        pathServiceSid: "IS20947bdf7e214bd087b6492358cfcee2"
        //    );


        //    return Json(new
        //    {
        //        user  = users
        //    }, JsonRequestBehavior.AllowGet);
        //}

        /// <summary>
        /// To get all channels for Logged user
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetUserChannels(string Token)
        {
            // Load Twilio configuration from Web.config
            var accountSid = "ACe20c78fc2bb903d344e7ca98bcab2d86";
            const string authToken = "3712d9b600dac77588b17bf7a23d55f7";

            TwilioClient.Init(accountSid, authToken);

            var userChannels = UserChannelResource.Read(
                pathServiceSid: "IS20947bdf7e214bd087b6492358cfcee2",
                pathUserSid: "US2a275c362834465990bf155bc2bae3ee"
            );
            return Json(new
            {
                UserChannels = userChannels
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CreateInvite(string Token)
        {
            // Load Twilio configuration from Web.config
            var accountSid = "ACe20c78fc2bb903d344e7ca98bcab2d86";
            const string authToken = "3712d9b600dac77588b17bf7a23d55f7";


            TwilioClient.Init(accountSid, authToken);

            var invite = InviteResource.Create(
                identity: "abdiel.deckow",
                pathServiceSid: "IS20947bdf7e214bd087b6492358cfcee2",
                pathChannelSid: "CH02b84eeaa3d9484e98301fa186c4aa42"
            );


            return Json(new
            {
                CreatedInvite = invite
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> CreateChannel(string Token, string channelname)
        {
            // Load Twilio configuration from Web.config
            var accountSid = "ACe20c78fc2bb903d344e7ca98bcab2d86";
            const string authToken = "3712d9b600dac77588b17bf7a23d55f7";
            TwilioClient.Init(accountSid, authToken);
            var channels1 = ChannelResource.Read(
            pathServiceSid: "IS20947bdf7e214bd087b6492358cfcee2");
            string sid = "";
            var findChannel = channels1.Where(x => x.UniqueName == channelname).FirstOrDefault();
            if (findChannel == null || findChannel.UniqueName != channelname)
            {
                var channel = ChannelResource.Create(
                    friendlyName: channelname,
                    uniqueName: channelname,
                    pathServiceSid: "IS20947bdf7e214bd087b6492358cfcee2");
                sid = channel.Sid;
            }
            else
            {
                sid = "false";
            }

            return Json(sid, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> AcceptInvite(string Token)
        {
            // Load Twilio configuration from Web.config
            var accountSid = "ACe20c78fc2bb903d344e7ca98bcab2d86";
            const string authToken = "3712d9b600dac77588b17bf7a23d55f7";
            TwilioClient.Init(accountSid, authToken);

            var invite = InviteResource.Fetch(
               pathServiceSid: "IS20947bdf7e214bd087b6492358cfcee2",
                pathChannelSid: "CH02b84eeaa3d9484e98301fa186c4aa42",
                pathSid: "INddbe4579284e40f2872065947004e4cf"
            );
            return Json(new
            {
                AcceptedInvite = invite
            }, JsonRequestBehavior.AllowGet);
        }
        // GET: API/Event
        [HttpGet]
        public async Task<ActionResult> Index(string Token, string term = "")
        {
            eventLogic = new EventLogic();
            var allEvents = await eventLogic.GetAllEvents(term);
            return Json(new
            {
                allEvents = allEvents.Select(eventData => new
                {
                    eventData.ID,
                    eventData.EventName,
                    eventData.Message,
                    EventImage = imageToUrlConvertionHandler(eventData.EventImage),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(eventData.EventImage)),

                })
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CreateTwilioUser(string Token, string Device)
        {
            // Load Twilio configuration from Web.config
            var accountSid = "ACe20c78fc2bb903d344e7ca98bcab2d86";
            var apiKey = "SK8b98a548d8bfd469bb11c75a9625b4c4";
            var apiSecret = "QZSzGD9uHBvWmw8cJhKvxnOgwiwpnVrZ";
            var ipmServiceSid = "IS20947bdf7e214bd087b6492358cfcee2";

            // Create a random identity for the client
            var identity = Device;

            // Create an IP messaging grant for this token
            var grant = new IpMessagingGrant();
            grant.EndpointId = $"TwilioChatDemo:{identity}:{Device}";
            grant.ServiceSid = ipmServiceSid;
            var grants = new System.Collections.Generic.HashSet<IGrant>
            {
                { grant }
            };

            // Create an Access Token generator
            var Token1 = new Token(accountSid, apiKey, apiSecret, identity: identity, grants: grants);

            return Json(new
            {
                identity = identity,
                token = Token1.ToJwt()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> AllRecords(string Token, string term = "")
        {
            eventLogic = new EventLogic();
            var allEvents = await eventLogic.GetAllEvents(term);
            var Attendees = await attendesBusiness.GetAttendes(term);
            var Sponsors = await attendesBusiness.GetAttendes(term);
            return Json(new
            {

                Sponsors = (await SponsorBusiness.GetSponsors(term)).Select(x => new
                {
                    Description = x.Description,
                    DocURL = x.DocURL,
                    ID = x.ID,
                    Name = x.Name,
                    Status = x.Status,
                    URL = x.DocURL,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                }),
                Attendees = Attendees.Select(x => new
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
                }),
                allEvents = allEvents.Select(eventData => new
                {
                    eventData.ID,
                    eventData.EventName,
                    eventData.Message,
                    EventImage = imageToUrlConvertionHandler(eventData.EventImage),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(eventData.EventImage)),

                })
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get all photos based on provided eventID
        /// </summary>
        /// <param name="token"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<ActionResult> GetEventPhotos(string token, int eventId)
        {
            EventLogic logic = new EventLogic();
            SocialForumLogic socialForumLogic = new SocialForumLogic();
            var photos = await logic.GetEventPhotos(eventId);
            var jsonResult = Json(new
            {
                photos = photos.OrderByDescending(x => x.UploadON).Select(x => new
                {
                    Url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.APiImagePath(x.URL)),//  Helpers.ImageToBinary(MvcCustom.APiImagePath(x.URL)),
                    Description = (x.Description == null && x.Description =="" ? "No Description" : x.Description),
                    Status = x.Status,
                    Likes = x.PhotoLikes.Count,
                    IsLiked = x.PhotoLikes.Any(y => y.AttendeeID == CurrentUserSession.AttendesID),
                    x.ID,
                    UploadedBy = x.UploadBY,
                    UploadedDate = x.UploadON
                }),
                Albums = socialForumLogic.GetEventAlbums(eventId).Select(x => new
                {
                    x.AlbumName,
                    x.ID,
                    DisplayImage = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.APiImagePath(x.Photos.FirstOrDefault()?.URL)), // Helpers.ImageToBinary(MvcCustom.APiImagePath(x.Photos.FirstOrDefault().URL)),
                    ImagesCount = x.Photos.Count
                })
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        /// <summary>
        /// To get floor map details for specific event
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> EventFloorMaps(string Token, int eventId)
        {
            EventLogic logic = new EventLogic();
            var data = await logic.GetEventFloorMaps(eventId);
            //int wSize, hSize;
            //var imageu =  data.Select(x => x.Url).FirstOrDefault();

            //if (imageu!= null)
            //{
            //    string url = imageu != "" ? string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath, true), imageu))) : "";

            //    string image = @"" + url;
            //    System.IO.FileStream fs = new System.IO.FileStream(new Uri(image).r, System.IO.FileMode.Open, System.IO.FileAccess.Read);                
            //    MemoryStream ms = new MemoryStream();
            //    fs.CopyTo(ms);
            //   // MemoryStream imgStream = new MemoryStream(imageData);
            //    Image img = Image.FromStream(ms);

            //    wSize = img.Width;
            //    hSize = img.Height;
            //}
            //else
            //{
            //    wSize = 0;
            //    hSize = 0;

            //}


            return Json(new
            {
                Data = data.Select(x => new
                {
                    Name = x.Name,
                    ID = x.ID,
                    Url = imageToUrlConvertionHandler(x.Url),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Url)),
                    PhotoWidth = x.PhotoWidth,
                    PhotoHeight = x.PhotoHeight,
                    Description = x.Description,
                    RegionComments = x.Comments,
                })
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get all activities for given event based on loggedin user
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> UserActivitiesByEvent(string Token, int eventId)
        {
            EventLogic logic = new EventLogic();
            AuthLogic authLogic = new AuthLogic();
            authLogic.AddAttendeeBookMarksOfEvent(CurrentUserSession.Attende.Email, eventId);
            var data = await logic.ActivitiesByEvent(CurrentUserSession.AttendesID.Value, eventId);
            return Json(new { Data = data }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get all the activity related data for given activity and loggedin user
        /// </summary>
        /// <param name="token"></param>
        /// <param name="activityid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetActivityById(string token, int activityid)
        {
            EventLogic logic = new EventLogic();
            var data = await logic.GetActivityById(activityid);
            // we might have to change this behaviour. That is why we have not removed unused mappings
            var floorMapping = data.FloorMapLocation?.FloorRegionMappings.FirstOrDefault();
            var ActivitySurveyQuestions = await logic.ActivitySurveyQuestions(CurrentUserSession.AttendesID.Value, activityid);
            return Json(new
            {
                data.ID,
                data.Name,
                data.EventID,
                Qrcodes = data.QRCodes.Select(y => new
                {
                    y.Description,
                    y.WebURL,
                    y.Status
                }),
                StartDate = data.StartTime.ToShortDateString(),
                StratTime = data.StartTime.ToShortTimeString(),
                EndDate = data.EndTime.ToShortDateString(),
                EndTime = data.EndTime.ToShortTimeString(),
                data.Description,
                Thumbnail = imageToUrlConvertionHandler(data.Thumbnail),
                //Helpers.ImageToBase64(MvcCustom.APiImagePath(data.Thumbnail)),
                data.Address,
                data.GalacticActivityId,
                data.Status,
                IsBookMarked = data.BookMarks.Any(x => x.AttendesID == CurrentUserSession.AttendesID.Value),
                data.HideDate,
                data.HideTime,
                data.HideLocation,
                data.HideMap,
                data.HideDescription,
                data.HidePhoto,
                data.HideAddNotes,
                data.HideAttendees,
                data.HideDocuments,
                data.HideSurvey,
                data.HideSpeaker,
                Documents = data.Documents.Select(y => new
                {
                    y.ID,
                    y.DocumentName,
                    FilePath = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.ApiDocumentPath(y.FilePath))
                }),
                Speakers = data.BookMarks.Where(x => x.Attende.IsSpeaker).Select(x => new
                {
                    x.Attende.ID,
                    x.Attende.FirstName,
                    x.Attende.Lastname,
                    Thumbnail = imageToUrlConvertionHandler(x.Attende.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Attende.Thumbnail)),
                    x.Attende.Description
                }),
                FloorMap = floorMapping != null ? new
                {
                    floorMapping?.FloorMapping?.ID,
                    FloorName = data.FloorMapLocation?.Location
                } : null,
                SuveyQuestions = ActivitySurveyQuestions.Select(x => new QuestionsWithOptions
                {
                    QID = x.QID,
                    QuestionText = x.QuestionText,
                    IsAtActiivtyLevel = x.IsAtActiivtyLevel,
                    QuestionResponseType = x.QuestionResponseType,
                    HideorShowQuestion = x.HideorShowQuestion,
                    options = x.options,
                    IsSubmitted = x.IsSubmitted

                }),
                Note = data.Notes.FirstOrDefault(x => x.UserID == CurrentUserSession.AttendesID.Value && x.Activity_id == activityid)?.Text
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get all activitues for given event
        /// </summary>
        /// <param name="token"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<ActionResult> GetEventActivities(string token, int eventId)

        {
            eventLogic = new EventLogic();
            var eventData = await eventLogic.GetEventById(eventId);
            return Json(new
            {
                eventData.ID,
                Activites = eventData.Activites.Select(x => new
                {
                    x.Address,
                    CreateON = x.CreateON == null ? "not set" : x.CreateON.Value.ToShortDateString(),
                    x.Description,
                    x.Name,
                    EndTime = x.EndTime.ToShortDateString(),
                    StartTime = x.StartTime.ToShortDateString(),
                    x.Status,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                    //QRCode = x.QRCodes.Any() ? string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath, true), x.QRCodes.FirstOrDefault().WebURL))) : "",
                    Bookmarked = CurrentUserSession.Attende.BookMarks.Any(bm => bm.ActivityID == x.ID),
                    Attendees= x.BookMarks.Select(y => new { y.AttendesID,y.Attende.FirstName}),
                }).OrderBy(x => x.StartTime),
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> EventActivities(string Token, int eventId)
        {
            eventLogic = new EventLogic();
            var eventData = await eventLogic.GetEventById(eventId);
            return Json(new
            {
                // .Where(n => n.EndTime > DateTime.Now)
                Activites = eventData.Activites.Select(x => new
                {
                    x.Address,
                    x.latitude,
                    x.longitude,
                    CreateON = x.CreateON == null ? "not set" : x.CreateON.Value.ToShortDateString(),
                    x.Description,
                    StartDate = x.StartTime.ToShortDateString(),
                    EndDate= x.EndTime.ToShortDateString(),
                    EndTime = x.EndTime.ToShortTimeString(),
                    StartTime = x.StartTime.ToShortTimeString(),
                    x.Status,
                    x.Name,
                    IsCompleted = x.EndTime < DateTime.Now,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                    x.ID,
                    //QRCode = x.QRCodes.Any() ? string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath, true), x.QRCodes.FirstOrDefault().WebURL))) : "",
                    Bookmarked = CurrentUserSession.Attende.BookMarks.Any(bm => bm.ActivityID == x.ID),
                    Documents = x.Documents.Select(y => new
                    {
                        y.ID,
                        y.DocumentName,
                        FilePath = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.ApiDocumentPath(y.FilePath))
                    }),
                    FloorMapName = x.FloorMapLocation?.Location,
                    Attendees= x.BookMarks.Where(a => a.ActivityID == x.ID).Select(y => new { y.AttendesID,y.Attende.FirstName})
                }

                ).OrderBy(x => x.StartDate),
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get theme data for given eventid
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> EventTheme(string Token, int eventId)
        {
            eventLogic = new EventLogic();
            var eventData = await eventLogic.GetEventById(eventId);
            return Json(new
            {
                Theme = eventData.Themes.Select(th => new
                {
                    AppBackgroundURL = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath, true), th.AppBackgroundURL))),
                    ButtonURL = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath, true), th.ButtonURL))),
                    SplashScreemURL = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.APiImagePath(th.SplashScreemURL)),
                    th.ButtonBackgroundColor,
                    th.ButtonForegroundColor,
                    th.HeadingForegroundColor,
                    ContentText = th.LabelForegroundColor,
                    th.MenuText,
                    th.MenuBackground,
                    th.ContentBackground,
                    th.LinkText,
                }),
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get sponsors for given eventid
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> EventSponsors(string Token, int eventId)
        {
            eventLogic = new EventLogic();
            var eventData = await eventLogic.GetEventById(eventId);
            return Json(new
            {
                SponsorsEvents = eventData.SponsorsEvents.Select(sp => new
                {
                    sp.ID,
                    sp.Sponsor.Name,
                    Thumnail = imageToUrlConvertionHandler(sp.Sponsor.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(sp.Sponsor.Thumbnail)),
                })
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> EventQrCodes(string Token, int eventId)
        {
            eventLogic = new EventLogic();
            var eventData = await eventLogic.GetEventById(eventId);
            return Json(new
            {
                QRCodes = eventData.QRCodes.Select(qr => new
                {
                    CodePath = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath, true), qr.CodePath))),
                })
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> EventAttendies(string Token, int eventId)
        {
            eventLogic = new EventLogic();
            var eventData = await eventLogic.GetEventById(eventId);
            return Json(new
            {
                AttendesEvents = eventData.AttendesEvents.Select(ae => new
                {
                    ae.Attende.FirstName,
                    ae.Attende.Lastname,
                    ae.AttendesID,
                    ae.Attende.Email,
                    ae.Attende.Description,
                    Thumbnail = imageToUrlConvertionHandler(ae.Attende.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(ae.Attende.Thumbnail)),
                    ae.Attende.InstagramURL,
                    ae.Attende.FacebookURL,
                    ae.Attende.LinkedinURL,
                    ae.Attende.TwitterURL
                })
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get event data for specified event
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventId"></param>
        /// <returns>Retruns event data for specific event based on login</returns>
        [HttpGet]
        public async Task<ActionResult> GetEventById(string Token, int eventId)
        {
            try
            {
                eventLogic = new EventLogic();
                AuthLogic authLogic = new AuthLogic();
                authLogic.AddAttendeeBookMarksOfEvent(CurrentUserSession.Attende.Email, eventId);
                var eventData = await eventLogic.GetEventById(eventId);
                var data = eventLogic.createEventAPIEntity(eventData, imageToUrlConvertionHandler, CurrentUserSession.AttendesID.Value, IsAdmin, imageConvertionHandler);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ActionResult> RecentChatConverstaions(string Token, int eventId)
        {
            EventLogic eventLogic = new EventLogic();
            return Json(eventLogic.GetRecentChatConverstaions(eventId, x => string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.APiImagePath(x)), CurrentUserSession.AttendesID.Value, IsAdmin), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Used for refresh of home page
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventId"></param>
        /// <returns>Returns eventdata for specified event</returns>
        [HttpGet]
        public async Task<ActionResult> GetHomeDataByEventID(string Token, int eventId)
        {
            eventLogic = new EventLogic();
            var eventData = await eventLogic.GetEventById(eventId);
            return Json(eventLogic.CreateHomePageEntity(eventData, imageToUrlConvertionHandler, CurrentUserSession.AttendesID.Value, IsAdmin), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public async Task<ActionResult> GetEventInfoById(string Token, int eventId)
        //{
        //    eventLogic = new EventLogic();
        //    var eventData = await eventLogic.GetEventById(eventId);
        //    var data = await eventLogic.ActivitiesByEvent(CurrentUserSession.AttendesID.Value, eventId);
        //    var attdata = await eventLogic.ActivitiesByEvent(CurrentUserSession.AttendesID.Value, eventId);
        //    return Json(new
        //    {
        //        eventData.ID,
        //        eventData.EventName,
        //        eventData.ClientName,
        //        eventData.Destination,
        //        eventData.latitude,
        //        eventData.longitude,
        //        eventData.StartDate,
        //        eventData.EndDate,
        //        EventImage = Helpers.ImageToBase64(eventData.EventImage == "" ? "" : MvcCustom.APiImagePath(eventData.EventImage)),
        //        eventData.Message,
        //        eventData.EnableThemes,
        //        eventData.EnableAttendees,
        //        eventData.EnableGeneralAgenda,
        //        eventData.EnableComments,
        //        eventData.EnableFloormap,
        //        eventData.EnableQrCode,
        //        eventData.EnableSponsor,
        //        eventData.EnableGoogleApi,
        //        eventData.EnablePhotos,
        //        eventData.allow_bookmark,
        //        eventData.EnableSocialScreen,
        //        eventData.GeneralAgendaTitle,
        //        eventData.QrCodeTitle,
        //        eventData.AttendeeTitle,
        //        eventData.FloormapTitle,
        //        eventData.PhotosTitle,
        //        eventData.SponsorTitle,
        //        eventData.MapTitle,
        //        eventData.SocialTitle,
        //        eventData.SearchTitle,
        //        eventData.EnableSpeaker,
        //        eventData.SpeakerTitle,
        //        eventData.ExhibitorTitle,
        //        eventData.EnableExhibitor,
        //        eventData.VendorTitle,
        //        eventData.EnableVendor,
        //        //eventData.EnableChatscreen,
        //        //eventData.AttendeeMessagingEmails,
        //        //eventData.AlertEmails,
        //        eventData.OpenUrlInsideorOutsideApp,
        //        eventData.AllowTaggingPeopleandLocations,
        //        eventData.AllowPhotosBulkDownload,
        //        //eventData.OpenLinksExternallyorInternally,
        //        //eventData.DisplaySurveyforActivitywithAgenda,
        //        eventData.AutoPushNotLocationChange,
        //        eventData.AutoPushNotMinReminder,
        //        eventData.AutoPushNotTimeChange,
        //        eventData.EmailNotesusingAccountEmail,
        //        eventData.EmailFavouriteDocumentsusingAccountEmail,
        //        eventData.ShowAttendeewithSpeakers,
        //        eventData.OpenDocumentsWithinorOutsideURL,
        //        eventData.EnablePersonalAgenda,
        //        eventData.PersonalAgendaTitle,
        //        Theme = eventData.Themes.Select(th => new
        //        {
        //            AppBackgroundURL = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath, true), th.AppBackgroundURL))),
        //            ButtonURL = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath, true), th.ButtonURL))),
        //            SplashScreemURL = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.APiImagePath(th.SplashScreemURL)),

        //            th.ButtonBackgroundColor,
        //            th.ButtonForegroundColor,
        //            th.HeadingForegroundColor,
        //            th.LabelForegroundColor,
        //        }),
        //        SponsorsEvents = eventData.SponsorsEvents.Select(sp => new
        //        {
        //            sp.ID,
        //            sp.Sponsor.Name,
        //            sp.Sponsor.Description,
        //            Thumnail = Helpers.ImageToBase64(MvcCustom.APiImagePath(sp.Sponsor.Thumbnail)),
        //            //Helpers.ImageToBinary(string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath, true), sp.Sponsor.Thumbnail)))),
        //        }),
        //        AttendesEvents = eventData.AttendesEvents.Select(ae => new
        //        {
        //            ae.Attende.FirstName,
        //            ae.AttendesID,
        //            ae.Attende.Description,
        //            Thumbnail = Helpers.ImageToBase64(MvcCustom.APiImagePath(ae.Attende.Thumbnail)),
        //            //Helpers.ImageToBinary(string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath, true), ae.Attende.Thumbnail)))),
        //        }),
        //        Activites = eventData.Activites.Select(x => new
        //        {
        //            x.Address,
        //            CreateON = x.CreateON == null ? "not set" : x.CreateON.Value.ToShortDateString(),
        //            x.Description,
        //            //AttendeeActivities = attdata,
        //            StartDate = x.StartTime.ToShortDateString(),
        //            EndTime = x.EndTime.ToShortTimeString(),
        //            StartTime = x.StartTime.ToShortTimeString(),
        //            x.Status,
        //            x.Name,
        //            x.ID,
        //            Thumbnail = Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
        //            //Helpers.ImageToBinary(string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath, true), x.Thumbnail)))),
        //            QRCode = x.QRCodes.Any() ? string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content(string.Format("/{0}/{1}", Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath), x.QRCodes.FirstOrDefault().WebURL))) : "",
        //            Bookmarked = CurrentUserSession.Attende.BookMarks.Any(bm => bm.ActivityID == x.ID)
        //        }).OrderBy(x => x.StartDate),
        //        UserActivity = data,
        //        eventInformation = eventData.EventInformations.Select(y => new
        //            {
        //                y.HideSection,
        //                y.AboutSection,
        //                Thumbnail= Helpers.ImageToBase64(MvcCustom.APiImagePath(y.IconSection)),
        //                y.DescriptionSection,
        //                y.IsExternalLink,
        //                y.ExternalLinkURL
        //            })
        //    }, JsonRequestBehavior.AllowGet) ;
        //}
        /// <summary>
        /// To get count of notifications for given event and loggedin user
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> NotificationCount(string Token, int eventId)
        {
            Notifications notifications = new Notifications();
            var res = notifications.GetUnreadUserNotificationsForEvent(CurrentUserSession.AttendesID.Value, eventId).Count;
            return Json(new { Data = res }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Send notification to all attendees specific to event or activity
        /// </summary>
        /// <param name="token"></param>
        /// <param name="NotiText"></param>
        /// <param name="title"></param>
        /// <param name="Attendes"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task<ActionResult> SendNotification(string token, string NotiText, string title, int Attendes, string channel)
        {
            Entities db = new Entities();
            try
            {
                if (db.UserSessions.Any(x => x.AttendesID == Attendes))
                {
                    var device = db.UserSessions.Where(x => x.AttendesID == Attendes && x.DeviceToken != null && x.IsActive == true).ToList();
                    foreach (var item in device)
                    {
                        if (item.IsActive != false)
                        {
                            FcmResponse fr;
                            var result = "-1";
                            var webAddr = "https://fcm.googleapis.com/fcm/send";
                            var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "key=AAAAWm-Xg_E:APA91bGjR6GVwQARJu9b50Y726fkR7-V2IgLRvCPMPQrBoWUQkRrTeSpEb0BB8Eqzij1-nFi8WRSOgJHIi6_DvbSMUBajoCShwv5l7WmKAcyg2Z4K7XzPDECP3E7ItVQ22SvcjXT5cUv");
                            httpWebRequest.Method = "POST";
                            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                            {
                                string strNJson = "{\"to\": \"" + item.DeviceToken + "\",\"notification\": {\"title\": \"" + title + "\",\"text\": \"" + NotiText + "\",\"sound\":\"default\",\"click_action\":\"com.test.click\"},\"data\" {\"Variable1\":\"" + channel + "\"}}";

                                streamWriter.Write(strNJson);
                                streamWriter.Flush();
                            }

                            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                            }
                            var data = JsonConvert.DeserializeObject<FcmResponse>(result.ToString());


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
        /// To get all the documents for specific event and loggedin user
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetAllDocuments(string Token, int eventId)
        {
            EventLogic eventLogic = new EventLogic();
            var allDocs = await eventLogic.GetAllDocuments(eventId, CurrentUserSession.AttendesID.Value);
            return Json(allDocs.Select(x => new
            {
                DocumentID = x.ID,
                FilePath = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.ApiDocumentPath(x.FilePath)),
                x.DocumentName,
                IsFavorite = x.DocumentFavourites.Any(y => y.AttendeeID == CurrentUserSession.AttendesID.Value)
            }), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To add specified document into favourites
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="documentID"></param>
        /// <returns>Returns status</returns>
        public async Task<ActionResult> FavoriteDocument(string Token, int documentID)
        {
            EventLogic eventLogic = new EventLogic();
            await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Favourite Document", "Favourite Document", "Document has been Favourited Successfully");
            return Json(eventLogic.MarkDocumentAsFavourite(CurrentUserSession.AttendesID.Value, documentID) ? "Success" : "Document is already made favorite", JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// to remove a document from loggedin users favourite documents
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="documentID"></param>
        /// <returns>Returns status</returns>
        public async Task<ActionResult> RemoveFavoriteDocument(string Token, int documentID)
        {
            var status = eventLogic.RemoveDocumentFromFavorite(CurrentUserSession.AttendesID.Value, documentID);
            await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Removing Favourite Document", "Removing Favourite Document", "Document has been removed from Favourites");
            return Json(status ? "Success" : "Specified document is not in favorites", JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get all the favourite documents which are marked by loggedin user as his favourite
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetFavouriteDocuments(string Token, int eventID)
        {
            EventLogic eventLogic = new EventLogic();
            var favouriteDocs = await eventLogic.GetFavouriteDocs(CurrentUserSession.AttendesID.Value, eventID);
            return Json(favouriteDocs.Select(x => new
            {
                DocumentID = x.ID,
                FilePath = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.ApiDocumentPath(x.FilePath)),
                x.DocumentName
            }), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get all notes which are saved by loggedin Attendee or user
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="EventID"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetAllNotes(string Token, int EventID)
        {
            try
            {
                EventLogic eventLogic = new EventLogic();
                var notes = await eventLogic.GetNotes(CurrentUserSession.AttendesID.Value, EventID);
                return Json(notes.Select(x => new
                {
                    NoteID = x.ID,
                    x.Text,
                    x.UserID,
                    ActivityName = x.Activite?.Name,
                    ActivityID = x.Activite?.ID,
                    x.Activite?.StartTime,
                    x.Activite?.EndTime,
                    SponsorID = x.Sponsor?.ID,
                    SponsorName = x.Sponsor?.Name,
                    VendorID = x.Vendor?.ID,
                    VendorName = x.Vendor?.Name,
                    ExhibitorID = x.Exhibitor?.ID,
                    ExhibitorName = x.Exhibitor?.Name,
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// used by attendee to save a note
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="Notetext"></param>
        /// <param name="ActivityID"></param>
        /// <param name="SponsorID"></param>
        /// <param name="VendorID"></param>
        /// <param name="ExhibitorID"></param>
        /// <param name="EventID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> SaveNotes(string Token, string Notetext, int? ActivityID, int? SponsorID, int? VendorID, int? ExhibitorID, int? EventID, int? MarkerID)
        {
            try
            {
                EventLogic eventLogic = new EventLogic();
                await eventLogic.SaveNoteDetails(Notetext, CurrentUserSession.AttendesID.Value, ActivityID, SponsorID, VendorID, ExhibitorID, EventID, MarkerID);
                await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Notes", "Saving Notes", "Note has been saved successfully");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        /// <summary>
        /// Send email with all notes written by an attendee
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="EventID"></param>
        /// <returns></returns>
        public async Task<bool> EmailAllNotes(string Token, int EventID)
        {
            try
            {
                EventLogic eventLogic = new EventLogic();
                await eventLogic.EmailNotes(CurrentUserSession.AttendesID.Value, EventID);
                await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Notes", "Email Notes", "Emails has been sent successfully");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// Send email with all documents marked as favourites by an attendee
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public async Task<bool> EmailAllDocuments(string Token, int eventID)
        {
            try
            {
                var Alldocuments = await eventLogic.GetAllDocuments(eventID, CurrentUserSession.AttendesID.Value);
                var FavouriteDocuments = await eventLogic.GetFavouriteDocs(CurrentUserSession.AttendesID.Value, eventID);
                var documents = Alldocuments.ToList().Intersect(FavouriteDocuments.ToList()).ToList();
                if (documents.Count == 0)
                {
                    throw new Exception("No Documents available");
                }

                IList<FileStream> DocumentFiles = new List<FileStream>();
                foreach (var document in documents)
                {
                    string fullDocumentPath = Path.Combine(DocumentsSavePath, document.FilePath);
                    DocumentFiles.Add(System.IO.File.OpenRead(fullDocumentPath));
                }
                //await Helpers.SendEmail("Document", "Document", null, DocumentFile, null, true); 

                using (var EmailSender = new EmailSender(CurrentUserSession.Attende?.Email, "Document"))
                {
                    EmailSender.SendEmailWithAttachment(DocumentFiles.ToArray());
                    await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Documents", "Email Documents", "Documents has been mailed successfully");
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// Used in event info
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="attendeeEmail"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool ContactSupport(string Token, string attendeeEmail, string message)
        {
            return eventLogic.ContactSupport(attendeeEmail, message);
        }

        /// <summary>
        /// Download specific document
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        public async Task<ActionResult> DownloadDocument(int documentID)
        {
            var document = await this.eventLogic.getDocumentByID(documentID);
            string fullDocumentPath = Path.Combine(DocumentsSavePath, document.FilePath);
            await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Documents", "Download Documents", "Documents has been downloaded successfully");
            return File(fullDocumentPath, System.Net.Mime.MediaTypeNames.Application.Octet, document.FilePath);
        }

        /// <summary>
        /// To get the eventinfromation for given eventid
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventId"></param>
        /// <returns>Returns information about given event </returns>
        [HttpGet]
        public async Task<ActionResult> GetEventInformationByID(string Token, int eventId)
        {
            var eventData = await eventLogic.GetEventById(eventId);
            var eventinfo = eventData.EventInformations.Select(y => new
            {
                y.HideSection,
                y.AboutSection,
                Thumbnail = imageToUrlConvertionHandler(y.IconSection),
                //Helpers.ImageToBase64(MvcCustom.APiImagePath(y.IconSection)),
                y.DescriptionSection,
                y.IsExternalLink,
                y.ExternalLinkURL,
                y.SectionType,
                y.SectionOrder
            }).OrderBy(x => x.SectionOrder);

            return Json(eventinfo, JsonRequestBehavior.AllowGet);
        }

        public async Task SaveChatMessages(string Token, int SID, int RID, bool IsGroup, int EventGroupID, String msg)
        {
            EventLogic eventLogic = new EventLogic();
            await eventLogic.SaveChatMessages(SID, RID, IsGroup, EventGroupID, msg);
            await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Chat", "Save Chat Messages", "Chat Messages has been mailed successfully");
        }

        public async Task GetChatMessages(string Token, int SID, int RID, bool IsGroup)
        {
            EventLogic eventLogic = new EventLogic();
            await eventLogic.GetChatMessages(SID, RID, IsGroup);
        }

        public async Task EmailChatMessages(string Token, int UID, int PID, bool IsGroup)
        {
            EventLogic eventLogic = new EventLogic();
            await eventLogic.EmailChatMessages(UID, PID, IsGroup);
            await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Chat", "Email Chat Messages", "Chat Messages has been mailed successfully");
        }

        public async Task<ActionResult> SurveyQuestionswithOptions(string Token, bool IsAtEventLevel, bool IsAtAppLevel, int EventID)
        {
            EventLogic eventLogic = new EventLogic();
            var QWO = await eventLogic.QuestionswithOptions(IsAtEventLevel, IsAtAppLevel, CurrentUserSession.AttendesID.Value, EventID);
            return Json(QWO.Select(x => new
            {
                x.QID,
                x.QuestionText,
                x.IsAtActiivtyLevel,
                x.QuestionResponseType,
                x.HideorShowQuestion,
                x.options,
                x.IsSubmitted
            }), JsonRequestBehavior.AllowGet);

        }

        //public async Task<ActionResult> ActivitySurveyQuestionswithOptions(string Token)
        //{
        //    EventLogic eventLogic = new EventLogic();
        //    var QWO = await eventLogic.ActivitySurveyQuestions(CurrentUserSession.AttendesID.Value, Even);
        //    return Json(QWO.Select(x => new
        //    {
        //        x.QID,
        //        x.QuestionText,
        //        x.IsAtActiivtyLevel,
        //        x.QuestionResponseType,
        //        x.HideorShowQuestion,
        //        x.options,
        //        x.IsSubmitted
        //    }), JsonRequestBehavior.AllowGet);

        // }

        public async Task<bool> SaveSurvey(string Token, Response[] SurveyResponse, int? EventID, int? ActivityID, int? vendorID , int? sponsorID )
        {
            try
            {
                EventLogic eventLogic = new EventLogic();
                await eventLogic.AddNewSurvey(SurveyResponse, CurrentUserSession.AttendesID.Value, EventID, ActivityID,vendorID,sponsorID);
                await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Survey", "Survey", "Survey has been submitted successfully");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeletePhoto(int PhotoID)
        {
            bool status = eventLogic.DeletePhoto(PhotoID);
            return status;
        }

        public bool DeletePost(int PostID)
        {
            bool status = eventLogic.DeletePost(PostID);
            return status;
        }

        [HttpGet]
        public ActionResult GetWeatherDetails(string Token, int eventId, string locationName)
        {
            string weatherData = eventLogic.GetWeatherDetails(eventId, locationName);

            //var json = JsonConvert.DeserializeObject(weatherData);

            //var ss = new { weatherData };

            return Content(weatherData);

            //return Json(ss, JsonRequestBehavior.AllowGet);
        }

    }
}