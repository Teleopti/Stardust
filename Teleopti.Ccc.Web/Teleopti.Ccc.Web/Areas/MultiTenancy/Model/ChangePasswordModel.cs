namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Model
{
	public class ChangePasswordModel
	{
		public string UserName { get; set; }
		public string OldPassword { get; set; }
		public string NewPassword { get; set; }
	}
}