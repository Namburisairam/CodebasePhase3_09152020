using CommonHelpers;
using EventManagement.Base;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Classes;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EventManagement.Areas.API.Controllers
{
    [AuthFilter]
    public class ChatController : BaseAPIController
    {
        ChatLogic cl = new ChatLogic();
        Entities db = new Entities();
        EventLogic eventLogic = new EventLogic();
        /// <summary>
        /// Generate token based on email and device
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="device"></param>
        /// <returns>Returns token data</returns>
        // GET: API/Chat
        public ActionResult Token(string Token, string device)
        {
            var identity = db.Attendes.Find(CurrentUserSession.AttendesID.Value).Email;
            var token = cl.GetChatToken(device, identity);
            return Json(new { Token = token }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Shows existing conversations if exists else creates a new conversation
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventID"></param>
        /// <param name="isAdmin"></param>
        /// <param name="ReceiverID"></param>
        /// <returns>Returns channelID</returns>

        public async Task<ActionResult> GetOrCreateConversation(string Token, int eventID, bool isAdmin, int? ReceiverID = null)
        {
            try
            {
                string channelID;
                // isAdmin is to check whether the request is for chat with staff
                bool isMobileAdmin = IsAdmin;
                if (!isAdmin)
                {
                    if (!isMobileAdmin)
                    {
                        channelID = cl.startOneToOneConversation(CurrentUserSession.AttendesID.Value, ReceiverID.Value, eventID);
                    }
                    else
                    {
                        channelID = await cl.startAdminToAttendeeConversation(ReceiverID.Value, eventID);
                    }
                }
                else
                {
                    channelID = await cl.startAdminToAttendeeConversation(CurrentUserSession.AttendesID.Value, eventID);
                }
                return Json(channelID, JsonRequestBehavior.AllowGet);
            }
            catch (AdminNotFoundException ex)
            {
                Response.StatusCode = 500;
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception("Chat instance creation failed");
            }
        }
        /// <summary>
        /// sends message to reciever based on channelID 
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="receiverID"></param>
        /// <param name="channelID"></param>
        /// <param name="isAdminGroup"></param>
        /// <param name="chatMessage"></param>
        /// <returns>Status whether message is sent or not</returns>
        public async Task<ActionResult> SendMessage(string Token, int? receiverID, string channelID, bool isAdminGroup, string chatMessage, int eventID)
        {
            try
            {
                if (!IsAdmin)
                {
                    await cl.sendAttendeeMessage(chatMessage, channelID, CurrentUserSession.AttendesID.Value, isAdminGroup, compressedImageConvertionHandler, eventID, receiverID);
                }
                else
                {
                    // When Admin logs-in as Attendee in Mobile, this method will be called
                    await cl.sendMessageFromAdminToGroup(chatMessage, channelID, CurrentUserSession.AttendesID.Value, compressedImageConvertionHandler, eventID);
                }
                await eventLogic.SaveLogs(CurrentUserSession.AttendesID.Value, "Chat", "Chat", "Attendee conversation started Successfully");
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Helpers.LogError(ex.Message, ex);
                throw;
            }
        }

        public ActionResult GetChannelAttendeeImages(string Token, string channelID)
        {            
            var channel = db.ChatChannels.FirstOrDefault(x => string.Equals(x.ChannelID, channelID, StringComparison.CurrentCultureIgnoreCase));
            if (channel == null)
            {
                Response.StatusCode = 500;
                return Json("Channel not found");
            }
            var channelAttendeeImages = channel.ChannelUsers.Select(x => new
            {
                x.AttendeeID,
                x.Attende?.FirstName,
                x.Attende?.Lastname,
                image = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.APiImagePath(x.Attende?.Thumbnail))
            });
            return Json(channelAttendeeImages, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAttendeeImages(string Token, int eventID)
        {
            var attendeeImages = db.Attendes.Where(x => x.AttendesEvents.Any(y => y.EventID == eventID)).ToList().Select(
                x => new
                {
                    x.ID,
                    x.FirstName,
                    x.Lastname,
                    Image = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.APiImagePath(x.Thumbnail))
                }
                );

            return Json(attendeeImages, JsonRequestBehavior.AllowGet);
        }
    }
}