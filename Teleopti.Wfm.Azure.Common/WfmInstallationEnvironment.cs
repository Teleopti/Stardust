using System;

namespace Teleopti.Wfm.Azure.Common
{
	public class WfmInstallationEnvironment : IInstallationEnvironment
	{
		public bool IsAzure
		{
			get => bool.TryParse(Environment.GetEnvironmentVariable("TeleoptiIsAzure"), out var result) && result;
			set { }
		}

		public int RoleInstanceID
		{
			get => int.TryParse(Environment.GetEnvironmentVariable("RoleInstanceID"), out var result) ? result : -1;
			set { }
		}
	}
}
