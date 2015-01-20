namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IAuthenticationQuerier
	{
		AuthenticationQueryResult TryLogon(string userName, string password);
		AuthenticationQueryResult TryIdentityLogon(string identity);
	}
}