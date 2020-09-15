using System.Web.Configuration;

namespace CommonHelpers
{
    public class TwilioConfiguration
    {
        public static string AccountSID
        {
            get { return WebConfigurationManager.AppSettings["TwilioAccountSid"]; }
        }

        public static string AuthToken
        {
            get { return WebConfigurationManager.AppSettings["TwilioAuthToken"]; }
        }

        public static string ApiKey
        {
            get { return WebConfigurationManager.AppSettings["TwilioApiKey"]; }
        }

        public static string ApiSecret
        {
            get { return WebConfigurationManager.AppSettings["TwilioApiSecret"]; }
        }

        public static string ChatServiceSid
        {
            get { return WebConfigurationManager.AppSettings["TwilioChatServiceSid"]; }
        }

        public static string PushCredentialSid
        {
            get { return WebConfigurationManager.AppSettings["TwilioPushCredentialSid"]; }
        }

        public static string FirebaseSID
        {
            get { return WebConfigurationManager.AppSettings["FirebaseSID"]; }
        }

    }
}
