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

			var cookie = createCookie(userName, _now.Time, userData);

			attachCookieToResposne(cookie);
		}

		private void attachCookieToResposne(HttpCookie cookie)
		{
			_httpContext.Response.Cookies.Remove(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName);
			_httpContext.Response.Cookies.Add(cookie);
		}

		public SessionSpecificData Grab()
		{
			var authCookie = _httpContext.Request.Cookies[_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName];
			if (authCookie == null || String.IsNullOrEmpty(authCookie.Value))
			{
				return null;
			}
			string userData = getUserData(authCookie);
			attachCookieToResposne(authCookie);
			return sessionSpecificDataStringSerializer.Deserialize(userData);
		}


		private HttpCookie createCookie(string userName, DateTime now, string userData)
		{
			var ticket = new FormsAuthenticationTicket(
				1,
				userName,
				now,
				now.Add(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieExpirationTimeSpan),
				false,
				userData,
				_sessionSpecificCookieDataProviderSettings.AuthenticationCookiePath);

			var encryptedTicket = FormsAuthentication.Encrypt(ticket);

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

		private string getUserData(HttpCookie authCookie)
		{
		    FormsAuthenticationTicket ticket;
		    try
		    {
		        ticket = FormsAuthentication.Decrypt(authCookie.Value);
		    }
		    catch (CryptographicException)
		    {
		        ticket = null;
		    }

			if (ticket == null || ticket.Expired) return string.Empty;

			if (_sessionSpecificCookieDataProviderSettings.AuthenticationCookieSlidingExpiration)
			{
				var newTicket = FormsAuthentication.RenewTicketIfOld(ticket);
				if (newTicket != null)
				{
					authCookie.Value = FormsAuthentication.Encrypt(newTicket);
				}
			}

			return ticket.UserData;
		}

		private static class sessionSpecificDataStringSerializer
		{
			public static string Serialize(SessionSpecificData data)
			{
				return string.Format("{0:N}{1:N}{2}", data.BusinessUnitId, data.PersonId, data.DataSourceName);
			}

			public static SessionSpecificData Deserialize(string stringData)
			{
				if (string.IsNullOrEmpty(stringData) || stringData.Length <= 64)
					return null;
				return new SessionSpecificData(Guid.ParseExact(stringData.Substring(0, 32), "N"),
				                               stringData.Substring(64),
				                               Guid.ParseExact(stringData.Substring(32, 32), "N"));
			}
		}
	}
}