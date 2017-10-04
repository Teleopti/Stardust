namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public interface IBaseSessionSpecificDataProvider
	{
		void StoreInCookie(SessionSpecificData data, bool isPersistent, bool isLogonFromBrowser, string tenantName);
		SessionSpecificData GrabFromCookie();
		void ExpireTicket();
		void MakeCookie(string userName, string userData, bool isPersistent, bool isLogonFromBrowser, string tenantName);
		void RemoveCookie();
		void RemoveAuthBridgeCookie();
	}

	public interface ISessionSpecificWfmCookieProvider : IBaseSessionSpecificDataProvider
	{
	}

	public interface ISessionSpecificTeleoptiCookieProvider : IBaseSessionSpecificDataProvider
	{
	}
}
