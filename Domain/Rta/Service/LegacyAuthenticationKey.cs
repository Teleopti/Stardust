namespace Teleopti.Ccc.Domain.Rta.Service
{
	public class LegacyAuthenticationKey
	{
		public static string TheKey = "!#¤atAbgT%";
		
		public static string MakeEncodingSafe(string authenticationKey)
		{
			if (authenticationKey.StartsWith("!#") && authenticationKey.EndsWith("tAbgT%"))
				return "!#?atAbgT%";
			return authenticationKey;
		}
	}
}