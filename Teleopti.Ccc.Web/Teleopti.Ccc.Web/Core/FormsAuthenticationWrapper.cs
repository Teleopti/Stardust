using System;
using System.Web;
using System.Web.Security;
using Common.Logging;
using Microsoft.IdentityModel.Web;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Core
{
	public class FormsAuthenticationWrapper : IFormsAuthentication
	{
		private ILog _logger = LogManager.GetLogger<FormsAuthenticationWrapper>();
		private readonly ICurrentHttpContext _httpContext;
		private readonly ISessionSpecificForIdentityProviderDataProvider _sessionSpecificForIdentityProviderDataProvider;

		public FormsAuthenticationWrapper(ICurrentHttpContext httpContext, ISessionSpecificForIdentityProviderDataProvider sessionSpecificForIdentityProviderDataProvider)
		{
			_httpContext = httpContext;
			_sessionSpecificForIdentityProviderDataProvider = sessionSpecificForIdentityProviderDataProvider;
		}

		public void SetAuthCookie(string userName)
		{
			_sessionSpecificForIdentityProviderDataProvider.MakeCookie(userName, userName);
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