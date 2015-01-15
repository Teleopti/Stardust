namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public interface IApplicationAuthentication
	{
		IdentificationResult TryAuthenticate(string userName, string password);
	}
}