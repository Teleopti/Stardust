using System;

namespace Teleopti.Wfm.Azure.Common
{
	public static class InstallationEnvironment
	{
		public static bool IsAzure => bool.TryParse(Environment.GetEnvironmentVariable("TeleoptiIsAzure"), out var result) && result;
	}
}
