using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventManagement.Base;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Models;
using EventManagement.DataAccess.DataBase.Model;
using Microsoft.AspNet.SignalR;

namespace EventManagement.Areas.API.Controllers
{
    public class FireChatController : BaseAPIController
    {
        Firechatlogic flc = new Firechatlogic();

        Entities db = new Entities();
        EventLogic eventLogic = new EventLogic();

        public ActionResult GetOrCreateConversation(string Token, int eventID, bool isAdmin, int ReceiverID)
        {
            try
            {
                List<int> receiverList = new List<int>();
                int senderId = CurrentUserSession.AttendesID.Value;

                if (ReceiverID == 0)
                {
                    receiverList = flc.GetAdminId(eventID, senderId);
                }
                else
                {
                    receiverList = new List<int>() { ReceiverID };
                }

                List<UserChannelMessages> channelMessages = new List<UserChannelMessages>();
                int i = 0;

                bool isMobileAdmin = IsAdmin;

                //if (!isAdmin)
                //{
                //    if (!isMobileAdmin)
                //    {
                //        channelMessages = flc.FetchMessagesByChannelID(senderId, new List<int> { ReceiverID }, eventID);

                //        flc.FetchMessagesByChannelID(senderId, new List<int>{ ReceiverID}, eventID);
                //    }
                //    else
                //    {
                //        channelMessages = flc.startAdminToAttendeeConversation(ReceiverID, eventID);
                //    }
                //}
                //else
                //{
                //    channelMessages = flc.FetchMessagesByChannelID(senderId, new List<int> { ReceiverID }, eventID);
                //}

                if (!receiverList.Any())
                {
                    throw new Exception("No recievers found");
                }

                int channelId = flc.GetOrCreateChannel(senderId, receiverList, eventID);

                channelMessages = flc.FetchingChannelMessages(channelId, senderId, receiverList, eventID);

                foreach (var item in channelMessages)
                {
                    item.MessageIndex = i;
                    item.AttendeImage = imageToUrlConvertionHandler(item.AttendeImage);
                    i = i + 1;
                }

                return Json(channelMessages, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception("Chat instance creation failed");
            }
        }

        public ActionResult GetUnreadMessagesCount(string Token, int eventId)
        {
            int messagesCount = flc.GetUnReadMessageCountForEvent(CurrentUserSession.AttendesID.Value, eventId);
            return Json(messagesCount, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAttendeeImagesbyChannelId(int ChannelId)
        {
            List<AttendeeChannelImages> channelImages = flc.GetAttendeeImagesbyChannelId(ChannelId);
            foreach (var image in channelImages)
            {
                image.AttendeeImage = imageToUrlConvertionHandler(image.AttendeeImage);
            }
            return Json(channelImages, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FetchMessagesByChannelID(int SenderId, List<int> RecieverID, int EventId)
        {
            int i = 0;
            List<UserChannelMessages> channelMessages = flc.FetchMessagesByChannelID(SenderId, RecieverID, EventId);

            foreach (var item in channelMessages)
            {
                item.MessageIndex = i;
                item.AttendeImage = imageToUrlConvertionHandler(item.AttendeImage);
                i = i + 1;
                if (item.type == "Image")
                {
                    item.MessageText = imageToUrlConvertionHandler(item.MessageText);
                }
            }
            return Json(channelMessages, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRecentChatConversation(int userId, int eventId)
        {
            List<RecentMessage> recentMessages = flc.GetRecentChatConversation(userId, eventId);

            foreach (RecentMessage item in recentMessages)
            {
                item.Photo = imageToUrlConvertionHandler(item.Photo);
                if (item.type == "Image")
                {
                    item.TextMessage = imageToUrlConvertionHandler(item.TextMessage);
                }
            }

            return Json(recentMessages, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetUnreadCountToZero(int senderId, int channelId, int eventId)
        {
            flc.SetUnreadCountToZero(senderId, channelId, eventId);
            int totalUnreadCount = flc.GetUnReadMessageCountForEvent(senderId, eventId);
            return Json(new { ChannelId = channelId, IsSuccess = true, TotalUnreadCount = totalUnreadCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SendMessage(int senderId, string message, int channelId, int eventId, HttpPostedFileBase image)
        {
            string imageName = "";
            string imageURL = "";
            if (image != null)
            {
                imageName = eventLogic.createGUIDFileNameFromFile(image);
                imageURL = imageToUrlConvertionHandler(imageName);
            }

            Message msg = flc.sendMessage(senderId, message, channelId, eventId, image, imageName, ImageSavePath, imageURL);
            if (msg.status)
            {
                List<string> webNotifcationTokens = flc.GetWebNotificationTokens(channelId, senderId);

                NotificationHub notificationHub = new NotificationHub();

                foreach (string webNotificationToken in webNotifcationTokens)
                {
                    var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    context.Clients.Client(webNotificationToken).addChatMessageToPage(message);
                    //notificationHub.SendwebNotification(webNotificationToken, message);
                }
            }

            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LeaveGroup(string token, int channelId)
        {
            int userId = CurrentUserSession.Attende.ID;
            bool status = flc.LeaveGroup(userId, channelId);

            return Json(new { ChannelId = channelId, IsSuccess = status }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateGroup(string token, int eventId, List<int> receiverIds, string groupName)
        {
            int senderId = CurrentUserSession.Attende.ID;
            //receiverIds = new List<int> { 1, 220 };
            List<UserChannelMessages> channelMessages = new List<UserChannelMessages>();
            int i = 0;

            if (receiverIds == null || !receiverIds.Any())
            {
                throw new Exception($"No recievers Ids received for senderid: {senderId} and EventID: {eventId}");
            }

            channelMessages = flc.CreateGroup(eventId, senderId, receiverIds, groupName, ImageSavePath);

            foreach (var item in channelMessages)
            {
                item.MessageIndex = i;
                item.AttendeImage = imageToUrlConvertionHandler(item.AttendeImage);
                i = i + 1;
            }

            return Json(channelMessages, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAttendeGroupsForEvent(string token, int eventId)
        {
            int userId = CurrentUserSession.Attende.ID;
            List<UserGroupInformation> userGroups = flc.GetAttendeGroupsForEvent(userId, eventId);

            foreach (UserGroupInformation userGroup in userGroups)
            {
                userGroup.ChannelImage = string.IsNullOrEmpty(userGroup.ChannelImage) ? null : imageToUrlConvertionHandler(userGroup.ChannelImage);
            }

            return Json(userGroups, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetChannelMessagesByChannelId(string token, int channelId, int eventId)
        {
            List<UserChannelMessages> channelMessages = new List<UserChannelMessages>();
            int i = 0;

            int userId = CurrentUserSession.Attende.ID;

            List<int> receivers = flc.GetChannelRecievers(channelId, userId);
            channelMessages = flc.FetchingChannelMessages(channelId, userId, receivers, eventId);

            foreach (var item in channelMessages)
            {
                item.MessageIndex = i;
                item.AttendeImage = string.IsNullOrEmpty(item.AttendeImage) ? null : imageToUrlConvertionHandler(item.AttendeImage);
                i = i + 1;
                if (item.type == "Image")
                {
                    item.MessageText = imageToUrlConvertionHandler(item.MessageText);
                }
            }

            return Json(channelMessages, JsonRequestBehavior.AllowGet);
        }
    }
}