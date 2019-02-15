namespace Teleopti.Wfm.Azure.Common
{
	public class FakeInstallationEnvironment : IInstallationEnvironment
	{
		public bool IsAzure { get; set; }
		public int RoleInstanceID { get; set; }
	}
}
