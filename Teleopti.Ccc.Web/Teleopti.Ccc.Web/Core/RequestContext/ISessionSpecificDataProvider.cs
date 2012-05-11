using System;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public interface ISessionSpecificDataProvider
	{
		void StoreInCookie(SessionSpecificData data);
		SessionSpecificData GrabFromCookie();
		void ExpireCookie();
		void MakeCookie(string userName, DateTime now, string userData);
	}
}
