using System;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public interface ISessionSpecificDataProvider
	{
		void StoreInCookie(SessionSpecificData data);
		SessionSpecificData GrabFromCookie();
		void ExpireTicket();
		void MakeCookie(string userName, string userData);
		void RemoveCookie();
	}
}
