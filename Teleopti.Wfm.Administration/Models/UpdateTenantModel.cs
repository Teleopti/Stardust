namespace Teleopti.Wfm.Administration.Models
{
	public class UpdateTenantModel
	{
		public string OriginalName { get; set; }
		public string NewName { get; set; }
		public string AppDatabase { get; set; }
		public string AnalyticsDatabase { get; set; }
	}
}