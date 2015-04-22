using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using log4net.Config;

namespace Teleopti.Analytics.Portal
{
	public class Global : HttpApplication
	{

		protected void Application_Start(object sender, EventArgs e)
		{
			//xtLoader.LoadAllTextsToDatabase();
			XmlConfigurator.Configure();
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{
			var cookieFound = false;

			HttpCookie authCookie = null;

			for (var i = 0; i < Request.Cookies.Count; i++)
			{
				var cookie = Request.Cookies[i];

				if (cookie.Name == FormsAuthentication.FormsCookieName)
				{
					cookieFound = true;
					authCookie = cookie;
					break;
				}
			}

			// If the cookie has been found, it means it has been issued from either
			// the windows authorisation site, is this forms auth site.
			if (cookieFound)
			{
				// Extract the roles from the cookie, and assign to our current principal, which is attached to the
				// HttpContext.
				try
				{
					var winAuthTicket = FormsAuthentication.Decrypt(authCookie.Value);
					var roles = winAuthTicket.UserData.Split(';');
					var formsId = new FormsIdentity(winAuthTicket);
					var princ = new GenericPrincipal(formsId, roles);
					HttpContext.Current.User = princ;
					HttpContext.Current.Items.Add("FROMCOOKIE", true);
				}
				catch (Exception)
				{
				}

			}
			else
			{
				// No cookie found, we can redirect to the Windows auth site if we want, or let it pass through so
				// that the forms auth system redirects to the logon page for us.
			}
		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{

		}
	}
}