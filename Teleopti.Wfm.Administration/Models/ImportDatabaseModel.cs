namespace Teleopti.Wfm.Administration.Models
{
	public class ImportDatabaseModel
	{
		public string Tenant { get; set; }
		public string ConnStringAppDatabase { get; set; }
		public string ConnStringAnalyticsDatabase { get; set; }
	}
}