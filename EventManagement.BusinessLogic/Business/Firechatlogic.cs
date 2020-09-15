using CommonHelpers;
using EventManagement.BusinessLogic.Business.Email_Notifier;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.BusinessLogic.Classes;
using EventManagement.BusinessLogic.Models;
using EventManagement.DataAccess.DataBase.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static EventManagement.BusinessLogic.Business.EventLogic;
using System.Data.Entity;
using System.Drawing.Design;
using RestSharp.Extensions;
using System.Web;

namespace EventManagement.BusinessLogic.Business
{
    public class Firechatlogic : BaseLogic
    {
        DateTime UTCDateTime = DateTime.Now.ToUniversalTime();
        public List<int> GetAdminId(int eventId, int senderId)
        {
            List<int> allAdminsOfEvent = GetAllAdminsOfEvent(eventId);

            return allAdminsOfEvent;

            //List<int> channelList = Db.UserChannels.Where(s => s.Eventid == eventId && s.userid == senderId && allAdminsOfEvent.Contains(s.touserid)).Select(x => x.channelid)
            //    .ToList();

            //if (channelList.Count == 0)
            //{
            //    return allAdminsOfEvent.FirstOrDefault();
            //}

            //int channelid = Db.ChannelMessages.Where(s => channelList.Contains(s.channelid)).ToList().OrderByDescending(x => x.sentdate).FirstOrDefault().channelid;

            //int adminUser = Db.UserChannels.FirstOrDefault(s => s.channelid == channelid && s.userid == senderId)
            //    .touserid;

            //return adminUser;
        }


        public List<ChatAttendee> GetAttendes()
        {
            List<ChatAttendee> attendes = Db.Attendes.Select(x => new ChatAttendee { ID = x.ID, Name = x.FirstName }).ToList();
            return attendes;
        }

        public async Task<List<EventData>> GetEvents()
        {
            return await Db.Events.Select(x => new EventData { EventId = x.ID, EventName = x.EventName }).ToListAsync();
        }

        public async Task<List<EventAttendees>> GetEventAttendees(int EventId)
        {
            return await Db.AttendesEvents.Where(x => x.EventID == EventId && x.Attende.IsAdmin == false).Select(y => new EventAttendees { AttendeeId = y.AttendesID, AttendeeName = y.Attende.FirstName }).ToListAsync();
        }

        public List<CMSChannelMessages> GetChannelMessages(int senderId, int? ReceiverID, int EventID, int? channelid)
        {
            int? ChannelId;
            if (ReceiverID != null)
            {
                ChannelId = Db.UserChatChannels.FirstOrDefault(x => x.userid == senderId && x.touserid == ReceiverID && x.Eventid == EventID)?.channelid;

                if (ChannelId == null)
                {
                    ChannelId = GetOrCreateChannel(senderId, new List<int>() { ReceiverID.Value }, EventID);
                }
                //else
                //{
                //    ChannelId = Db.UserChatChannels.FirstOrDefault(x => x.userid == senderId && x.touserid == ReceiverID && x.Eventid == EventID).channelid;
                //}

                List<CMSChannelMessages> msgs = Db.channelmessages.
                                    Where(x => x.channelid == ChannelId).
                                    Select(y => new CMSChannelMessages
                                    {
                                        channelid = ChannelId ?? 0,
                                        userid = y.userid,
                                        sendername = Db.Attendes.FirstOrDefault(a => a.ID == senderId).FirstName,
                                        message = y.message,
                                        sentdate = y.sentdate,
                                        type = y.type
                                    }).ToList();

                if (msgs.Count == 0)
                {
                    DateTime date = DateTime.Now.ToUniversalTime();
                    msgs.Add(new CMSChannelMessages
                    {
                        channelid = ChannelId ?? 0,
                        userid = senderId,
                        sendername = Db.Attendes.FirstOrDefault(a => a.ID == senderId).FirstName,
                        message = string.Empty,
                        sentdate = date,
                        type = string.Empty
                    });
                }

                return msgs;
            }
            else
            {
                ChannelId = channelid ?? 0;
                List<CMSChannelMessages> msgs = Db.channelmessages.
                                    Where(x => x.channelid == ChannelId).
                                    Select(y => new CMSChannelMessages
                                    {
                                        channelid = ChannelId ?? 0,
                                        userid = y.userid,
                                        sendername = Db.Attendes.FirstOrDefault(a => a.ID == senderId).FirstName,
                                        message = y.message,
                                        sentdate = y.sentdate
                                    }).ToList();

                if (msgs.Count == 0)
                {
                    DateTime date = DateTime.Now.ToUniversalTime();
                    msgs.Add(new CMSChannelMessages
                    {
                        channelid = ChannelId ?? 0,
                        userid = senderId,
                        sendername = Db.Attendes.FirstOrDefault(a => a.ID == senderId).FirstName,
                        message = string.Empty,
                        sentdate = date,
                    });
                }
                return msgs;
            }
        }

        //public List<CMSChannelMessages> GetChannelMessages(int senderId, int? ReceiverID, int EventID, int? channelid)
        //{
        //    int ChannelId;
        //    if (ReceiverID != null)
        //    {
        //        ChannelId = Db.UserChatChannels.FirstOrDefault(x => x.userid == senderId && x.touserid == ReceiverID && x.Eventid == EventID).channelid;
        //        List<CMSChannelMessages> msgs = Db.ChannelMessages.
        //                            Where(x => x.channelid == ChannelId).
        //                            Select(y => new CMSChannelMessages
        //                            {
        //                                channelid = ChannelId,
        //                                userid = y.userid,
        //                                sendername = Db.Attendes.FirstOrDefault(a => a.ID == senderId).FirstName,
        //                                message = y.message,
        //                                sentdate = y.sentdate
        //                            }).ToList();
        //        return msgs;
        //    }
        //    else
        //    {
        //        ChannelId = channelid ?? 0;
        //        List<CMSChannelMessages> msgs = Db.ChannelMessages.
        //                            Where(x => x.channelid == ChannelId).
        //                            Select(y => new CMSChannelMessages
        //                            {
        //                                channelid = ChannelId,
        //                                userid = y.userid,
        //                                sendername = Db.Attendes.FirstOrDefault(a => a.ID == senderId).FirstName,
        //                                message = y.message,
        //                                sentdate = y.sentdate
        //                            }).ToList();
        //        return msgs;
        //    }
        //}

        public List<CMSChannelMessages> GetMessagesByChannelId(int channelId)
        {
            List<CMSChannelMessages> msgs = Db.channelmessages.
                Where(x => x.channelid == channelId).
                Select(y => new CMSChannelMessages
                {
                    userid = y.userid,
                    channelid = channelId,
                    sendername = Db.Attendes.FirstOrDefault(a => a.ID == y.userid).FirstName,
                    message = y.message,
                    sentdate = y.sentdate,
                    type = y.type
                }).ToList();
            return msgs;
        }

        public CMSChannelMessages GetEmptyChannelMessages(int channelId, int userId)
        {
            DateTime date = DateTime.Now.ToUniversalTime();
            return new CMSChannelMessages
            {
                userid = userId,
                channelid = channelId,
                sendername = Db.Attendes.FirstOrDefault(a => a.ID == userId).FirstName,
                message = string.Empty,
                sentdate = date
            };
        }


        public List<ChannelAttendeeImages> GetChannelImages(int ChannelId)
        {
            List<ChannelAttendeeImages> channelAttendeeImages = Db.ChannelUsers.
                                                                Where(x => x.ChannelID == ChannelId)
                                                                .Select(y => new ChannelAttendeeImages
                                                                {
                                                                    AttendeeId = y.AttendeeID,
                                                                    AttendeeName = Db.Attendes.FirstOrDefault(a => a.ID == y.AttendeeID).FirstName,
                                                                    AttendeeImage = Db.Attendes.FirstOrDefault(a => a.ID == y.AttendeeID).Thumbnail
                                                                }).ToList();
            return channelAttendeeImages;
        }

        private void SendNotificationFromWeb(int senderId, int channelId, string chatMessage, List<int> receiverIds, string imageName, string UploadType, string imageURL)
        {
            EventLogic eventLogic = new EventLogic();
            //List<int> receiversList = GetChannelRecieversToSendNotification(channelId, senderId);
            int eventId = GetEventID(channelId);
            //List<UserSession> channelUserSessions = new List<UserSession>();
            List<string> receiverTokens = new List<string>();

            string SenderName = Db.Attendes.FirstOrDefault(x => x.ID == senderId).FirstName;

            List<string> webNotificationToken = new List<string>();

            foreach (int receiverId in receiverIds)
            {
                List<string> sessionToken = eventLogic.GetAttendeeSessions(receiverId).Select(s => s.DeviceToken).ToList();
                receiverTokens.AddRange(sessionToken);
            }

            foreach (string receiverToken in receiverTokens)
            {
                if (UploadType == "Image")
                {
                    chatMessage = imageURL;
                    SendFirebaseNotification(receiverToken, "Chat Message", chatMessage, channelId, senderId, SenderName, "Chat Message", eventId, UploadType);
                }
                else
                {
                    SendFirebaseNotification(receiverToken, "Chat Message", chatMessage, channelId, senderId, SenderName, "Chat Message", eventId, UploadType);
                }

            }
        }


        public Tuple<int, List<int>> GetRecepientsByChannleId(int channelId, int senderId)
        {
            List<UserChatChannel> uchannels = Db.UserChatChannels.Where(x => x.userid == senderId && x.channelid == channelId).ToList();

            List<int> recepients = uchannels.Select(s => s.touserid).ToList();
            int eventId = uchannels.FirstOrDefault().Eventid;

            return new Tuple<int, List<int>>(eventId, recepients);
        }

        public int GetOrCreateChannel(int fromUserId, List<int> toUserId, int eventId, bool isGroup = false, int groupId = default, string imageSavePath = null)
        {
            UserChatChannel uchannel = null;

            if (!isGroup)
            {
                uchannel = Db.UserChatChannels.FirstOrDefault(x => x.Eventid == eventId && x.userid == fromUserId && toUserId.Contains(x.touserid));
            }


            DateTime date = DateTime.Now.ToUniversalTime();
            if (uchannel != null)
            {
                List<int> toUserDbList = Db.UserChatChannels.Where(s => s.channelid == uchannel.channelid && s.userid == fromUserId).Select(s => s.touserid).ToList();
                List<int> newUsers = toUserId.Except(toUserDbList).ToList();
                List<int> fromUsers = toUserDbList.Except(newUsers).ToList();

                fromUsers.Add(fromUserId);

                foreach (int fromUser in fromUsers)
                {
                    AddusersToChannel(fromUser, toUserId.Except(toUserDbList).ToList(), eventId, uchannel.channelid);
                }

                return uchannel.channelid;
            }
            else
            {
                return CreateChatChannel(fromUserId, toUserId, eventId, isGroup, groupId, imageSavePath);
            }
        }


        public int CreateChatChannel(int fromUserId, List<int> toUserId, int eventId, bool isGroup = false, int groupId = default, string imageSavePath = null)
        {
            DateTime date = DateTime.Now.ToUniversalTime();
            channel channel = new channel();
            channel.CreatedDate = date;
            channel.isGroup = isGroup;

            if (groupId != default)
            {
                channel.GroupId = groupId;

                AttendesLogic attendBuisness = new AttendesLogic();
                channel.GroupImage = attendBuisness.GenerateImageFromName("", "", imageSavePath);
            }


            Db.channels.Add(channel);
            Db.SaveChanges();

            List<int> fromUsers = new List<int>();
            fromUsers.AddRange(toUserId);
            fromUsers.Add(fromUserId);

            foreach (int fromUser in fromUsers)
            {
                AddusersToChannel(fromUser, fromUsers.Where(s => s != fromUser).ToList(), eventId, channel.channelid);
            }

            return channel.channelid;

        }

        public int GetEventID(int channelId)
        {
            var userChatChannel = Db.UserChatChannels.FirstOrDefault(s => s.channelid == channelId);

            if (userChatChannel != null)
            {
                return userChatChannel.Eventid;
            }
            return default(int);
        }


        private void RemoveRedundantChannels(int userId, List<RecentMessage> recentMessages)
        {
            bool isAdminUser = false;

            Attende attende = Db.Attendes.FirstOrDefault(s => s.ID == userId);

            if (attende != null)
            {
                isAdminUser = attende.IsAdmin;
            }
            List<int> channels = recentMessages.Select(s => s.ChannelID).ToList();
            channels = channels.GroupBy(x => x).Where(g => g.Count() > 1).Select(x => x.Key).ToList();

            foreach (int channel in channels)
            {
                List<int> receiversList = GetChannelRecievers(channel, userId);

                if (isAdminUser)
                {
                    foreach (int receiver in receiversList)
                    {
                        bool receiverAttendee = Db.Attendes.FirstOrDefault(s => s.ID == userId).IsAdmin;

                        if (receiverAttendee)
                        {
                            recentMessages.Remove(recentMessages.FirstOrDefault(s =>
                                s.AttendeeID == receiver && s.ChannelID == channel));
                        }
                    }
                }
                else
                {
                    List<channelmessage> channelMessages = Db.channelmessages.Where(s => s.channelid == channel)
                .OrderByDescending(t => t.sentdate).ToList();

                    var channelMessage = channelMessages.FirstOrDefault();

                    if (channelMessage.userid == userId)
                    {
                        if (channelMessages.Count == 1)
                        {
                            int receiverFirst = receiversList.FirstOrDefault();

                            recentMessages.RemoveAll(s => s.ChannelID == channel && s.AttendeeID != receiverFirst);

                            //recentMessages.Remove(recentMessages.FirstOrDefault(s =>
                            //    s.AttendeeID == receiver && s.ChannelID == channel));


                        }
                        else
                        {
                            var latestChannelExceptUser = channelMessages.FirstOrDefault(s => s.userid != userId);

                            if (latestChannelExceptUser == null)
                            {
                                int receiverFirst = receiversList.FirstOrDefault();

                                recentMessages.RemoveAll(s => s.ChannelID == channel && s.AttendeeID != receiverFirst);
                            }
                            else
                            {
                                foreach (int receiver in receiversList)
                                {
                                    if (latestChannelExceptUser.userid != receiver)
                                    {
                                        recentMessages.Remove(recentMessages.FirstOrDefault(s =>
                                            s.AttendeeID == receiver && s.ChannelID == channel));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (int receiver in receiversList)
                        {
                            if (channelMessage.userid != receiver)
                            {
                                recentMessages.Remove(recentMessages.FirstOrDefault(s =>
                                    s.AttendeeID == receiver && s.ChannelID == channel));
                            }
                        }
                    }
                }
            }
        }

        public void AddusersToChannel(int fromUserId, List<int> toUserId, int eventId, int channelId)
        {
            UserChannelUnreadCount fromUserChannel = Db.UserChannelUnreadCounts.FirstOrDefault(s => s.userid == fromUserId && s.channelId == channelId);

            if (fromUserChannel == null)
            {
                UserChannelUnreadCount ucuc = new UserChannelUnreadCount
                {
                    channelId = channelId,
                    userid = fromUserId,
                    unReadMessageCount = 0
                };
                Db.UserChannelUnreadCounts.Add(ucuc);
                Db.SaveChanges();
            }

            foreach (int toUser in toUserId)
            {
                UserChatChannel fromUserChatChannel = Db.UserChatChannels.FirstOrDefault(s => s.channelid == channelId && s.touserid == toUser && s.userid == fromUserId);

                if (fromUserChatChannel == null)
                {
                    UserChatChannel uc = new UserChatChannel
                    {
                        channelid = channelId,
                        userid = fromUserId,
                        touserid = toUser,
                        Eventid = eventId,
                        hasleft = false,
                        isdeleted = false
                    };
                    Db.UserChatChannels.Add(uc);
                    Db.SaveChanges();
                }

                UserChatChannel toUserChatChannel = Db.UserChatChannels.FirstOrDefault(s => s.channelid == channelId && s.touserid == fromUserId && s.userid == toUser);

                if (toUserChatChannel == null)
                {
                    UserChatChannel uc1 = new UserChatChannel
                    {
                        channelid = channelId,
                        userid = toUser,
                        touserid = fromUserId,
                        Eventid = eventId,
                        hasleft = false,
                        isdeleted = false
                    };
                    Db.UserChatChannels.Add(uc1);
                    Db.SaveChanges();
                }
                UserChannelUnreadCount toUserChannel = Db.UserChannelUnreadCounts.FirstOrDefault(s => s.userid == toUser && s.channelId == channelId);

                if (toUserChannel == null)
                {
                    UserChannelUnreadCount ucuc = new UserChannelUnreadCount
                    {
                        channelId = channelId,
                        userid = toUser,
                        unReadMessageCount = 0
                    };

                    Db.UserChannelUnreadCounts.Add(ucuc);
                    Db.SaveChanges();
                }

            }
        }

        //public List<string> GetChannelUserTokens(int ChannelId, string LoginUserFcmToken)
        //{
        //    try
        //    {
        //        var userTokens = ((from c in Db.UserChatChannels.Where(c => c.channelid == ChannelId)
        //                           from d in Db.DeviceFcmTokens.Where(i => i.userid == c.userid && i.fcmtoken != LoginUserFcmToken)
        //                           select d.fcmtoken).ToList());
        //        return userTokens;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public channelmessage SaveChannelMessage(channelmsgs _msg)
        {
            channelmessage newMsg = new channelmessage();
            DateTime date = DateTime.Now.ToUniversalTime();
            try
            {
                newMsg.channelid = _msg.channelId;
                newMsg.userid = _msg.FromuserId;
                newMsg.sentdate = date;
                newMsg.messagetype = unchecked((byte)_msg.msgtype);
                newMsg.message = "";
                Db.channelmessages.Add(newMsg);
                Db.SaveChanges();

                if (_msg.msgtype == 1)
                {
                    newMsg.message = _msg.strmessage;
                    Db.SaveChanges();
                }
                if (_msg.msgtype == 2)
                {
                    newMsg.message = newMsg.id + "." + _msg.ImgExt;
                    Db.SaveChanges();
                }
                if (_msg.msgtype == 1)
                {
                    var ch = Db.channels.FirstOrDefault(x => x.channelid == _msg.channelId);
                    //ch.latestmsgid = (int)newMsg.id;
                    Db.SaveChanges();
                }
                Db.UserChatChannels.Where(c => c.channelid == _msg.channelId &&
                                           c.userid != _msg.FromuserId).ToList().ForEach(k =>
                                           {
                                               k.unreadmsgcount = k.unreadmsgcount + 1;
                                               Db.SaveChanges();
                                           });
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return newMsg;

        }

        public int SendNotification(string userTokens, string bodymsg, string Type, string Title, int channelId, string LoginuserToken, int fromuserId, int ToUserId)
        {
            try
            {
                String sResponseFromServer = "-1";

                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.Headers.Add(HttpRequestHeader.Authorization, $"key={TwilioConfiguration.FirebaseSID}");
                //tRequest.Headers.Add(string.Format("Sender: id={0}", "735466763409"));
                tRequest.ContentType = "application/json";
                var payload = new
                {
                    registration_ids = userTokens,
                    priority = "high",
                    content_available = true,
                    notification = new
                    {
                        body = bodymsg,
                        title = Title,
                    },
                    data = new
                    {
                        text = bodymsg,
                        ChannelId = channelId,
                        type = Type,
                        fromFcmToken = LoginuserToken,
                        fromUserId = fromuserId,
                    }
                };

                string postbody = JsonConvert.SerializeObject(payload).ToString();
                Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                tRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    sResponseFromServer = tReader.ReadToEnd();
                                    //result.Response = sResponseFromServer;
                                }
                        }
                    }
                }

                var data = JsonConvert.DeserializeObject<EventLogic.FcmResponse>(sResponseFromServer.ToString());
                return data.Success;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<UserChannelMessages> startAdminToAttendeeConversation(int ReceiverID, int eventID)
        {
            List<int> allAdminsOfEvent = GetAllAdminsOfEvent(eventID);

            int admin = allAdminsOfEvent.First();

            List<int> recieverList = allAdminsOfEvent.Except(new List<int> { admin }).ToList();

            recieverList.Add(ReceiverID);

            return FetchMessagesByChannelID(admin, recieverList, eventID);
        }

        public string startAdminToAttendeeWebConversation(int senderId, int ReceiverID, int eventID)
        {
            var senderIdentity = "CMSAdmin";
            var receiverIdentity = Db.Attendes.Find(ReceiverID).Email;

            var adminChannelUsers = GetAdminChannelsByEventID(eventID);
            var attendeeChannelUsers = GetAttendeeChannelByEventID(ReceiverID, eventID);

            var commonChannels = adminChannelUsers.Join(attendeeChannelUsers, x => x.channelid, y => y.channelid, (x, y) => x);

            if (commonChannels.Count() > 0)
            {
                return commonChannels.ToList()[0].channelid.ToString();
            }

            List<int> attendees = Db.AttendesEvents.Where(x => x.Attende.IsAdmin == true && x.EventID == eventID && x.AttendesID != senderId).Select(y => y.AttendesID).ToList();
            attendees.Add(ReceiverID);
            string channelID = GetOrCreateChannel(senderId, attendees, eventID).ToString();
            return channelID;
        }

        public List<int> GetAllAdminsOfEvent(int eventId)
        {

            //List<ChatAdminChannel> adminChannels = Db.UserChatChannels.Where(s => s.userid == userId)
            //    .Join(Db.Events, uc => uc.Eventid, ee => ee.ID, (uc, ee) => new { uc, ee })
            //    .Select(z => new ChatAdminChannel
            //    {
            //        ChannelId = z.uc.channelid,
            //        EventName = z.ee.EventName,
            //        ToUserId = z.uc.touserid,
            //        ChannelName = string.Empty,
            //        UnreadMessageCount = 0,
            //        ChannelLatestTime = date,
            //        ChannelLastMessage = string.Empty
            //    }).ToList();

            //var ss = Db.AttendesEvents.Where(s=> s.EventID == eventId).Join(Db.Attendes.Where(x=>x.IsAdmin)
            //                                    , ae => ae.AttendesID, att=> att. )

            IQueryable<int> adminsOfAnEvent = from ae in Db.AttendesEvents
                                              from att in Db.Attendes
                                              where ae.AttendesID == att.ID
                                              where ae.EventID == eventId && att.IsAdmin
                                              select att.ID;


            //List<int> adminsOfAnEvent = Db.Attendes.Where(x => x.IsAdmin &&
            //                                                   x.AttendesEvents.All(y => y.EventID == eventID)).Select(s => s.ID).ToList();

            return adminsOfAnEvent.ToList();
        }

        public List<UserChatChannel> GetAdminChannelsByEventID(int eventID)
        {
            return Db.UserChatChannels.Where(x => (Db.Attendes.Any(y => y.ID == x.userid && y.IsAdmin == true)) && x.Eventid == eventID).ToList();

            //Db.ChannelUsers.Where(x => x.Attende.IsAdmin && x.Attende.AttendesEvents.Any(y => y.EventID == eventID) && x.ChatChannel.EventID == eventID).ToList();
        }

        public List<UserChatChannel> GetAttendeeChannelByEventID(int attendeeID, int eventID)
        {
            return Db.UserChatChannels.Where(x => x.userid == attendeeID && x.Eventid == eventID).ToList();
        }

        public List<UserChannelMessages> FetchMessagesByChannelID(int SenderId, List<int> RecieverID, int EventId, bool isGroup = false, int groupId = default, string imageSavePath = null)
        {
            int channelId = GetOrCreateChannel(SenderId, RecieverID, EventId, isGroup, groupId, imageSavePath);

            return FetchingChannelMessages(channelId, SenderId, RecieverID, EventId);
        }

        public List<UserChannelMessages> FetchingChannelMessages(int channelId, int SenderId, List<int> RecieverID, int EventId)
        {
            List<UserChannelMessages> channelMessages = Db.channelmessages.
                    Where(x => x.channelid == channelId).
                    Select(y => new UserChannelMessages
                    {
                        ChannelId = y.channelid,
                        EventId = EventId,
                        AttendeeId = y.userid,
                        AttendeeName = Db.Attendes.FirstOrDefault(a => a.ID == y.userid).FirstName,
                        MessageText = y.message,
                        AttendeImage = Db.Attendes.FirstOrDefault(a => a.ID == y.userid).Thumbnail,
                        DateCreated = y.sentdate,
                        HasLeft = false,
                        type = y.type
                    }).OrderBy(z => z.DateCreated).ToList();

            UserChatChannel ucc = Db.UserChatChannels.FirstOrDefault(s => s.userid == SenderId && s.channelid == channelId);

            if (ucc != null && ucc.hasleft == true)
            {
                channelMessages.RemoveAll(s => s.DateCreated >= ucc.hasLeftDate.Value);
                channelMessages.ForEach(c => c.HasLeft = true);
            }

            int receiverId = RecieverID.FirstOrDefault();

            DateTime date = DateTime.Now.ToUniversalTime();

            if (channelMessages.Count == 0)
            {
                UserChannelMessages userChannelMessages = new UserChannelMessages
                {
                    ChannelId = channelId,
                    EventId = EventId,
                    AttendeeId = receiverId,
                    AttendeeName = Db.Attendes.FirstOrDefault(a => a.ID == receiverId)?.FirstName,
                    AttendeImage = Db.Attendes.FirstOrDefault(a => a.ID == receiverId)?.Thumbnail,
                    MessageText = string.Empty,
                    DateCreated = date,
                    HasLeft = ucc != null ? ucc.hasleft : false,
                    type = "Text"
                };


                //Channel channel = Db.Channels.FirstOrDefault(s => s.channelid == channelId);

                //if (channel.isGroup == true)
                //{
                //    List<int> channelReceivers = GetChannelRecievers(channel.channelid, SenderId);

                //    userChannelMessages.AttendeeName = GetChannelName(channel.GroupId.Value, channel.channelid, SenderId, channelReceivers);
                //    userChannelMessages.AttendeImage = channel.GroupImage;
                //}

                channelMessages.Add(userChannelMessages);
            }

            SetUnreadCountToZero(SenderId, channelId, EventId);

            return channelMessages;
        }

        public List<ChatAdminChannel> GetAdminChannels(int userId)
        {
            DateTime date = DateTime.Now.ToUniversalTime();
            List<ChatAdminChannel> adminChannels = Db.UserChatChannels.Where(s => s.userid == userId)
                .Join(Db.Events, uc => uc.Eventid, ee => ee.ID, (uc, ee) => new { uc, ee })
                .Select(z => new ChatAdminChannel
                {
                    ChannelId = z.uc.channelid,
                    EventName = z.ee.EventName,
                    ToUserId = z.uc.touserid,
                    ChannelName = string.Empty,
                    UnreadMessageCount = 0,
                    ChannelLatestTime = date,
                    ChannelLastMessage = string.Empty
                }).ToList();

            foreach (var adminChannel in adminChannels)
            {
                adminChannel.ChannelName = GetReceiverText(adminChannel.ToUserId);
                adminChannel.UnreadMessageCount = GetUnReadCount(userId, adminChannel.ChannelId);

                channelmessage latestChannelInfo = GetChannelLatest(adminChannel.ChannelId, userId);
                adminChannel.ChannelLatestTime = latestChannelInfo?.sentdate ?? date;
                adminChannel.ChannelLastMessage = latestChannelInfo?.message ?? string.Empty;
            }

            adminChannels.RemoveAll(x => string.IsNullOrEmpty(x.ChannelLastMessage));

            adminChannels.RemoveAll(s => string.IsNullOrEmpty(s.ChannelName));

            List<int> toUsers = adminChannels.Select(s => s.ToUserId).ToList();

            List<int> chatDisabledUsers = Db.Attendes.Where(s => s.EnableMessaging == false && toUsers.Contains(s.ID)).Select(x => x.ID).ToList();

            adminChannels.RemoveAll(s => chatDisabledUsers.Contains(s.ToUserId));

            return adminChannels;
            //return Db.ChannelUsers.Where(x => x.Attende.IsAdmin && x.Attende.AttendesEvents.Any(y => y.EventID == eventID) && x.ChatChannel.EventID == eventID).ToList();
        }

        public string GetReceiverText(int toUserId)
        {
            Attende attendee =
                Db.Attendes.FirstOrDefault(s => s.IsAdmin != true && s.ID == toUserId && s.EnableMessaging == true);

            return attendee != null ? string.Concat(attendee.FirstName, " ", attendee.Lastname) : string.Empty;
        }

        public List<RecentMessage> GetRecentChatConversation(int userId, int eventId)
        {
            DateTime date = DateTime.Now.ToUniversalTime();
            List<RecentMessage> recentMessages = new List<RecentMessage>();
            //var ss1 = from rr1 in r1
            //          join rr2 in r2 on new { rr1.Month, rr1.ID } equals new { rr2.Month, rr2.ID }
            //          select new { R1 = rr1, R2 = rr2 };

            List<int> userChannels = Db.UserChatChannels.Where(uc => uc.Eventid == eventId && uc.userid == userId).Select(s => s.channelid).Distinct().ToList();

            foreach (int userChannel in userChannels)
            {
                channel channel = Db.channels.FirstOrDefault(s => s.channelid == userChannel);

                RecentMessage recentMessage = new RecentMessage
                {
                    ChannelID = userChannel,
                    TextMessage = string.Empty,
                    LastMessageSentDate = date,
                    UnreadMessageCount = GetUnReadCount(userId, userChannel)

                };

                channelmessage latestChannelInfo = GetChannelLatest(recentMessage.ChannelID, userId);
                recentMessage.TextMessage = latestChannelInfo != null ? latestChannelInfo.message : string.Empty;
                recentMessage.LastMessageSentDate = latestChannelInfo?.sentdate ?? date;
                recentMessage.type = latestChannelInfo != null ? latestChannelInfo.type : "Text";
                if (latestChannelInfo == null)
                {
                    int attendeeId = GetChannelRecievers(userChannel, userId).FirstOrDefault();

                    recentMessage.AttendeeID = attendeeId;
                }
                else
                {
                    recentMessage.AttendeeID = latestChannelInfo.userid;
                }

                if (channel.isGroup == true)
                {
                    var channelReceivers = GetChannelRecieversToSendNotification(channel.channelid, userId);
                    recentMessage.UserName = GetChannelName(channel.GroupId.Value, userId, channelReceivers);
                    recentMessage.Photo = channel.GroupImage;
                }
                else
                {
                    int channelReciever = GetChannelRecieversToSendNotification(userChannel, userId).FirstOrDefault();
                    Attende attende = Db.Attendes.FirstOrDefault(s => s.ID == channelReciever);
                    recentMessage.UserName = attende.FirstName;
                    recentMessage.Photo = attende.Thumbnail;

                }

                recentMessages.Add(recentMessage);
            }

            recentMessages.RemoveAll(s => string.IsNullOrEmpty(s.TextMessage));
            //recentMessages.RemoveAll(s => string.IsNullOrEmpty(s.UserName));

            //RemoveRedundantChannels(userId, recentMessages);

            recentMessages = recentMessages.OrderByDescending(s => s.LastMessageSentDate).ToList();

            return recentMessages;
        }

        public channelmessage GetChannelLatest(int channelId, int userId)
        {
            UserChatChannel userChatChannel = Db.UserChatChannels.FirstOrDefault(s => s.userid == userId && s.channelid == channelId);

            channelmessage channelMessage = new channelmessage();

            if (userChatChannel.hasleft == false)
            {
                channelMessage = Db.channelmessages.Where(s => s.channelid == channelId)
                    .OrderByDescending(t => t.sentdate).FirstOrDefault();
            }
            else
            {

                channelMessage = Db.channelmessages.Where(s => s.channelid == channelId && s.sentdate <= userChatChannel.hasLeftDate.Value)
                        .OrderByDescending(t => t.sentdate).FirstOrDefault();
            }

            return channelMessage;
        }

        public int GetUnReadCount(int userId, int channelId)
        {
            UserChannelUnreadCount userChannelUnreadCount = Db.UserChannelUnreadCounts.FirstOrDefault(s => s.channelId == channelId && s.userid == userId);

            if (userChannelUnreadCount == null)
            {
                return 0;
            }

            return userChannelUnreadCount.unReadMessageCount;
        }

        public int SetUnreadCountToZero(int senderId, int channelId, int eventId)
        {
            var userChannelUnreadCount = Db.UserChannelUnreadCounts.FirstOrDefault(s => s.channelId == channelId && s.userid == senderId);

            if (userChannelUnreadCount != null)
            {
                userChannelUnreadCount.unReadMessageCount = 0;
            }
            else
            {
                Db.UserChannelUnreadCounts.Add(new UserChannelUnreadCount
                {
                    userid = senderId,
                    channelId = channelId,
                    unReadMessageCount = 0
                });
            }

            Db.SaveChanges();

            return channelId;
        }

        public Message sendMessage(int senderId, string message, int channelId, int eventId, HttpPostedFileBase uploadedImage, string imageName, string ImageSavePath, string imageURL)
        {
            bool status = SendMessage(senderId, message, channelId, uploadedImage, imageName, ImageSavePath, imageURL);
            Message msg = new Message();
            msg.SenderID = senderId;
            msg.ChannelId = channelId;
            msg.EventId = eventId;
            msg.AttendeeName = Db.Attendes.FirstOrDefault(x => x.ID == senderId).FirstName;
            msg.NotificationType = "Live Chat";
            msg.status = status ? true : false;
            if (uploadedImage != null)
            {
                msg.MessageText = imageName;
                msg.type = "Image";
            }
            else
            {
                msg.MessageText = message;
                msg.type = "Text";
            }
            return msg;
        }

        public bool SendMessage(int senderId, string chatMessage, int channelId, HttpPostedFileBase image, string imageName, string ImageSavePath, string imageURL)
        {
            string Uploadtype = "";
            DateTime date = DateTime.Now.ToUniversalTime();
            //int channelId = GetOrCreateChannel(senderId, receiverId, eventID); 

            if (image != null)
            {
                Db.channelmessages.Add(new channelmessage
                {
                    userid = senderId,
                    channelid = channelId,
                    message = imageName,
                    sentdate = date,
                    messagetype = 0,
                    type = "Image"
                });
                Uploadtype = "Image";
                image.SaveAs(Path.Combine(ImageSavePath, imageName));
            }
            else
            {
                Db.channelmessages.Add(new channelmessage
                {
                    userid = senderId,
                    channelid = channelId,
                    message = chatMessage,
                    sentdate = date,
                    messagetype = 0,
                    type = "Text"
                });
                Uploadtype = "Text";
            }

            Db.SaveChanges();
            List<int> receiverIds = GetChannelRecieversToSendNotification(channelId, senderId);

            foreach (int user in receiverIds)
            {
                UserChannelUnreadCount MessageCount = Db.UserChannelUnreadCounts.FirstOrDefault(x => x.channelId == channelId && x.userid == user);
                MessageCount.unReadMessageCount = MessageCount.unReadMessageCount + 1;
            }
            Db.SaveChanges();

            Task.Run(() => SendNotificationFromWeb(senderId, channelId, chatMessage, receiverIds, imageName, Uploadtype, imageURL));

            return true;
        }


        public List<int> GetChannelRecievers(int channelId, int senderId)
        {
            var channelReceivers = Db.UserChatChannels.Where(s => s.channelid == channelId && s.userid == senderId)
                .Select(x => x.touserid).ToList();
            channelReceivers = channelReceivers.Distinct().ToList();
            return channelReceivers;
        }

        public List<int> GetChannelRecieversToSendNotification(int channelId, int senderId)
        {
            var channelReceivers = Db.UserChatChannels.Where(s => s.channelid == channelId && s.userid == senderId && s.hasleft == false)
                .Select(x => x.touserid).ToList();
            channelReceivers = channelReceivers.Distinct().ToList();
            return channelReceivers;
        }

        public List<AttendeeChannelImages> GetAttendeeImagesbyChannelId(int channelId)
        {
            List<AttendeeChannelImages> channelImages = Db.UserChatChannels.
                                                            Where(x => x.channelid == channelId).
                                                            Select(y => new AttendeeChannelImages
                                                            {
                                                                AttendeeId = y.userid,
                                                                AttendeeName = Db.Attendes.FirstOrDefault(a => a.ID == y.userid).FirstName,
                                                                AttendeeImage = Db.Attendes.FirstOrDefault(a => a.ID == y.userid).Thumbnail
                                                            }).ToList();
            return channelImages;
        }

        public int GetUnReadMessageCountForEvent(int userId, int eventId)
        {
            List<int> userChannelsForEvent = GerUserChannelsOfEvent(userId, eventId);

            int totalCount = 0;

            foreach (int userChannel in userChannelsForEvent)
            {
                totalCount += GetUnReadCount(userId, userChannel);
            }

            return totalCount;
        }

        public List<int> GerUserChannelsOfEvent(int userId, int eventId)
        {
            return Db.UserChatChannels.Where(s => s.userid == userId && s.Eventid == eventId).Select(x => x.channelid).Distinct()
                .ToList();
        }

        public int SendFirebaseNotification(string devicetoke, string title, string message, int channelId, int senderId, string senderName, string notificationType, int eventId, string UploadType)
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
                string strNJson = "{\"to\": \"" + devicetoke + "\",\"notification\": {\"title\": \"" + senderName + "\", \"body\": \"" + message + "\",\"sound\":\"default\"},\"data\": {\"channel\":\"" + channelId + "\",\"senderId\":\"" + senderId + "\",\"NotificationType\":\"" + notificationType + "\",\"EventId\":\"" + eventId + "\",\"text\": \"" + message + "\", \"SenderName\": \"" + senderName + "\" , \"Uploadtype\": \"" + UploadType + "\" }}";

                streamWriter.Write(strNJson);
                streamWriter.Flush();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            var data = JsonConvert.DeserializeObject<FcmResponse>(result.ToString());

            Attende senderAttendee = Db.Attendes.FirstOrDefault(s => s.ID == senderId);

            if (senderAttendee != null && !senderAttendee.IsAdmin)
            {
                SendMailForAdmin(message, senderAttendee.Email, GetRecepientsByChannleId(channelId, senderId));
            }

            return data.Success;
        }

        public void SendMailForAdmin(string message, string senderMailId, Tuple<int, List<int>> eventByChannelId)
        {
            string eventName = Db.Events.FirstOrDefault(s => s.ID == eventByChannelId.Item1)?.EventName ?? string.Empty;

            List<string> adminEmails = Db.Attendes.Where(s => s.IsAdmin && s.EnableAlertEmails == true && eventByChannelId.Item2.Contains(s.ID)).Select(x => x.Email).ToList();

            foreach (string adminEmail in adminEmails)
            {
                using (var emailSender = new EmailNotifier(adminEmail))
                {
                    emailSender.Subject = string.Concat(eventName, " - ", senderMailId);
                    emailSender.eBody = message;
                    emailSender.SendEmail(senderMailId);
                }
            }
        }

        public bool LeaveGroup(int userId, int channelId)
        {
            UserChannelUnreadCount userChannelUnreadCount = Db.UserChannelUnreadCounts.FirstOrDefault(s => s.channelId == channelId && s.userid == userId);

            Db.UserChannelUnreadCounts.Remove(userChannelUnreadCount);

            List<UserChatChannel> userChatChannels = Db.UserChatChannels.Where(s => s.channelid == channelId && (s.userid == userId || s.touserid == userId)).ToList();

            foreach (UserChatChannel userChatChannel in userChatChannels)
            {
                userChatChannel.hasleft = true;
                userChatChannel.hasLeftDate = DateTime.Now.ToUniversalTime();

            }

            Db.SaveChanges();

            return true;
        }

        public List<UserChannelMessages> CreateGroup(int eventId, int senderId, List<int> receiverIds, string groupName, string imageSavePath = null)
        {
            List<int> chatGroupReceivers = new List<int>() { senderId };
            chatGroupReceivers.AddRange(receiverIds);

            if (string.IsNullOrEmpty(groupName))
            {
                groupName = GetChannelName(0, senderId, receiverIds);
            }

            GroupInfo groupInfo = new GroupInfo
            {
                GroupName = groupName,
                GroupDescription = groupName,
                EventID = eventId,
                CreatedBy = senderId,
                IsChatAllowed = true,
                AttendeeGroups = chatGroupReceivers.Select(x => new AttendeeGroup() { AttendeeID = x }).ToList(),
                CreatedDate = DateTime.Now.ToUniversalTime()
            };
            Db.GroupInfoes.Add(groupInfo);

            Db.SaveChanges();

            int groupId = groupInfo.ID;

            int channelId = CreateChatChannel(senderId, receiverIds, eventId, true, groupId, imageSavePath);

            List<UserChannelMessages> userChannelMessages = FetchingChannelMessages(channelId, senderId, receiverIds, eventId);

            foreach (UserChannelMessages userChannelMessage in userChannelMessages)
            {
                userChannelMessage.AttendeeName = groupName;
            }

            return userChannelMessages;
        }

        public List<UserGroupInformation> GetAttendeGroupsForEvent(int userId, int eventId)
        {
            List<UserGroupInformation> userGroupInformations = new List<UserGroupInformation>();

            List<int> channelList = Db.UserChatChannels.Where(s => s.Eventid == eventId && (s.userid == userId || s.touserid == userId)).Select(s => s.channelid).ToList();

            channelList = channelList.Distinct().ToList();

            Dictionary<int, int?> channelToGroup = Db.channels.Where(s => s.isGroup == true && channelList.Contains(s.channelid)).ToDictionary(s => s.channelid, s => s.GroupId);

            foreach (int channel in channelToGroup.Keys.ToList())
            {
                List<int> channelReceivers = GetChannelRecieversToSendNotification(channel, userId);
                channelReceivers = channelReceivers.Distinct().ToList();
                string groupImage = Db.channels.FirstOrDefault(s => s.channelid == channel).GroupImage;
                UserGroupInformation userGroupInformation = new UserGroupInformation
                {
                    ChannelId = channel,
                    ChannelReceivers = channelReceivers,
                    ChannelName = GetChannelName(channelToGroup[channel].Value, userId, channelReceivers),
                    ChannelImage = groupImage
                };
                userGroupInformations.Add(userGroupInformation);

            }

            return userGroupInformations;
        }

        private string GetChannelName(int groupId, int userId, List<int> channelReceivers)
        {
            string groupName = Db.GroupInfoes.FirstOrDefault(s => s.ID == groupId)?.GroupName;

            if (string.IsNullOrEmpty(groupName))
            {
                channelReceivers.Add(userId);

                List<string> userNameList = new List<string>();

                foreach (int attendeeId in channelReceivers)
                {
                    Attende attende = Db.Attendes.FirstOrDefault(s => s.ID == attendeeId);

                    if (attende != null)
                    {
                        string userName = string.Empty;

                        if (!string.IsNullOrEmpty(attende.FirstName))
                            userName = string.Concat(attende.FirstName.Trim(), ' ');

                        if (!string.IsNullOrEmpty(attende.Lastname))
                            userName = string.Concat(userName, attende.Lastname.Trim());

                        if (!string.IsNullOrEmpty(userName))
                        {
                            userNameList.Add(userName);
                        }
                    }
                }

                groupName = String.Join(",", userNameList);
            }

            return groupName;
        }


        public List<string> GetWebNotificationTokens(int channelId, int userId)
        {
            List<string> sessionTokens = new List<string>();

            List<int> users = GetChannelRecieversToSendNotification(channelId, userId);

            foreach (int user in users)
            {
                var attendee = Db.Attendes.Find(user);

                var sessions = attendee?.UserSessions.Where(y => y.IsActive == true && y.Platform == "Web" && !string.IsNullOrEmpty(y.DeviceToken)).Distinct(new UserDeviceTokenComparer());

                foreach (UserSession session in sessions)
                {
                    sessionTokens.Add(session.DeviceToken);
                }
            }

            return sessionTokens;
        }
    }
}
