using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Classes;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class ChatController : BaseController
    {
        ChatLogic chatLogic = new ChatLogic();
        Entities db = new Entities();
        // GET: Chat
        public async Task<ActionResult> Index()
        {
            var eventLogic = new EventLogic();
            ViewBag.events = (await eventLogic.GetAllEvents()).Where(x => x.AttendesEvents.Any(y => y.AttendesID == CurrentUserSession.AttendesID.Value)).ToList();
            return View();
        }

        /// <summary>
        /// Get token and attendee info for given device
        /// </summary>
        /// <param name="device"></param>
        /// <returns> returns token, identity and attendee info</returns>
        public ActionResult Token(string device)
        {
            var identity = CurrentUserSession.Attende.Email;
            var token = chatLogic.GetChatToken(device, identity);
            return Json(new { identity, token, id = CurrentUserSession.Attende.ID }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Start new converstaion in specific event
        /// </summary>
        /// <param name="ReceiverID"></param>
        /// <param name="EventID"></param>
        /// <returns>Returns channelID</returns>
        public async Task<ActionResult> CreateNewConversation(int ReceiverID, int EventID)
        {
            try
            {
                var channelID = await chatLogic.startAdminToAttendeeConversation(ReceiverID, EventID);
                return Json(new { channelID }, JsonRequestBehavior.AllowGet);
            }
            catch (AdminNotFoundException ex)
            {
                Response.StatusCode = 500;
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Send message though given channel
        /// </summary>
        /// <param name="chatMessage"></param>
        /// <param name="channelID"></param>
        /// <param name="adminID"></param>
        /// <returns>returns status of sent message</returns>
        public async Task<bool> SendMessage1(string chatMessage, string channelID, int adminID, int eventID)
        {
            try
            {
                await chatLogic.sendMessageFromAdminToGroup(chatMessage, channelID, adminID, compressedImageConvertionHandler, eventID);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Get attendees info for specific event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public ActionResult GetAttendees(int eventID)
        {
            var eventLogic = new EventLogic();
            return Json(eventLogic.GetEeventAttendes(eventID).
                     Where(x => !x.IsAdmin && (x.EnableMessaging == true)).
                     Select(x => new { AttendeeID = x.ID, x.FirstName, x.Lastname }), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Admin channels which are mapped to given event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public ActionResult GetAdminChannels(int eventID)
        {
            var adminChannels = chatLogic.GetAdminChannelsByEventID(eventID);
            var currentAdminChannel = adminChannels;
            return Json(
                currentAdminChannel.Select(
                    x =>
                new
                {
                    x.ChannelID,
                    TwilioChannelID = x.ChatChannel.ChannelID,
                    ReceiverDisplayText = db.ChatChannels.Find(x.ChannelID).ChannelUsers.FirstOrDefault(y => y.AttendeeID != null).Attende.FirstName
                }), JsonRequestBehavior.AllowGet
            );
        }

        // Duplicate code need to change
        public async Task<ActionResult> GetAttendeeProfilePic(int attendeeID)
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            var attendee = await attendesBusiness.GetAttendesById(attendeeID);
            return Json(compressedImageConvertionHandler(attendee.Thumbnail), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetChannelAttendeeImages(string channelID)
        {
            var channel = db.ChatChannels.FirstOrDefault(x => string.Equals(x.ChannelID, channelID, StringComparison.CurrentCultureIgnoreCase));
            if (channel == null)
            {
                Response.StatusCode = 500;
                return Json("Channel not found", JsonRequestBehavior.AllowGet);
            }
            var channelAttendeeImages = channel.ChannelUsers.Select(x => new
            {
                x.AttendeeID,
                x.Attende?.FirstName,
                x.Attende?.Lastname,
                image = compressedImageConvertionHandler(x.Attende?.Thumbnail)
            });
            var isChatDisabled = !channel.ChannelUsers.FirstOrDefault(x => !x.Attende.IsAdmin)?.Attende.EnableMessaging;
            return Json(new { channelAttendeeImages, isChatDisabled }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetAttendeesDetails(int[] attendeeIDs)
        {
            var attendeeDetails = db.Attendes.AsNoTracking().Where(x => attendeeIDs.Contains(x.ID)).Select(x => new
            {
                x.ID,
                x.FirstName,
                x.Lastname,
                x.EnableMessaging
            }).ToList();
            return Json(attendeeDetails);
        }

    }
}