namespace Teleopti.Wfm.Api
{
	public interface ITokenVerifier
	{
		bool TryGetUser(string token, out UserIdWithTenant user);
	}
}