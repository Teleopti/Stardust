namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IRtaAuthenticator
	{
		bool Autenticate(string authenticationKey);
	}
}