using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using CommonHelpers;
using EventManagement.BusinessLogic.Business.Email_Notifier;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.BusinessLogic.Classes;
using EventManagement.BusinessLogic.Models.APIDTOOjbects;
using EventManagement.BusinessLogic.Static_Configs;
using EventManagement.DataAccess.DataBase.Model;
using Newtonsoft.Json;
using RestSharp;

namespace EventManagement.BusinessLogic.Business
{
    public class AttendeeComparer : IEqualityComparer<Attende>
    {
        public bool Equals(Attende x, Attende y)
        {
            if (x.ID == y.ID)
                return true;

            return false;
        }

        public int GetHashCode(Attende obj)
        {
            return obj.ID.GetHashCode();
        }
    }

    public class AuthLogic : BaseLogic
    {
        //private const string _dateFormat = "dd/MM/yyyy hh:mm:ss";
        //public string GalacticApiBaseUrl
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(Helpers.GetAppSetting("GalacticApi")))
        //            return Helpers.GetAppSetting("GalacticApi");
        //        else return "https://ws.galacticps.com/";
        //    }
        //}

        public string GalacticApiSecretKeyEncoded
        {
            get
            {
                return Helpers.Base64Encode(string.Format("{0}+{1}", Helpers.GetAppSetting("GalacticSecretKey"), DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")));
            }
        }
        
        #region webcode
        public async Task<Attende> GetUserByEmail(string email)
        {
            return await Db.Attendes.FirstOrDefaultAsync(x =>
            x.Email.Trim().ToLower() == email.Trim().ToLower());
        }

        public async Task<UserSession> WebLogin(string email, string password)
        {
            var User = await GetUserByEmail(email);
            if (User == null && !User.IsAdmin)
                throw new Exception("User not found");
            else if (User.AdminInfo?.Password != password)
                throw new Exception("Invalid password");

            var token = Guid.NewGuid().ToString().Replace("-", "");
            // var ateendeeid = Db.Attendes.Where(x => x.Email == email).Select(y => y.ID).FirstOrDefault();
            var newSession = new UserSession
            {
                AttendesID = User.ID,
                CreateDate = DateTime.Now,
                AuthToken = token,
                IsActive = true,
                //AttendesID = 0,
                Platform = enPlatformTypes.Web.ToString(),
            };
            Db.UserSessions.Add(newSession);
            await Db.SaveChangesAsync();
            return newSession;
        }

        public async Task<List<Attende>> GetAllUser()
        {
            AttendesLogic attendesLogic = new AttendesLogic();
            return await attendesLogic.GetAttendes(checkAttendeeValidity: new AtteendeeValidator());
        }

        public static List<Attende> GetManagers()
        {
            Entities DbStatic = new Entities();
            return DbStatic.Attendes.Where(a => a.AdminInfo.UserRole == 1).ToList();
        }

        public static List<Widget> GetWidgetList()
        {
            Entities DbStatic = new Entities();
            return DbStatic.Widgets.ToList();
        }

        public static List<WidgetType> GetWidgetTypeList()
        {
            Entities DbStatic = new Entities();
            return DbStatic.WidgetTypes.ToList();
        }
        #endregion

        #region api code

        public async Task<object> UpdateAttendePassword(string email, string newPassword)
        {
            //Update attende password
            var client = new RestClient(string.Format("{0}/eventapp/api/updatepassword", GalacticConfigs.GalacticApiBaseUrl));
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", string.Format("{{\n\tEmail:'{0}',\n\tToken:'{1}'\n,NewPassword:'{2}'}}", email, GalacticApiSecretKeyEncoded, newPassword), ParameterType.RequestBody);
            IRestResponse<object> response2 = client.Execute<object>(request);
            return response2.Data;
        }

        public async Task<GPSAPILogin> AttendeVerifyEmail(string email)
        {
            bool IsAdmin = Db.Attendes.FirstOrDefault(x => x.Email == email)?.IsAdmin ?? false;
            if (IsAdmin)
            {
                GPSAPILogin login = new GPSAPILogin
                {
                    Token = null,
                    Email = email,
                    IsAuthenticated = true,
                    Message = null,
                    HasPassword = true,
                    Unique_Participant_ID = null,
                    IsValid = false,
                    UserEvents = null
                };
                return login;
            }
            else
            {

                //verify email from galactic code will be here
                var client = new RestClient(string.Format("{0}/eventapp/api/verifyEmail", GalacticConfigs.GalacticApiBaseUrl));
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", string.Format("{{\n\tEmail:'{0}',\n\tToken:'{1}'\n}}", email, GalacticApiSecretKeyEncoded), ParameterType.RequestBody);
                IRestResponse<GPSAPILogin> response2 = client.Execute<GPSAPILogin>(request);
                if (!response2.IsSuccessful)
                {
                    return response2.Data;
                }
                //var user = Db.Attendes.Where(x => x.Email == response2.Data.Email).FirstOrDefault();
                //if (user.Unique_Participant_ID == "" || user.Unique_Participant_ID == null)
                //{
                //    user.Unique_Participant_ID = response2.Data.Unique_Participant_ID;
                //    await Db.SaveChangesAsync();
                //}
                return response2.Data;
            }
        }
        
        public EventActivitiesGAPI EventActivities(int eventId)
        {
            var client = new RestClient(string.Format("{0}/eventapp/api/getprogramactivities", GalacticConfigs.GalacticApiBaseUrl));
            var request = new RestRequest(Method.POST);
            //request.AddHeader("postman-token", );
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", string.Format("{{\n\tGalacticEventid:'{0}',\n\tToken:'{1}'\n}}", eventId, GalacticApiSecretKeyEncoded), ParameterType.RequestBody);
            IRestResponse<EventActivitiesGAPI> response2 = client.Execute<EventActivitiesGAPI>(request);
            return response2.Data;
        }
        
        public GPSAttendees GetAttendeesByActivity(int eventId, int activityid)
        {
            var client = new RestClient(string.Format("{0}/eventapp/api/GetAttendeesByActivity", GalacticConfigs.GalacticApiBaseUrl));
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", string.Format("{{\n\t'ActivityID':{0},\n\'GalacticEventid':{1},\n\t'Token':'{2}'\n}}", activityid, eventId, GalacticApiSecretKeyEncoded), ParameterType.RequestBody);
            IRestResponse<GPSAttendees> response2 = client.Execute<GPSAttendees>(request);
            return response2.Data;
        }
        
        public GetPaxActivities getPaxActivities(int GalacticEventid, string Email)
        {
            var client = new RestClient(string.Format("{0}/eventapp/api/getPaxActivities", GalacticConfigs.GalacticApiBaseUrl));
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(new { Email = Email, GalacticEventid = GalacticEventid, Token = GalacticApiSecretKeyEncoded }), ParameterType.RequestBody); // string.Format("{{\n\t'Email':{0},\n\'GalacticEventid':{1},\n\t'Token':'{2}'\n}}", Email, GalacticEventid, GalacticApiSecretKeyEncoded)
            IRestResponse<GetPaxActivities> response2 = client.Execute<GetPaxActivities>(request);
            return response2.Data;
        }
        
        public GPSAttendees GetProgramAttendees(int eventId)
        {
            var client = new RestClient(string.Format("{0}/eventapp/api/getProgramAttendees", GalacticConfigs.GalacticApiBaseUrl));
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(new { GalacticEventID = eventId, Token = GalacticApiSecretKeyEncoded }), ParameterType.RequestBody);
            IRestResponse<GPSAttendees> response2 = client.Execute<GPSAttendees>(request);
            return response2.Data;
        }
        
        public GetEventInfo EventInfo(int eventId)
        {
            var client = new RestClient(string.Format("{0}/eventapp/api/GetEventInfo", GalacticConfigs.GalacticApiBaseUrl));
            var request = new RestRequest(Method.POST);
            //request.AddHeader("postman-token", );
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", string.Format("{{\n\tGalacticEventid:'{0}',\n\tToken:'{1}'\n}}", eventId, GalacticApiSecretKeyEncoded), ParameterType.RequestBody);
            IRestResponse<GetEventInfo> response2 = client.Execute<GetEventInfo>(request);
            return response2.Data;
        }

        public async Task<GPSAPILogin1> AttendiLogin(string email, string password, enPlatformTypes platform, string deviceToken, string isactive, Func<string, string> imageConvertionHandler, string imageSavePath, string mobileOSVersion, string mobileDeviceUniqueToken)
        {
            try
            {
                var client = new RestClient(string.Format("{0}/eventapp/api/Login", GalacticConfigs.GalacticApiBaseUrl));
                var request = new RestRequest(Method.POST);
                List<Event> AccessList = new List<Event>();
                GPSAPILogin1 accessEvents = new GPSAPILogin1();
                AttendesLogic attendesLogic = new AttendesLogic();
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", string.Format("{{\n\tEmail:'{0}',\n\tPassword:'{1}',\n\tToken:'{2}'\n}}", email, password, GalacticApiSecretKeyEncoded), ParameterType.RequestBody);
                IRestResponse<GPSAPILogin> response2 = client.Execute<GPSAPILogin>(request);
                EventLogic eventLogic = new EventLogic();
                Attende attende;

                if (response2.Data == null)
                {
                    throw response2.ErrorException;
                }
                bool isAdmin = await IsAdmin(email, password);
                if (response2.Data.IsAuthenticated || isAdmin)
                {
                    LogoutExistingUsersWithSameFBToken(deviceToken);
                    attende = await Db.Attendes.FirstOrDefaultAsync(x => x.Email == email);
                    string authToken; // Guid.NewGuid().ToString("n");
                    response2.Data.Message = null;
                    response2.Data.IsAuthenticated = true;
                    response2.Data.HasPassword = true;
                    if (attende == null)
                    {
                        attende = new Attende
                        {
                            Email = email,
                            Thumbnail = attendesLogic.GenerateImageFromName(response2.Data.FirstName, response2.Data.LastName, imageSavePath),
                            AddedON = DateTime.Now,
                            FirstName = response2.Data.FirstName,
                            Lastname = response2.Data.LastName,
                            IsSpeaker = response2.Data.AttendeeType?.ToLower() == "speaker"
                        };
                        Db.Attendes.Add(attende);
                        Db.SaveChanges();
                        authToken = Authenticate.GenerateToken(attende.ID);
                        response2.Data.Token = authToken;
                        attende.UserSessions.Add(new UserSession
                        {
                            AuthToken = authToken,
                            IsActive = (isactive == "0" ? false : true),
                            Platform = platform.ToString(),
                            CreateDate = DateTime.Now,
                            DeviceToken = deviceToken
                        });
                        Db.SaveChanges();
                        foreach (var item in response2.Data.UserEvents)
                        {
                            if (Db.Events.Any(x => x.GalacticEventId == item.GalacticEventID))
                            {
                                var eventRecord = Db.Events.FirstOrDefault(x => x.GalacticEventId == item.GalacticEventID);
                                if (!Db.AttendesEvents.Any(x => x.AttendesID == attende.ID && x.EventID == eventRecord.ID))
                                    Db.AttendesEvents.Add(new AttendesEvent
                                    {
                                        AttendesID = attende.ID,
                                        EventID = eventRecord.ID,
                                    });
                            }
                            else
                            {
                                //create event if not exsit 
                                //add event activities
                                var eventActivities = EventActivities(item.GalacticEventID);
                                var @event = new Event
                                {
                                    GalacticEventId = item.GalacticEventID,
                                    EventName = item.EventName,
                                    AttendesEvents = new List<AttendesEvent> { new AttendesEvent {
                                      AttendesID = attende.ID
                                 } },
                                    Activites = eventActivities.ProgramActivities.Select(x => new Activite
                                    {
                                        StartTime = x.ActivityStartTime.ToSafeDate(),
                                        EndTime = x.ActivityEndTime.ToSafeDate(),
                                        CreateON = DateTime.Now,
                                        Description = x.ActivityName,
                                    }).ToList()
                                };
                                await eventLogic.AddUpdateEvent(@event);
                            }
                        }
                        await Db.SaveChangesAsync();
                    }
                    else
                    {
                        authToken = Authenticate.GenerateToken(attende.ID);
                        response2.Data.Token = authToken;
                        foreach (var item in response2.Data.UserEvents)
                        {
                            if (Db.Events.Any(x => x.GalacticEventId == item.GalacticEventID))
                            {
                                if (!Db.AttendesEvents.Any(x => x.AttendesID == attende.ID))
                                {
                                    Db.AttendesEvents.Add(new AttendesEvent
                                    {
                                        AttendesID = attende.ID,
                                        EventID = Db.Events.FirstOrDefault(x => x.GalacticEventId == item.GalacticEventID).ID,
                                    });
                                }

                            }
                            else
                            {
                                //create event if not exsit                            
                                var @event = new Event
                                {
                                    GalacticEventId = item.GalacticEventID,
                                    EventName = item.EventName,
                                    AttendesEvents = new List<AttendesEvent> { new AttendesEvent {
                                    AttendesID = attende.ID
                                 } }
                                };
                                await eventLogic.AddUpdateEvent(@event);
                            }
                        }
                        attende.UserSessions.Add(new UserSession
                        {
                            AuthToken = authToken,
                            IsActive = (isactive == "0" ? false : true),
                            Platform = platform.ToString(),
                            CreateDate = DateTime.Now,
                            DeviceToken = deviceToken,
                            MobileOSVersion = mobileOSVersion,
                            MobileDeviceUniqueToken = mobileDeviceUniqueToken
                        });
                        await Db.SaveChangesAsync();
                    }


                    //if (!isAdmin)
                    //{
                    //    foreach (var item in response2.Data.UserEvents)
                    //    {
                    //        var detailing = Db.Events.Where(x => x.GalacticEventId == item.GalacticEventID).FirstOrDefault();
                    //        if (detailing != null)
                    //        {
                    //            AccessList.Add(detailing);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    AccessList = Db.AttendesEvents.Where(x => x.AttendesID == attende.ID).Select(x => x.Event).ToList();
                    //}

                    accessEvents.Token = response2.Data.Token;
                    accessEvents.Email = response2.Data.Email;

                    accessEvents.IsAuthenticated = response2.Data.IsAuthenticated;
                    accessEvents.Message = response2.Data.Message;
                    accessEvents.HasPassword = response2.Data.HasPassword;
                    accessEvents.IsValid = response2.Data.IsValid;
                    accessEvents.UserID = attende.ID;
                    accessEvents.UserFullName = $"{attende.FirstName} {attende.Lastname}";
                    accessEvents.isAdmin = attende.IsAdmin;
                    accessEvents.UserIcon = imageConvertionHandler(attende.Thumbnail);
                    accessEvents.UserEvents = CreateLoginEventDataEntity(AccessList, imageConvertionHandler, email).ToList();

                    return accessEvents;
                }
                else
                {
                    //if(attende == null)
                    //{
                    //    throw new System.Web.HttpException((int)HttpStatusCode.Unauthorized, "Not Authorized");
                    //}
                    //response2.Data.UserEvents = new List<APIUserEvents>();
                }
                foreach (var item in response2.Data.UserEvents)
                {
                    var detailing = Db.Events.Where(x => x.GalacticEventId == item.GalacticEventID).FirstOrDefault();
                    if (detailing != null)
                    {
                        AccessList.Add(detailing);
                    }
                }

                accessEvents.Token = response2.Data.Token;
                accessEvents.Email = response2.Data.Email;
                accessEvents.IsAuthenticated = response2.Data.IsAuthenticated;
                accessEvents.Message = response2.Data.Message;
                accessEvents.Password = password;
                accessEvents.HasPassword = response2.Data.HasPassword;
                accessEvents.IsValid = response2.Data.IsValid;
                accessEvents.UserEvents = CreateLoginEventDataEntity(AccessList, imageConvertionHandler, email).ToList();

                return accessEvents;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        /// <summary>
        /// Logs out existing users with same firebase token
        /// </summary>
        private void LogoutExistingUsersWithSameFBToken(string fbToken)
        {
            var activeUserSessionsWithSameToken = Db.UserSessions.Where(x => x.DeviceToken == fbToken && x.IsActive == true);

            foreach (var item in activeUserSessionsWithSameToken)
            {
                item.IsActive = false;
            }
            Db.SaveChanges();
        }

        private async Task<bool> IsAdmin(string email, string password)
        {
            return await Db.Attendes.AnyAsync(x => x.Email == email && x.IsAdmin && x.AdminInfo.Password == password);
        }

        private void AddAttendeeBookMarks(string email)
        {
            Attende attendee = Db.Attendes.FirstOrDefault(x => x.Email == email) ?? throw new NullReferenceException("Email is not found");
            foreach (var attendesEvent in attendee.AttendesEvents)
            {
                AddAttendeeBookMarksOfEvent(email, attendesEvent.EventID);
            }
        }

        public void AddAttendeeBookMarksOfEvent(string email, int eventID)
        {
            var GalEventID = Db.Events.Find(eventID).GalacticEventId.Value;
            GetPaxActivities attendeeActivities = getPaxActivities(GalEventID, email);
            var Activities = attendeeActivities.PaxActivities;
            if (Activities != null)
            {
                foreach (var Act in Activities)
                {
                    var attendeesid = Db.Attendes.FirstOrDefault(y => y.Email == email)?.ID;
                    if (!Db.BookMarks.Any(x => x.Activite.GalacticActivityId == Act.ActivityID && x.AttendesID == attendeesid))
                    {
                        var dbActivityID = Db.Activites.Where(x => x.EventID == eventID).FirstOrDefault(y => y.GalacticActivityId == Act.ActivityID)?.ID;
                        if (dbActivityID != null)
                        {
                            Db.BookMarks.Add(new BookMark
                            {
                                ActivityID = dbActivityID.Value,
                                AttendesID = Db.Attendes.FirstOrDefault(y => y.Email == email).ID
                            });
                        }
                    }
                }
            }
        }

        public IEnumerable<object> CreateLoginEventDataEntity(IEnumerable<Event> events, Func<string, string> imageConvertionHandler, string email)
        {

            return events.Select(e => new { e.ID, e.EventName, e.GalacticEventId });
            //var isoConvert = new IsoDateTimeConverter();
            //isoConvert.DateTimeFormat = _dateFormat;

            //return events.Select(s => new LoginEventData
            //{
            //    ID = s.ID,
            //    ModeratePost = s.ModeratePost,
            //    EventName = s.EventName,
            //    GalacticEventId = s.GalacticEventId,
            //    ManagerUserId = s.ManagerUserId,
            //    EnableThemes = s.EnableThemes,
            //    EnableGeneralAgenda = s.EnableGeneralAgenda,
            //    EnablePersonalAgenda = s.EnablePersonalAgenda,
            //    EnableQrCode = s.EnableQrCode,
            //    EnableAttendees = s.EnableAttendees,
            //    EnableFloormap = s.EnableFloormap,
            //    EnablePhotos = s.EnablePhotos,
            //    EnableSponsor = s.EnableSponsor,
            //    EnableComments = s.EnableComments,
            //    EnableGoogleApi = s.EnableGoogleApi,
            //    allow_bookmark = s.allow_bookmark,
            //    EventImage = s.EventImage,
            //    EnableSocialScreen = s.EnableSocialScreen,
            //    EnableSearchScreen = s.EnableSearchScreen,
            //    Destination = s.Destination,
            //    ClientName = s.ClientName,
            //    StartDate = s.StartDate,
            //    EndDate = s.EndDate,
            //    WebURL = s.WebURL,
            //    GeneralAgendaTitle = s.GeneralAgendaTitle,
            //    PersonalAgendaTitle = s.PersonalAgendaTitle,
            //    QrCodeTitle = s.QrCodeTitle,
            //    AttendeeTitle = s.AttendeeTitle,
            //    FloormapTitle = s.FloormapTitle,
            //    PhotosTitle = s.PhotosTitle,
            //    SponsorTitle = s.SponsorTitle,
            //    MapTitle = s.MapTitle,
            //    SocialTitle = s.SocialTitle,
            //    SearchTitle = s.SearchTitle,
            //    longitude = s.longitude,
            //    latitude = s.latitude,
            //    EnableSpeaker = s.EnableSpeaker,
            //    SpeakerTitle = s.SpeakerTitle,
            //    EnableExhibitor = s.EnableExhibitor,
            //    ExhibitorTitle = s.ExhibitorTitle,
            //    EnableVendor = s.EnableVendor,
            //    VendorTitle = s.VendorTitle,
            //    GoogleFolderID = s.GoogleFolderID,
            //    GoogleAlbumphotoUrl = s.GoogleAlbumphotoUrl,
            //    EnableChatscreen = s.EnableChatscreen,
            //    EnableMessaging = s.EnableMessaging,
            //    AlertEmails = s.AlertEmails,
            //    AttendeeMessagingEmails = s.AttendeeMessagingEmails,
            //    OpenUrlInsideorOutsideApp = s.OpenUrlInsideorOutsideApp,
            //    DisableFeaturesNotinUse = s.DisableFeaturesNotinUse,
            //    AllowTaggingPeopleandLocations = s.AllowTaggingPeopleandLocations,
            //    AllowPhotosBulkDownload = s.AllowPhotosBulkDownload,
            //    OpenLinksExternallyorInternally = s.OpenLinksExternallyorInternally,
            //    DisplayRating = s.DisplayRating,
            //    SurveyResponseType = s.SurveyResponseType,
            //    DisplaySurveyActivity = s.DisplaySurveyActivity,
            //    AutoPushNotLocationChange = s.AutoPushNotLocationChange,
            //    AutoPushNotMinReminder = s.AutoPushNotMinReminder,
            //    AutoPushNotTimeChange = s.AutoPushNotTimeChange,
            //    coordinates = s.coordinates,
            //    EmailNotesusingAccountEmail = s.EmailNotesusingAccountEmail,
            //    EmailFavouriteDocumentsusingAccountEmail = s.EmailFavouriteDocumentsusingAccountEmail,
            //    ShowAttendeewithSpeakers = s.ShowAttendeewithSpeakers,
            //    OpenDocumentsWithinorOutsideURL = s.OpenDocumentsWithinorOutsideURL,
            //    EnablePhotoDescription = s.EnablePhotoDescription,
            //    Themes = s.Themes.Select(n => new EventTheme
            //    {
            //        AppBackgroundURL = n.AppBackgroundURL,
            //        ButtonURL = n.ButtonURL,
            //        SplashScreemURL = n.SplashScreemURL,
            //        ButtonBackgroundColor = n.ButtonBackgroundColor,
            //        ButtonForegroundColor = n.ButtonForegroundColor,
            //        HeadingForegroundColor = n.HeadingForegroundColor,
            //        LabelForegroundColor = n.LabelForegroundColor
            //    }),
            //    HomePageConfig = s.WidgetMappings.OrderBy(x => x.YLocation).ThenBy(x => x.XLocation).Select((x, idx) => new
            //    {
            //        x.DisplayName,
            //        x.ExternalLink,
            //        x.isExternalLink,
            //        ImagePath = x.ImagePath!=null? imageConvertionHandler(x.ImagePath): null,
            //        x.ID,
            //        x.Height,
            //        x.Width,
            //        PageType = x.Widget?.Name,
            //        Index = $"{x.YLocation},{x.XLocation}",
            //        IsImageWidget = x.WidgetType.ID == 1
            //    }).ToList(),
            //    UpcomingAgenda = s.AttendesEvents.Where(y => y.Attende.Email.ToLower() == email.ToLower())
            //                        .SelectMany(y => y.Attende.BookMarks.Select(z => z.Activite))
            //                        .OrderBy(y => y.StartTime)
            //                        .Where(y => y.StartTime > DateTime.Now)
            //                        .Take(3)
            //                        .Select(y=> new {
            //                            y.StartTime,
            //                            y.EndTime,
            //                            y.Name,
            //                            y.FloorRegionMapping?.FloorMapping?.FloorName
            //                        }),
            //    Sponsors = s.SponsorsEvents.Select(y=>y.Sponsor).OrderBy(y=> y.Packagetype).ThenBy(y=>y.Name).Select(y=> new {
            //        y.ID,
            //        y.Name,
            //        Thumbnail = imageConvertionHandler(y.Thumbnail),
            //        y.Description,
            //       PackageName = y.UserPackageType.Name
            //    })
            //});
        }

        public bool logout(string authtoken)
        {
            if (authtoken != null && authtoken != "" && IsTokenExists(authtoken))
            {
                var check = Db.UserSessions.Where(x => x.AuthToken == authtoken).FirstOrDefault();
                check.IsActive = false;
                Db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool IsTokenExists(string authtoken)
        {
            return Db.UserSessions.Any(x => x.AuthToken == authtoken);
        }

        public bool IsAuthenticated(string Token)
        {
            var userID = Authenticate.ValidateToken(Token);
            if (userID == null)
            {
                return false;
            }
            return true;
        }

        public UserSession CheckSession(string Token)
        {
            return Db.UserSessions.FirstOrDefault(x => x.AuthToken == Token);
        }
        
        public object ForgotPassowrd(string email)
        {
            //var user =Db.Attendes.FirstOrDefault(x => x.Email == email);
            //if (user == null)
            //    throw new APIException("user does not exist.");
            //else
            //forgot password logic here
            return null;
        }

        public string GenerateCode(string email)
        {
            //Update
            string message = string.Empty;

            var objUser = Db.Attendes.Where(x => x.Email == email).FirstOrDefault();
            if (objUser != null)
            {
                Random rnd = new Random();
                objUser.code = rnd.Next(1000, 9999);
                objUser.codeDate = DateTime.Now;

                Db.SaveChangesAsync();
                message = "success";
                using (var EmailSender = new EmailSender(objUser?.Email))
                {
                    bool IsEmailSent = EmailSender.SendResetPasswordEmail(objUser.code.Value);
                    if (!IsEmailSent)
                    {
                        message = "Problem in sending auto-generated code";
                    }
                }
                // SendEmail(email, "Password Reset Code", "Your code to reset the password is : " + objUser.code);
            }
            else
            {
                message = "Email Address Not Found";
            }
            return message + "|" + objUser.code + "|" + objUser.codeDate;
        }
        
        public bool SendEmail(string to, string subject, string message)
        {
            try
            {

                //var fromAddress = new MailAddress("galmobdev@gmail.com", "From Name");
                //var toAddress = new MailAddress(to, "To Name");
                //const string fromPassword = "2018NBeyond";
                // string subjects = subject;
                // string body = message;

                //var smtp = new SmtpClient
                //{
                //    Host = "smtp.gmail.com",
                //    Port = 587,
                //    EnableSsl = true,
                //    DeliveryMethod = SmtpDeliveryMethod.Network,
                //    UseDefaultCredentials = false,
                //    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                //};
                //using (var messages = new MailMessage(fromAddress, toAddress)
                //{
                //    Subject = subjects,
                //    Body = body
                //})
                //{
                //    smtp.Send(messages);
                //}
                string smtp = "smtp.gmail.com";
                int port = 587;
                string from = "galmobdev@gmail.com";
                string password = "2018NBeyond";
                string DisplayName = "Galactic Event Managment";
                bool IsHTML = false;
                bool IsSSLEnable = true;
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(smtp);
                mail.From = new MailAddress(from, DisplayName);
                mail.To.Add(to);
                mail.Subject = subject;
                mail.IsBodyHtml = IsHTML;
                mail.Body = message;

                //Attachment attachment = new Attachment(file);
                //mail.Attachments.Add(attachment);

                SmtpServer.Port = port;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential(from, password);

                SmtpServer.EnableSsl = IsSSLEnable;

                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        #endregion
    }
}
