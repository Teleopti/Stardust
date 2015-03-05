namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IAuthenticationQuerier
	{
		AuthenticationQueryResult TryApplicationLogon(string userName, string password, string userAgent);
		AuthenticationQueryResult TryIdentityLogon(string identity, string userAgent);
	}
}