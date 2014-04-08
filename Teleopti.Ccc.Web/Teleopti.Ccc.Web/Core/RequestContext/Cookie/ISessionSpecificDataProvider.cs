namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public interface IBaseSessionSpecificDataProvider
	{
		void StoreInCookie(SessionSpecificData data);
		SessionSpecificData GrabFromCookie();
		void ExpireTicket();
		void MakeCookie(string userName, string userData);
		void RemoveCookie();
	}

	public interface ISessionSpecificDataProvider : IBaseSessionSpecificDataProvider
	{
	}

	public interface ISessionSpecificForIdentityProviderDataProvider : IBaseSessionSpecificDataProvider
	{
	}
}
