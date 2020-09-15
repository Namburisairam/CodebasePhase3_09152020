using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Classes;
using EventManagement.BusinessLogic.Models;
using EventManagement.DataAccess.DataBase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    public class FireChatController : BaseController
    {
        Firechatlogic firechatlogic = new Firechatlogic();
        // GET: FireChat
        public async Task<ActionResult> Index()
        {
            ViewBag.events = await firechatlogic.GetEvents();
            return View();
        }

        public async Task<ActionResult> GetEventAttendees(int EventId)
        {
            List<EventAttendees> eventAttendees = await firechatlogic.GetEventAttendees(EventId);
            return Json(eventAttendees, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetChannelMessages(int? ReceiverID, int EventID, int? ChannelId)
        {
            List<CMSChannelMessages> channelMessages = firechatlogic.GetChannelMessages(CurrentUserSession.Attende.ID, ReceiverID, EventID, ChannelId);
            return Json(channelMessages, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendWebPushnotification(string Token, string Message, int ChannelId)
        {
            List<string> receiverTokens = new List<string>();
            receiverTokens.Add(Token);
            ChatLogic cl = new ChatLogic();
            cl.SendNotification(receiverTokens, Message, "Push Notification", "Push Notification", ChannelId, null, null, null, null);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCurrentSessionUserId()
        {
            int currentUserId = CurrentUserSession.AttendesID.Value;
            return Json(currentUserId, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetChannelImages(int channelID)
        {
            List<ChannelAttendeeImages> channelAttendeeImages = firechatlogic.GetChannelImages(channelID);
            return Json(channelAttendeeImages, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAdminChannels()
        {
            List<ChatAdminChannel> channelMessages = firechatlogic.GetAdminChannels(CurrentUserSession.Attende.ID);
            return Json(channelMessages, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMessagesByChannelId(int channelId)
        {
            List<CMSChannelMessages> channelMessages = firechatlogic.GetMessagesByChannelId(channelId);

            int userId = CurrentUserSession.Attende.ID;
            if (channelMessages.Count == 0)
            {
                channelMessages.Add(firechatlogic.GetEmptyChannelMessages(channelId, userId));
            }

            //List<int> receiversList = firechatlogic.GetChannelRecievers(channelId, userId);

            int eventId = firechatlogic.GetEventID(channelId);

            firechatlogic.SetUnreadCountToZero(userId, channelId, eventId);

            return Json(channelMessages, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateNewWebConversation(int ReceiverID, int EventID)
        {
            try
            {
                var channelID = firechatlogic.startAdminToAttendeeWebConversation(CurrentUserSession.AttendesID.Value, ReceiverID, EventID);
                return Json(new { channelID }, JsonRequestBehavior.AllowGet);
            }
            catch (AdminNotFoundException ex)
            {
                Response.StatusCode = 500;
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetOrCreateChannel(int FromuserId, List<int> TouserId, int eventId)
        {
            try
            {
                var channelId = firechatlogic.GetOrCreateChannel(FromuserId, TouserId, eventId);
                return Json(channelId, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
                throw ex;
            }
        }
        [AllowAnonymous]
        public ActionResult SendMessage(string chatMessage, int channelId, HttpPostedFileBase uploadedImage)
        {
            EventLogic eventLogic = new EventLogic();
            string imageName = "";
            string imageURL = "";
            int senderId = CurrentUserSession.Attende.ID;
            if (uploadedImage != null)
            {
                imageName = eventLogic.createGUIDFileNameFromFile(uploadedImage);
                imageURL = imageToUrlConvertionHandler(imageName);
            }
            bool status = firechatlogic.SendMessage(senderId, chatMessage, channelId, uploadedImage, imageName, ImageSavePath, imageURL);
            return Json(status, JsonRequestBehavior.AllowGet);
        }
    }
}