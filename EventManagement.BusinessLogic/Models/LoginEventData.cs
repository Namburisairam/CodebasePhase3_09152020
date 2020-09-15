using System;
using System.Collections.Generic;

namespace EventManagement.BusinessLogic.Models
{
    public class LoginEventData: HomePageData
    {
        public int ID { get; set; }
        
        public bool? ModeratePost { get; set; }
        
        public string EventName { get; set; }        
        
        public int? GalacticEventId { get; set; }
        
        public int? ManagerUserId { get; set; }
        
        public bool EnableThemes { get; set; }
        
        public bool? EnableGeneralAgenda { get; set; }
        
        public bool? EnablePersonalAgenda { get; set; }
        
        public bool EnableQrCode { get; set; }
        
        public bool EnableAttendees { get; set; }
        
        public bool EnableFloormap { get; set; }
        
        public bool EnablePhotos { get; set; }
        
        public bool EnableSponsor { get; set; }
        
        public bool EnableComments { get; set; }
        
        public bool EnableGoogleApi { get; set; }
        
        public bool allow_bookmark { get; set; }
        
        public object EventImage { get; set; }
        
        public object LeftMenuLogo { get; set; }
        
        public bool EnableSocialScreen { get; set; }
        
        public bool EnableSearchScreen { get; set; }
        
        public string Destination { get; set; }
        
        public string ClientName { get; set; }
        
        public string WebURL { get; set; }
        
        public string GeneralAgendaTitle { get; set; }
        
        public string PersonalAgendaTitle { get; set; }
        
        public string QrCodeTitle { get; set; }
        
        public string AttendeeTitle { get; set; }
        
        public string FloormapTitle { get; set; }
        
        public string PhotosTitle { get; set; }
        
        public string SponsorTitle { get; set; }
        
        public string MapTitle { get; set; }
        
        public string SocialTitle { get; set; }
        
        public string SearchTitle { get; set; }
        
        public double? longitude { get; set; }
        
        public double? latitude { get; set; }
        
        public bool EnableSpeaker { get; set; }
        
        public string SpeakerTitle { get; set; }
        
        public bool EnableExhibitor { get; set; }
        
        public string ExhibitorTitle { get; set; }
        
        public bool EnableVendor { get; set; }
        
        public string VendorTitle { get; set; }
        
        public string GoogleFolderID { get; set; }
        
        public string GoogleAlbumphotoUrl { get; set; }
        public bool EnableChatscreen { get; set; }
        
        //public bool? AlertEmails { get; set; }
        
        //public bool? AttendeeMessagingEmails { get; set; }
        
        public bool? OpenUrlInsideorOutsideApp { get; set; }
        
        public bool? DisableFeaturesNotinUse { get; set; }
        
        public bool? AllowTaggingPeopleandLocations { get; set; }
        
        //public bool? AllowPhotosBulkDownload { get; set; }
        
        //public bool? OpenLinksExternallyorInternally { get; set; }
        
        public bool? AutoPushNotLocationChange { get; set; }
        
        public bool? AutoPushNotMinReminder { get; set; }
        
        public bool? AutoPushNotTimeChange { get; set; }
        
        public bool? EmailNotesusingAccountEmail { get; set; }
        
        public bool? EmailFavouriteDocumentsusingAccountEmail { get; set; }
        
        public bool? ShowAttendeewithSpeakers { get; set; }
        
        public bool? OpenDocumentsWithinorOutsideURL { get; set; }
        
        public bool HideAddPhotos { get; set; }
        
        public bool EnablePhotoDescription { get; set; }
        
        public IEnumerable<EventTheme> Themes { get; set; }
        
        public bool EnableSurveys { get; internal set; }
        
        public string SurveyTitle { get; internal set; }
        
        public bool EnableDocuments { get; internal set; }
        
        public string DocumentsTitle { get; internal set; }
        
        public bool EnableEventInfo { get; internal set; }
        
        public string EventInfoTitle { get; internal set; }
        
        public bool EnableAnnouncements { get; internal set; }
        
        public string AnnouncementsTitle { get; internal set; }
        public int UnreadMessageCount { get; set; }
    }


    public class HomePageData
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public object Coordinates { get; set; }
        public object HomePageConfig { get; set; }

        public object UpcomingAgenda { get; set; }

        public object Sponsors { get; set; }
        public object RecentDiscussions { get; set; }

        public int UnreadNotificationsCount { get; set; }

        public object Attendees { get; set; }
        public object Speakers { get; set; }
        public object Vendors { get; set; }
        public object Surveys { get; set; }
        public object eventVideos { get; set; }
        public object SocialFeed { get; set; }
        public object VendorBannerHomepage { get; set; }

        public object RecentSocialPosts { get; set; }
    }
}