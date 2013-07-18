namespace Teleopti.Analytics.Portal.Utils
{
    public static class AppKeyReader
    {
        public static string GetSetting(string theSetting)
        {
            if (0 < System.Web.Configuration.WebConfigurationManager.AppSettings.Count)
            {
                string customSetting =
                    System.Web.Configuration.WebConfigurationManager.AppSettings[theSetting];
                if (null != customSetting)
                    return customSetting;
            }
            return "";
        }
    }
}