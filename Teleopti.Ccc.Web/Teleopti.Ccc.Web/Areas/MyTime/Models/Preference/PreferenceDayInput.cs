using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceDayInput : PreferenceInput
	{
		public DateOnly Date { get; set; }
		protected override IEnumerable<ValidationResult> ValidateMore(ValidationContext validationContext)
		{
			return new List<ValidationResult>();
		}
	}
}