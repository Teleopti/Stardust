namespace Teleopti.Wfm.Administration.Models
{
	public class TenantModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string AppDatabase { get; set; }
		public string AnalyticsDatabase { get; set; }
	}
}