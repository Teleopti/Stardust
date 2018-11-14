namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Model
{
	public class PasswordResetModel
	{
		public string NewPassword { get; set; }
		public string ResetToken { get; set; }
	}
}