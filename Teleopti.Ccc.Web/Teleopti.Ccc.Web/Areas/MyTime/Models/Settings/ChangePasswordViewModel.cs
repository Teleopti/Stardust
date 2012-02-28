namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Settings
{
	public class ChangePasswordViewModel
	{
		//[Required(ErrorMessageResourceName = "PasswordRequired", ErrorMessageResourceType = typeof(Resources))]
		//[DataType(DataType.Password)]
		//[Display(Name = "Password", Prompt = "Password")]
		public string OldPassword { get; set; }

		//[Required(ErrorMessageResourceName = "PasswordRequired", ErrorMessageResourceType = typeof(Resources))]
		//[DataType(DataType.Password)]
		//[Display(Name = "Password", Prompt = "Password")]
		public string NewPassword { get; set; }
	}
}