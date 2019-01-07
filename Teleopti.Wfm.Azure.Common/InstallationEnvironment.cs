using System;

namespace Teleopti.Wfm.Azure.Common
{
	public static class InstallationEnvironment
	{
		public static bool IsAzure => Environment.GetEnvironmentVariable("TeleoptiIsAzure")?.ToLower() == "true";
	}
}
