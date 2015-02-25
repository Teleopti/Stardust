namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class ChangePasswordInput
	{
		public string OldPassword { get; set; }
		public string NewPassword { get; set; }
		public string UserName { get; set; }
	}
}