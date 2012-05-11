using System;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class SessionSpecificCookieDataProvider : ISessionSpecificDataProvider
	{
		private readonly HttpContextBase _httpContext;
		private readonly ISessionSpecificCookieDataProviderSettings _sessionSpecificCookieDataProviderSettings;
		private readonly INow _now;
		private readonly ISessionSpecificDataStringSerializer _dataStringSerializer;

		public SessionSpecificCookieDataProvider(HttpContextBase httpContext,
																ISessionSpecificCookieDataProviderSettings sessionSpecificCookieDataProviderSettings,
																INow now,
																ISessionSpecificDataStringSerializer dataStringSerializer
			)
		{
			_httpContext = httpContext;
			_sessionSpecificCookieDataProviderSettings = sessionSpecificCookieDataProviderSettings;
			_now = now;
			_dataStringSerializer = dataStringSerializer;
		}

		public void Store(SessionSpecificData data)
		{
			var userData = _dataStringSerializer.Serialize(data);
			var userName = data.PersonId.ToString();

			MakeCookie(userName, _now.Time, userData);
		}

		public SessionSpecificData Grab()
		{
			var cookie = getCookie();
			if (cookie == null || string.IsNullOrEmpty(cookie.Value))
			{
				return null;
			}

			var ticket = decryptCookie(cookie);
			var userData = string.Empty;
			if (ticket != null && !ticket.Expired)
			{
				userData = ticket.UserData;
				handleSlidingExpiration(cookie, ticket);
			}

			setCookie(cookie);
			return _dataStringSerializer.Deserialize(userData);
		}

		public void ExpireCookie()
		{
			var cookie = getCookie();
			var ticket = decryptCookie(cookie);
			ticket = makeTicket(ticket.Name, _now.Time, ticket.UserData, DateTime.Now.AddSeconds(-1));
			cookie.Value = encryptTicket(ticket);
			setCookie(cookie);
		}

		private HttpCookie getCookie()
		{
			return _httpContext.Request.Cookies[_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName];
		}

		private void setCookie(HttpCookie cookie)
		{
			_httpContext.Response.Cookies.Remove(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName);
			_httpContext.Response.Cookies.Add(cookie);
		}

		public void MakeCookie(string userName, DateTime now, string userData)
		{
			var ticket = makeTicket(userName, now, userData);

			var encryptedTicket = encryptTicket(ticket);

			var cookie = new HttpCookie(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName, encryptedTicket)
			             	{
			             		HttpOnly = true,
			             		Secure = _sessionSpecificCookieDataProviderSettings.AuthenticationRequireSsl,
			             		Path = _sessionSpecificCookieDataProviderSettings.AuthenticationCookiePath
			             	};
			if (!string.IsNullOrEmpty(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieDomain))
			{
				cookie.Domain = _sessionSpecificCookieDataProviderSettings.AuthenticationCookieDomain;
			}
			setCookie(cookie);
		}

		private FormsAuthenticationTicket makeTicket(string userName, DateTime now, string userData)
		{
			return makeTicket(
				userName,
				now,
				userData,
				now.Add(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieExpirationTimeSpan)
				);
		}

		private FormsAuthenticationTicket makeTicket(string userName, DateTime now, string userData, DateTime expiration)
		{
			var ticket = new FormsAuthenticationTicket(
				1,
				userName,
				now,
				expiration,
				false,
				userData,
				_sessionSpecificCookieDataProviderSettings.AuthenticationCookiePath);
			return ticket;
		}

		private void handleSlidingExpiration(HttpCookie cookie, FormsAuthenticationTicket ticket)
		{
			if (!_sessionSpecificCookieDataProviderSettings.AuthenticationCookieSlidingExpiration) return;

			var newTicket = FormsAuthentication.RenewTicketIfOld(ticket);
			if (newTicket != null)
				cookie.Value = encryptTicket(newTicket);
		}

		private static FormsAuthenticationTicket decryptCookie(HttpCookie cookie)
		{
			FormsAuthenticationTicket ticket;
			try
			{
				ticket = FormsAuthentication.Decrypt(cookie.Value);
			}
			catch (CryptographicException)
			{
				ticket = null;
			}
			return ticket;
		}

		private static string encryptTicket(FormsAuthenticationTicket ticket) { return FormsAuthentication.Encrypt(ticket); }
	}
}