namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAuthenticator
	{
		string TenantForKey(string authenticationKey);
	}
}