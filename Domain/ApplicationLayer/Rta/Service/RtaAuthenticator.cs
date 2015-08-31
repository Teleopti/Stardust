using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaAuthenticator : IRtaAuthenticator
	{
		private readonly string _authenticationKey;

		public RtaAuthenticator(IConfigReader configReader)
		{
			_authenticationKey = configReader.AppConfig("AuthenticationKey");
			if (string.IsNullOrEmpty(_authenticationKey))
				_authenticationKey = Rta.LegacyAuthenticationKey;
		}

		public string Autenticate(string authenticationKey)
		{
			return authenticationKey == _authenticationKey ? "ok" : null;
		}
	}
}