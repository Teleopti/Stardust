using System;
using System.Web;
using Teleopti.Analytics.Portal.AnalyzerProxy.Properties;

namespace Teleopti.Analytics.Portal.AnalyzerProxy
{
    public static class PermissionInformation
    {
        public static PermissionLevel UserPermissions
        {
            get
            {
                if (HttpContext.Current.Session["UserPermissions"] == null)
                {
                    var olapInformation = new OlapInformation();

                    using (var clientProxy = new ClientProxy(olapInformation.OlapServer, olapInformation.OlapDatabase))
                    {
                        HttpContext.Current.Session["UserPermissions"] = clientProxy.GetUserPermissions(AnalyzerUserName);
                    }
                }

                return (PermissionLevel)HttpContext.Current.Session["UserPermissions"];
            }
        }

        private static string AnalyzerUserName
        {
            get
            {
                if (Settings.Default.PM_Authentication_Mode.Trim() == "Anonymous")
                {
                    return Settings.Default.PM_Anonymous_User_Name.Trim();
                }

                return HttpContext.Current.User.Identity.Name;
            }
        }
        
        public static bool IsPmAuthenticationWindows
        {
            get
            {
                if (Settings.Default.PM_Authentication_Mode.Trim() == "Windows")
                {
                    return true;
                }

                return false;
            }
        }

        public static string AnonymousUserName
        {
            get
            {
                if (Settings.Default.PM_Authentication_Mode.Trim() == "Windows")
                    throw new InvalidOperationException("PM_Authentication_Mode in the configuration file must be set to Anonymous before you can access the property AnonymousUserName.");

                string userName = Settings.Default.PM_Anonymous_User_Name.Trim();
                int pos = userName.IndexOf("\\", StringComparison.CurrentCultureIgnoreCase);
                if (pos < 1)
                    return null;

                return userName.Substring(pos + 1);
            }
        }

        public static string AnonymousDomain
        {
            get
            {
                if (Settings.Default.PM_Authentication_Mode.Trim() == "Windows")
                    throw new InvalidOperationException("PM_Authentication_Mode in the configuration file must be set to Anonymous before you can access the property AnonymousDomain.");

                string userName = Settings.Default.PM_Anonymous_User_Name.Trim();
                int pos = userName.IndexOf("\\", StringComparison.CurrentCultureIgnoreCase);
                if (pos < 1)
                    return null;

                return userName.Substring(0, pos);
            }
        }

        public static string AnonymousPassword
        {
            get
            {
                if (Settings.Default.PM_Authentication_Mode.Trim() == "Windows")
                    throw new InvalidOperationException("PM_Authentication_Mode in the configuration file must be set to Anonymous before you can access the property AnonymousPassword.");

                return Settings.Default.PM_Anonymous_User_Password.Trim();
            }
        }
    }
}
