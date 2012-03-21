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

		public SessionSpecificCookieDataProvider(HttpContextBase httpContext,
		                                         ISessionSpecificCookieDataProviderSettings sessionSpecificCookieDataProviderSettings,
		                                         INow now)
		{
			_httpContext = httpContext;
			_sessionSpecificCookieDataProviderSettings = sessionSpecificCookieDataProviderSettings;
			_now = now;
		}

		public void Store(SessionSpecificData data)
		{
			var userData = sessionSpecificDataStringSerializer.Serialize(data);
			var userName = data.PersonId.ToString();

			var cookie = MakeCookie(userName, _now.Time, userData);

			SetCookie(cookie);
		}

		public SessionSpecificData Grab()
		{
			var cookie = GetCookie();
			if (cookie == null || string.IsNullOrEmpty(cookie.Value))
			{
				return null;
			}

			var ticket = DecryptCookie(cookie);
			var userData = string.Empty;
			if (ticket == null || ticket.Expired)
			{
			}
			else
			{
				userData = ticket.UserData;
				HandleSlidingExpiration(cookie, ticket);
			}

			SetCookie(cookie);
			return sessionSpecificDataStringSerializer.Deserialize(userData);
		}

		public void ExpireCookie()
		{
			var cookie = GetCookie();
			var ticket = DecryptCookie(cookie);
			ticket = MakeTicket(ticket.Name, _now.Time, ticket.UserData, DateTime.Now.AddSeconds(-1));
			cookie.Value = EncryptTicket(ticket);
			SetCookie(cookie);
		}

		private HttpCookie GetCookie()
		{
			return _httpContext.Request.Cookies[_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName];
		}

		private void SetCookie(HttpCookie cookie)
		{
			_httpContext.Response.Cookies.Remove(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName);
			_httpContext.Response.Cookies.Add(cookie);
		}

		private HttpCookie MakeCookie(string userName, DateTime now, string userData)
		{
			var ticket = MakeTicket(userName, now, userData);

			var encryptedTicket = EncryptTicket(ticket);

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
			return cookie;
		}

		private FormsAuthenticationTicket MakeTicket(string userName, DateTime now, string userData)
		{
			return MakeTicket(
				userName,
				now,
				userData,
				now.Add(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieExpirationTimeSpan)
				);
		}
		private FormsAuthenticationTicket MakeTicket(string userName, DateTime now, string userData, DateTime expiration)
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

		private void HandleSlidingExpiration(HttpCookie cookie, FormsAuthenticationTicket ticket)
		{
			if (!_sessionSpecificCookieDataProviderSettings.AuthenticationCookieSlidingExpiration) return;

			var newTicket = FormsAuthentication.RenewTicketIfOld(ticket);
			if (newTicket != null)
				cookie.Value = EncryptTicket(newTicket);
		}

		private static FormsAuthenticationTicket DecryptCookie(HttpCookie cookie)
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

		private static string EncryptTicket(FormsAuthenticationTicket ticket) { return FormsAuthentication.Encrypt(ticket); }

		private static class sessionSpecificDataStringSerializer
		{
			public static string Serialize(SessionSpecificData data)
			{
				return string.Format("{0}{1:N}{2:N}{3}", (int)data.AuthenticationType, data.BusinessUnitId, data.PersonId, data.DataSourceName);
			}

			public static SessionSpecificData Deserialize(string stringData)
			{
				if (string.IsNullOrEmpty(stringData) || stringData.Length <= 64)
					return null;

				var authType = (AuthenticationTypeOption)Enum.Parse(typeof (AuthenticationTypeOption), stringData.Substring(0, 1));
				return new SessionSpecificData(Guid.ParseExact(stringData.Substring(1, 32), "N"),
														 stringData.Substring(65),
				                               Guid.ParseExact(stringData.Substring(33, 32), "N"),
														 authType);
			}
		}
	}
}