namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IAuthenticationTenantClient
	{
		AuthenticationQuerierResult TryLogon(ApplicationLogonClientModel applicationLogonClientModel, string userAgent);
		AuthenticationQuerierResult TryLogon(IdentityLogonClientModel identityLogonClientModel, string userAgent);
		AuthenticationQuerierResult TryLogon(IdLogonClientModel idLogonClientModel, string userAgent);
	}
}