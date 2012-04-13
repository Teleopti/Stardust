using System;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public interface ISessionSpecificDataProvider
	{
		void Store(SessionSpecificData data);
		SessionSpecificData Grab();
		void ExpireCookie();
		void MakeCookie(string userName, DateTime now, string userData);
	}
}
