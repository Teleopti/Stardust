namespace Teleopti.Wfm.Administration.Models
{
	public class UpdateTenantModel
	{
		public string OriginalName { get; set; }
		public string NewName { get; set; }
		public string AppDatabase { get; set; }
		public string AnalyticsDatabase { get; set; }
		public int CommandTimeout { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Server { get; set; }
	    public bool Active { get; set; }
		public bool UseIntegratedSecurity { get; set; }
		public string MobileQRCodeUrl { get; set; }
	}
}