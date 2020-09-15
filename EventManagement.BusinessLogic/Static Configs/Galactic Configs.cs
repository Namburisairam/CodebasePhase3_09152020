using CommonHelpers;

namespace EventManagement.BusinessLogic.Static_Configs
{
    public static class GalacticConfigs
    {
        public static string GalacticApiBaseUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(Helpers.GetAppSetting("GalacticApi")))
                    return Helpers.GetAppSetting("GalacticApi");
                else return "https://ws.galacticps.com/";
            }
        }
    }
}
