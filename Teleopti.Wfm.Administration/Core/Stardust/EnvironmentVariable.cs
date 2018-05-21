using System;

namespace Teleopti.Wfm.Administration.Core.Stardust
{
	public class EnvironmentVariable
	{
		public static string GetValue(string environmentKey)
		{
			return Environment.GetEnvironmentVariable(environmentKey, EnvironmentVariableTarget.Machine) ??
				   Environment.GetEnvironmentVariable(environmentKey, EnvironmentVariableTarget.User) ??
				   Environment.GetEnvironmentVariable(environmentKey, EnvironmentVariableTarget.Process) ??
				   string.Empty;
		}
	}
}
