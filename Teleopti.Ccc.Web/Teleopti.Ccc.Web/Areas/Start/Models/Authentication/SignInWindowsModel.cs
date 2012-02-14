using Teleopti.Ccc.UserTexts;
using System.ComponentModel.DataAnnotations;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class SignInWindowsModel
    {
        [Required(ErrorMessageResourceName = "DataSourceRequired", ErrorMessageResourceType = typeof(Resources))]
        [Display(Name = "PleaseChooseADatasource", ResourceType = typeof(Resources))]
        public string DataSourceName { get; set; }
    }
}