using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using CommonHelpers;
using EventManagement.BusinessLogic.Business.Email_Notifier;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.BusinessLogic.Classes;
using EventManagement.BusinessLogic.Models;
using EventManagement.BusinessLogic.Models.APIDTOOjbects;
using EventManagement.BusinessLogic.Models.Enums;
using EventManagement.DataAccess.DataBase.Model;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace EventManagement.BusinessLogic.Business
{
    public class UserDeviceTokenComparer : IEqualityComparer<UserSession>
    {
        public bool Equals(UserSession x, UserSession y)
        {
            if (x.DeviceToken == y.DeviceToken)
                return true;

            return false;
        }

        public int GetHashCode(UserSession obj)
        {
            return obj.DeviceToken.GetHashCode();
        }
    }

    public class EventLogic : BaseLogic
    {
        Firechatlogic firechatlogic = new Firechatlogic();

        protected string googleplus_client_id = "458878619548-khuatamj3qpiccnsm4q6dbulf13jumva.apps.googleusercontent.com";    // Replace this with your Client ID
        
        protected string googleplus_client_secret = "4hiVJYlomswRd_PV5lyNQlfN";                                                // Replace this with your Client Secret
        
        protected string googleplus_redirect_url = "http://localhost:2443/Index.aspx";                                         // Replace this with your Redirect URL; Your Redirect URL from your developer.google application should match this URL.
        
        protected string Parameters;
        
        private Func<UserSession, bool> PushNotificationPredicate = x => x.IsActive == true && x.Platform != "Web" && !string.IsNullOrEmpty(x.DeviceToken);
        
        Notifications notifications = new Notifications();
        
        public async Task<object> ActivitiesByEvent(int attendeeId, int eventId)
        {
            var attendeeEvent = await Db.Events.
                FindAsync(eventId);
            var activities = Db.BookMarks.Where(x => x.AttendesID == attendeeId).Select(x => x.ActivityID).ToArray();
            //.Where(n => n.EndTime > DateTime.Now)
            return attendeeEvent.Activites.Where(x => activities.Contains(x.ID)).Select(x => new
            {
                x.Address,
                CreateON = x.CreateON == null ? "not set" : x.CreateON.Value.ToShortDateString(),
                x.Description,
                StartDate = x.StartTime.ToShortDateString(),
                StartTime = x.StartTime.ToShortTimeString(),
                EndTime = x.EndTime.ToShortTimeString(),
                x.Name,
                x.Status,
                x.Thumbnail,
                x.ID,
                QRCode = x.QRCodes.Any() ? x.QRCodes.FirstOrDefault().WebURL : string.Empty,
                IsCompleted = x.EndTime < DateTime.Now,
                //Documents = x.Documents.Select(y => new
                //{
                //    y.ID,
                //    y.DocumentName,
                //    FilePath = documentConvetionHandler(y.FilePath)
                //}),
                FloorMapName = x.FloorMapLocation?.Location
            }).OrderBy(x => x.StartDate);
        }

        public LoginEventData createEventAPIEntity(Event @event, Func<string, string> imageConvertionHandler, int attendeeID, bool isAdmin, Func<string, string> compressedImageConvertionHandler)
        {
            Firechatlogic flc = new Firechatlogic();

            var loginData = new LoginEventData
            {
                ID = @event.ID,
                ModeratePost = @event.ModeratePost,
                EventName = @event.EventName,
                GalacticEventId = @event.GalacticEventId,
                ManagerUserId = @event.ManagerEvent?.ManagerID,
                EnableThemes = @event.EnableThemes,
                EnableGeneralAgenda = @event.EnableGeneralAgenda,
                EnablePersonalAgenda = @event.EnablePersonalAgenda,
                EnableQrCode = @event.EnableQrCode,
                EnableAttendees = @event.EnableAttendees,
                EnableFloormap = @event.EnableFloormap,
                EnablePhotos = @event.EnablePhotos,
                EnableSponsor = @event.EnableSponsor,
                EnableComments = @event.EnableComments,
                EnableGoogleApi = @event.EnableGoogleApi,
                allow_bookmark = @event.allow_bookmark,
                EventImage = imageConvertionHandler(@event.EventImage),
                LeftMenuLogo = imageConvertionHandler(@event.LeftMenuLogo),
                EnableSocialScreen = @event.EnableSocialScreen,
                //EnableSearchScreen = @event.EnableSearchScreen,
                Destination = @event.Destination,
                ClientName = @event.ClientName,
                //StartDate = @event.StartDate,
                //EndDate = @event.EndDate,
                WebURL = @event.WebURL,
                GeneralAgendaTitle = @event.GeneralAgendaTitle,
                PersonalAgendaTitle = @event.PersonalAgendaTitle,
                QrCodeTitle = @event.QrCodeTitle,
                AttendeeTitle = @event.AttendeeTitle,
                FloormapTitle = @event.FloormapTitle,
                PhotosTitle = @event.PhotosTitle,
                SponsorTitle = @event.SponsorTitle,
                MapTitle = @event.MapTitle,
                SocialTitle = @event.SocialTitle,
                SearchTitle = @event.SearchTitle,
                longitude = @event.longitude,
                latitude = @event.latitude,
                EnableSpeaker = @event.EnableSpeaker,
                SpeakerTitle = @event.SpeakerTitle,
                EnableExhibitor = @event.EnableExhibitor,
                ExhibitorTitle = @event.ExhibitorTitle,
                EnableVendor = @event.EnableVendor,
                VendorTitle = @event.VendorTitle,
                GoogleFolderID = @event.GoogleFolderID,
                GoogleAlbumphotoUrl = @event.GoogleAlbumphotoUrl,
                EnableChatscreen = Db.Attendes.Find(attendeeID)?.EnableMessaging ?? false,
                //AlertEmails = @event.AlertEmails,
                //AttendeeMessagingEmails = @event.AttendeeMessagingEmails,
                OpenUrlInsideorOutsideApp = @event.OpenUrlInsideorOutsideApp,
                //DisableFeaturesNotinUse = @event.DisableFeaturesNotinUse,
                AllowTaggingPeopleandLocations = @event.AllowTaggingPeopleandLocations,
                //AllowPhotosBulkDownload = @event.AllowPhotosBulkDownload,
                //OpenLinksExternallyorInternally = @event.OpenLinksExternallyorInternally,
                AutoPushNotLocationChange = @event.AutoPushNotLocationChange,
                AutoPushNotMinReminder = @event.AutoPushNotMinReminder,
                AutoPushNotTimeChange = @event.AutoPushNotTimeChange,
                //Coordinates = @event.EventLocationDetails.Select(x => new { x.Coordinates, x.LocationDescription }),
                EmailNotesusingAccountEmail = @event.EmailNotesusingAccountEmail,
                EmailFavouriteDocumentsusingAccountEmail = @event.EmailFavouriteDocumentsusingAccountEmail,
                ShowAttendeewithSpeakers = @event.ShowAttendeewithSpeakers,
                OpenDocumentsWithinorOutsideURL = @event.OpenDocumentsWithinorOutsideURL,
                EnablePhotoDescription = @event.EnablePhotoDescription,
                EnableSurveys = @event.EnableSurveys,
                SurveyTitle = @event.SurveyTitle,
                EnableDocuments = @event.EnableDocuments,
                DocumentsTitle = @event.DocumentsTitle,
                EnableEventInfo = @event.EnableEventInfo,
                EventInfoTitle = @event.EventInfoTitle,
                EnableAnnouncements = @event.EnableAnnouncements,
                AnnouncementsTitle = @event.AnnouncementsTitle,
                HideAddPhotos = @event.HideAddPhotos,
                UnreadMessageCount = flc.GetUnReadMessageCountForEvent(attendeeID, @event.ID),
                Themes = @event.Themes.Select(n => new EventTheme
                {
                    AppBackgroundURL = n.AppBackgroundURL,
                    ButtonURL = n.ButtonURL,
                    SplashScreemURL = n.SplashScreemURL,
                    ButtonBackgroundColor = n.ButtonBackgroundColor,
                    ButtonForegroundColor = n.ButtonForegroundColor,
                    HeadingForegroundColor = n.HeadingForegroundColor,
                    //LabelForegroundColor = n.LabelForegroundColor,
                    ContentText = n.LabelForegroundColor,
                    MenuText = n.MenuText,
                    MenuBackground = n.MenuBackground,
                    ContentBackground = n.ContentBackground,
                    LinkText = n.LinkText
                })
                //HomePageConfig = @event.WidgetMappings.OrderBy(x => x.YLocation).ThenBy(x => x.XLocation).Select((x, idx) => new
                //{
                //    x.DisplayName,
                //    x.ExternalLink,
                //    x.isExternalLink,
                //    ImagePath = x.ImagePath != null ? imageConvertionHandler(x.ImagePath) : null,
                //    x.ID,
                //    x.Height,
                //    x.Width,
                //    PageType = x.Widget?.Name,
                //    Index = $"{x.YLocation},{x.XLocation}",
                //    IsImageWidget = x.WidgetType.ID == 1
                //}).ToList(),
                //UpcomingAgenda = @event.AttendesEvents.Where(y => y.Attende.ID == attendeeID)
                //                     .SelectMany(y => y.Attende.BookMarks.Select(z => z.Activite))
                //                     .OrderBy(y => y.StartTime)
                //                     .Where(y => y.StartTime > DateTime.Now)
                //                     .Take(3)
                //                     .Select(y => new
                //                     {
                //                         y.StartTime,
                //                         y.EndTime,
                //                         y.Name,
                //                         y.FloorRegionMapping?.FloorMapping?.FloorName
                //                     }),
                //Sponsors = @event.SponsorsEvents.Select(y => y.Sponsor).OrderBy(y => y.Packagetype).ThenBy(y => y.Name).Select(y => new
                //{
                //    y.ID,
                //    y.Name,
                //    Thumbnail = imageConvertionHandler(y.Thumbnail),
                //    y.Description,
                //    PackageName = y.UserPackageType.Name
                //}),
                //RecentDiscussions = @event.ChatChannels.Select(x =>
                //x.TextChats.Where(y => y.SenderID == attendeeID || y.ReceiverID == attendeeID)
                //.OrderByDescending(y => y.Createdate)
                //.FirstOrDefault()).Where(x => x != null).Select(x => new
                //{
                //    UserName = x.IsGroup ? "Admin" : x.Attende?.FirstName,
                //    x.TextMessage,
                //    x.ChatChannel?.ChannelID,
                //    Photo = x.IsGroup ? null : imageConvertionHandler(x.Attende.Thumbnail)
                //}),
                //UnreadNotificationsCount = @event.Notifications.Where(y => y.AttendesID == attendeeID && y.Isread == false).Count()
            };

            AssignHomeMetaData(loginData, @event, imageConvertionHandler, attendeeID, isAdmin);
            return loginData;
        }

        public HomePageData CreateHomePageEntity(Event @event, Func<string, string> imageConvertionHandler, int attendeeID, bool isAdmin)
        {
            return AssignHomeMetaData(new HomePageData(), @event, imageConvertionHandler, attendeeID, isAdmin);
        }

        public HomePageData AssignHomeMetaData(HomePageData homePageData, Event @event, Func<string, string> imageConvertionHandler, int attendeeID, bool isAdmin)
        {
            if (homePageData == null) throw new NullReferenceException();
            homePageData.StartDate = @event.StartDate;
            homePageData.EndDate = @event.EndDate;
            homePageData.Coordinates = @event.EventLocationDetails.Select(x => new
            {
                x.ID,
                x.Coordinates,
                x.LocationDescription,
                x.Name,
                MarkerImage1 = imageConvertionHandler(x.MarkerImage1),
                MarkerImage2 = imageConvertionHandler(x.MarkerImage2),
                x.Webpageurl,
                x.WebPageName,
                Note = x.Notes.FirstOrDefault(y => y.UserID == attendeeID && y.MarkerID == x.ID)?.Text
            });
            bool messagingStatus = Db.Attendes.FirstOrDefault(x => x.ID == attendeeID).EnableMessaging;
            if (!messagingStatus)
            {
                homePageData.HomePageConfig = @event.WidgetMappings.OrderBy(x => x.YLocation).ThenBy(x => x.XLocation).Where(x => x.WidgetID != 4 && x.WidgetID != 16).Select((x, idx) => new // removing the chat with staff if the logged in user is admin
                {
                    x.DisplayName,
                    x.ExternalLink,
                    x.isExternalLink,
                    ImagePath = x.ImagePath != null ? imageConvertionHandler(x.ImagePath) : null,
                    x.ID, 
                    x.Height,
                    x.Width,
                    PageType = x.Widget?.Name,
                    Index = $"{x.YLocation},{x.XLocation}",
                    IsImageWidget = x.WidgetType.ID == 1
                }).ToList();
            }
            else
            {
                homePageData.HomePageConfig = @event.WidgetMappings.OrderBy(x => x.YLocation).ThenBy(x => x.XLocation).Where(x => (!isAdmin || (isAdmin && x.WidgetID != 4))).Select((x, idx) => new // removing the chat with staff if the logged in user is admin
                {
                    x.DisplayName,
                    x.ExternalLink,
                    x.isExternalLink,
                    ImagePath = x.ImagePath != null ? imageConvertionHandler(x.ImagePath) : null,
                    x.ID,
                    x.Height,
                    x.Width,
                    PageType = x.Widget?.Name,
                    Index = $"{x.YLocation},{x.XLocation}",
                    IsImageWidget = x.WidgetType.ID == 1
                }).ToList();
            }

            homePageData.UpcomingAgenda = @event.AttendesEvents.Where(y => y.Attende.ID == attendeeID)
                                 .SelectMany(y => y.Attende.BookMarks.Select(z => z.Activite).Where(z => z.EventID == @event.ID))
                                 .OrderBy(y => y.StartTime)
                                 .Where(y => y.StartTime > DateTime.Now)
                                 .Take(3)
                                 .Select(y => new
                                 {
                                     y.ID,
                                     y.StartTime,
                                     y.EndTime,
                                     y.Name,
                                     y.FloorMapLocation?.FloorRegionMappings.FirstOrDefault()?.FloorMapping?.FloorName
                                 });
            homePageData.Sponsors = @event.SponsorsEvents.Select(y => y.Sponsor).OrderBy(y => y.SponsorsEvents.FirstOrDefault(a => a.EventID == @event.ID)?.packagetype).ThenBy(y => y.Name).Select(y => new
            {
                y.ID,
                y.Name,
                Thumbnail = imageConvertionHandler(y.Thumbnail),
                y.Description,
                PackageName = y.SponsorsEvents.FirstOrDefault(a => a.EventID == @event.ID)?.UserPackageType?.Name
            });

            homePageData.VendorBannerHomepage = @event.VendorsEvents.Select(y => y.Vendor).OrderBy(y => y.Name).Select(y => new
            {
                y.ID,
                y.Name,
                Thumbnail = imageConvertionHandler(y.Thumbnail)
            });

            homePageData.RecentDiscussions = @event.ChatChannels.Where(x => x.ChannelUsers.Any(y => y.AttendeeID == attendeeID)).Select(x =>
              x.TextChats
              .OrderByDescending(y => y.Createdate)
              .FirstOrDefault()).Where(x => x != null).OrderByDescending(x => x.Createdate).Select(x => new RecentConversations
              {
                  UserName = x.IsGroup ? (!isAdmin ? "Admin" : GetNonAdminInAChannel(x.ChannelID.Value)?.FirstName) : x.Attende?.FirstName,    //(x.SenderID == attendeeID ? x.Attende?.FirstName : x.Attende1?.FirstName),
                  TextMessage = x.TextMessage,
                  ChannelID = x.ChatChannel?.ChannelID,
                  AttendeeID = x.SenderID == attendeeID ? x.ReceiverID : x.SenderID,
                  IsAdmin = x.IsGroup,
                  CreatedDateTime = x.Createdate,
                  Photo = x.IsGroup ? (
                        !isAdmin ? imageConvertionHandler(x.Attende1?.Thumbnail)
                        : imageConvertionHandler(GetNonAdminInAChannel(x.ChannelID.Value)?.Thumbnail))
                        : imageConvertionHandler(x.SenderID == attendeeID ? x.Attende?.Thumbnail : x.Attende1?.Thumbnail)
                  //UserName = x.Attende1?.FirstName,
                  //x.TextMessage,
                  //x.ChatChannel?.ChannelID,
                  //AttendeeID = x.SenderID == attendeeID ? x.ReceiverID : x.SenderID,
                  //IsAdmin = x.IsGroup,
                  //Photo = x.IsGroup ? null : imageConvertionHandler(x.Attende.Thumbnail)
              }).Take(3);

            List<RecentMessage> UserChat = firechatlogic.GetRecentChatConversation(attendeeID, @event.ID).Take(3).ToList();
            foreach (var chat in UserChat)
            {
                chat.Photo = imageConvertionHandler(chat.Photo);
            }

            homePageData.UnreadNotificationsCount = Db.Notifications.AsNoTracking().Where(y => y.EventID == @event.ID && y.UserNotifications.Any(z => z.AttendeeID == attendeeID && !z.Isread)).Count();
            homePageData.eventVideos = @event.EventVideos.Select(x => new { x.ID, x.VideoDescription, x.VideoPath });

            homePageData.SocialFeed = Db.SocialForums.Where(x => x.EventID == @event.ID).Select(x => new { x.ID, x.URL, x.SocialForumDescription, x.PostTypeid, postedBy = x.AttendesID });

            List<SocialForum> social  = Db.SocialForums.Where(x => x.EventID == @event.ID).OrderByDescending(x => x.ID).Take(3).ToList();

            foreach(var x in social)
            {
                if(!string.IsNullOrEmpty(x.URL))
                {
                    x.URL = imageConvertionHandler(x.URL);
                }
            }

            homePageData.RecentSocialPosts = social.Select(x => new
            {
                x.ID,
                x.SocialForumDescription,
                x.UploadBY,
                x.URL
            });   
            
            //homePageData.RecentSocialPosts = Db.SocialForums.Where(x => x.EventID == @event.ID)
            //                                   .Select(y => new
            //                                   {
            //                                       y.ID,
            //                                       y.SocialForumDescription,
            //                                       Thumbnail = imageConvertionHandler(y.URL),
            //                                       PostedBy = y.UploadBY,
            //                                       Postedon= y.UploadON
            //                                   }).OrderByDescending(z => z.Postedon).Take(3);
            return homePageData;
        }

        /// <summary>
        /// TODO: We need to re look into this logic. LOgic is little confusing and need to remove duplicate logic
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="imageConvertionHandler"></param>
        /// <param name="attendeeID"></param>
        /// <returns></returns>
        public List<RecentConversations> GetRecentChatConverstaions(int EventID, Func<string, string> imageConvertionHandler, int attendeeID, bool isAdminLoggedIn)
        {
            try
            {
                var eventsdata = Db.Events.Find(EventID);
                List<RecentConversations> chat = eventsdata.ChatChannels.Where(x => x.ChannelUsers.Any(y => y.AttendeeID == attendeeID)).Select(x =>
                                 x.TextChats
                                   .OrderByDescending(y => y.Createdate)
                                   .FirstOrDefault()).Where(x => x != null).OrderByDescending(x => x.Createdate).Select(x => new RecentConversations
                                   {
                                       UserName = x.IsGroup ? (!isAdminLoggedIn ? "Admin" : GetNonAdminInAChannel(x.ChannelID.Value)?.FirstName) : (x.SenderID == attendeeID ? x.Attende?.FirstName : x.Attende1?.FirstName),
                                       Company = x.IsGroup ? (!isAdminLoggedIn ? null : GetNonAdminInAChannel(x.ChannelID.Value)?.CompanyName) : (x.SenderID == attendeeID ? x.Attende?.CompanyName : x.Attende1?.CompanyName),
                                       JobTitle = x.IsGroup ? (!isAdminLoggedIn ? null : GetNonAdminInAChannel(x.ChannelID.Value)?.TitleName) : (x.SenderID == attendeeID ? x.Attende?.TitleName : x.Attende1?.TitleName),
                                       TextMessage = x.TextMessage,
                                       ChannelID = x.ChatChannel?.ChannelID,
                                       AttendeeID = x.SenderID == attendeeID ? x.ReceiverID : x.SenderID,
                                       IsAdmin = x.IsGroup,
                                       CreatedDateTime = x.Createdate,
                                       Photo = x.IsGroup ?
                                       (!isAdminLoggedIn ?
                                            imageConvertionHandler(x.Attende1?.Thumbnail)
                                            : imageConvertionHandler(GetNonAdminInAChannel(x.ChannelID.Value)?.Thumbnail))
                                       : imageConvertionHandler(x.SenderID == attendeeID ? x.Attende?.Thumbnail : x.Attende1?.Thumbnail)
                                   }).ToList();
                return chat;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Attende GetNonAdminInAChannel(int cID)
        {
            return Db.ChatChannels.Find(cID).ChannelUsers.FirstOrDefault(x => !x.Attende.IsAdmin)?.Attende;
        }

        public GooglePhotoDTO GoogleAlbumApi(string albumName)
        {
            var client = new RestClient(string.Format("{0}", "https://photoslibrary.googleapis.com/v1/albums"));
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", string.Format("{ \'album\': { \'title\': \'{0}\'}}", albumName), ParameterType.RequestBody);
            IRestResponse<GooglePhotoDTO> response2 = client.Execute<GooglePhotoDTO>(request);
            return response2.Data;
        }


        public async Task<IEnumerable<Event>> GetAllEvents(int? userID)
        {
            return await Db.Events.Where(x => x.ManagerEvent.ManagerID == userID.Value).ToListAsync();
        }
        
        public async Task<IEnumerable<Notification>> GetAllNotificationHistory(int? eventid)
        {
            return await Db.Notifications.Where(x => x.EventID == eventid).ToListAsync();
        }
        
        public async Task<IEnumerable<Photo>> GetAllEventPhoto(int? eventid)
        {
            return await Db.Photos.Where(x => x.EventID == eventid && !x.Status).ToListAsync();
        }

        public async Task<Event> GetEventById(int id)
        {
            if (id > 0)
            {
                var events = await Db.Events.Include(x => x.EventInformations).ToListAsync();
                return events.FirstOrDefault(x => x.ID == id);
            }
            else
                return new Event() { ManagerEvent = new ManagerEvent() };
        }
        
        public async Task<Activite> GetActivityById(int id)
        {
            if (id > 0)
            {
                var activiteData = await Db.Activites.FindAsync(id);
                //var findevent = Db.AttendesEvents.Any(x => x.EventID == activiteData.EventID && x.AttendesID == attendeeid);
                if (activiteData != null)
                {
                    return activiteData;
                }
                else
                {
                    return activiteData;

                }

            }
            else
            {

                return new Activite();
            }

        }
        
        public async Task<int> AddUpdateEvent1(Event saveEvent)
        {
            if (saveEvent.ID > 0)
            {
                if (await Db.Events.AnyAsync(x => x.GalacticEventId == saveEvent.GalacticEventId && x.ID != saveEvent.ID))
                    throw new Exception("Event with Galactic id already exist");
                var updateEvent = await Db.Events.FindAsync(saveEvent.ID);
                updateEvent.EventName = saveEvent.EventName;
                updateEvent.GalacticEventId = saveEvent.GalacticEventId;
                updateEvent.ModeratePost = saveEvent.ModeratePost;
                updateEvent.Message = saveEvent.Message;
                updateEvent.ClientName = saveEvent.ClientName;
                updateEvent.Destination = saveEvent.Destination;
                updateEvent.ManagerEvent = saveEvent.ManagerEvent;
                updateEvent.EnableGeneralAgenda = saveEvent.EnableGeneralAgenda;
                updateEvent.EnablePersonalAgenda = saveEvent.EnablePersonalAgenda;
                //updateEvent.latitude = Convert.ToInt32(latitude);
                //updateEvent.longitude = Convert.ToInt32(longitude);
                updateEvent.EnableAttendees = saveEvent.EnableAttendees;
                updateEvent.EnableComments = saveEvent.EnableComments;
                updateEvent.EnableFloormap = saveEvent.EnableFloormap;
                updateEvent.EnableGoogleApi = saveEvent.EnableGoogleApi;
                updateEvent.EnablePhotos = saveEvent.EnablePhotos;
                updateEvent.EnableQrCode = saveEvent.EnableQrCode;
                updateEvent.EnableSocialScreen = saveEvent.EnableSocialScreen;
                updateEvent.EnableSponsor = saveEvent.EnableSponsor;
                //updateEvent.EnableSearchScreen = saveEvent.EnableSearchScreen;
                updateEvent.EnableThemes = saveEvent.EnableThemes;
                updateEvent.allow_bookmark = saveEvent.allow_bookmark;
                if (!string.IsNullOrEmpty(saveEvent.EventImage))
                    updateEvent.EventImage = saveEvent.EventImage;
                await Db.SaveChangesAsync();
                return saveEvent.ID;
                //update code goes here
            }
            else
            {
                //save event
                if (await Db.Events.AnyAsync(x => x.GalacticEventId == saveEvent.GalacticEventId))
                    throw new Exception("Event with Galactic id already exist");
                Db.Events.Add(saveEvent);
                //Default Theme
                Theme th = new Theme
                {
                    ButtonBackgroundColor = "#31a3dd",
                    ButtonForegroundColor = "#ffffff"
                };
                Db.Themes.Add(th);
                await Db.SaveChangesAsync();
                return saveEvent.ID;
            }
        }
        
        public async Task<int> AddUpdateEvent(Event saveEvent)
        {
            if (saveEvent.ID > 0)
            {
                if (await Db.Events.AnyAsync(x => x.GalacticEventId == saveEvent.GalacticEventId && x.ID != saveEvent.ID))
                    throw new GalacticEventExistsException("Event with Galactic id already exist");
                var updateEvent = await Db.Events.FindAsync(saveEvent.ID);
                updateEvent.EventName = saveEvent.EventName;
                updateEvent.GalacticEventId = saveEvent.GalacticEventId;
                updateEvent.ModeratePost = saveEvent.ModeratePost;
                updateEvent.Message = saveEvent.Message;
                updateEvent.ClientName = saveEvent.ClientName;
                if (updateEvent.Destination != saveEvent.Destination)
                {
                    await SendEventNotifications(updateEvent, "Event Destination has changed", $"Event Destination has changed to {saveEvent.Destination}", 0);
                }
                updateEvent.Destination = saveEvent.Destination;
                if (updateEvent.ManagerEvent == null)
                {
                    updateEvent.ManagerEvent = saveEvent.ManagerEvent;
                }
                else
                {
                    updateEvent.ManagerEvent.ManagerID = saveEvent.ManagerEvent.ManagerID;
                }
                updateEvent.EnableGeneralAgenda = saveEvent.EnableGeneralAgenda;
                updateEvent.EnablePersonalAgenda = saveEvent.EnablePersonalAgenda;
                updateEvent.latitude = saveEvent.latitude;
                updateEvent.longitude = saveEvent.longitude;
                updateEvent.WebURL = saveEvent.WebURL;
                updateEvent.EnableAttendees = saveEvent.EnableAttendees;
                updateEvent.EnableComments = saveEvent.EnableComments;
                updateEvent.EnableFloormap = saveEvent.EnableFloormap;
                updateEvent.EnableGoogleApi = saveEvent.EnableGoogleApi;
                updateEvent.EnablePhotos = saveEvent.EnablePhotos;
                updateEvent.EnableQrCode = saveEvent.EnableQrCode;

                if (updateEvent.StartDate != saveEvent.StartDate)
                {
                    await SendEventNotifications(updateEvent, "Event Start Time has changed", $"Event Start Time has changed to {saveEvent.StartDate}", 0);
                }
                if (updateEvent.EndDate != saveEvent.EndDate)
                {
                    await SendEventNotifications(updateEvent, "Event End Time has changed", $"Event End Time has changed to {saveEvent.EndDate}", 0);
                }

                updateEvent.StartDate = saveEvent.StartDate;
                updateEvent.EndDate = saveEvent.EndDate;
                updateEvent.EnableSocialScreen = saveEvent.EnableSocialScreen;
                updateEvent.EnableSponsor = saveEvent.EnableSponsor;
                //updateEvent.EnableSearchScreen = saveEvent.EnableSearchScreen;
                updateEvent.EnableThemes = saveEvent.EnableThemes;
                updateEvent.allow_bookmark = saveEvent.allow_bookmark;
                updateEvent.GeneralAgendaTitle = saveEvent.GeneralAgendaTitle;
                updateEvent.PersonalAgendaTitle = saveEvent.PersonalAgendaTitle;
                updateEvent.AttendeeTitle = saveEvent.AttendeeTitle;
                updateEvent.MapTitle = saveEvent.MapTitle;
                updateEvent.PhotosTitle = saveEvent.PhotosTitle;
                updateEvent.QrCodeTitle = saveEvent.QrCodeTitle;
                updateEvent.SponsorTitle = saveEvent.SponsorTitle;
                updateEvent.FloormapTitle = saveEvent.FloormapTitle;
                updateEvent.SearchTitle = saveEvent.SearchTitle;
                updateEvent.SocialTitle = saveEvent.SocialTitle;
                updateEvent.EnableSpeaker = saveEvent.EnableSpeaker;
                updateEvent.SpeakerTitle = saveEvent.SpeakerTitle;
                updateEvent.ExhibitorTitle = saveEvent.ExhibitorTitle;
                updateEvent.EnableExhibitor = saveEvent.EnableExhibitor;
                updateEvent.VendorTitle = saveEvent.VendorTitle;
                updateEvent.EnableVendor = saveEvent.EnableVendor;
                //updateEvent.EnableChatscreen = saveEvent.EnableChatscreen;
                //updateEvent.AttendeeMessagingEmails = saveEvent.AttendeeMessagingEmails;
                //updateEvent.AlertEmails = saveEvent.AlertEmails;
                updateEvent.OpenUrlInsideorOutsideApp = saveEvent.OpenUrlInsideorOutsideApp;
                updateEvent.AllowTaggingPeopleandLocations = saveEvent.AllowTaggingPeopleandLocations;
                //updateEvent.AllowPhotosBulkDownload = saveEvent.AllowPhotosBulkDownload;
                //updateEvent.OpenLinksExternallyorInternally = saveEvent.OpenLinksExternallyorInternally;
                //updateEvent.DisplaySurveyforActivitywithAgenda = saveEvent.DisplaySurveyforActivitywithAgenda;
                updateEvent.AutoPushNotLocationChange = saveEvent.AutoPushNotLocationChange;
                updateEvent.AutoPushNotMinReminder = saveEvent.AutoPushNotMinReminder;
                updateEvent.AutoPushNotTimeChange = saveEvent.AutoPushNotTimeChange;
                updateEvent.EmailNotesusingAccountEmail = saveEvent.EmailNotesusingAccountEmail;
                updateEvent.EmailFavouriteDocumentsusingAccountEmail = saveEvent.EmailFavouriteDocumentsusingAccountEmail;
                updateEvent.ShowAttendeewithSpeakers = saveEvent.ShowAttendeewithSpeakers;
                updateEvent.OpenDocumentsWithinorOutsideURL = saveEvent.OpenDocumentsWithinorOutsideURL;
                updateEvent.EnablePhotoDescription = saveEvent.EnablePhotoDescription;
                updateEvent.HideAddPhotos = saveEvent.HideAddPhotos;

                updateEvent.EnableSurveys = saveEvent.EnableSurveys;
                updateEvent.SurveyTitle = saveEvent.SurveyTitle;

                updateEvent.EnableDocuments = saveEvent.EnableDocuments;
                updateEvent.DocumentsTitle = saveEvent.DocumentsTitle;

                updateEvent.EnableEventInfo = saveEvent.EnableEventInfo;
                updateEvent.EventInfoTitle = saveEvent.EventInfoTitle;

                updateEvent.EnableAnnouncements = saveEvent.EnableAnnouncements;
                updateEvent.AnnouncementsTitle = saveEvent.AnnouncementsTitle;

                if (!string.IsNullOrEmpty(saveEvent.EventImage))
                    updateEvent.EventImage = saveEvent.EventImage;
                if (!string.IsNullOrEmpty(saveEvent.LeftMenuLogo))
                    updateEvent.LeftMenuLogo = saveEvent.LeftMenuLogo;

                updateEvent.EnableVendorNameSort = saveEvent.EnableVendorNameSort;
                updateEvent.VendorSortBy = saveEvent.VendorSortBy;
                updateEvent.EnableVendorTypeSort = saveEvent.EnableVendorTypeSort;
                updateEvent.VendorTypeSortOrder = saveEvent.VendorTypeSortOrder;

                updateEvent.EnableSponsorNameSort = saveEvent.EnableSponsorNameSort;
                updateEvent.SponsorSortBy = saveEvent.SponsorSortBy;
                updateEvent.EnableSponsorTypeSort = saveEvent.EnableSponsorTypeSort;
                updateEvent.SponsorTypeSortOrder = saveEvent.SponsorTypeSortOrder;
                updateEvent.EventTimeZone = saveEvent.EventTimeZone;
                updateEvent.ScheduledActivityNotificationsTime = saveEvent.ScheduledActivityNotificationsTime;
                updateEvent.EnableVendorSurvey = saveEvent.EnableVendorSurvey;
                updateEvent.EnableSponsorSurvey = saveEvent.EnableSponsorSurvey;
                
                await Db.SaveChangesAsync();
                return saveEvent.ID;

                //update code goes here
            }
            else
            {
                //save event
                if (await Db.Events.AnyAsync(x => x.GalacticEventId == saveEvent.GalacticEventId))
                    throw new GalacticEventExistsException("Event with Galactic id already exist");

                // var GoogleEvent = GoogleAlbumApi(saveEvent.EventName);
                //saveEvent.GoogleAlbumphotoUrl = GoogleEvent.productUrl;
                //saveEvent.GoogleFolderID = GoogleEvent.title;
                saveEvent.GeneralAgendaTitle = "General Agenda";
                saveEvent.PersonalAgendaTitle = "Personal Agenda";
                saveEvent.AttendeeTitle = "Attendee";
                saveEvent.MapTitle = "Maps";
                saveEvent.QrCodeTitle = "QR Code";
                saveEvent.SponsorTitle = "Sponsors";
                saveEvent.FloormapTitle = "Floor Map";
                saveEvent.SearchTitle = "Search Screen";
                saveEvent.SocialTitle = "Social Screen";
                saveEvent.PhotosTitle = "Photos";
                saveEvent.SpeakerTitle = "Speakers";
                saveEvent.VendorTitle = "Vendors";
                saveEvent.ExhibitorTitle = "Exhibitors";
                //Default Theme
                Theme th = new Theme
                {
                    ButtonBackgroundColor = "#31a3dd",
                    ButtonForegroundColor = "#ffffff",
                    LabelForegroundColor = "#000000",
                    HeadingForegroundColor = "#000000"
                };
                Db.Themes.Add(th);
                var widgets = Db.Widgets.ToList();
                for (var i = 0; i < widgets.Count(); i++)
                {
                    var XLocation = 0;
                    var YLocation = i;
                    WidgetMapping wM = new WidgetMapping
                    {
                        XLocation = XLocation,
                        YLocation = YLocation,
                        WidgetID = widgets[i].ID,
                        DisplayName = widgets[i].Name,
                        widgetTypeID = widgets[i].defaultWidgetType,
                        Width = 2,
                        Height = 1
                    };
                    saveEvent.WidgetMappings.Add(wM);
                }
                //var eventInfo = new EventInfo();
                //eventInfo.AboutEventTitle = "About Event";
                //eventInfo.EnableAboutEvent = true;
                //eventInfo.EnableAccomodations = true;
                //eventInfo.EnableAppHelpDesk = true;
                //eventInfo.EnableArrivalInfo = true;
                //eventInfo.EnableThingsToKnow = true;
                //eventInfo.AccomodationsTitle = "Accomidations";
                //eventInfo.ArrivalInfoTitle = "Arrival Information";
                //eventInfo.HelpDeskTitle = "Help Desk";
                //eventInfo.ThingsToKnowTitle = "Things to Know";
                //saveEvent.EventInfo = eventInfo;
                Db.Events.Add(saveEvent);

                await Db.SaveChangesAsync();
                return saveEvent.ID;
            }
        }

        public async Task SendEventNotifications(Event eventObj, string title, string message, int sentBy)
        {
            var attendees = eventObj.AttendesEvents.Select(x => x.Attende).Distinct(new AttendeeComparer());
            foreach (var attende in attendees)
            {
                var distinctDeviceTokenSessions = attende.BookMarks.SelectMany(x => x.Attende.UserSessions).Where(x => x.IsActive == true && x.Platform != "Web" && !string.IsNullOrEmpty(x.DeviceToken)).Distinct(new UserDeviceTokenComparer()).ToList();
                Notifications notifications = new Notifications();
                var notificationID = await notifications.MaintainNotificationHistory(eventObj.ID, title, message, sentBy);
                await SendNotificationForSessions(distinctDeviceTokenSessions, title, message, eventObj.ID, sentBy, notificationID);
            }
        }

        public async Task<IEnumerable<EventPhotosDto>> GetEventFloorMaps(int eventId)
        {
            var data = await Db.FloorMappings.Where(x => x.EventID == eventId).OrderBy(x => x.ID).ToListAsync();

            return data.Select(x => new EventPhotosDto
            {
                Name = x.FloorName,
                ID = x.ID,

                Url = x.PhotoURL,
                PhotoHeight = x.PhotoHeight,
                PhotoWidth = x.PhotoWidth,
                Comments = x.FloorRegionMappings.Select(frm => new FloorRegionComments { Description = frm.Description, Regioncolor = frm.Regioncolor, X = (int)frm.X, Y = (int)frm.Y })
                //Comments = x.FloorRegionMappings.Select(frm => new FloorRegionComments { Description = frm.Description, Regioncolor = frm.Regioncolor, X = (int)frm.X, Y = (int)frm.Y, Height = (int)frm.height, Width = (int)frm.Width })
            });
        }

        public async Task<dynamic> GetEventQrCodes(int? id)
        {
            return await Db.QRCodes.Where(x => x.EventID == id && x.AtivitityID == null).ToListAsync();
        }

        public async Task<bool> CheckEvent(int id)
        {
            var res = Db.Events.Any(x => x.GalacticEventId == id);
            if (res)
                return true;
            else
                return false;
        }

        public async Task<List<SocialForum>> GetComments(int? id)
        {
            return await Db.SocialForums.Where(x => x.EventID == id).ToListAsync();
        }
        
        public async Task<List<SocialForum>> PendingComments(int? id)
        {
            return await Db.SocialForums.Where(x => x.EventID == id).ToListAsync();
        }
        
        public async Task<List<SocialForum>> GetRequest(int? id)
        {
            return await Db.SocialForums.Where(x => x.EventID == id && x.DeleteRequest == true).ToListAsync();
        }

        public async Task<IEnumerable<Photo>> GetEventPhotos(int eventId)
        {
            var data = await Db.Photos.Where(x => x.EventID == eventId && x.isDeleted == false && x.AlbumID == null && (!x.Event.ModeratePost || x.Status)).OrderBy(x => x.ID).ToListAsync();
            return data;
        }

        public async Task<dynamic> ActivitiesType()
        {
            return await Db.ActivityTPYES.ToListAsync();
        }

        public IEnumerable<Activite> GetEeventActivities(int? id)
        {
            return Db.Activites.Where(x => x.EventID == id).ToList();
        }
        
        public IEnumerable<Attende> GetEeventAttendes(int? id)
        {
            // return Db.AttendesEvents.Where(x => x.EventID == id).Select(x => x.Attende).ToList();
            return Db.Attendes.Where(x => x.AttendesEvents.Any(y => y.EventID == id)).ToList();
        }

        public async Task<IEnumerable<Theme>> GetEeventThemes(int? id)
        {
            return await Db.Themes.Where(x => x.EventID == id).ToListAsync();
        }

        public async Task AddUpdateActivity(Activite activite, string FloorMapLocation, int cmsUserID)
        {
            int? floorMapLocationID = GetFloorMapLocationID(FloorMapLocation, activite.EventID.Value);

            if (activite.ID > 0)
            {
                var updateactivity = await Db.Activites.FindAsync(activite.ID);
                string[] attendeemails = Db.Attendes.Select(x => x.Email).ToArray();
                var attendeEmailConc = attendeemails.Where(x => !string.IsNullOrEmpty(x)).Aggregate((x, y) => $"{x},{y}");
                if (updateactivity.EndTime != activite.EndTime && (updateactivity.Event.AutoPushNotTimeChange))
                {
                    await sendNotification("End time has been changed for '" + activite.Name + "'", "End time for '" + activite.Name + "' has been changed to '" + activite.EndTime + "'", attendeEmailConc, updateactivity, cmsUserID);
                }
                if (updateactivity.StartTime != activite.StartTime && (updateactivity.Event.AutoPushNotTimeChange))
                {
                    await sendNotification("Start time has been changed for '" + activite.Name + "'", "Start time for '" + activite.Name + "' has been changed to '" + activite.StartTime + "'", attendeEmailConc, updateactivity, cmsUserID);
                }
                if (updateactivity.Address != activite.Address && (updateactivity.Event.AutoPushNotLocationChange))
                {
                    await sendNotification("Address has been changed for '" + activite.Name + "'", "Address for '" + activite.Name + "' has been changed to '" + activite.Address + "'", attendeEmailConc, updateactivity, cmsUserID);
                }
                updateactivity.StartTime = activite.StartTime;
                updateactivity.EndTime = activite.EndTime;
                updateactivity.Name = activite.Name;
                updateactivity.latitude = activite.latitude;
                updateactivity.longitude = activite.longitude;
                updateactivity.GalacticActivityId = activite.GalacticActivityId;
                updateactivity.Description = activite.Description;
                updateactivity.Status = activite.Status;
                updateactivity.Address = activite.Address;
                if (!string.IsNullOrEmpty(activite.Thumbnail))
                    updateactivity.Thumbnail = activite.Thumbnail;
                updateactivity.FloorMapLocationID = floorMapLocationID;

                updateactivity.HideDate = activite.HideDate;
                updateactivity.HideTime = activite.HideTime;
                updateactivity.HideLocation = activite.HideLocation;
                updateactivity.HideMap = activite.HideMap;
                updateactivity.HideDescription = activite.HideDescription;
                updateactivity.HidePhoto = activite.HidePhoto;
                updateactivity.HideAddNotes = activite.HideAddNotes;
                updateactivity.HideAttendees = activite.HideAttendees;
                updateactivity.HideDocuments = activite.HideDocuments;
                updateactivity.HideSurvey = activite.HideSurvey;
                updateactivity.HideSpeaker = activite.HideSpeaker;
                await Db.SaveChangesAsync();
                //update
            }
            else
            {
                activite.FloorMapLocationID = floorMapLocationID;
                Db.Activites.Add(activite);
                await Db.SaveChangesAsync();
                //created
            }
        }

        public async Task<bool> sendNotification(string subject, string body, string concatinatedEmails, Activite activity, int sentBy)
        {
            //using (var EmailNotifier = new EmailNotifier(concatinatedEmails))
            //{
            //    EmailNotifier.Subject = subject;
            //    EmailNotifier.eBody = body;
            //    EmailNotifier.SendEmail(Db.Configs.FirstOrDefault()?.SupportEmail);
            //}
            await pushActivityAttendeeNotifications(activity, subject, body, sentBy);
            return true;
        }

        public int? GetFloorMapLocationID(string FloorMapLocation, int eventID)
        {
            if (string.IsNullOrEmpty(FloorMapLocation))
            {
                return null;
            }

            var floorMapLocationID = Db.FloorMapLocations.Where(x => x.eventID == eventID).FirstOrDefault(x => x.Location == FloorMapLocation)?.ID;
            if (floorMapLocationID == null)
            {
                var floorDirection = Db.FloorMapLocations.Add(new DataAccess.DataBase.Model.FloorMapLocation()
                {
                    Location = FloorMapLocation,
                    eventID = eventID
                });
                Db.SaveChanges();
                floorMapLocationID = floorDirection.ID;
            }
            return floorMapLocationID;
        }

        public async Task pushActivityAttendeeNotifications(Activite activity, string title, string message, int sentBy)
        {
            // activity.Event.AttendesEvents.SelectMany(x => x.Attende.UserSessions).Where(x => x.IsActive == true && x.Platform != "Web").Distinct(new UserDeviceTokenComparer()).ToList()
            var distinctDeviceTokenSessions = activity.BookMarks.SelectMany(x => x.Attende.UserSessions).Where(x => x.IsActive == true && x.Platform != "Web" && !string.IsNullOrEmpty(x.DeviceToken)).Distinct(new UserDeviceTokenComparer()).ToList();
            Notifications notifications = new Notifications();
            var notificationID = await notifications.MaintainNotificationHistory(activity.Event.ID, title, message, sentBy);
            await SendNotificationForSessions(distinctDeviceTokenSessions, title, message, activity.Event.ID, sentBy, notificationID);
        }

        public async Task SendNotificationForSessions(IEnumerable<UserSession> userSessions, string title, string message, int eventID, int sentBy, int notificationID)
        {
            userSessions = userSessions.Distinct(new UserDeviceTokenComparer()).ToList();
            foreach (var session in userSessions)
            {
                if (session.AttendesID != null)
                {
                    if (session.IsActive == true && !string.IsNullOrEmpty(session.DeviceToken))
                    {
                        int attend = session.AttendesID.Value;
                        NotificationData notificationData = GetNotificationWithAuthToken(session);
                        await sendPushNotificationWithEmail(session.DeviceToken, title, message, eventID, attend, attend, notificationID);
                    }
                }
            }
        }

        public void ReplicateEvent(int sourceEventId, ReplicateEvent replicateEvent)
        {
            int targetEventId = CopyEvent(sourceEventId);

            if (replicateEvent.EventInformation)
            {
                CopyingEventInformation(sourceEventId, targetEventId);
            }

            if (replicateEvent.EventLevelSurvey)
            {
                CopyingEventLevelSurvey(sourceEventId, targetEventId);
            }

            if (replicateEvent.EventLocationDetails)
            {
                CopyingEventLocationDetails(sourceEventId, targetEventId);
            }

            if (replicateEvent.WidgetMappings)
            {
                CopyingWidgetMappings(sourceEventId, targetEventId);
            }

            if (replicateEvent.Attendees)
            {
                CopyingAttendees(sourceEventId, targetEventId);
            }

            if (replicateEvent.Theme)
            {
                CopyingTheme(sourceEventId, targetEventId);
            }

            if (replicateEvent.Sponsor)
            {
                CopyingSponsors(sourceEventId, targetEventId);
            }

            if (replicateEvent.Exhibitors)
            {
                CopyingExhibitors(sourceEventId, targetEventId);
            }

            if (replicateEvent.Vendors)
            {
                CopyingVendors(sourceEventId, targetEventId);
            }

            if (replicateEvent.Activities)
            {
                CopyingActivityRelatedData(sourceEventId, targetEventId, replicateEvent);
            }
        }

        public int CopyEvent(int sourceEventId)
        {
            Random random = new Random();
            Event eventData = Db.Events.FirstOrDefault(s => s.ID == sourceEventId);

            Event targetEvent = new Event
            {
                GalacticEventId = random.Next(1, 10000),
                EventName = string.Concat(eventData.EventName, "_Copy"),
                Message = eventData.Message,
                EnableThemes = eventData.EnableThemes,
                EnableQrCode = eventData.EnableQrCode,
                EnableAttendees = eventData.EnableAttendees,
                EnableFloormap = eventData.EnableFloormap,
                EnablePhotos = eventData.EnablePhotos,
                EnableSponsor = eventData.EnableSponsor,
                EnableComments = eventData.EnableComments,
                EnableGoogleApi = eventData.EnableGoogleApi,
                allow_bookmark = eventData.allow_bookmark,
                EventImage = eventData.EventImage,
                EnableSocialScreen = eventData.EnableSocialScreen,
                EnableSearchScreen = eventData.EnableSearchScreen,
                Destination = eventData.Destination,
                ClientName = eventData.ClientName,
                StartDate = eventData.StartDate,
                EndDate = eventData.EndDate,
                WebURL = eventData.WebURL,
                QrCodeTitle = eventData.QrCodeTitle,
                AttendeeTitle = eventData.AttendeeTitle,
                FloormapTitle = eventData.FloormapTitle,
                PhotosTitle = eventData.PhotosTitle,
                SponsorTitle = eventData.SponsorTitle,
                MapTitle = eventData.MapTitle,
                SocialTitle = eventData.SocialTitle,
                SearchTitle = eventData.SearchTitle,
                longitude = eventData.longitude,
                latitude = eventData.latitude,
                EnableSpeaker = eventData.EnableSpeaker,
                SpeakerTitle = eventData.SpeakerTitle,
                EnableExhibitor = eventData.EnableExhibitor,
                ExhibitorTitle = eventData.ExhibitorTitle,
                EnableVendor = eventData.EnableVendor,
                VendorTitle = eventData.VendorTitle,
                GoogleFolderID = eventData.GoogleFolderID,
                GoogleAlbumphotoUrl = eventData.GoogleAlbumphotoUrl,
                coordinates = eventData.coordinates,
                GeneralAgendaTitle = eventData.GeneralAgendaTitle,
                PersonalAgendaTitle = eventData.PersonalAgendaTitle,
                EnablePhotoDescription = eventData.EnablePhotoDescription,
                LocationInfo = eventData.LocationInfo,
                LeftMenuLogo = eventData.LeftMenuLogo,
                ScheduledActivityNotificationsTime = eventData.ScheduledActivityNotificationsTime,
                IsSurveyNotificationSent = eventData.IsSurveyNotificationSent,
                ModeratePost = eventData.ModeratePost,
                OpenUrlInsideorOutsideApp = eventData.OpenUrlInsideorOutsideApp,
                AllowTaggingPeopleandLocations = eventData.AllowTaggingPeopleandLocations,
                AllowPhotosBulkDownload = eventData.AllowPhotosBulkDownload,
                AutoPushNotLocationChange = eventData.AutoPushNotLocationChange,
                AutoPushNotMinReminder = eventData.AutoPushNotMinReminder,
                AutoPushNotTimeChange = eventData.AutoPushNotTimeChange,
                EmailNotesusingAccountEmail = eventData.EmailNotesusingAccountEmail,
                EmailFavouriteDocumentsusingAccountEmail = eventData.EmailFavouriteDocumentsusingAccountEmail,
                ShowAttendeewithSpeakers = eventData.ShowAttendeewithSpeakers,
                OpenDocumentsWithinorOutsideURL = eventData.OpenDocumentsWithinorOutsideURL,
                EnableGeneralAgenda = eventData.EnableGeneralAgenda,
                EnablePersonalAgenda = eventData.EnablePersonalAgenda,
                VendorSortBy = eventData.VendorSortBy,
                SponsorSortBy = eventData.SponsorSortBy,
                EnableSurveys = eventData.EnableSurveys,
                SurveyTitle = eventData.SurveyTitle,
                EnableDocuments = eventData.EnableDocuments,
                DocumentsTitle = eventData.DocumentsTitle,
                EnableAnnouncements = eventData.EnableAnnouncements,
                AnnouncementsTitle = eventData.AnnouncementsTitle,
                EnableEventInfo = eventData.EnableEventInfo,
                EventInfoTitle = eventData.EventInfoTitle,
                EnableVendorNameSort = eventData.EnableVendorNameSort,
                EnableVendorTypeSort = eventData.EnableVendorTypeSort,
                VendorTypeSortOrder = eventData.VendorTypeSortOrder,
                EnableSponsorNameSort = eventData.EnableSponsorNameSort,
                EnableSponsorTypeSort = eventData.EnableSponsorTypeSort,
                SponsorTypeSortOrder = eventData.SponsorTypeSortOrder,
                HideAddPhotos = eventData.HideAddPhotos,
                EventTimeZone = eventData.EventTimeZone
            };

            Db.Events.Add(targetEvent);

            Db.SaveChanges();

            return targetEvent.ID;

        }

        private void CopyingEventInformation(int sourceEventId, int targetEventId)
        {
            EventInformation eventInformationData = Db.EventInformations.FirstOrDefault(s => s.EventID == sourceEventId);

            if (eventInformationData != null)
            {
                EventInformation targetEventInformationData = new EventInformation
                {
                    EventID = targetEventId,
                    HideSection = eventInformationData.HideSection,
                    AboutSection = eventInformationData.AboutSection,
                    IconSection = eventInformationData.IconSection,
                    DescriptionSection = eventInformationData.DescriptionSection,
                    IsExternalLink = eventInformationData.IsExternalLink,
                    ExternalLinkURL = eventInformationData.ExternalLinkURL,
                    SectionType = eventInformationData.SectionType,
                    SectionOrder = eventInformationData.SectionOrder
                };

                Db.EventInformations.Add(targetEventInformationData);

                Db.SaveChanges();
            }
        }

        private void CopyingEventLevelSurvey(int sourceEventId, int targetEventId)
        {
            Survey survey = Db.Surveys.FirstOrDefault(s => s.EventID == sourceEventId && s.SurveyLevel == "Event Level");

            if (survey != null)
            {

                Survey targetSurvey = new Survey
                {
                    EventID = targetEventId,
                    SurveyName = survey.SurveyName,
                    SurveyLevel = survey.SurveyLevel,
                    SurveyDescription = survey.SurveyDescription,
                    ActivityID = survey.ActivityID,
                    SurveyTemplateID = survey.SurveyTemplateID
                };

                Db.Surveys.Add(targetSurvey);

                Db.SaveChanges();
            }
        }

        private void CopyingEventLocationDetails(int sourceEventId, int targetEventId)
        {
            EventLocationDetail eventLocationDetail = Db.EventLocationDetails.FirstOrDefault(s => s.EventId == sourceEventId);

            if (eventLocationDetail != null)
            {

                EventLocationDetail targetEventLocationDetail = new EventLocationDetail
                {
                    EventId = targetEventId,
                    Coordinates = eventLocationDetail.LocationDescription,
                    LocationDescription = eventLocationDetail.Coordinates,
                    Name = eventLocationDetail.Name,
                    MarkerImage1 = eventLocationDetail.MarkerImage1,
                    MarkerImage2 = eventLocationDetail.MarkerImage2,
                    Webpageurl = eventLocationDetail.Webpageurl,
                    WebPageName = eventLocationDetail.WebPageName
                };

                Db.EventLocationDetails.Add(targetEventLocationDetail);

                Db.SaveChanges();
            }
        }

        private void CopyingWidgetMappings(int sourceEventId, int targetEventId)
        {
            WidgetMapping widgetMapping = Db.WidgetMappings.FirstOrDefault(s => s.EventID == sourceEventId);

            if (widgetMapping != null)
            {
                WidgetMapping targetWidgetMapping = new WidgetMapping
                {
                    EventID = targetEventId,
                    WidgetID = widgetMapping.WidgetID,
                    XLocation = widgetMapping.XLocation,
                    YLocation = widgetMapping.YLocation,
                    Width = widgetMapping.Width,
                    Height = widgetMapping.Height,
                    DisplayName = widgetMapping.DisplayName,
                    isExternalLink = widgetMapping.isExternalLink,
                    ExternalLink = widgetMapping.ExternalLink,
                    ImagePath = widgetMapping.ImagePath,
                    widgetTypeID = widgetMapping.widgetTypeID
                };


                Db.WidgetMappings.Add(targetWidgetMapping);

                Db.SaveChanges();
            }
        }

        private void CopyingAttendees(int sourceEventId, int targetEventId)
        {
            List<AttendesEvent> attendees = Db.AttendesEvents.Where(s => s.EventID == sourceEventId).ToList();

            foreach (AttendesEvent attendee in attendees)
            {
                AttendesEvent targetAttendee = new AttendesEvent
                {
                    AttendesID = attendee.AttendesID,
                    IsAdmin = attendee.IsAdmin,
                    EventID = targetEventId
                };

                Db.AttendesEvents.Add(targetAttendee);

                Db.SaveChanges();
            }
        }

        private void CopyingTheme(int sourceEventId, int targetEventId)
        {
            Theme theme = Db.Themes.FirstOrDefault(s => s.EventID == sourceEventId);

            if (theme != null)
            {
                Theme targetTheme = new Theme
                {
                    EventID = targetEventId,
                    SplashScreemURL = theme.SplashScreemURL,
                    AppBackgroundURL = theme.AppBackgroundURL,
                    ButtonURL = theme.ButtonURL,
                    ButtonBackgroundColor = theme.ButtonBackgroundColor,
                    ButtonForegroundColor = theme.ButtonForegroundColor,
                    LabelForegroundColor = theme.LabelForegroundColor,
                    HeadingForegroundColor = theme.HeadingForegroundColor,
                    Message = theme.Message,
                    MenuText = theme.MenuText,
                    MenuBackground = theme.MenuBackground,
                    ContentBackground = theme.ContentBackground,
                    LinkText = theme.LinkText
                };

                Db.Themes.Add(targetTheme);

                Db.SaveChanges();
            }
        }


        private void CopyingSponsors(int sourceEventId, int targetEventId)
        {
            List<SponsorsEvent> sponsorsEvents = Db.SponsorsEvents.Where(s => s.EventID == sourceEventId).ToList();

            foreach (SponsorsEvent sponsorsEvent in sponsorsEvents)
            {
                SponsorsEvent targetSponsorsEvent = new SponsorsEvent
                {
                    EventID = targetEventId,
                    SponsorID = sponsorsEvent.SponsorID,
                    BoothNo = sponsorsEvent.BoothNo,
                    StartTime = sponsorsEvent.StartTime,
                    EndTime = sponsorsEvent.EndTime,
                    FloorMapLocationID = sponsorsEvent.FloorMapLocationID,
                    packagetype = sponsorsEvent.packagetype
                };


                Db.SponsorsEvents.Add(sponsorsEvent);

                Db.SaveChanges();
            }
        }

        private void CopyingExhibitors(int sourceEventId, int targetEventId)
        {
            List<ExhibitorsEvent> exhibitorsEvents = Db.ExhibitorsEvents.Where(s => s.EventID == sourceEventId).ToList();

            foreach (ExhibitorsEvent exhibitorsEvent in exhibitorsEvents)
            {
                ExhibitorsEvent targetExhibitorsEvent = new ExhibitorsEvent
                {
                    EventID = targetEventId,
                    ExhibitorsID = exhibitorsEvent.ExhibitorsID,
                    BoothNo = exhibitorsEvent.BoothNo,
                    StartTime = exhibitorsEvent.StartTime,
                    EndTime = exhibitorsEvent.EndTime,
                    FloorMapLocationID = exhibitorsEvent.FloorMapLocationID,
                    packagetype = exhibitorsEvent.packagetype
                };

                Db.ExhibitorsEvents.Add(targetExhibitorsEvent);

                Db.SaveChanges();
            }
        }

        private void CopyingVendors(int sourceEventId, int targetEventId)
        {
            List<VendorsEvent> vendorsEvents = Db.VendorsEvents.Where(s => s.EventID == sourceEventId).ToList();

            foreach (VendorsEvent vendorsEvent in vendorsEvents)
            {
                VendorsEvent targetVendorsEvent = new VendorsEvent
                {
                    EventID = targetEventId,
                    VendorsID = vendorsEvent.VendorsID,
                    FloorMapLocationID = vendorsEvent.FloorMapLocationID,
                    BoothNo = vendorsEvent.BoothNo,
                    StartTime = vendorsEvent.StartTime,
                    EndTime = vendorsEvent.EndTime,
                    packagetype = vendorsEvent.packagetype
                };


                Db.VendorsEvents.Add(targetVendorsEvent);

                Db.SaveChanges();
            }
        }

        private void CopyingActivityRelatedData(int sourceEventId, int targetEventId, ReplicateEvent replicateEvent)
        {
            List<Activite> activities = Db.Activites.Where(s => s.EventID == sourceEventId).ToList();

            foreach (Activite activity in activities)
            {
                int sourceActivityId = activity.ID;

                Activite targetActivity = new Activite
                {
                    EventID = targetEventId,
                    Description = activity.Description,
                    Thumbnail = activity.Thumbnail,
                    StartTime = activity.StartTime,
                    EndTime = activity.EndTime,
                    Address = activity.Address,
                    CreateON = activity.CreateON,
                    Status = activity.Status,
                    ActivityTypeid = activity.ActivityTypeid,
                    GalacticActivityId = activity.GalacticActivityId,
                    Name = activity.Name,
                    latitude = activity.latitude,
                    longitude = activity.longitude,
                    FloorRegionMappingID = activity.FloorRegionMappingID,
                    FloorMapLocationID = activity.FloorMapLocationID,
                    IsScheduleNotificationSent = activity.IsScheduleNotificationSent,
                    IsSurveyNotificationSent = activity.IsSurveyNotificationSent,
                    HideDate = activity.HideDate,
                    HideTime = activity.HideTime,
                    HideLocation = activity.HideLocation,
                    HideMap = activity.HideMap,
                    HideDescription = activity.HideDescription,
                    HidePhoto = activity.HidePhoto,
                    HideAddNotes = activity.HideAddNotes,
                    HideAttendees = activity.HideAttendees,
                    HideDocuments = activity.HideDocuments,
                    HideSpeaker = activity.HideSpeaker,
                    HideSurvey = activity.HideSurvey
                };

                Db.Activites.Add(targetActivity);
                Db.SaveChanges();

                int targetActivityId = targetActivity.ID;

                if (replicateEvent.ActivitiesSurvey)
                {
                    Survey sourceActivitySurvey = Db.Surveys.FirstOrDefault(s => s.ActivityID == sourceActivityId);

                    if (sourceActivitySurvey != null)
                    {
                        Survey targetActivitySurvey = new Survey
                        {
                            ActivityID = targetActivityId,
                            EventID = targetEventId,
                            SurveyName = sourceActivitySurvey.SurveyName,
                            SurveyLevel = sourceActivitySurvey.SurveyLevel,
                            SurveyDescription = sourceActivitySurvey.SurveyDescription,
                            SurveyTemplateID = sourceActivitySurvey.SurveyTemplateID
                        };

                        Db.Surveys.Add(sourceActivitySurvey);
                        Db.SaveChanges();
                    }
                }
            }

            if (replicateEvent.FloorMap)
            {
                CopyingFloorMapData(sourceEventId, targetEventId);
            }
        }

        private void CopyingFloorMapData(int sourceEventId, int targetEventId)
        {
            List<FloorMapLocation> floorMapLocations = Db.FloorMapLocations.Where(s => s.eventID == sourceEventId).ToList();
            Dictionary<string, int> floorMapLocationMapping = new Dictionary<string, int>();

            foreach (FloorMapLocation floorMapLocation in floorMapLocations)
            {
                FloorMapLocation targetFloorMapLocation = new FloorMapLocation
                {
                    eventID = targetEventId,
                    Location = floorMapLocation.Location
                };

                Db.FloorMapLocations.Add(targetFloorMapLocation);
                Db.SaveChanges();

                if (floorMapLocation.Location != null)
                {
                    floorMapLocationMapping.Add(floorMapLocation.Location, targetFloorMapLocation.ID);
                }
            }

            Dictionary<string, int> floorMappingData = new Dictionary<string, int>();

            List<FloorMapping> floorMappings = Db.FloorMappings.Where(s => s.EventID == sourceEventId).ToList();

            foreach (FloorMapping floorMapping in floorMappings)
            {
                FloorMapping targetFloorMapping = new FloorMapping
                {
                    EventID = targetEventId,
                    PhotoURL = floorMapping.PhotoURL,
                    PhotoHeight = floorMapping.PhotoHeight,
                    PhotoWidth = floorMapping.PhotoWidth,
                    FloorName = floorMapping.FloorName
                };

                Db.FloorMappings.Add(targetFloorMapping);
                Db.SaveChanges();

                if (string.IsNullOrEmpty(floorMapping.PhotoURL))
                {
                    floorMappingData.Add(floorMapping.PhotoURL, floorMapping.ID);
                }
            }

            CopyingFloorMapRegionData(sourceEventId, targetEventId, floorMapLocationMapping, floorMappingData);
        }

        private void CopyingFloorMapRegionData(int sourceEventId, int targetEventId, Dictionary<string, int> floorMapLocationMapping, Dictionary<string, int> floorMappingData)
        {
            List<FloorRegionMapping> floorRegionMappings = Db.FloorRegionMappings.Where(s => s.EventID == sourceEventId).ToList();

            foreach (FloorRegionMapping floorRegionMapping in floorRegionMappings)
            {
                int sourceFloorLocationId = 0;
                if (floorRegionMapping.floorLocationID != null)
                {
                    sourceFloorLocationId = floorRegionMapping.floorLocationID.Value;
                }

                string locationName = Db.FloorMapLocations.FirstOrDefault(s => s.ID == sourceFloorLocationId)
                    ?.Location ?? string.Empty;

                string sourcePhotoUrl = Db.FloorMappings.FirstOrDefault(s => s.ID == floorRegionMapping.FMid)?.PhotoURL ?? string.Empty;

                if (floorMappingData.ContainsKey(sourcePhotoUrl) && floorMapLocationMapping.ContainsKey(locationName))
                {
                    FloorRegionMapping targetFloorRegionMapping = new FloorRegionMapping
                    {
                        EventID = targetEventId,
                        Description = floorRegionMapping.Description,
                        X = floorRegionMapping.X,
                        Y = floorRegionMapping.Y,
                        Width = floorRegionMapping.Width,
                        height = floorRegionMapping.height,
                        Regioncolor = floorRegionMapping.Regioncolor,
                        FMid = floorMappingData[sourcePhotoUrl],
                        floorLocationID = floorMapLocationMapping[locationName]
                    };

                    Db.FloorRegionMappings.Add(targetFloorRegionMapping);
                    Db.SaveChanges();
                }
            }
        }


        public async Task AddUpdateTheme(Theme theme)
        {
            if (theme.ID > 0)
            {
                var updatetheme = await Db.Themes.FindAsync(theme.ID);
                updatetheme.SplashScreemURL = theme.SplashScreemURL;
                updatetheme.AppBackgroundURL = theme.AppBackgroundURL;
                updatetheme.ButtonURL = theme.ButtonURL;
                updatetheme.ButtonBackgroundColor = theme.ButtonBackgroundColor;
                updatetheme.ButtonForegroundColor = theme.ButtonForegroundColor;
                updatetheme.LabelForegroundColor = theme.LabelForegroundColor;
                updatetheme.HeadingForegroundColor = theme.HeadingForegroundColor;
                updatetheme.MenuText = theme.MenuText;
                updatetheme.MenuBackground = theme.MenuBackground;
                //updatetheme.ContentText = theme.ContentText;
                updatetheme.ContentBackground = theme.ContentBackground;
                updatetheme.LinkText = theme.LinkText;
                updatetheme.Message = theme.Message;
                await Db.SaveChangesAsync();
                //update
            }
            else
            {
                Db.Themes.Add(theme);
                await Db.SaveChangesAsync();
                //created
            }
        }

        public async Task AddRemoveAttendeEvent(int eventId, int attendeId)
        {
            var eventObj = await Db.Events.FindAsync(eventId);
            if (eventObj.AttendesEvents.Any(x => x.AttendesID == attendeId))
            {
                var attendeEvent = await Db.AttendesEvents.FirstOrDefaultAsync(x => x.AttendesID == attendeId);
                Db.AttendesEvents.Remove(attendeEvent);
                await Db.SaveChangesAsync();
            }
            else
            {
                Db.AttendesEvents.Add(new AttendesEvent
                {
                    AttendesID = attendeId,
                    EventID = eventId
                });
                await Db.SaveChangesAsync();
            }
        }

        public async Task AddRemoveSponsorActivity(int sponsorID, List<int> activityIDs)
        {
            if (activityIDs == null)
            {
                activityIDs = new List<int>();
            }

            var deletedSponsorActivities = Db.SponsorActivities.Where(x => x.SponsorID == sponsorID && !activityIDs.Any(y => y == x.ActivityID));
            Db.SponsorActivities.RemoveRange(deletedSponsorActivities);

            foreach (var activityID in activityIDs)
            {
                if (!Db.SponsorActivities.Any(x => x.ActivityID == activityID && x.SponsorID == sponsorID))
                {
                    Db.SponsorActivities.Add(new SponsorActivity() { ActivityID = activityID, SponsorID = sponsorID });
                }
            }
            await Db.SaveChangesAsync();
        }

        public async Task AddRemoveExhibitorActivity(int exhibitorID, List<int> activityIDs)
        {
            if (activityIDs == null)
            {
                activityIDs = new List<int>();
            }

            var deletedExhibitorActivities = Db.ExhibitorActivities.Where(x => x.ExhibitorID == exhibitorID && !activityIDs.Any(y => y == x.ActivityID));
            Db.ExhibitorActivities.RemoveRange(deletedExhibitorActivities);

            foreach (var activityID in activityIDs)
            {
                if (!Db.ExhibitorActivities.Any(x => x.ActivityID == activityID && x.ExhibitorID == exhibitorID))
                {
                    Db.ExhibitorActivities.Add(new ExhibitorActivity() { ActivityID = activityID, ExhibitorID = exhibitorID });
                }
            }
            await Db.SaveChangesAsync();
        }

        public async Task AddRemoveVendorActivity(int vendorID, List<int> activityIDs)
        {
            if (activityIDs == null)
            {
                activityIDs = new List<int>();
            }

            var deletedVendorActivities = Db.VendorActivities.Where(x => x.VendorID == vendorID && !activityIDs.Any(y => y == x.ActivityID));
            Db.VendorActivities.RemoveRange(deletedVendorActivities);

            foreach (var activityID in activityIDs)
            {
                if (!Db.VendorActivities.Any(x => x.ActivityID == activityID && x.VendorID == vendorID))
                {
                    Db.VendorActivities.Add(new VendorActivity() { ActivityID = activityID, VendorID = vendorID });
                }
            }

            await Db.SaveChangesAsync();
        }

        public async Task<bool> ApproveComment(int id)
        {
            var comment = await Db.SocialForums.FindAsync(id);
            comment.Status = true;
            await Db.SaveChangesAsync();
            return true;
        }
        
        public string GetSocialName(int id)
        {
            var comment = Db.SocialForums.Find(id);

            return comment.URL;
        }
        
        public async Task<bool> ApproveEventPhoto(int id)
        {
            var comment = await Db.Photos.FindAsync(id);
            comment.Status = true;
            await Db.SaveChangesAsync();
            return true;
        }
        
        public string GetPhotoName(int id)
        {
            var comment = Db.Photos.Find(id);

            return comment.URL;
        }
        
        public async Task<bool> DeleteComment(int id)
        {
            try
            {
                //Deleting Social Post Comments
                var comments = Db.ForumSocialComments.Where(x => x.PostID == id).ToList();
                Db.ForumSocialComments.RemoveRange(comments);
                //Deleting Social Likes
                var likes = Db.PostUserLikes.Where(x => x.PostID == id).ToList();
                Db.PostUserLikes.RemoveRange(likes);
                var comment = await Db.SocialForums.FindAsync(id);
                Db.SocialForums.Remove(comment);
                await Db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        //public async Task<bool> FetchAddAttendee(int eventid, int id)
        //{
        //    var result = al.EventActivities(eventid);
        //    return true;
        //}

        public bool DeletePost(int PostID)
        {
            try
            {
                SocialForum post = Db.SocialForums.FirstOrDefault(x => x.ID == PostID);
                post.isDeleted = true;
                Db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> FetchAttendeeFromGalactic(int galacticEventId, int eventId, string imageSavePath, bool isSpeakerRequest = false)
        {
            AuthLogic al = new AuthLogic();
            var records = al.GetProgramAttendees(galacticEventId);
            Func<ProgramAttendee, bool> validator = x => (isSpeakerRequest && x.AttendeeType.ToLower() == "speaker") || (!isSpeakerRequest && x.AttendeeType.ToLower() != "speaker");
            await AddResyncedAttendees(records, validator, eventId, imageSavePath);
            return true;
        }

        public async Task<bool> FetchUserGroupsFromGalactic(int galacticEventId, int eventId, string imageSavePath, int userId)
        {
            AuthLogic al = new AuthLogic();
            UserGroups userGroups = new UserGroups();
            var gpsAttendees = al.GetProgramAttendees(galacticEventId);
            await AddResyncedAttendees(gpsAttendees, x => true, eventId, imageSavePath); // might be a bad way, we need to relook into this
            userGroups.AddResyncedGroups(gpsAttendees.ProgramAttendees, eventId, userId);
            return true;
        }

        public void ResyncEventData(int eventId, string imageSavePath, int userId)
        {
            int galacticEventId = Db.Events.FirstOrDefault(s => s.ID == eventId).GalacticEventId.Value;

            FetchEventInfo(galacticEventId);
            BookmarkedActivities(galacticEventId, eventId);
            FetchAttendeeFromGalactic(galacticEventId, eventId, imageSavePath, false);
            FetchAttendeeFromGalactic(galacticEventId, eventId, imageSavePath, true);
            FetchUserGroupsFromGalactic(galacticEventId, eventId, imageSavePath, userId);
        }


        public async Task<bool> FetchActivities(int galEventid, int id, int eventID)
        {

            AuthLogic al = new AuthLogic();
            var result = al.EventActivities(galEventid);
            bool flag = true;
            foreach (var value in result.ProgramActivities)
            {
                var checkexistance = Db.Activites.Where(x => x.EventID == eventID).Any(x => x.GalacticActivityId == value.ActivityID);

                if (!checkexistance)
                {
                    Activite ac = new Activite
                    {
                        GalacticActivityId = value.ActivityID,
                        StartTime = DateTime.Parse(value.ActivityDate),
                        EndTime = DateTime.Parse(value.ActivityDate),
                        Name = (string.IsNullOrEmpty(value.ActivityName) ? string.Empty : value.ActivityName),
                        Address = string.Empty,
                        Thumbnail = string.Empty,
                        EventID = id,
                        Status = true,
                        ActivityTypeid = 1,
                        Description = string.Empty
                    };


                    Db.Activites.Add(ac);

                    flag = checkexistance;


                }

            }
            await Db.SaveChangesAsync();
            return flag;
        }
        
        public bool FetchEventInfo(int id)
        {
            AuthLogic al = new AuthLogic();
            var result = al.EventInfo(id);
            if (result.EventInfos != null)
            {
                var record = Db.Events.Where(y => y.GalacticEventId == id).FirstOrDefault();
                record.ClientName = result.EventInfos.ClientName;
                record.Destination = result.EventInfos.Destination;
                record.EventName = result.EventInfos.Name;
                record.StartDate = Convert.ToDateTime(result.EventInfos.StartDate);
                record.EndDate = Convert.ToDateTime(result.EventInfos.EndDate);
                record.WebURL = result.EventInfos.WebURL;
                Db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task AddRemoveSponsorEvent(int eventId, int sponsorId)
        {
            var eventObj = await Db.Events.FindAsync(eventId);
            if (eventObj.SponsorsEvents.Any(x => x.SponsorID == sponsorId))
            {
                var sponsorEvent = await Db.SponsorsEvents.FirstOrDefaultAsync(x => x.SponsorID == sponsorId && x.EventID == eventId);
                Db.SponsorsEvents.Remove(sponsorEvent);
                await Db.SaveChangesAsync();
            }
            else
            {
                Db.SponsorsEvents.Add(new SponsorsEvent
                {
                    SponsorID = sponsorId,
                    EventID = eventId
                });
                await Db.SaveChangesAsync();
            }
        }

        //public async Task AddRemoveSpeakerEvent(int eventId, int speakerId)
        //{
        //    var eventObj = await Db.Events.FindAsync(eventId);
        //    if (eventObj.SpeakersEvents.Any(x => x.SpeakerID == speakerId && x.EventID == eventId))
        //    {
        //        var sponsorEvent = await Db.SpeakersEvents.FirstOrDefaultAsync(x => x.SpeakerID == speakerId && x.EventID == eventId);
        //        Db.SpeakersEvents.Remove(sponsorEvent);
        //        await Db.SaveChangesAsync();
        //    }
        //    else
        //    {
        //        Db.SpeakersEvents.Add(new SpeakersEvent
        //        {
        //            SpeakerID = speakerId,
        //            EventID = eventId
        //        });
        //        await Db.SaveChangesAsync();
        //    }
        //}

        public async Task AddRemoveExhibitorEvent(int eventId, int exhibitorId)
        {
            var eventObj = await Db.Events.FindAsync(eventId);
            if (eventObj.ExhibitorsEvents.Any(x => x.ExhibitorsID == exhibitorId && x.EventID == eventId))
            {
                var exhibitorEvent = await Db.ExhibitorsEvents.FirstOrDefaultAsync(x => x.ExhibitorsID == exhibitorId && x.EventID == eventId);
                Db.ExhibitorsEvents.Remove(exhibitorEvent);
                await Db.SaveChangesAsync();
            }
            else
            {
                Db.ExhibitorsEvents.Add(new ExhibitorsEvent
                {
                    ExhibitorsID = exhibitorId,
                    EventID = eventId
                });
                await Db.SaveChangesAsync();
            }
        }

        public async Task AddRemoveVendorEvent(int eventId, int vendorId)
        {
            var eventObj = await Db.Events.FindAsync(eventId);
            if (eventObj.VendorsEvents.Any(x => x.VendorsID == vendorId && x.EventID == eventId))
            {
                var vendorEvent = await Db.VendorsEvents.FirstOrDefaultAsync(x => x.VendorsID == vendorId && x.EventID == eventId);
                Db.VendorsEvents.Remove(vendorEvent);
                await Db.SaveChangesAsync();
            }
            else
            {
                Db.VendorsEvents.Add(new VendorsEvent
                {
                    VendorsID = vendorId,
                    EventID = eventId
                });
                await Db.SaveChangesAsync();
            }
        }

        public async Task AddUpdateExhibitorboothnumber(int EventID, int exhibitorId, string boothNo)
        {
            var exhibitorEvent = await Db.ExhibitorsEvents.FirstOrDefaultAsync(x => x.ExhibitorsID == exhibitorId && x.EventID == EventID);
            exhibitorEvent.BoothNo = boothNo;
            await Db.SaveChangesAsync();
        }

        public async Task AddUpdateExhibitorboothdetails(int EventID, int exhibitorId, string ExhibitorBoothNo, DateTime? ExhibitorStartTime, DateTime? ExhibitorEndTime, int? FloorMapLocationID, int? ExhibitorPackageType)
        {
            var exhibitorEvent = await Db.ExhibitorsEvents.FirstOrDefaultAsync(x => x.ExhibitorsID == exhibitorId && x.EventID == EventID);
            exhibitorEvent.BoothNo = ExhibitorBoothNo;
            exhibitorEvent.StartTime = ExhibitorStartTime;
            exhibitorEvent.EndTime = ExhibitorEndTime;
            exhibitorEvent.FloorMapLocationID = FloorMapLocationID;
            exhibitorEvent.packagetype = ExhibitorPackageType;
            await Db.SaveChangesAsync();
        }

        public async Task AddUpdateSponsorboothdetails(int eventId, int sponsorId, string SponsorBoothNo, DateTime? SponsorStartTime, DateTime? SponsorEndTime, int? FloorMapLocationID, int? SponsorPackageType)
        {
            var sponsorsEvent = await Db.SponsorsEvents.FirstOrDefaultAsync(x => x.SponsorID == sponsorId && x.EventID == eventId);
            sponsorsEvent.BoothNo = SponsorBoothNo;
            sponsorsEvent.StartTime = SponsorStartTime;
            sponsorsEvent.EndTime = SponsorEndTime;
            sponsorsEvent.FloorMapLocationID = FloorMapLocationID;
            sponsorsEvent.packagetype = SponsorPackageType;
            await Db.SaveChangesAsync();
        }

        public async Task AddUpdateVendorboothdetails(int eventId, int vendorId, string VendorBoothNo, DateTime? VendorStartTime, DateTime? VendorEndTime, int? FloorMapLocationID, int? VendorPackageType)
        {
            var vendorsEvent = await Db.VendorsEvents.FirstOrDefaultAsync(x => x.VendorsID == vendorId && x.EventID == eventId);
            vendorsEvent.BoothNo = VendorBoothNo;
            vendorsEvent.StartTime = VendorStartTime;
            vendorsEvent.EndTime = VendorEndTime;
            vendorsEvent.FloorMapLocationID = FloorMapLocationID;
            vendorsEvent.packagetype = VendorPackageType;
            await Db.SaveChangesAsync();
        }

        public async Task<int> ActivityQrCode(int id, string qrCode, int userId)
        {
            var code = new QRCode
            {
                EventID = Db.Activites.Find(id).Event.ID,
                AtivitityID = id,
                GeneratedBY = userId,
                GeneratedON = DateTime.Now,
                Description = string.Empty,
                Status = true,
                WebURL = string.Empty,
                CodePath = qrCode
            };
            Db.QRCodes.Add(code);
            await Db.SaveChangesAsync();
            return code.ID;
        }
        
        public async Task<int> UpdateActivityQrCode(int id, string qrCode)
        {
            var data = await Db.QRCodes.FindAsync(id);
            data.CodePath = qrCode;
            await Db.SaveChangesAsync();
            return 0;
        }

        public async Task<int> EventQrCode(int id, string qrCode, int userId, string description, string url)
        {
            var qrcode = new QRCode
            {
                Description = description,
                EventID = id,
                GeneratedBY = userId,
                GeneratedON = DateTime.Now,
                Status = true,
                WebURL = url,
                CodePath = qrCode
            };
            Db.QRCodes.Add(qrcode);
            await Db.SaveChangesAsync();
            // qr code goes here
            return qrcode.ID;
        }
        
        public async Task<int> UpdateEventQrCode(int id, string qrCode)
        {
            var data = await Db.QRCodes.FindAsync(id);
            data.CodePath = qrCode;
            await Db.SaveChangesAsync();
            // qr code goes here
            return 0;
        }

        public async Task<IEnumerable<Photo>> EventPhotos(int id)
        {
            return await Db.Photos.Where(x => x.EventID == id).ToListAsync(); ;
        }

        public async Task<bool> DeleteEvent(int eventId)
        {
            //delete event code goes here
            return false;
        }

        public async Task<IEnumerable<Event>> GetAllEvents(string value = "")
        {
            if (string.IsNullOrEmpty(value))
                return await Db.Events.OrderByDescending(x => x.Activites.Count).ToListAsync(); // .Where(x => x.Activites.Count > 0)
            else return await Db.Events.Where(x => x.EventName.StartsWith(value)).ToListAsync();
        }

        public string GetEventName(int id)
        {
            return Db.Events.Where(x => x.ID == id).Select(y => y.EventName).FirstOrDefault();
        }
        
        public async Task SaveEventImagesFromWeb(int id, string file, int user, int? AlbumID, string description = "")
        {
            Db.Photos.Add(new Photo
            {
                EventID = id,
                UploadON = DateTime.Now,
                UploadBY = user,
                Description = description,
                Status = true,
                URL = file,
                isSync = true,
                AlbumID = AlbumID
            });

            await Db.SaveChangesAsync();
        }
        
        public async Task SaveEventImages(int id, string file, int user, string description = "")
        {
            Db.Photos.Add(new Photo
            {
                EventID = id,
                UploadON = DateTime.Now,
                UploadBY = user,
                Description = description,
                Status = false,
                URL = file,
                isSync = true
            });
            await Db.SaveChangesAsync();
        }

        public static List<PhotoAlbum> GetPhotoAlbums()
        {
            Entities DbStatic = new Entities();
            return DbStatic.PhotoAlbums.ToList();
        }

        public bool DeleteEventPhotos(Int32 photoId)
        {
            if (photoId > 0)
            {
                var indphoto = Db.Photos.Where(a => a.ID == photoId).FirstOrDefault();
                if (indphoto != null)
                {
                    List<PhotoLike> Photos = Db.PhotoLikes.Where(x => x.PhotoID == photoId).ToList();
                    Db.PhotoLikes.RemoveRange(Photos);
                    Db.Photos.Remove(indphoto);
                    Db.SaveChanges();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public Task SaveEventExcelFileData(List<EventExcelDto> eventData, List<EventActivitiesDTO> eventActivitiesData, int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> DeleteThee(int id)
        {
            var theme = await Db.Themes.FindAsync(id);
            Db.Themes.Remove(theme);
            await Db.SaveChangesAsync();
            return theme.EventID;
        }
        
        public async Task<bool> CheckActiviteDuplicate(string name)
        {
            var record = Db.Activites.Where(x => x.Name == name).Select(y => y.Name).FirstOrDefault();
            if (record != null)
            {
                return true;
            }
            else

            {
                return false;
            }

        }

        public async Task SaveFloorMap(int eventId, string fileName, int width, int height, string name)
        {
            //if (Db.FloorMappings.Any(x => x.EventID == eventId))
            //{
            //    //remove old floor mappings
            //    if (Db.FloorMappings.Any(x => x.EventID == eventId && x.FloorRegionMappings.Any()))
            //        Db.FloorRegionMappings.RemoveRange((await Db.FloorMappings.FirstOrDefaultAsync(x => x.EventID == eventId)).FloorRegionMappings);
            //    Db.FloorMappings.Remove(await Db.FloorMappings.FirstOrDefaultAsync(x => x.EventID == eventId));
            //    await Db.SaveChangesAsync();
            //}
            Db.FloorMappings.Add(new FloorMapping
            {
                EventID = eventId,
                FloorName = name,
                PhotoURL = fileName,
                PhotoHeight = height,
                PhotoWidth = width
            });
            await Db.SaveChangesAsync();
        }

        public async Task SaveFloorDescription(FloorRegionMapping file)
        {
            Db.FloorRegionMappings.Add(file);
            await Db.SaveChangesAsync();
        }

        public async Task<int> FloorMapDeleteComment(int id)
        {
            var floor = Db.FloorRegionMappings.Find(id);
            Db.FloorRegionMappings.Remove(floor);
            await Db.SaveChangesAsync();
            return floor.EventID;
        }
        
        public async Task<int> deletefloormap(int id)
        {
            try
            {
                //First Deleting Floor Map Comments
                var comment = Db.FloorRegionMappings.Where(x => x.FMid == id).ToList();
                var delete = Db.FloorRegionMappings.RemoveRange(comment);
                // Deleting Floor Map 
                var floor = Db.FloorMappings.Find(id);
                Db.FloorMappings.Remove(floor);
                await Db.SaveChangesAsync();
                return floor.EventID;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public async Task<IEnumerable<FloorRegionMapping>> GetFloorMapComments(int id)
        {
            return await Db.FloorRegionMappings.Where(x => x.FMid == id).ToListAsync();
        }
        
        public class FcmResponse
        {
            public long Multicast_id { get; set; }
            
            public int Success { get; set; }
            
            public int Failure { get; set; }
            
            public int Canonical_ids { get; set; }
            
            public List<Result> Results { get; set; }
        }

        public class Result
        {
            public string Error { get; set; }
        }

        public int SendNotificationFromFirebaseCloud(string devicetoke, string title, string message)
        {
            FcmResponse fr;
            var result = "-1";
            var webAddr = "https://fcm.googleapis.com/fcm/send";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, $"key={TwilioConfiguration.FirebaseSID}");
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string strNJson = "{\"to\": \"" + devicetoke + "\",\"notification\": {\"title\": \"" + title + "\",\"text\": \"" + message + "\",\"sound\":\"default\"}}";

                streamWriter.Write(strNJson);
                streamWriter.Flush();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            var data = JsonConvert.DeserializeObject<FcmResponse>(result.ToString());
            return data.Success;
        }

        public IEnumerable<UserSession> GetDeviceTokensByUserid(int id, int eventID)
        {
            return Db.UserSessions.Where(x => x.AttendesID == id && (x.IsActive ?? false) && !string.IsNullOrEmpty(x.DeviceToken) && x.Platform != "Web").Where(x => x.Attende.AttendesEvents.Any(y => y.EventID == eventID)).ToList().Distinct(new UserDeviceTokenComparer());
        }

        public async Task<Dictionary<string, string>> getDocumentDropDownValuesByType(ScreenTypes screenType, int eventID)
        {
            if (screenType == ScreenTypes.Activity)
            {
                return await this.Db.Activites.Where(x => x.EventID == eventID).Select(x => new { x.ID, x.Name }).ToDictionaryAsync(x => x.ID.ToString(), y => y.Name);
            }
            else if (screenType == ScreenTypes.Exhibitor)
            {
                return await this.Db.Exhibitors.Where(x => x.ExhibitorsEvents.Any(y => y.EventID == eventID)).Select(x => new { x.ID, x.Name }).ToDictionaryAsync(x => x.ID.ToString(), y => y.Name);
            }
            else if (screenType == ScreenTypes.Speaker)
            {
                AttendesLogic attendesLogic = new AttendesLogic();
                var speakers = await attendesLogic.GetAttendes(checkAttendeeValidity: new SpeakerValidator());
                return speakers.Where(x => x.AttendesEvents.Any(y => y.EventID == eventID)).Select(x => new { x.ID, x.FirstName }).ToDictionary(x => x.ID.ToString(), y => y.FirstName);
            }
            else if (screenType == ScreenTypes.Sponsor)
            {
                return await this.Db.Sponsors.Where(x => x.SponsorsEvents.Any(y => y.EventID == eventID)).Select(x => new { x.ID, x.Name }).ToDictionaryAsync(x => x.ID.ToString(), y => y.Name);
            }
            else
            {
                return null;
            }
        }

        public async Task uploadDocuments(ScreenTypes screenType, int? selectedID, int eventID, string fileName, string documentName)
        {
            Document document;
            if (screenType == ScreenTypes.Activity)
            {
                document = new Document() { ActivityID = selectedID, EventID = eventID, FilePath = fileName, DocumentName = documentName };
            }
            else if (screenType == ScreenTypes.Exhibitor)
            {
                document = new Document() { exhibitorID = selectedID, EventID = eventID, FilePath = fileName, DocumentName = documentName };
            }
            else if (screenType == ScreenTypes.Speaker)
            {
                document = new Document() { AttendeeID = selectedID, EventID = eventID, FilePath = fileName, DocumentName = documentName };
            }
            else if (screenType == ScreenTypes.Sponsor)
            {
                document = new Document() { sponsorID = selectedID, EventID = eventID, FilePath = fileName, DocumentName = documentName };
            }
            else
            {
                document = new Document() { EventID = eventID, FilePath = fileName, DocumentName = documentName };
            }
            this.Db.Documents.Add(document);
            await this.Db.SaveChangesAsync();
        }


        public async Task<Document> getDocumentByID(int documentID)
        {
            return await this.Db.Documents.FindAsync(documentID);
        }

        public List<DocumentModel> GetDocuments(int? id)
        {
            var eventDocs = Db.Documents.Where(x => x.EventID == id);

            List<DocumentModel> DocumentModel = new List<DocumentModel>();

            foreach (Document eventDoc in eventDocs)
            {
                var documentObj = new DocumentModel()
                {
                    FilePath = eventDoc.FilePath,
                    DocumentID = eventDoc.ID,

                    DocumentName = eventDoc.DocumentName
                };
                if (eventDoc.exhibitorID != null)
                {
                    documentObj.MappedType = ScreenTypes.Exhibitor.ToString();
                    documentObj.ID = (int)ScreenTypes.Exhibitor;
                    documentObj.AssignedTo = eventDoc.Exhibitor.Name;
                }
                else if (eventDoc.AttendeeID != null)
                {
                    documentObj.MappedType = ScreenTypes.Speaker.ToString();
                    documentObj.ID = (int)ScreenTypes.Speaker;
                    documentObj.AssignedTo = eventDoc.Attende.FirstName;
                }
                else if (eventDoc.sponsorID != null)
                {
                    documentObj.MappedType = ScreenTypes.Sponsor.ToString();
                    documentObj.ID = (int)ScreenTypes.Sponsor;
                    documentObj.AssignedTo = eventDoc.Sponsor.Name;
                }
                else if (eventDoc.ActivityID != null)
                {
                    documentObj.MappedType = ScreenTypes.Activity.ToString();
                    documentObj.ID = (int)ScreenTypes.Activity;
                    documentObj.AssignedTo = eventDoc.Activite.Name;
                }
                else
                {
                    documentObj.MappedType = ScreenTypes.Documents.ToString();
                    documentObj.ID = (int)ScreenTypes.Documents;
                }

                DocumentModel.Add(documentObj);
            }

            return DocumentModel;

        }

        public async Task<IEnumerable<Document>> GetAllDocuments(int eventID, int userID)
        {
            var eventDocs = await Db.Documents.Where(n => n.EventID == eventID).ToListAsync();

            //var activityDocs = await Db.BookMarks.Join(Db.Documents,
            //    x => x.ActivityID,
            //    y => y.ActivityID,
            //    (bookmark, document) => new { document, bookmark })
            //    .Where(x => x.document.EventID == eventID && x.bookmark.AttendesID == userID)
            //    .Select(x => x.document)
            //    .ToListAsync();

            //var documentsFolderDocs = await Db.Documents.Where(x => x.ActivityID == null && x.exhibitorID == null && x.SpeakerID == null && x.sponsorID == null && x.EventID == eventID).ToListAsync();

            //return activityDocs.Union(documentsFolderDocs);
            return eventDocs;
        }

        public List<Document> GetAllDocumentsExcludingImages(int eventID, int userID)
        {
            var eventDocs = Db.Documents.Where(n => n.EventID == eventID).ToList();

            eventDocs.RemoveAll(s => s.FilePath.Contains("jpg") || s.FilePath.Contains("jpeg") || s.FilePath.Contains("bmp") || s.FilePath.Contains("bmp"));
            return eventDocs;
        }

        public bool MarkDocumentAsFavourite(int userID, int documentID)
        {
            if (!Db.DocumentFavourites.Any(x => x.DocumentID == documentID && x.AttendeeID == userID))
            {
                Db.DocumentFavourites.Add(
                new DocumentFavourite()
                {
                    DocumentID = documentID,
                    AttendeeID = userID
                });
                Db.SaveChanges();
                return true;
            }
            return false;
        }

        public bool RemoveDocumentFromFavorite(int userID, int documentID)
        {
            if (Db.DocumentFavourites.Any(x => x.DocumentID == documentID && x.AttendeeID == userID))
            {
                var favoriteDoc = Db.DocumentFavourites.FirstOrDefault(x => x.DocumentID == documentID && x.AttendeeID == userID);
                Db.DocumentFavourites.Remove(favoriteDoc);
                Db.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<List<Document>> GetFavouriteDocs(int attendeeID, int eventID)
        {
            return await Db.DocumentFavourites.Where(x => x.AttendeeID == attendeeID).Select(x => x.Document).Where(x => x.EventID == eventID).ToListAsync();
        }

        public async Task<bool> SaveNoteDetails(string Notetext, int AttendeeID, int? ActivityID, int? SponsorID, int? VendorID, int? ExhibitorID, int? EventID, int? MarkerID)
        {
            try
            {
                var notedata = Db.Notes.FirstOrDefault(x => x.UserID == AttendeeID && x.Activity_id == ActivityID && x.Sponsor_id == SponsorID && x.Vendor_id == VendorID && x.Exhibitor_id == ExhibitorID && x.EventID == EventID && x.MarkerID == MarkerID);
                if (notedata != null)
                {
                    notedata.Text = Notetext;
                    await Db.SaveChangesAsync();
                }
                else
                {
                    Db.Notes.Add(new Note
                    {
                        Text = Notetext,
                        UserID = AttendeeID,
                        Activity_id = ActivityID,
                        Sponsor_id = SponsorID,
                        Vendor_id = VendorID,
                        Exhibitor_id = ExhibitorID,
                        EventID = EventID,
                        MarkerID = MarkerID
                    });
                    await Db.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<Note>> GetNotes(int userID, int EventID)
        {
            return Db.Notes.Where(s => s.UserID == userID && s.EventID == EventID).ToList();
        }

        public async Task<bool> EmailNotes(int userid, int EventID)
        {
            try
            {
                var notesdata = Db.Notes.Where(n => n.UserID == userid && n.EventID == EventID).ToList();
                List<MemoryStream> stream = new List<MemoryStream>();
                for (int i = 0; i < notesdata.Count; i++)
                {
                    MemoryStream ms = new MemoryStream();
                    StreamWriter sw = new StreamWriter(ms);
                    sw.WriteLine(notesdata[i].Text);
                    stream.Add(ms);
                    sw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);

                }
                //  await CommonHelpers.Helpers.SendEmail("Note", "Note", stream, null, null, true);

                using (var EmailSender = new EmailSender(Db.Attendes.Find(userid).Email, "Notes"))
                {
                    EmailSender.SendEmailWithAttachment(stream.ToArray());
                    return true;
                }
                // await CommonHelpers.Helpers.SendEmail("Note", "Note", stream, null, null, true);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void AddOrUpdateWidgets(List<WidgetMapping> widgetMappings, int eventID)
        {
            var deleteWidgetList = Db.WidgetMappings.ToList().Where(x => x.EventID == eventID && widgetMappings.All(y => y.ID != x.ID));


            if (deleteWidgetList.Count() > 0)
            {
                Db.WidgetMappings.RemoveRange(deleteWidgetList);
            }

            foreach (var widgetMapping in widgetMappings)
            {
                if (widgetMapping.ID > 0)
                {
                    var widget = Db.WidgetMappings.Find(widgetMapping.ID);
                    Db.Entry(widget).CurrentValues.SetValues(new
                    {
                        widgetMapping.ID,
                        widgetMapping.Height,
                        widgetMapping.Width,
                        widgetMapping.XLocation,
                        widgetMapping.YLocation
                    });
                }
                else
                {
                    Db.WidgetMappings.Add(widgetMapping);
                }
            }
            Db.SaveChanges();
        }
        public void AddUpdateWidget(WidgetMapping widgetMapping)
        {
            if (widgetMapping.ID > 0)
            {
                var widget = Db.WidgetMappings.Find(widgetMapping.ID);
                if (widgetMapping.widgetTypeID == 1)
                {
                    widget.DisplayName = widgetMapping.DisplayName;
                    widget.widgetTypeID = widgetMapping.widgetTypeID;
                    widget.ExternalLink = widgetMapping.isExternalLink ? widgetMapping.ExternalLink : null;
                    if (widgetMapping.ImagePath != null)
                    {
                        widget.ImagePath = widgetMapping.ImagePath;
                    }
                    widget.isExternalLink = widgetMapping.isExternalLink;
                    widget.WidgetID = widgetMapping.isExternalLink ? null : widgetMapping.WidgetID;
                }
                else if (widgetMapping.widgetTypeID == 2)
                {
                    widget.DisplayName = widgetMapping.DisplayName;
                    widget.WidgetID = widgetMapping.WidgetID;
                    widget.widgetTypeID = widgetMapping.widgetTypeID;
                    widget.ExternalLink = null;
                    widget.ImagePath = null;
                    widget.isExternalLink = false;
                }
                widget.FontSize = widgetMapping.FontSize;
                widget.Color = widgetMapping.Color;
                widget.Justification = widgetMapping.Justification;
            }
            else
            {
                widgetMapping.XLocation = 0;
                widgetMapping.YLocation = Db.WidgetMappings.Where(x => x.EventID == widgetMapping.EventID).Count() > 0 ? Db.WidgetMappings.Where(x => x.EventID == widgetMapping.EventID).Max(x => x.YLocation) + 1 : 0;
                widgetMapping.Height = 1;
                widgetMapping.Width = 2;
                Db.WidgetMappings.Add(widgetMapping);
            }
            Db.SaveChanges();
        }

        //public async Task AddEventInfo(EventInfo eventInfo)
        //{
        //    if (Db.EventInfoes.Any(x => x.ID == eventInfo.ID))
        //    {
        //        EventInfo eInfo = Db.EventInfoes.Find(eventInfo.ID);
        //        eventInfo.AboutEventIcon = string.IsNullOrEmpty(eventInfo.AboutEventIcon) ? eInfo.AboutEventIcon : eventInfo.AboutEventIcon;
        //        eventInfo.AccomodationsIcon = string.IsNullOrEmpty(eventInfo.AccomodationsIcon) ? eInfo.AccomodationsIcon : eventInfo.AccomodationsIcon;
        //        eventInfo.ArrivalInfoIcon = string.IsNullOrEmpty(eventInfo.ArrivalInfoIcon) ? eInfo.ArrivalInfoIcon : eventInfo.ArrivalInfoIcon;
        //        eventInfo.HelpDeskIcon = string.IsNullOrEmpty(eventInfo.HelpDeskIcon) ? eInfo.HelpDeskIcon : eventInfo.HelpDeskIcon;
        //        eventInfo.ThingsToKnowIcon = string.IsNullOrEmpty(eventInfo.ThingsToKnowIcon) ? eInfo.ThingsToKnowIcon : eventInfo.ThingsToKnowIcon;
        //        Db.Entry(eInfo).CurrentValues.SetValues(eventInfo);
        //        Db.SaveChanges();
        //        return;
        //    }
        //    Db.EventInfoes.Add(eventInfo);
        //    Db.SaveChanges();
        //}

        public void AddEventInformation(EventInformation eventInformation, string aboutEventIconfileName, int eventID)
        {
            EventInformation updateEventInfo = Db.EventInformations.FirstOrDefault(x => x.ID == eventInformation.ID);

            if (updateEventInfo != null)
            {
                updateEventInfo.HideSection = eventInformation.HideSection;
                updateEventInfo.AboutSection = eventInformation.AboutSection;
                if (string.IsNullOrEmpty(aboutEventIconfileName))
                {
                    updateEventInfo.IconSection = eventInformation.IconSection;
                }
                updateEventInfo.DescriptionSection = eventInformation.DescriptionSection;
                updateEventInfo.IsExternalLink = eventInformation.IsExternalLink;
                updateEventInfo.ExternalLinkURL = eventInformation.ExternalLinkURL;
                updateEventInfo.SectionType = eventInformation.SectionType;
                updateEventInfo.SectionOrder = eventInformation.SectionOrder;
                Db.SaveChanges();
            }
            else
            {
                Db.EventInformations.Add(new EventInformation
                {
                    EventID = eventID,
                    HideSection = eventInformation.HideSection,
                    AboutSection = eventInformation.AboutSection,
                    IconSection = aboutEventIconfileName,
                    DescriptionSection = eventInformation.DescriptionSection,
                    IsExternalLink = eventInformation.IsExternalLink,
                    ExternalLinkURL = eventInformation.ExternalLinkURL,
                    SectionType = eventInformation.SectionType,
                    SectionOrder = eventInformation.SectionOrder
                });
                Db.SaveChanges();
            }
        }

        public void DeleteEventInfos(int id)
        {
            List<EventInformation> eventInfos = Db.EventInformations.Where(x => x.EventID == id).ToList();
            Db.EventInformations.RemoveRange(eventInfos);
            Db.SaveChanges();
        }

        public async Task<List<EventInformation>> geteventinformation(int id)
        {
            List<EventInformation> eventInformation = await Db.EventInformations.Where(x => x.EventID == id).ToListAsync();
            return eventInformation;
        }
        
        public string createGUIDFileNameFromFile(System.Web.HttpPostedFileBase file)
        {
            string fileName = string.Empty;
            if (file != null)
                fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
            return fileName;
        }

        public async Task SaveChatMessages(int SID, int RID, bool IsGroup, int? EventGroupID, String msg)
        {
            Db.Messagings.Add(new Messaging
            {
                SenderID = SID,
                ReceiverID = RID,
                IsGroup = IsGroup,
                EventGroupID = EventGroupID,
                Text = msg,
                CreatedDate = DateTime.Now
            });
            await Db.SaveChangesAsync();
        }

        public async Task<IEnumerable<ChatMessaging>> GetChatMessages(int SID, int RID, bool IsGroup)
        {
            var msgs = await Db.Messagings
                         .Where(n => (n.SenderID == SID && n.ReceiverID == RID && IsGroup == false) || (n.SenderID == RID && n.ReceiverID == SID && IsGroup == false))
                         .OrderBy(o => o.CreatedDate)
                         .Select(c => new ChatMessaging
                         {
                             SendersID = c.SenderID,
                             Text = c.Text
                         }).ToListAsync();
            return msgs;
        }

        public async Task<bool> EmailChatMessages(int SID, int RID, bool IsGroup)
        {
            try
            {
                var msgs = await Db.Messagings
                             .Where(n => (n.SenderID == SID && n.ReceiverID == RID && IsGroup == false) || (n.SenderID == RID && n.ReceiverID == SID && IsGroup == false))
                             .OrderBy(o => o.CreatedDate)
                             .Select(c => new ChatMessaging
                             {
                                 SendersID = c.SenderID,
                                 Text = c.Text
                             }).ToListAsync();

                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);

                foreach (var msg in msgs)
                {
                    var uname = Db.Attendes.Find(msg.SendersID).FirstName;
                    sw.WriteLine(uname + ':' + msg.Text);
                }
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                // await CommonHelpers.Helpers.SendEmail("ChatMessage", "ChatMessage", null, null, ms, true);
                using (var EmailSender = new EmailSender(Db.Attendes.Find(SID).Email, "Notes"))
                {
                    EmailSender.SendEmailWithAttachment(ms);
                    return true;
                }
                // await CommonHelpers.Helpers.SendEmail("ChatMessage", "ChatMessage", null, null, ms, true);
            }

            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<Question>> Questions()
        {
            return await Db.Questions.Where(n => n.Survey.SurveyLevel == "Activity Level").ToListAsync();
        }

        public async Task<IEnumerable<QuestionsWithOptions>> QuestionswithOptions(bool IsEventLevel, bool IsAtAppLevel, int UserID, int eventID)
        {
            if (IsEventLevel == true && IsAtAppLevel == true)
            {
                //var AttemptedQuestions = Db.SurveySubmittedFors.Where(x => x.UserID == UserID && (x.Question.IsAtEventLevel == true || x.Question.IsAtAppLevel == true)).Count();
                var AttemptedQuestions = Db.SurveySubmittedFors.Where(x => x.UserID == UserID && (x.Question.Survey.SurveyLevel == "Event Level" || x.Question.Survey.SurveyLevel == "App Level") && x.EventID == eventID).Count();
                if (AttemptedQuestions == 0)
                {
                    var QuestionswithOptions = Db.Questions.
                                               Where(x => (x.Survey.SurveyLevel == "Event Level" || x.Survey.SurveyLevel == "App Level") && x.Survey.EventID == eventID && x.Survey.ActivityID == null).
                                               Select(n => new QuestionsWithOptions
                                               {
                                                   QID = n.ID,
                                                   QuestionText = n.QuestionText,
                                                   IsAtActiivtyLevel = false,
                                                   QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                                   HideorShowQuestion = n.HideorShowQuestion,
                                                   options = Db.Options.Where(o => o.QID == n.ID).Select(p => p.option1).ToList(),
                                                   IsSubmitted = false
                                               }).ToList();
                    return QuestionswithOptions;
                }
                else
                {
                    var QuestionswithOptions = Db.Questions.
                                           Where(x => (x.Survey.SurveyLevel == "Event Level" || x.Survey.SurveyLevel == "App Level") && x.Survey.EventID == eventID && x.Survey.ActivityID == null).
                                           Select(n => new QuestionsWithOptions
                                           {
                                               QID = n.ID,
                                               QuestionText = n.QuestionText,
                                               IsAtActiivtyLevel = false,
                                               QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                               HideorShowQuestion = n.HideorShowQuestion,
                                               options = Db.Options.Where(o => o.QID == n.ID).Select(p => p.option1).ToList(),
                                               IsSubmitted = true
                                           }).ToList();
                    return QuestionswithOptions;
                }
            }
            else if (IsEventLevel == true)
            {
                var AttemptedQuestions = Db.SurveySubmittedFors.Where(x => x.UserID == UserID && (x.Question.Survey.SurveyLevel == "Event Level") && x.EventID == eventID).Count();
                if (AttemptedQuestions == 0)
                {
                    var QuestionswithOptions = Db.Questions.
                                               //Join(Db.Options, q => q.ID, o => o.QID,
                                               //(q, o) => new { q, o }).
                                               Where(x => x.Survey.SurveyLevel == "Event Level" && x.Survey.EventID == eventID && x.Survey.ActivityID == null).
                                               Select(n => new QuestionsWithOptions
                                               {
                                                   QID = n.ID,
                                                   QuestionText = n.QuestionText,
                                                   IsAtActiivtyLevel = false,
                                                   QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                                   HideorShowQuestion = n.HideorShowQuestion,
                                                   options = Db.Options.Where(o => o.QID == n.ID).Select(p => p.option1).ToList(),
                                                   IsSubmitted = false
                                               }).ToList();
                    return QuestionswithOptions;
                }
                else
                {
                    var QuestionswithOptions = Db.Questions.
                                           Where(x => x.Survey.SurveyLevel == "Event Level" && x.Survey.EventID == eventID && x.Survey.ActivityID == null).
                                           Select(n => new QuestionsWithOptions
                                           {
                                               QID = n.ID,
                                               QuestionText = n.QuestionText,
                                               IsAtActiivtyLevel = false,
                                               QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                               HideorShowQuestion = n.HideorShowQuestion,
                                               options = Db.Options.Where(o => o.QID == n.ID).Select(p => p.option1).ToList(),
                                               IsSubmitted = true
                                           }).ToList();
                    return QuestionswithOptions;
                }
            }
            else
            {
                var AttemptedQuestions = Db.SurveySubmittedFors.Where(x => x.UserID == UserID && (x.Question.Survey.SurveyLevel == "App Level") && x.EventID == eventID).Count();
                if (AttemptedQuestions == 0)
                {
                    var QuestionswithOptions = Db.Questions.
                                           Where(x => x.Survey.SurveyLevel == "App Level" && x.Survey.EventID == eventID && x.Survey.ActivityID == null).
                                           Select(n => new QuestionsWithOptions
                                           {
                                               QID = n.ID,
                                               QuestionText = n.QuestionText,
                                               IsAtActiivtyLevel = false,
                                               QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                               HideorShowQuestion = n.HideorShowQuestion,
                                               options = Db.Options.Where(o => o.QID == n.ID).Select(p => p.option1).ToList(),
                                               IsSubmitted = false
                                           }).ToList();
                    return QuestionswithOptions;
                }
                else
                {
                    var QuestionswithOptions = Db.Questions.
                                          Where(x => x.Survey.SurveyLevel == "App Level" && x.Survey.EventID == eventID && x.Survey.ActivityID == null).
                                          Select(n => new QuestionsWithOptions
                                          {
                                              QID = n.ID,
                                              QuestionText = n.QuestionText,
                                              IsAtActiivtyLevel = false,
                                              QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                              HideorShowQuestion = n.HideorShowQuestion,
                                              options = Db.Options.Where(o => o.QID == n.ID).Select(p => p.option1).ToList(),
                                              IsSubmitted = true
                                          }).ToList();
                    return QuestionswithOptions;
                }
            }
        }

        public async Task<IEnumerable<QuestionsWithOptions>> ActivitySurveyQuestions(int UserID, int activityID)
        {
            var AttemptedQuestions = Db.SurveySubmittedFors.Where(x => x.UserID == UserID && x.ActivityID == activityID).Count();
            if (AttemptedQuestions == 0)
            {
                var QuestionswithOptions = Db.Questions.
                                    Where(x => x.Survey.SurveyLevel == "Activity Level" && x.Survey.ActivityID == activityID).
                                    Select(n => new QuestionsWithOptions
                                    {
                                        QID = n.ID,
                                        QuestionText = n.QuestionText,
                                        IsAtActiivtyLevel = true,
                                        IsAtVendorLevel = false,
                                        IsAtSponsorLevel = false,
                                        QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                        HideorShowQuestion = n.HideorShowQuestion,
                                        options = Db.Options.Where(x => x.QID == n.ID).Select(y => y.option1).ToList(),
                                        IsSubmitted = false
                                    }).ToList();
                return QuestionswithOptions;
            }
            else
            {
                var QuestionswithOptions = Db.Questions.
                                Where(x => x.Survey.SurveyLevel == "Activity Level" && x.Survey.ActivityID == activityID).
                                Select(n => new QuestionsWithOptions
                                {
                                    QID = n.ID,
                                    QuestionText = n.QuestionText,
                                    IsAtActiivtyLevel = true,
                                    IsAtVendorLevel = false,
                                    IsAtSponsorLevel = false,
                                    QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                    HideorShowQuestion = n.HideorShowQuestion,
                                    options = Db.Options.Where(x => x.QID == n.ID).Select(y => y.option1).ToList(),
                                    IsSubmitted = true
                                }).ToList();
                return QuestionswithOptions;
            }
        }

        public async Task<IEnumerable<QuestionsWithOptions>> VendorSurveyQuestions(int UserID, int vendorID)
        {
            var AttemptedQuestions = Db.SurveySubmittedFors.Where(x => x.UserID == UserID && x.VendorID == vendorID).Count();
            if (AttemptedQuestions == 0)
            {
                var QuestionswithOptions = Db.Questions.
                                    Where(x => x.Survey.SurveyLevel == "Vendor Level" && x.Survey.VendorID == vendorID).
                                    Select(n => new QuestionsWithOptions
                                    {
                                        QID = n.ID,
                                        QuestionText = n.QuestionText,
                                        IsAtActiivtyLevel = false,
                                        IsAtVendorLevel = true,
                                        IsAtSponsorLevel = false,
                                        QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                        HideorShowQuestion = n.HideorShowQuestion,
                                        options = Db.Options.Where(x => x.QID == n.ID).Select(y => y.option1).ToList(),
                                        IsSubmitted = false
                                    }).ToList();
                return QuestionswithOptions;
            }
            else
            {
                var QuestionswithOptions = Db.Questions.
                                Where(x => x.Survey.SurveyLevel == "Vendor Level" && x.Survey.VendorID == vendorID).
                                Select(n => new QuestionsWithOptions
                                {
                                    QID = n.ID,
                                    QuestionText = n.QuestionText,
                                    IsAtActiivtyLevel = false,
                                    IsAtVendorLevel = true,
                                    IsAtSponsorLevel = false,
                                    QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                    HideorShowQuestion = n.HideorShowQuestion,
                                    options = Db.Options.Where(x => x.QID == n.ID).Select(y => y.option1).ToList(),
                                    IsSubmitted = true
                                }).ToList();
                return QuestionswithOptions;
            }
        }

        public async Task<IEnumerable<QuestionsWithOptions>> SponsorSurveyQuestions(int UserID, int sponsorID)
        {

            var AttemptedQuestions = Db.SurveySubmittedFors.Where(x => x.UserID == UserID && x.SponsorID == sponsorID).Count();
            if (AttemptedQuestions == 0)
            {
                var QuestionswithOptions = Db.Questions.
                                    Where(x => x.Survey.SurveyLevel == "Sponsor Level" && x.Survey.SponsorID == sponsorID).
                                    Select(n => new QuestionsWithOptions
                                    {
                                        QID = n.ID,
                                        QuestionText = n.QuestionText,
                                        IsAtActiivtyLevel = false,
                                        IsAtVendorLevel = false,
                                        IsAtSponsorLevel = true,
                                        QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                        HideorShowQuestion = n.HideorShowQuestion,
                                        options = Db.Options.Where(x => x.QID == n.ID).Select(y => y.option1).ToList(),
                                        IsSubmitted = false
                                    }).ToList();
                return QuestionswithOptions;
            }
            else
            {
                var QuestionswithOptions = Db.Questions.
                                Where(x => x.Survey.SurveyLevel == "Sponsor Level" && x.Survey.SponsorID == sponsorID).
                                Select(n => new QuestionsWithOptions
                                {
                                    QID = n.ID,
                                    QuestionText = n.QuestionText,
                                    IsAtActiivtyLevel = false,
                                    IsAtVendorLevel = false,
                                    IsAtSponsorLevel = true,
                                    QuestionResponseType = Db.ResponseTypes.FirstOrDefault(x => x.ResponseTypeID == n.QuestionResponseType).ResponseName,
                                    HideorShowQuestion = n.HideorShowQuestion,
                                    options = Db.Options.Where(x => x.QID == n.ID).Select(y => y.option1).ToList(),
                                    IsSubmitted = true
                                }).ToList();
                return QuestionswithOptions;
            }
        }

        public async Task<IEnumerable<EventQuestions>> GetEventQuestions(int? eventID)
        {
            var questions = Db.Questions.Where(x => x.Survey.EventID == eventID && x.Survey.SurveyLevel == "Event Level")
                              .Select(y => new EventQuestions
                              {
                                  QID = y.ID,
                                  QuestionText = y.QuestionText,
                                  QuestionResponseType = Db.ResponseTypes.FirstOrDefault(a => a.ResponseTypeID == y.QuestionResponseType).ResponseName,
                                  CreateDate = y.CreateDate,
                                  HideorShowQuestion = y.HideorShowQuestion
                              }).ToList();
            return questions;
        }

        public async Task<bool> AddNewSurvey(Response[] SurveyReposne, int UserID, int? eventID = null, int? activityID = null, int? vendorID = null, int? sponsorID = null)
        {
            try
            {
                for (int i = 0; i < SurveyReposne.Count(); i++)
                {
                    string[] OptionsSelected = SurveyReposne[i].OptionsSelected.Split(',');
                    for (int j = 0; j < OptionsSelected.Count(); j++)
                    {
                        Db.Responses.Add(new Response
                        {
                            QuestionID = SurveyReposne[i].QuestionID,
                            UserID = SurveyReposne[i].UserID,
                            OptionsSelected = OptionsSelected[j].ToString(),
                            CreatedDate = DateTime.Now
                        });
                        Db.SurveySubmittedFors.Add(new SurveySubmittedFor
                        {
                            UserID = UserID,
                            QuestionID = SurveyReposne[i].QuestionID,
                            EventID = eventID,
                            ActivityID = activityID,
                            VendorID = vendorID,
                            SponsorID = sponsorID
                        });
                        await Db.SaveChangesAsync();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public IEnumerable<Group> GetGroups(int eventID)
        {
            return Db.Events.Find(eventID).Activites.Select(x => new Group(
                 x.ID,
                 x.Name,
                 false
             )).Union(
                 Db.Events.Find(eventID).GroupInfoes.Select(x => new Group(
                     x.ID,
                     x.GroupName,
                     true
                 )));
        }
        
        public async Task SaveLocationDetails(int EventID, string coordinates, string LocationInfo, string name, string webpage, HttpPostedFileBase markerimagename1, HttpPostedFileBase markerimagename2, string imageSavePath, string WebpageName)
        {
            string Marker1Image = null;
            string Marker2Image = null;

            if (markerimagename1 != null)
            {
                Marker1Image = createGUIDFileNameFromFile(markerimagename1);
                markerimagename1.SaveAs(Path.Combine(imageSavePath, Marker1Image));
            }
            if (markerimagename2 != null)
            {
                Marker2Image = createGUIDFileNameFromFile(markerimagename2);
                markerimagename2.SaveAs(Path.Combine(imageSavePath, Marker2Image));
            }
            EventLocationDetail location = Db.EventLocationDetails.FirstOrDefault(x => x.EventId == EventID && x.Coordinates == coordinates);
            if (location != null)
            {
                location.LocationDescription = LocationInfo;
                location.Name = name;
                location.Webpageurl = webpage;
                if (Marker1Image != null)
                {
                    location.MarkerImage1 = Marker1Image;
                }
                if (Marker2Image != null)
                {
                    location.MarkerImage2 = Marker2Image;
                }
                location.WebPageName = webpage;
                await Db.SaveChangesAsync();
            }
            else
            {
                Db.EventLocationDetails.Add(new EventLocationDetail
                {
                    EventId = EventID,
                    Coordinates = coordinates,
                    LocationDescription = LocationInfo,
                    Name = name,
                    Webpageurl = webpage,
                    MarkerImage1 = Marker1Image,
                    MarkerImage2 = Marker2Image,
                    WebPageName = webpage
                });
                await Db.SaveChangesAsync();
            }
        }

        public async Task RemoveMarker(int EventID, string coordinates, string loactiondetails)
        {
            EventLocationDetail Loaction = await Db.EventLocationDetails.FirstOrDefaultAsync(x => x.EventId == EventID && x.Coordinates == coordinates && x.LocationDescription == loactiondetails);
            Db.EventLocationDetails.Remove(Loaction);
            await Db.SaveChangesAsync();
        }
        
        public bool ContactSupport(string attendeeEmail, string message)
        {
            using (var emailSender = new EmailNotifier(Db.Configs.FirstOrDefault()?.SupportEmail))
            {
                emailSender.Subject = string.Concat("Attendee Support - ", attendeeEmail); ;
                emailSender.eBody = message;
                emailSender.SendEmail(attendeeEmail);
            }
            return true;
        }

        public void ScheduleActivityTimeNotifications()
        {
            foreach (var activity in Db.Activites)
            {
                //if(activity.StartTime )
            }
        }

        public async Task SaveLogs(int User_ID, string Module, string Page, string operation)
        {

            Db.Sys_Log.Add(new Sys_Log
            {
                Module = Module,
                Page = Page,
                Operation = operation,
                Logdate = DateTime.Now,
                ModifiedBy = User_ID
            });
            await Db.SaveChangesAsync();
        }

        public async Task SendScheduledActivityNotifications()
        {
            try
            {
                var eventIds = Db.Events.Select(x => x.ID).ToList();
                foreach (var id in eventIds)
                {
                    var activitiesAboutToStart = ActivitiesAboutToStart(id);
                    foreach (var activity in activitiesAboutToStart)
                    {
                        var userSessions = GetActivityUserSessions(activity);
                        var successCount = 0;
                        var title = $"{activity.Name} is About To Start";
                        var body = $"{activity.Name} is About To Start.";
                        var notificationID = await notifications.MaintainNotificationHistory(activity.EventID.Value, title, body, null);
                        foreach (var session in userSessions)
                        {
                            try
                            {
                                NotificationData notificationData = GetNotificationWithAuthToken(session);
                                await sendPushNotificationWithEmail(session.DeviceToken, title, body, activity.EventID.Value, session.AttendesID.Value, session.AttendesID.Value, notificationID);
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                new Error().LogErrorToDB(ex, "Job");
                            }
                        }
                        if (successCount > 0)
                            activity.IsScheduleNotificationSent = true;
                    }
                }
            }
            finally
            {
                Db.SaveChanges();
            }
        }

        public List<UserSession> GetActivityUserSessions(Activite activity)
        {
            return activity?.BookMarks.SelectMany(x => x.Attende.UserSessions
                 .Where(y => y.IsActive == true && y.Platform != "Web" && !string.IsNullOrEmpty(y.DeviceToken))
                 .Distinct(new UserDeviceTokenComparer())).ToList();
        }

        public List<UserSession> GetEventUserSessions(Event @event)
        {
            return @event.AttendesEvents.SelectMany(x => x.Attende.UserSessions
           .Where(y => y.IsActive == true && y.Platform != "Web" && !string.IsNullOrEmpty(y.DeviceToken))
               .Distinct(new UserDeviceTokenComparer())
            ).ToList();
        }

        public async Task SendSurveyNotifications()
        {
            try
            {
                foreach (var @event in Db.Events)
                {
                    var completedActivities = CompletedActivities(GetEventTimeZone(@event), @event.ID);
                    foreach (var activity in completedActivities)
                    {
                        var activitySuccessCount = 0;
                        var subject = "Submit Survey";
                        var body = $"Submit Survey for {activity.Name}";
                        var userSessions = GetActivityUserSessions(activity);
                        var notificationID = await notifications.MaintainNotificationHistory(activity.EventID.Value, subject, body, null);
                        foreach (var session in userSessions)
                        {
                            try
                            {
                                NotificationData notificationData = GetNotificationWithAuthToken(session);
                                await sendPushNotificationWithEmail(session.DeviceToken, subject, body, activity.EventID.Value, session.AttendesID.Value, 0, notificationID);
                                activitySuccessCount++;
                            }
                            catch (Exception ex)
                            {
                                new Error().LogErrorToDB(ex, "Job");
                            }
                        }
                        if (activitySuccessCount > 0)
                            activity.IsSurveyNotificationSent = true;
                    }
                }
                var completedEvents = CompletedEvents();
                foreach (var @event in completedEvents)
                {
                    var eventSucessCount = 0;
                    var userSessions = GetEventUserSessions(@event);
                    var subject = "Submit Survey";
                    var body = $"Submit Survey for {@event.EventName}";
                    var notificationID = await notifications.MaintainNotificationHistory(@event.ID, subject, body, null);
                    foreach (var session in userSessions)
                    {
                        try
                        {
                            NotificationData notificationData = GetNotificationWithAuthToken(session);
                            await sendPushNotificationWithEmail(session.DeviceToken, subject, body, @event.ID, session.AttendesID.Value, 0, notificationID);
                            eventSucessCount++;
                        }
                        catch (Exception ex)
                        {
                            new Error().LogErrorToDB(ex, "Job");
                        }
                    }
                    if (eventSucessCount > 0)
                        @event.IsSurveyNotificationSent = true;
                }
            }
            finally
            {
                Db.SaveChanges();
            }
        }

        public async Task sendPushNotificationWithEmail(string deviceToken, string subject, string body, int eventID, int aid, int sentBy, int notificationID/*, NotificationData notificationData*/)
        {
            var status = await SendNotificationAndLogInDB(deviceToken, subject, body, eventID, aid, sentBy, notificationID);

            if (status == 1)
            {
                AttendesLogic attendesLogic = new AttendesLogic();
                if (attendesLogic.checkIfAttendeeAlertEmailEnabled(aid))
                {
                    var email = (await attendesLogic.GetAttendee(aid))?.Email;
                    using (var emailSender = new EmailSender(email, subject))
                    {
                        await emailSender.SendEmailAsync(body);
                    }
                }
            }
        }

        public NotificationData GetNotificationWithAuthToken(UserSession userSession)
        {
            return new NotificationData() { Token = userSession.AuthToken, IsChat = false };
        }

        public async Task<int> SendPushNotification(string deviceToken, string subject, string body, int eventID, int aid, int sentBy, int notificationID)
        {
            var status = await SendNotificationAndLogInDB(deviceToken, subject, body, eventID, aid, sentBy, notificationID);
            return status;
        }


        public async Task SendEmail(int aid, string subject, string body)
        {
            AttendesLogic attendesLogic = new AttendesLogic();
            if (attendesLogic.checkIfAttendeeAlertEmailEnabled(aid))
            {
                var email = (await attendesLogic.GetAttendee(aid))?.Email;
                using (var emailSender = new EmailSender(email, subject))
                {
                    await emailSender.SendEmailAsync(body);
                }
            }
        }

        public bool DeletePhoto(int PhotoID)
        {
            try
            {
                Photo eventPhoto = Db.Photos.FirstOrDefault(x => x.ID == PhotoID);
                eventPhoto.isDeleted = true;
                Db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public async Task<int> SendNotificationAndLogInDB(string deviceToken, string subject, string body, int eventID, int aid, int sentBy, int notificationID/*, NotificationData notificationData*/)
        {
            Notifications notifications = new Notifications();
            var rec = SendNotificationFromFirebaseCloud(deviceToken, subject, body);
            if (rec == 1)
            {
                notifications.MaintainUserNotificationHistory(notificationID, aid);
            }
            return rec;
        }

        public IEnumerable<UserSession> GetAttendeeSessions(int attendeeID)
        {
            var attendee = Db.Attendes.Find(attendeeID);
            var sessions = attendee?.UserSessions.Where(y => y.IsActive == true && y.Platform != "Web" && !string.IsNullOrEmpty(y.DeviceToken)).Distinct(new UserDeviceTokenComparer());
            return sessions;
        }

        public List<Activite> CompletedActivities(string timezone, int eventID)
        {
            var endTimeThreshold = GetTimeZoneAdjustedTime(DateTime.Now.AddMinutes(5), timezone);
            return Db.Activites.Where(x => x.EventID == eventID && x.EndTime <= endTimeThreshold && !x.IsSurveyNotificationSent).ToList();
        }

        public List<Event> CompletedEvents()
        {
            List<Event> events = new List<Event>();
            foreach (var @event in Db.Events)
            {
                var endTimeThreshold = DateTime.Now.AddMinutes(5);

                if (@event.EndDate <= GetTimeZoneAdjustedTime(endTimeThreshold, GetEventTimeZone(@event)) && !@event.IsSurveyNotificationSent)
                {
                    events.Add(@event);
                }
            }
            return events;
        }

        public List<Activite> ActivitiesAboutToStart(int eventID)
        {
            var @event = Db.Events.Find(eventID) ?? throw new Exception("No events are present");

            string eventTimeZone = GetEventTimeZone(@event);
            int starttime = @event?.ScheduledActivityNotificationsTime ?? 20;

            DateTime currentDateTime = GetTimeZoneAdjustedCurrentTime(eventTimeZone);
            DateTime thresholdDateTime = DateTime.Now.AddMinutes(starttime);
            DateTime convertedThresholdDateTime = GetTimeZoneAdjustedTime(thresholdDateTime, eventTimeZone);
            return Db.Activites.Where(x => x.EventID == eventID).Where(x => x.StartTime > currentDateTime && x.StartTime <= convertedThresholdDateTime && !x.IsScheduleNotificationSent).ToList();
        }

        public DateTime GetTimeZoneAdjustedCurrentTime(string timeZone)
        {
            TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return TimeZoneInfo.ConvertTime(DateTime.Now, timeInfo);
        }

        public DateTime GetTimeZoneAdjustedTime(DateTime dateTime, string timeZone)
        {
            TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return TimeZoneInfo.ConvertTime(dateTime, timeInfo);
        }

        public string GetEventTimeZone(Event @event)
        {
            return @event.EventTimeZone ?? TimeZoneInfo.Utc.Id;
        }

        public async Task<bool> AddResyncedAttendees(GPSAttendees gPSAttendees, Func<ProgramAttendee, bool> validator, int eventID, string ImageSavePath)
        {
            AttendesLogic attendesLogic = new AttendesLogic();

            foreach (var item in gPSAttendees.ProgramAttendees.Where(validator))
            {
                var attendee = Db.Attendes.FirstOrDefault(x => x.Email == item.Email);
                if (attendee == null)
                {
                    Attende at = new Attende
                    {
                        FirstName = item.FirstName,
                        Lastname = item.LastName,
                        Thumbnail = attendesLogic.GenerateImageFromName(item.FirstName, item.LastName, ImageSavePath),
                        Email = item.Email,
                        LinkedinURL = item.LinkedInURL,
                        IsSpeaker = item.AttendeeType.ToLower() == "speaker"
                    };
                    Db.Attendes.Add(at);
                    AttendesEvent ae = new AttendesEvent
                    {
                        AttendesID = at.ID,
                        EventID = eventID
                    };
                    at.EnableMessaging = true;
                    Db.AttendesEvents.Add(ae);
                    await Db.SaveChangesAsync();
                }
                else
                {
                    attendee.FirstName = item.FirstName;
                    attendee.Lastname = item.LastName;
                    // attendee.Thumbnail = attendesLogic.GenerateImageFromName(item.FirstName, item.LastName, ImageSavePath);
                    attendee.Email = item.Email;
                    attendee.LinkedinURL = item.LinkedInURL;
                    attendee.IsSpeaker = item.AttendeeType.ToLower() == "speaker";
                    if (!Db.AttendesEvents.Any(x => x.AttendesID == attendee.ID && x.EventID == eventID))
                    {
                        AttendesEvent ae = new AttendesEvent
                        {
                            AttendesID = attendee.ID,
                            EventID = eventID
                        };
                        Db.AttendesEvents.Add(ae);
                    }
                    await Db.SaveChangesAsync();
                }
            }
            return true;
        }

        public void SendEmailforAttendeeNotification(string subject, string body, string Emailid)
        {
            try
            {
                using (var EmailNotifier = new EmailNotifier(Emailid))
                {
                    EmailNotifier.Subject = subject;
                    EmailNotifier.eBody = body;
                    EmailNotifier.SendEmail(Db.Configs.FirstOrDefault()?.SupportEmail);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<UserPackageType> UserPackageTypes()
        {
            return Db.UserPackageTypes.ToList();
        }

        public async Task DeleteDocument(int id)
        {
            var documents = Db.Documents.FirstOrDefault(x => x.ID == id);
            var favouritedocuments = Db.DocumentFavourites.Where(x => x.DocumentID == id).ToList();
            foreach (var x in favouritedocuments)
            {
                Db.DocumentFavourites.Remove(x);
            }
            Db.Documents.Remove(documents);
            await Db.SaveChangesAsync();
        }



        public async Task BookmarkedActivities(int galEventid, int EventId)
        {
            try
            {
                AuthLogic authLogic = new AuthLogic();
                int GalacticEventId = galEventid;  //Db.Events.FirstOrDefault(x => x.ID == EventId).GalacticEventId;

                await FetchActivities(GalacticEventId, EventId, EventId); //authLogic.EventActivities(GalacticEventId);

                var AttendeeEmails = Db.AttendesEvents.Where(x => x.EventID == EventId).Select(y => y.Attende.Email).ToList();

                foreach (var Email in AttendeeEmails)
                {
                    var paxactivities = authLogic.getPaxActivities(GalacticEventId, Email);
                    var Activities = paxactivities.PaxActivities.ToList();
                    if (Activities != null)
                    {
                        foreach (var Act in Activities)
                        {
                            var attendeesid = Db.Attendes.FirstOrDefault(y => y.Email == Email)?.ID;
                            if (!Db.BookMarks.Any(x => x.Activite.GalacticActivityId == Act.ActivityID && x.AttendesID == attendeesid))
                            {
                                var dbActivityID = Db.Activites.Where(x => x.EventID == EventId).FirstOrDefault(y => y.GalacticActivityId == Act.ActivityID)?.ID;
                                if (dbActivityID != null)
                                {
                                    Db.BookMarks.Add(new BookMark
                                    {
                                        ActivityID = dbActivityID.Value,
                                        AttendesID = Db.Attendes.FirstOrDefault(y => y.Email == Email).ID
                                    });
                                }
                            }
                        }
                    }
                }
                Db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void UploadFormWaiverTemplate(string waiverFormTemplateName, int eventID, string docBasePath, int activityID)
        {
            //Event @event = Db.Events.Find(eventID);
            //@event.WaiverFormTemplatePath = waiverFormTemplateName;

            var activity = Db.Activites.Find(activityID);
            activity.WaiverFormTemplatePath = waiverFormTemplateName;

            var documentLocation = Path.Combine(docBasePath, waiverFormTemplateName);
            PDFFormEditor pDFFormEditor = new PDFFormEditor(documentLocation);
            var keys = pDFFormEditor.GetKeyTemplateKeys();
            foreach (string formKey in keys)
            {
                Db.WaiverFormTemplateMappings.Add(new WaiverFormTemplateMapping()
                {
                    EventID = eventID,
                    FormKey = formKey,
                    ActivityID = activityID
                });
            }
            Db.SaveChanges();
        }

        public void SaveFormWaiverMappingsAndSendEmails(string docBasePath, int activityID, Dictionary<int, int> mappingData)
        {
            Activite activity = Db.Activites.Find(activityID);
            var templatePath = Path.Combine(docBasePath, activity.WaiverFormTemplatePath);

            sendWaiverFormEmails(templatePath, activityID, Guid.NewGuid() + ".pdf", docBasePath, mappingData);
        }

        public List<WaiverFormTemplateMapping> GetFormWaiverTemplateFieldMappings(int activityID)
        {
            IQueryable<WaiverFormTemplateMapping> eventFormMappings = Db.WaiverFormTemplateMappings.Where(x => x.ActivityID == activityID);
            return eventFormMappings.ToList();
        }

        public bool SaveWaiverFormMappingData(IDictionary<int, int> mappingData, int activityID)
        {
            var mappings = Db.WaiverFormTemplateMappings.Where(x => x.ActivityID == activityID).ToList();
            mappings.ForEach(x =>
            {
                x.Value = null;
            });
            foreach (var item in mappingData)
            {
                int mappingKeyID = item.Key;
                var mappingEntity = mappings.FirstOrDefault(x => x.ID == mappingKeyID);
                mappingEntity.Value = item.Value;
            }
            Db.SaveChanges();
            return true;
        }

        public bool DeleteWaiverForm(int activityID)
        {
            var activity = Db.Activites.Find(activityID);
            activity.WaiverFormTemplatePath = null;
            var waiverFormTemplateMappings = Db.WaiverFormTemplateMappings.Where(x => x.ActivityID == activityID);
            Db.WaiverFormTemplateMappings.RemoveRange(waiverFormTemplateMappings);
            Db.SaveChanges();
            return true;
        }

        // Need to check stream solution, instead of storing in physical location
        public void sendWaiverFormEmails(string pdfLocation, int activityID, string desFile, string docBasePath, Dictionary<int, int> mappingData)
        {
            var desFilePath = Path.Combine(docBasePath, desFile);
            var activityAttendees = Db.Attendes.Where(x => x.BookMarks.Any(y => y.ActivityID == activityID));

            foreach (var attendee in activityAttendees)
            {
                var mappingStrings = new Dictionary<string, string>();
                PDFFormEditor pDFFormEditor = new PDFFormEditor(pdfLocation);
                foreach (var item in mappingData)
                {
                    var key = Db.WaiverFormTemplateMappings.Find(item.Key).FormKey;
                    var mappingValueName = Db.WaiverFormMappingValues.Find(item.Value).Name;
                    string value = string.Empty;
                    if (mappingValueName == "Guest Name")
                    {
                        value = attendee.FirstName + " " + attendee.Lastname;
                    }
                    mappingStrings.Add(key, value);
                }
                pDFFormEditor.GeneratePDFFromTemplate(desFilePath, mappingStrings);
                using (var EmailSender = new EmailSender(attendee.Email, "Waiver Form"))
                {
                    EmailSender.eBody = "Please find the attached Waiver Form";
                    EmailSender.SendEmailWithAttachment(File.OpenRead(desFilePath));
                }
                //destStream.Dispose();
            }
        }

        public  void UploadFilesAzureAsync(HttpPostedFileBase image, string imagename)
        {
            try
            { 
                string AzureConnectionString = "DefaultEndpointsProtocol=https;AccountName=itsmytrpstor;AccountKey=yAY3ojfm5Xusdf9E2E78QTlsrwLdPKUNM5x6/Ve3s2a3zMhdnl4nNMtpyOjUw2uI4abXAS8bxSYzA5opd9UeBQ==;EndpointSuffix=core.windows.net";
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(AzureConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                string containerName = "eventappcontainer";  //storageDetails.BlobContainer;
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
                blobContainer.CreateIfNotExists();
                blobContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                CloudBlockBlob cloudBlockBlob = blobContainer.GetBlockBlobReference(@"EventApp\" + imagename);
                cloudBlockBlob.Properties.ContentType = image.ContentType;
                cloudBlockBlob.UploadFromStream(image.InputStream);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DownloadFile(string imagename)
        {
            try
            {
                string containerName = "eventappcontainer";
                string AzureConnectionString = "DefaultEndpointsProtocol=https;AccountName=itsmytrpstor;AccountKey=yAY3ojfm5Xusdf9E2E78QTlsrwLdPKUNM5x6/Ve3s2a3zMhdnl4nNMtpyOjUw2uI4abXAS8bxSYzA5opd9UeBQ==;EndpointSuffix=core.windows.net";
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(AzureConnectionString);
                CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = blobClient.GetContainerReference(containerName);
                CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(imagename);
                Stream file = File.OpenWrite(@"D:\AzureImage\" + imagename);
                blockBlob.DownloadToStream(file);

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public string GetWeatherDetails(int eventId, string locationName)
        {

            if (string.IsNullOrEmpty(locationName))
            {
                locationName = Db.Events.FirstOrDefault(s => s.ID == eventId).Destination;
            }

            var currentWeather = "https://api.openweathermap.org/data/2.5/weather?q=";

            //var locationName = "Machilipatnam,India";
            var apiKey = "&APPID=779a802219349d36aef0cef9674b0d93";
            var url = string.Concat(currentWeather, locationName, apiKey);


            WebRequest request = HttpWebRequest.Create(url);

            WebResponse response = request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());

            string responseText = reader.ReadToEnd();

            //JObject json = JObject.Parse(responseText);

            return responseText;
        }

        

        //public class Result
        //{
        //    public string error { get; set; }
        //}

        //public class RootObject
        //{
        //    public long multicast_id { get; set; }
        //    public int success { get; set; }
        //    public int failure { get; set; }
        //    public int canonical_ids { get; set; }
        //    public List<Result> results { get; set; }
        //}
        //public RootObject SendPush(string devicetoken)
        //{

        //    WebClient clients = new WebClient();
        //    Uri uri = new Uri("https://gcm-http.googleapis.com/gcm/send");
        //    NameValueCollection parameters = new NameValueCollection();
        //    parameters.Add("to",devicetoken);
        //    parameters.Add("",);
        //    clients.UploadValuesAsync(uri, parameters);
        //    clients.UploadValuesCompleted += Client_UploadValuesCompleted;
        //    return RootObject;
        //}

        //private void Client_UploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}
        //create event / update event
        //public async Task<Event> CreateUpdateEvent(Event saveEvent)
        //{
        //    if (saveEvent.ID > 0)
        //    {
        //        //write update logic here
        //        var updateEvent = Db.Events.Find(saveEvent.ID);

        //        return saveEvent;
        //    }
        //    else
        //    {
        //        Db.Events.Add((saveEvent));
        //        return saveEvent;
        //    }
        //}
    }
}
