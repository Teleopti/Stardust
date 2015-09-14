namespace Teleopti.Wfm.Administration.Models
{
	public class TenantModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string AppDatabase { get; set; }
		public string AnalyticsDatabase { get; set; }
		public int CommandTimeout { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Server { get; set; }
		public string AggregationDatabase { get; set; }
		public VersionResultModel Version { get; set; }
	}
}