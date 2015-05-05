namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IAuthenticationQuerier
	{
		AuthenticationQueryResult TryLogon(ApplicationLogonClientModel applicationLogonClientModel, string userAgent);
		AuthenticationQueryResult TryLogon(IdentityLogonClientModel identityLogonClientModel, string userAgent);
	}
}