namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public interface IIdentityLogon
	{
		AuthenticateResult LogonWindowsUser(string dataSourceName);
		AuthenticateResult LogonApplicationIdentityUser(string dataSourceName);
	}
}
