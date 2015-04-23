using System;
using System.Globalization;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Teleopti.Analytics.Portal.PerformanceManager.Helper;
using Teleopti.Analytics.Portal.Utils;
using Teleopti.Analytics.ReportTexts;

namespace Teleopti.Analytics.Portal
{
	public partial class Login : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Login1.Authenticate += login1Authenticate;
			Login1.TitleText = "";
			Login1.UserNameLabelText = Resources.LogInName;
			Login1.PasswordLabelText = Resources.LogInPassword;

			Login1.TextBoxStyle.CssClass = "LoginTextBox";
			Login1.LoginButtonStyle.CssClass = "btn-primary btn";
			Login1.LoginButtonText = Resources.LogInButton;
			Login1.FailureText = Resources.LogInError;
			Login1.UserNameRequiredErrorMessage = Resources.LogInUserRequired;
			Login1.PasswordRequiredErrorMessage = Resources.LogInPassRequired;
			Login1.RememberMeText = Resources.LogInRemember;
			Login1.Focus();

			AuthenticationMode mode = AuthorizationHelper.GetWebAuthenticationMode();

			if (!string.IsNullOrEmpty(Request.ServerVariables["LOGON_USER"]))
			{
				// In IIS NTLM authentication is set
				var showLogin = StateHolder.DoForceFormsLogOn;
				var loggedOut = false;
				if (HttpContext.Current.Session["LOGOUT"] != null)
					loggedOut = (bool)HttpContext.Current.Session["LOGOUT"];

				ID = HttpContext.Current.Session.SessionID;

				if (loggedOut || (mode == AuthenticationMode.Windows && !showLogin))
				{
					// Windows authentication in web.config AND we get a flag 
					// telling that we should not force a forms authentication.

					// Get user from aspnet_Users
					var winUser = Request.ServerVariables["LOGON_USER"];
					var winUserHasAcccess = AuthorizationHelper.DoCurrentUserHavePmPermission(winUser);
					if (winUserHasAcccess && !loggedOut)
					{
						FormsAuthentication.SetAuthCookie(winUser, false);
						Page.Response.Redirect(RedirectUrl(winUser));
					}
					else
					{
						// Win user could not be validated in db. Can use Forms login instead.
						//not show this when user has logged out
						if (!loggedOut)
						labelInfo.Text = string.Concat(Resources.AuthenticationFailedForUser,   //"xxAuthentication failed for user"
														" '",
														winUser,
														"'. ",
														Resources.PleaseUseApplicationLoginInstead);   //"xxPlease use application login instead."
					}
				}
			}
		}
		private string RedirectUrl(string userName)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}?{1}",
								 FormsAuthentication.GetRedirectUrl(userName, false),
								 Request.QueryString);
		}

		void login1Authenticate(object sender, AuthenticateEventArgs e)
		{
			//bool Authenticated;
			var tenantLogin = new TenantLogin();
			e.Authenticated = tenantLogin.TryApplicationLogon(new ApplicationLogonClientModel{UserName = Login1.UserName, Password  = Login1.Password});

			if (e.Authenticated)
			{
				StateHolder.UserObject = null;
				FormsAuthentication.SetAuthCookie(Login1.UserName, Login1.RememberMeSet);
				HttpContext.Current.Session["LOGOUT"] = false;
				HttpContext.Current.Session["FORCEFORMSLOGIN"] = true;
				StateHolder.UserName = Login1.UserName;
				if (Request.QueryString.Get("ReturnUrl") != null)
				{
					FormsAuthentication.RedirectFromLoginPage(Login1.UserName, false);
				}
				else
				{
					Page.Response.Redirect(RedirectUrl(Login1.UserName));
				}
			}

		}

	}
}