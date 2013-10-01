using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;
using Teleopti.Analytics.Portal.AnalyzerProxy;
using Teleopti.Analytics.Portal.Utils;

namespace Teleopti.Analytics.Portal.PerformanceManager.Helper
{
	public static class AuthorizationHelper
	{
		public static bool DoCurrentUserHavePmPermission(string currentuser)
		{
			IList<SqlParameter> parameters = new List<SqlParameter>
												 {
													 new SqlParameter("user_name", currentuser),
													 new SqlParameter("is_windows_logon", CheckWindowsAuthentication())
												 };

			return (bool)
			DatabaseHelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.pm_user_check", parameters,
										  ConnectionString);
		}

		public static bool IsAuthenticationConfigurationValid()
		{
			AuthenticationMode webAuthenticationMode = GetWebAuthenticationMode();

			if (webAuthenticationMode == AuthenticationMode.Forms & PermissionInformation.IsPmAuthenticationWindows)
				return false; // Invalid configuration with Forms web auth and PM auth set to windows

			return true;
		}

		public static AuthenticationMode GetWebAuthenticationMode()
		{
			var sec = (AuthenticationSection)HttpContext.Current.GetSection("system.web/authentication");

			return sec.Mode;
		}

		public static string LoggedOnUserName
		{
			get
			{
				var userName = StateHolder.UserName;
				return HttpContext.Current.User.Identity.Name == userName ? HttpContext.Current.User.Identity.Name : userName;
			}
		}

		public static bool CheckWindowsAuthentication()
		{
			if (GetWebAuthenticationMode() == AuthenticationMode.Windows)
				return !StateHolder.DoForceFormsLogOn;

			return false;
		}

		private static string ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
			}
		}
	}
}
