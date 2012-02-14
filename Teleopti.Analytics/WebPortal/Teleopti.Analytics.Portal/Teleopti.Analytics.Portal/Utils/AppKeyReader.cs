using System.Web;
using System.Configuration;
using System.Globalization;

namespace Teleopti.Analytics.Portal.Utils
{
    /// <summary>
    /// Summary description for AppKeyReader
    /// </summary>
    public sealed class AppKeyReader
    {
        private AppKeyReader()
        { }

        public static string GetString(string name)
        {
            try
            {
                return ConfigurationManager.AppSettings[name];
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool GetBool(string name)
        {
            try
            {
                return bool.Parse(ConfigurationManager.AppSettings[name]);
            }
            catch
            {
                return false;
            }
        }

        public static int GetInt(string name)
        {
            try
            {
                return int.Parse(ConfigurationManager.AppSettings[name],CultureInfo.CurrentCulture);
            }
            catch
            {
                return 0;
            }
        }

        public static string GetSetting(string theSetting)
        {
            //Configuration rootWebConfig1 =
            //   System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);

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