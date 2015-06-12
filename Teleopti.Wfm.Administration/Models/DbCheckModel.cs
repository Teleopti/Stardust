namespace Teleopti.Wfm.Administration.Models
{
	public class DbCheckModel
	{
		public string DbConnectionString { get; set; }
		//1 = app 2 = analytics
		public int DbType { get; set; }
	}
}