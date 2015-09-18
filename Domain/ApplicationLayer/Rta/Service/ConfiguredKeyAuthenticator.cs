using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ConfiguredKeyAuthenticator : IAuthenticator
	{
		public static string InputLegacyAuthenticationKey = "!#¤atAbgT%";
		public static string InternalEncodingFixedLegacyAuthenticationKey = "!#?atAbgT%";

		private readonly string _authenticationKey;

		public ConfiguredKeyAuthenticator(IConfigReader configReader)
		{
			_authenticationKey = configReader.AppConfig("AuthenticationKey");
			if (string.IsNullOrEmpty(_authenticationKey))
				_authenticationKey = InternalEncodingFixedLegacyAuthenticationKey;
		}

		public bool Authenticate(string authenticationKey)
		{
			return authenticationKey == _authenticationKey;
		}

		public static string MakeLegacyKeySafe(string authenticationKey)
		{
			if (authenticationKey.StartsWith("!#") && authenticationKey.EndsWith("tAbgT%"))
				return InternalEncodingFixedLegacyAuthenticationKey;
			return authenticationKey;
		}
	}
}