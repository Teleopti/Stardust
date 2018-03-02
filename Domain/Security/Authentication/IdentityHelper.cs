using System;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public static class IdentityHelper
	{
		private const char identitySplitter = '\\';

		// This function is used only once in test, should it moved to test project?
		public static string Merge(string domainName, string userName)
		{
			var trimmedDomainName = domainName?.Trim() ?? string.Empty;
			var trimmedUserName = userName?.Trim() ?? string.Empty;
			return !string.IsNullOrEmpty(trimmedDomainName)
				? string.Format("{0}" + identitySplitter + "{1}", trimmedDomainName, trimmedUserName)
				: trimmedUserName;
		}

		public static Tuple<string, string> Split(string identity)
		{
			if (string.IsNullOrEmpty(identity))
			{
				return new Tuple<string, string>(string.Empty, string.Empty);
			}

			string logOn;
			string domain;

			var parts = identity.Split(identitySplitter);
			if (parts.Length > 1)
			{
				domain = parts[0];
				logOn = parts[1];
			}
			else
			{
				domain = string.Empty;
				logOn = parts[0];
			}

			return new Tuple<string, string>(domain.Trim(), logOn.Trim());
		}
	}
}