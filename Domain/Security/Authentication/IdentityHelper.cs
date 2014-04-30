using System;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public static class IdentityHelper
	{
		public static string Merge(string domainName, string userName)
		{
			if (string.IsNullOrEmpty(domainName)) return userName;
			return domainName + "\\" + userName;
		}

		public static Tuple<string, string> Split(string identity)
		{
			var logOn = string.Empty;
			var domain = string.Empty;
			if (!string.IsNullOrEmpty(identity))
			{
				var parts = identity.Split('\\');
				if (parts.Length > 1)
				{
					domain = parts[0];
					logOn = parts[1];
				}
				else
				{
					logOn = parts[0];
				}
			}
			return new Tuple<string, string>(domain,logOn);
		}
	}
}