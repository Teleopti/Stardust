using System;
using System.Web;
using System.Web.Security;
using Microsoft.IdentityModel.Web;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Core
{
	public class FormsAuthenticationWrapper : IFormsAuthentication
	{
		private readonly ICurrentHttpContext _httpContext;

		public FormsAuthenticationWrapper(ICurrentHttpContext httpContext)
		{
			_httpContext = httpContext;
		}

		public void SetAuthCookie(string userName)
		{
			FormsAuthentication.SetAuthCookie(userName, false);
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
						return true;
					}
				}
				catch (Exception)
				{
				}
			}

			userName = null;
			return false;
		}
	}
}