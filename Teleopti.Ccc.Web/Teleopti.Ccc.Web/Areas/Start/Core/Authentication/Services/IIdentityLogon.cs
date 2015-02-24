namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public interface IIdentityLogon
	{
		//TODO: tenant, remove datasourceName parameter here when toggle is removed
		AuthenticateResult LogonWindowsUser(string dataSourceName);
		//TODO: tenant, remove datasourceName parameter here when toggle is removed
		AuthenticateResult LogonApplicationIdentityUser(string dataSourceName);
	}
}
