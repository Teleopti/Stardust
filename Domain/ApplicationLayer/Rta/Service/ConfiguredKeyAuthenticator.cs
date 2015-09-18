using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ConfiguredKeyAuthenticator : IAuthenticator
	{
		public static string LegacyAuthenticationKey = "!#¤atAbgT%";

		private readonly string _appliedKey;

		public ConfiguredKeyAuthenticator(IConfigReader configReader)
		{
			_appliedKey = configReader.AppConfig("AuthenticationKey");
			if (string.IsNullOrEmpty(_appliedKey))
				_appliedKey = LegacyAuthenticationKey;
			_appliedKey = MakeLegacyKeyEncodingSafe(_appliedKey);
		}

		public bool Authenticate(string authenticationKey)
		{
			return authenticationKey == _appliedKey;
		}

		public static string MakeLegacyKeyEncodingSafe(string authenticationKey)
		{
			if (authenticationKey.StartsWith("!#") && authenticationKey.EndsWith("tAbgT%"))
				return "!#?atAbgT%";
			return authenticationKey;
		}
	}
}