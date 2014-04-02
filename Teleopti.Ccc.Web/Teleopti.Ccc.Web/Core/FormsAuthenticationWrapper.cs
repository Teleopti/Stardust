using System;
using System.Web;
using System.Web.Security;
using Common.Logging;
using Microsoft.IdentityModel.Web;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core
{
	public class FormsAuthenticationWrapper : IFormsAuthentication
	{
		private ILog _logger = LogManager.GetLogger<FormsAuthenticationWrapper>();
		private readonly ICurrentHttpContext _httpContext;
		private readonly INow _now;

		public FormsAuthenticationWrapper(ICurrentHttpContext httpContext, INow now)
		{
			_httpContext = httpContext;
			_now = now;
		}

		public void SetAuthCookie(string userName)
		{
			MakeCookie(userName, userName);
		}

		public void MakeCookie(string userName, string userData)
		{
			var ticket = makeTicket(userName, userData);

			var encryptedTicket = encryptTicket(ticket);

			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
			{
				HttpOnly = true,
				Secure = FormsAuthentication.RequireSSL,
				Path = FormsAuthentication.FormsCookiePath
			};
			if (!string.IsNullOrEmpty(FormsAuthentication.CookieDomain))
			{
				cookie.Domain = FormsAuthentication.CookieDomain;
			}
			setCookie(cookie);
		}

		private static string encryptTicket(FormsAuthenticationTicket ticket) { return FormsAuthentication.Encrypt(ticket); }

		private void setCookie(HttpCookie cookie)
		{
			_httpContext.Current().Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
			_httpContext.Current().Response.Cookies.Add(cookie);
		}

		private FormsAuthenticationTicket makeTicket(string userName, string userData)
		{
			return makeTicket(
				userName,
				userData,
				_now.LocalDateTime().Add(new TimeSpan(0, 30, 0))
				);
		}

		private FormsAuthenticationTicket makeTicket(string userName, string userData, DateTime expiration)
		{
			var ticket = new FormsAuthenticationTicket(
				1,
				userName,
				_now.LocalDateTime(),
				expiration,
				false,
				userData,
				FormsAuthentication.FormsCookiePath);
			return ticket;
		}

		public void SignOut()
		{
			FederatedAuthentication.SessionAuthenticationModule.SignOut();
			FormsAuthentication.SignOut();
		}

		public bool TryGetCurrentUser(out string userName)
		{
			HttpCookie authCookie = _httpContext.Current().Request.Cookies[FormsAuthentication.FormsCookieName];
			if (authCookie != null)
			{
				try
				{
					FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
					if (ticket != null && !ticket.Expired)
					{
						userName = ticket.Name;
						_logger.DebugFormat("Found user {0}!",userName);
						return true;
					}
				}
				catch (Exception)
				{
				}
			}

			_logger.Debug("Didn't find user!");
			userName = null;
			return false;
		}
	}
}