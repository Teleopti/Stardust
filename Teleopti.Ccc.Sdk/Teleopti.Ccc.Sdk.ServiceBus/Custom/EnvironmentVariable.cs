using System;

namespace Teleopti.Ccc.Sdk.ServiceBus.Custom
{
	public class EnvironmentVariable: IEnvironmentVariable
	{
		public string GetValue(string environmentKey)
		{
			return Environment.GetEnvironmentVariable(environmentKey, EnvironmentVariableTarget.Machine) ??
				   Environment.GetEnvironmentVariable(environmentKey, EnvironmentVariableTarget.User) ??
				   Environment.GetEnvironmentVariable(environmentKey, EnvironmentVariableTarget.Process) ??
				   string.Empty;
		}
	}
}
