using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ConfiguredKeyAuthenticator : IAuthenticator
	{
		private readonly string _authenticationKey;

		public ConfiguredKeyAuthenticator(IConfigReader configReader)
		{
			_authenticationKey = configReader.AppConfig("AuthenticationKey");
			if (string.IsNullOrEmpty(_authenticationKey))
				_authenticationKey = Rta.LegacyAuthenticationKey;
		}

		public string TenantForKey(string authenticationKey)
		{
			return authenticationKey == _authenticationKey ? "ok" : null;
		}
	}
}