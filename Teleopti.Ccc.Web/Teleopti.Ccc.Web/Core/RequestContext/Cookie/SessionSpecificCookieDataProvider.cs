﻿using System;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificCookieDataProvider : ISessionSpecificDataProvider
	{
		private readonly ICurrentHttpContext _httpContext;
		private readonly ISessionSpecificCookieDataProviderSettings _sessionSpecificCookieDataProviderSettings;
		private readonly INow _now;
		private readonly ISessionSpecificDataStringSerializer _dataStringSerializer;

		public SessionSpecificCookieDataProvider(ICurrentHttpContext httpContext,
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

		public void StoreInCookie(SessionSpecificData data)
		{
			var userData = _dataStringSerializer.Serialize(data);
			var userName = data.PersonId.ToString();

			MakeCookie(userName, userData);
		}

		public SessionSpecificData GrabFromCookie()
		{
			var cookie = getCookie();
			if (cookie == null || string.IsNullOrEmpty(cookie.Value))
			{
				return null;
			}

			var ticket = decryptCookie(cookie);
			var userData = string.Empty;

			if (ticket != null)
			{
				if (!tickedExpired(ticket))
				{
					userData = ticket.UserData;
					handleSlidingExpiration(cookie, ticket);
				}
			}

			return _dataStringSerializer.Deserialize(userData);
		}

		private bool tickedExpired(FormsAuthenticationTicket ticket)
		{
			return _now.UtcDateTime() > ticket.Expiration.ToUniversalTime();
		}

		public void ExpireTicket()
		{
			var cookie = new HttpCookie(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName) { Expires = DateTime.Now.AddDays(-1d) };
			_httpContext.Current().Response.Cookies.Add(cookie);

			var fedAuthCookie = new HttpCookie("FedAuth") { Expires = DateTime.Now.AddDays(-1d) };
			_httpContext.Current().Response.Cookies.Add(fedAuthCookie);
		}

		public void RemoveCookie()
		{
			var cookie = new HttpCookie(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName)
			             	{Expires = DateTime.Now.AddYears(-2)};
			setCookie(cookie);
		}

		private HttpCookie getCookie()
		{
			return _httpContext.Current().Request.Cookies[_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName];
		}

		private void setCookie(HttpCookie cookie)
		{
			_httpContext.Current().Response.Cookies.Remove(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName);
			_httpContext.Current().Response.Cookies.Add(cookie);
		}

		public void MakeCookie(string userName, string userData)
		{
			var ticket = makeTicket(userName, userData);

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

		private FormsAuthenticationTicket makeTicket(string userName, string userData)
		{
			return makeTicket(
				userName,
				userData,
				_now.LocalDateTime().Add(_sessionSpecificCookieDataProviderSettings.AuthenticationCookieExpirationTimeSpan)
				);
		}

		private FormsAuthenticationTicket makeTicket(FormsAuthenticationTicket ticket)
		{
			return makeTicket(
				ticket.Name,
				ticket.UserData)
				;
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
				_sessionSpecificCookieDataProviderSettings.AuthenticationCookiePath);
			return ticket;
		}


		private void handleSlidingExpiration(HttpCookie cookie, FormsAuthenticationTicket ticket)
		{
			if (!_sessionSpecificCookieDataProviderSettings.AuthenticationCookieSlidingExpiration) return;

			if (!timeToRenewTicket(ticket)) return;

			var newTicket = makeTicket(ticket);
			cookie.Value = encryptTicket(newTicket);
			setCookie(cookie);
		}

		private bool timeToRenewTicket(FormsAuthenticationTicket ticket)
		{
			var ticketAge = _now.LocalDateTime() - ticket.IssueDate;
			var expiresIn = ticket.Expiration - _now.LocalDateTime();
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