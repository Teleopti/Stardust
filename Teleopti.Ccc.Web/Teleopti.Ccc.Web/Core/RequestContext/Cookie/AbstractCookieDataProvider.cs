using System;
using System.IdentityModel.Services;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public abstract class AbstractCookieDataProvider : IBaseSessionSpecificDataProvider
	{
		private readonly ICurrentHttpContext _httpContext;
		private readonly ISessionSpecificCookieSettings _sessionSpecificCookieDataProviderSettings;
		private readonly INow _now;
		private readonly ISessionSpecificDataStringSerializer _dataStringSerializer;

		protected AbstractCookieDataProvider(ICurrentHttpContext httpContext, ISessionSpecificCookieSettings sessionSpecificCookieDataProviderSettings, INow now, ISessionSpecificDataStringSerializer dataStringSerializer)
		{
			_httpContext = httpContext;
			_sessionSpecificCookieDataProviderSettings = sessionSpecificCookieDataProviderSettings;
			_now = now;
			_dataStringSerializer = dataStringSerializer;
		}

		public void StoreInCookie(SessionSpecificData data, bool isPersistent, bool isLogonFromBrowser)
		{
			var userData = _dataStringSerializer.Serialize(data);
			var userName = data.PersonId.ToString();

			MakeCookie(userName, userData, isPersistent, isLogonFromBrowser);
		}

		public SessionSpecificData GrabFromCookie()
		{
			var cookie = getCookie();
			if (string.IsNullOrEmpty(cookie?.Value))
			{
				return null;
			}

			var ticket = decryptCookie(cookie);
			var userData = string.Empty;

			if (ticket != null)
			{
				if (!ticketExpired(ticket))
				{
					userData = ticket.UserData;
					handleSlidingExpiration(cookie, ticket);
				}
			}

			return _dataStringSerializer.Deserialize(userData);
		}

		private bool ticketExpired(FormsAuthenticationTicket ticket)
		{
			return _now.UtcDateTime() > ticket.Expiration.ToUniversalTime();
		}

		public void ExpireTicket()
		{
			var cookie = getCookie();
			var ticket = decryptCookie(cookie);
			ticket = makeTicket(ticket.Name, ticket.UserData, _now.ServerDateTime_DontUse().AddHours(-1));
			cookie.HttpOnly = true;
			cookie.Value = encryptTicket(ticket);
			setCookie(cookie);
		}

		public void RemoveAuthBridgeCookie()
		{
			FederatedAuthentication.SessionAuthenticationModule.DeleteSessionTokenCookie();
		}

		public void RemoveCookie()
		{
			var cookie = new HttpCookie(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName) { Expires = _now.ServerDateTime_DontUse().AddYears(-2), HttpOnly = true };
			setCookie(cookie);
			RemoveAuthBridgeCookie();
		}

		private HttpCookie getCookie()
		{
			return _httpContext.Current().Request.Cookies[_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName];
		}

		private void setCookie(HttpCookie cookie)
		{
			var httpCookieCollection = _httpContext.Current().Response.Cookies;
			httpCookieCollection.Remove(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName);
			httpCookieCollection.Add(cookie);
		}

		public void MakeCookie(string userName, string userData, bool isPersistent, bool isLogonFromBrowser)
		{
			var ticket = makeTicket(userName, userData, isPersistent);

			var encryptedTicket = encryptTicket(ticket);

			var cookie = new HttpCookie(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName, encryptedTicket)
			{
				HttpOnly = true,
				Secure = _sessionSpecificCookieDataProviderSettings.AuthenticationRequireSsl,
				Path = _sessionSpecificCookieDataProviderSettings.AuthenticationCookiePath
			};
			if (isPersistent && isLogonFromBrowser)
				cookie.Expires = _now.ServerDateTime_DontUse().Add(TimeSpan.FromDays(5000));

			if (!string.IsNullOrEmpty(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieDomain))
			{
				cookie.Domain = _sessionSpecificCookieDataProviderSettings.AuthenticationCookieDomain;
			}
			setCookie(cookie);
		}

		private FormsAuthenticationTicket makeTicket(string userName, string userData, bool isPersistent)
		{
			return makeTicket(
				userName,
				userData,
				_now.UtcDateTime()
					.Add(isPersistent
						? _sessionSpecificCookieDataProviderSettings.AuthenticationCookieExpirationTimeSpanLong
						: _sessionSpecificCookieDataProviderSettings.AuthenticationCookieExpirationTimeSpan).ToLocalTime()
				);
		}

		private FormsAuthenticationTicket makeTicket(string userName, string userData, DateTime expiration)
		{
			var ticket = new FormsAuthenticationTicket(
				1,
				userName,
				_now.ServerDateTime_DontUse(),
				expiration,
				false,
				userData,
				_sessionSpecificCookieDataProviderSettings.AuthenticationCookiePath);
			return ticket;
		}


		private void handleSlidingExpiration(HttpCookie cookie, FormsAuthenticationTicket ticket)
		{
			if (!_sessionSpecificCookieDataProviderSettings.AuthenticationCookieSlidingExpiration) return;

			if (!timeToRenewTicket(ticket)) return;

			var newTicket = makeTicket(ticket.Name, ticket.UserData, false);
			cookie.Value = encryptTicket(newTicket);
			cookie.HttpOnly = true;
			setCookie(cookie);
		}

		private bool timeToRenewTicket(FormsAuthenticationTicket ticket)
		{
			var ticketAge = _now.ServerDateTime_DontUse() - ticket.IssueDate;
			var expiresIn = ticket.Expiration - _now.ServerDateTime_DontUse();
			var renew = ticketAge > expiresIn;
			return renew;
		}

		private static FormsAuthenticationTicket decryptCookie(HttpCookie cookie)
		{
			FormsAuthenticationTicket ticket;
			try
			{
				ticket = FormsAuthentication.Decrypt(cookie.Value);
			}
			catch(HttpException)
			{
				ticket = null;
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