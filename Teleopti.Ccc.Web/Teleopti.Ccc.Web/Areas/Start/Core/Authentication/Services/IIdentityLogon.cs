namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	//TODO: tenant - these two methods could be one I think. We should keep also app logon credentials in same auth table as identity
	public interface IIdentityLogon
	{
		AuthenticateResult LogonWindowsUser();
		//TODO: tenant, remove datasourceName parameter here when toggle is removed
		AuthenticateResult LogonApplicationIdentityUser(string dataSourceName);
	}
}
