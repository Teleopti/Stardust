namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class TenantAuthenticator : IAuthenticator
	{
		private readonly IDatabaseLoader _databaseLoader;

		public TenantAuthenticator(IDatabaseLoader databaseLoader)
		{
			_databaseLoader = databaseLoader;
		}

		public bool Authenticate(string authenticationKey)
		{
			return _databaseLoader.AuthenticateKey(authenticationKey);
		}
	}
}