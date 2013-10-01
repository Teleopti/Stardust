using System;
using System.Configuration;
using System.Globalization;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI.WebControls;
using Teleopti.Analytics.Portal.PerformanceManager.Helper;
using Teleopti.Analytics.Portal.Utils;
using Teleopti.Analytics.ReportTexts;

namespace Teleopti.Analytics.Portal
{
	public partial class Login : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Login1.Authenticate += Login1_Authenticate;
			Login1.TitleText = Resources.LogInTitle;
			Login1.UserNameLabelText = Resources.LogInName;
			Login1.PasswordLabelText = Resources.LogInPassword;

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
				if (mode == AuthenticationMode.Windows && !StateHolder.DoForceFormsLogOn)
				{
					// Windows authentication in web.config AND we get a flag 
					// telling that we should not force a forms authentication.

					// Get user from aspnet_Users
					MembershipUser u = Membership.GetUser(Request.ServerVariables["LOGON_USER"]);
					if (u != null)
					{
						FormsAuthentication.SetAuthCookie(u.UserName, false);
						Page.Response.Redirect(RedirectUrl(u.UserName));
					}
					else
					{
						// Win user could not be validated in db. Can use Forms login instead.
						_labelInfo.Text = string.Concat(Resources.AuthenticationFailedForUser,   //"xxAuthentication failed for user"
														" '",
														Request.ServerVariables["LOGON_USER"],
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

		void Login1_Authenticate(object sender, AuthenticateEventArgs e)
		{
			//bool Authenticated;
			var lUtil = new LogOnUtilities(ConnectionString);
			e.Authenticated = lUtil.CheckPassword(Login1.UserName, Login1.Password);

			if (e.Authenticated)
			{
				StateHolder.UserObject = null;
				FormsAuthentication.SetAuthCookie(Login1.UserName, Login1.RememberMeSet);
				
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

		private static string ConnectionString
		{
			get { return ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString; }
		}
	}


}
