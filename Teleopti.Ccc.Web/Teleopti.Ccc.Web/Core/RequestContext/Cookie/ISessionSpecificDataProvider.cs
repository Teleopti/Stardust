namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public interface IBaseSessionSpecificDataProvider
	{
		void StoreInCookie(SessionSpecificData data, bool isPersistent, bool isLogonFromBrowser);
		SessionSpecificData GrabFromCookie();
		void ExpireTicket();
		void MakeCookie(string userName, string userData, bool isPersistent, bool isLogonFromBrowser);
		void RemoveCookie();
		void RemoveAuthBridgeCookie();
	}

	public interface ISessionSpecificDataProvider : IBaseSessionSpecificDataProvider
	{
	}

	public interface ISessionSpecificForIdentityProviderDataProvider : IBaseSessionSpecificDataProvider
	{
	}
}
