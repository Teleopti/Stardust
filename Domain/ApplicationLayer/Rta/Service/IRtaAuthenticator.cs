namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IRtaAuthenticator
	{
		string Autenticate(string authenticationKey);
	}
}