using System.Threading.Tasks;
using EventManagement.BusinessLogic.Business;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace EventManagement
{
    [HubName("notificationHub")]
    public class NotificationHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }

        public void SendwebNotification(string userId, string message)
        {
            //Clients.User(userId).send(message);
            Clients.Client(userId).addChatMessageToPage(message);
        }
        
        public void GetToken()
        {
            string userName = Context.User.Identity.Name;
            string connectionId = Context.ConnectionId;
        }

        public void SendNotifications(string message)
        {
            string userName = Context.User.Identity.Name;
            string connectionId = Context.ConnectionId;
            //95f37097-3f20-4f47-9dcb-b7c144bcfeb4
            //Clients.All.addContosoChatMessageToPage(message);
            //Clients.All.send(message);
            //Clients.All.receiveNotification(message);
            //Clients.All.broadcastMessage(message); 
            //Clients.User("4f598886-2dc8-4ac1-8c1d-b49bd77e819a").addContosoChatMessageToPage(message);
            Clients.Client("33f3fc0f-580d-4012-b9d0-45666d6fadb2").addContosoChatMessageToPage(message);

            //Clients.Caller("95f37097-3f20-4f47-9dcb-b7c144bcfeb4").receiveNotification(message);
        }  

        public override Task OnConnected()
        {
            var httpContext = Context.Request.GetHttpContext();

            var dummyCookie = httpContext.Request.Cookies["AuthToken"];

            string token = dummyCookie.Value.Substring(6);
            
            string connectionId = Context.ConnectionId;

            CommonLogic commonLogic = new CommonLogic();

            commonLogic.InsertNotificationId(token, connectionId);

            return base.OnConnected();
        }
    }
}