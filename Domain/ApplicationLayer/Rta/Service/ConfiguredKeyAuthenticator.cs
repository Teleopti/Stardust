using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ConfiguredKeyAuthenticator : IAuthenticator
	{
		public static string LegacyAuthenticationKey = "!#¤atAbgT%";

		private readonly string _authenticationKey;

		public ConfiguredKeyAuthenticator(IConfigReader configReader)
		{
			_authenticationKey = configReader.AppConfig("AuthenticationKey");
			if (string.IsNullOrEmpty(_authenticationKey))
				_authenticationKey = LegacyAuthenticationKey;
		}

		public bool Authenticate(string authenticationKey)
		{
			return authenticationKey == _authenticationKey;
		}
	}
}