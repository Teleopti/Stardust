using System;

namespace Teleopti.Wfm.Azure.Common
{
	public interface IInstallationEnvironment
	{
		bool IsAzure { get; set; }
		int RoleInstanceID { get; set; }
	}
}
