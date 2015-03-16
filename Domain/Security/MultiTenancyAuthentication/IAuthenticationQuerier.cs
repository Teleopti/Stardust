namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IAuthenticationQuerier
	{
		AuthenticationQueryResult TryApplicationLogon(ApplicationLogonClientModel applicationLogonClientModel, string userAgent);
		AuthenticationQueryResult TryIdentityLogon(IdentityLogonClientModel identityLogonClientModel, string userAgent);
	}
}