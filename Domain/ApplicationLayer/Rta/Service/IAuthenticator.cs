namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAuthenticator
	{
		bool Authenticate(string authenticationKey);
	}
}