using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;
using Teleopti.Analytics.Portal.AnalyzerProxy;

namespace Teleopti.Analytics.Portal.PerformanceManager.Helper
{
    public static class AuthorizationHelper
    {
        public static bool DoCurrentUserHavePmPermission(string currentuser)
        {
            IList<SqlParameter> parameters = new List<SqlParameter>
                                                 {
                                                     new SqlParameter("user_name", currentuser),
                                                     new SqlParameter("is_windows_logon", IsWebAuthenticationWindows)
                                                 };

            return
                (bool)
                DatabaseHelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.pm_user_check", parameters,
                                              ConnectionString);
        }

        public static bool IsAuthenticationConfigurationValid()
        {
            AuthenticationMode webAuthenticationMode = GetWebAuthenticationMode();

            if (webAuthenticationMode == AuthenticationMode.Forms & PermissionInformation.IsPmAuthenticationWindows)
            {
                // Invalid configuration with Forms web auth and PM auth set to windows
                return false;
            }

            return true;
        }

        public static AuthenticationMode GetWebAuthenticationMode()
        {
            AuthenticationSection sec = (AuthenticationSection)HttpContext.Current.GetSection("system.web/authentication");

            return sec.Mode;
        }

        public static string LoggedOnUserName
        {
            get
            {
                return HttpContext.Current.User.Identity.Name;
            }
        }

        public static bool IsWebAuthenticationWindows
        {
            get { return GetWebAuthenticationMode() == AuthenticationMode.Windows; }
        }

        private static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["Database"].ConnectionString;
            }
        }
    }
}
