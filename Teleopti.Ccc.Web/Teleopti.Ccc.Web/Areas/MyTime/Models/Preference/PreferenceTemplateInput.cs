using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceTemplateInput : PreferenceInput
	{
		public string TemplateName { get; set; }
		protected override IEnumerable<ValidationResult> ValidateMore(ValidationContext validationContext)
		{
			var result = new List<ValidationResult>();
			if (string.IsNullOrEmpty(TemplateName))
				result.Add(new ValidationResult(string.Format(Resources.EmptyTemplateName, Resources.ExtendedPreferencesTemplate)));
			return result;
		}
	}
}