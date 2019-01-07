namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Settings
{
	public class ChangePasswordResultInfo
	{
		public bool IsSuccessful { get; set; }
		public bool IsAuthenticationSuccessful { get; set; }
		public string ErrorCode { get; set; }
	}
}