using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceTemplateInput : PreferenceInput
	{
		[Required(AllowEmptyStrings = false,
			ErrorMessageResourceType = typeof (Resources),
			ErrorMessageResourceName = nameof(Resources.Name50CharactersLimit))]
		[StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.Name50CharactersLimit))]
		public string NewTemplateName { get; set; }
	}
}