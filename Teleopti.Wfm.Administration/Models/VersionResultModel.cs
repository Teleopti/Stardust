namespace Teleopti.Wfm.Administration.Models
{
	public class VersionResultModel
	{
		public int HeadVersion { get; set; }
		public int ImportAppVersion { get; set; }
		public bool AppVersionOk { get; set; }
		public string Error { get; set; }
	}
}