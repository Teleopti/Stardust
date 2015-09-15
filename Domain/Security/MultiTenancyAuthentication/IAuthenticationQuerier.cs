namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IAuthenticationQuerier
	{
		AuthenticationQuerierResult TryLogon(ApplicationLogonClientModel applicationLogonClientModel, string userAgent);
		AuthenticationQuerierResult TryLogon(IdentityLogonClientModel identityLogonClientModel, string userAgent);
		AuthenticationQuerierResult TryLogon(IdLogonClientModel idLogonClientModel, string userAgent);
	}
}