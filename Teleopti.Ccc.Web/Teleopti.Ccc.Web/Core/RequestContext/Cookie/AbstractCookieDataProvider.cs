using System;
using System.IdentityModel.Services;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public abstract class AbstractCookieDataProvider : IBaseSessionSpecificDataProvider
	{
		private readonly ICurrentHttpContext _httpContext;
		private readonly ISessionSpecificCookieSettings _sessionSpecificCookieDataProviderSettings;
		private readonly INow _now;
		private readonly ISessionSpecificDataStringSerializer _dataStringSerializer;
		private readonly MaximumSessionTimeProvider _maximumSessionTimeProvider;

		protected AbstractCookieDataProvider(ICurrentHttpContext httpContext, ISessionSpecificCookieSettings sessionSpecificCookieDataProviderSettings, INow now, ISessionSpecificDataStringSerializer dataStringSerializer, MaximumSessionTimeProvider maximumSessionTimeProvider)
		{
			_httpContext = httpContext;
			_sessionSpecificCookieDataProviderSettings = sessionSpecificCookieDataProviderSettings;
			_now = now;
			_dataStringSerializer = dataStringSerializer;
			_maximumSessionTimeProvider = maximumSessionTimeProvider;
		}

		public void StoreInCookie(SessionSpecificData data, bool isPersistent, bool isLogonFromBrowser, string tenantName)
		{
			var userData = _dataStringSerializer.Serialize(data);
			var userName = data.PersonId.ToString();

			MakeCookie(userName, userData, isPersistent, isLogonFromBrowser, tenantName);
		}

		public SessionSpecificData GrabFromCookie()
		{
			var cookie = GetCookie();
			if (string.IsNullOrEmpty(cookie?.Value))
			{
				return null;
			}

			var ticket = DecryptCookie(cookie);
			SessionSpecificData sessionSpecificData = null;

			if (ticket != null)
			{
				if (!ticketExpired(ticket))
				{
					sessionSpecificData = _dataStringSerializer.Deserialize(ticket.UserData);
					handleSlidingExpiration(cookie, ticket, sessionSpecificData.DataSourceName);
				}
			}

			return sessionSpecificData;
		}

		private bool ticketExpired(FormsAuthenticationTicket ticket)
		{
			return _now.UtcDateTime() > ticket.Expiration.ToUniversalTime();
		}

		public void ExpireTicket()
		{
			var cookie = GetCookie();
			var ticket = DecryptCookie(cookie);
			ticket = makeTicket(ticket.Name, ticket.UserData, _now.ServerDateTime_DontUse().AddHours(-1));
			cookie.HttpOnly = true;
			cookie.Secure = _sessionSpecificCookieDataProviderSettings.AuthenticationRequireSsl;
			cookie.Value = encryptTicket(ticket);
			setCookie(cookie);
		}

		public void RemoveAuthBridgeCookie()
		{
			FederatedAuthentication.SessionAuthenticationModule.DeleteSessionTokenCookie();
		}

		public void RemoveCookie()
		{
			var cookie = new HttpCookie(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName) { Expires = _now.ServerDateTime_DontUse().AddYears(-2), HttpOnly = true, Secure = _sessionSpecificCookieDataProviderSettings.AuthenticationRequireSsl};
			setCookie(cookie);
			RemoveAuthBridgeCookie();
		}

		protected HttpCookie GetCookie()
		{
			return _httpContext.Current().Request.Cookies[_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName];
		}

		private void setCookie(HttpCookie cookie)
		{
			var httpCookieCollection = _httpContext.Current().Response.Cookies;
			httpCookieCollection.Remove(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName);
			httpCookieCollection.Add(cookie);
		}

		public void MakeCookie(string userName, string userData, bool isPersistent, bool isLogonFromBrowser, string tenantName)
		{
			var ticket = makeTicket(userName, userData, isPersistent, tenantName);

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

		private FormsAuthenticationTicket makeTicket(string userName, string userData, bool isPersistent, string tenantName)
		{
			var maximumSessionTimeInMinutes = _maximumSessionTimeProvider.ForTenant(tenantName);
			return makeTicket(userName, userData, isPersistent, maximumSessionTimeInMinutes);
		}

		private FormsAuthenticationTicket makeTicket(string userName, string userData, bool isPersistent,
			double maximumSessionTimeInMinutes)
		{
			var longTimeSpan = maximumSessionTimeInMinutes > 0
				? TimeSpan.FromMinutes(maximumSessionTimeInMinutes)
				: _sessionSpecificCookieDataProviderSettings.AuthenticationCookieExpirationTimeSpanLong;
			var shortTimeSpan = maximumSessionTimeInMinutes > 0 && maximumSessionTimeInMinutes <
								_sessionSpecificCookieDataProviderSettings.AuthenticationCookieExpirationTimeSpan.TotalMinutes
				? TimeSpan.FromMinutes(maximumSessionTimeInMinutes)
				: _sessionSpecificCookieDataProviderSettings.AuthenticationCookieExpirationTimeSpan;
			return makeTicket(
				userName,
				userData,
				_now.UtcDateTime()
					.Add(isPersistent
						? longTimeSpan
						: shortTimeSpan).ToLocalTime()
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


		private void handleSlidingExpiration(HttpCookie cookie, FormsAuthenticationTicket ticket, string tenantName)
		{
			if (!_sessionSpecificCookieDataProviderSettings.AuthenticationCookieSlidingExpiration) return;
			var maximumSessionTimeInMinutes = _maximumSessionTimeProvider.ForTenant(tenantName);
			if (maximumSessionTimeInMinutes != 0) return;

			if (!timeToRenewTicket(ticket)) return;
			
			var newTicket = makeTicket(ticket.Name, ticket.UserData, false, (ticket.Expiration - ticket.IssueDate).TotalMinutes);
			cookie.Value = encryptTicket(newTicket);
			cookie.Secure = _sessionSpecificCookieDataProviderSettings.AuthenticationRequireSsl;
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

		protected static FormsAuthenticationTicket DecryptCookie(HttpCookie cookie)
		{
			FormsAuthenticationTicket ticket;
			try
			{
				ticket = FormsAuthentication.Decrypt(cookie.Value);
			}
			catch (HttpException)
			{
				ticket = null;
			}
			catch (CryptographicException)
			{
				ticket = null;
			}
			catch (FormatException)
			{
				ticket = null;
			}
			catch (ArgumentException)
			{
				ticket = null;
			}
			return ticket;
		}

		private static string encryptTicket(FormsAuthenticationTicket ticket) { return FormsAuthentication.Encrypt(ticket); }
	}
}