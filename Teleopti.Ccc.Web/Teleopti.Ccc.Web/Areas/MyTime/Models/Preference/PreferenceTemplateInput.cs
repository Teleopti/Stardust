using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceTemplateInput : PreferenceInput
	{
		[Required(AllowEmptyStrings = false,
			ErrorMessageResourceType = typeof(Resources),
			ErrorMessageResourceName = "EmptyTemplateName")]
		[StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "TheNameIsTooLong")]
		public string TemplateName { get; set; }
	}
}