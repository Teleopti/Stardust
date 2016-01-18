namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public interface IBaseSessionSpecificDataProvider
	{
		void StoreInCookie(SessionSpecificData data, bool isPersistent);
		SessionSpecificData GrabFromCookie();
		void ExpireTicket();
		void MakeCookie(string userName, string userData, bool isPersistent);
		void RemoveCookie();
	}

	public interface ISessionSpecificDataProvider : IBaseSessionSpecificDataProvider
	{
	}

	public interface ISessionSpecificForIdentityProviderDataProvider : IBaseSessionSpecificDataProvider
	{
	}
}
