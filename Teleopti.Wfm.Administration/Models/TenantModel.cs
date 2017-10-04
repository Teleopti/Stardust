namespace Teleopti.Wfm.Administration.Models
{
	public class TenantModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string AppDatabase { get; set; }
		public string AnalyticsDatabase { get; set; }
		public int CommandTimeout { get; set; }
		public string Server { get; set; }
		public string AggregationDatabase { get; set; }
		public VersionResultModel Version { get; set; }
		public bool Active { get; set; }
		public bool UseIntegratedSecurity { get; set; }
		public string MobileQRCodeUrl { get; set; }
		public int MaximumSessionTime { get; set; }
	}
}