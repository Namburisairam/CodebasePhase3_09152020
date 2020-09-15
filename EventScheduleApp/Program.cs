using EventManagement.BusinessLogic.Business;
using System;
using System.Net;
using System.Threading.Tasks;
using CommonHelpers;

namespace EventScheduleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                EventLogic eventLogic = new EventLogic();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                await eventLogic.SendScheduledActivityNotifications();
                await eventLogic.SendSurveyNotifications();
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.LogErrorToDB(ex, "Job");
                Helpers.LogError(ex.Message, ex);
                Environment.Exit(0);
            }
        }
    }
}
