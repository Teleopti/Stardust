using Teleopti.Ccc.UserTexts;
using System.ComponentModel.DataAnnotations;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class SignInApplicationModel
	{

		[Required(ErrorMessageResourceName = "DataSourceRequired", ErrorMessageResourceType = typeof (Resources))]
		[Display(Name = "PleaseChooseADatasource", ResourceType = typeof (Resources))]
		public string DataSourceName { get; set; }

		[Required(ErrorMessageResourceName = "UserNameRequired", ErrorMessageResourceType = typeof (Resources))]
		[Display(Name = "UserName", ResourceType = typeof (Resources))]
		public string UserName { get; set; }

		[Required(ErrorMessageResourceName = "PasswordRequired", ErrorMessageResourceType = typeof (Resources))]
		[DataType(DataType.Password)]
		[Display(Name = "Password", Prompt = "Password", ResourceType = typeof (Resources))]
		public string Password { get; set; }

	}
}