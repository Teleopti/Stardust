using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Configuration;
using Teleopti.Analytics.Portal.AnalyzerProxy;
using Teleopti.Analytics.Portal.Utils;

namespace Teleopti.Analytics.Portal.PerformanceManager.Helper
{
	public static class AuthorizationHelper
	{
		public static PermissionLevel CurrentUserPmPermission(Guid? currentuser)
		{
			if (currentuser == null)
				return PermissionLevel.None;

			var parameters = new Dictionary<string, object>{{ "person_id", currentuser}};
			var databaseAccess = new DatabaseAccess(CommandType.StoredProcedure, "mart.pm_user_check", connectionString);
			return (PermissionLevel) databaseAccess.ExecuteScalar(parameters);
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

		public static Guid? LoggedOnUser
		{
			get
			{

				if (HttpContext.Current.Session["PERSONID"] == null)
					return null;

					Guid personid;
					Guid.TryParse(HttpContext.Current.Session["PERSONID"].ToString(), out personid);

				return personid;

			}
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

		private static string connectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
			}
		}
	}
}
