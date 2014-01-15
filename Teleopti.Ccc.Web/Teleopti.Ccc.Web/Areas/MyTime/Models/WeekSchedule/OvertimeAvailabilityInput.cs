using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule
{
	public class OvertimeAvailabilityInput : IValidatableObject
	{
		public DateOnly Date { get; set; }
		public TimeOfDay StartTime { get; set; }
		public TimeOfDay EndTime { get; set; }
		public bool EndTimeNextDay { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var result = new List<ValidationResult>();
			if (isInvalid(StartTime, EndTime))
				result.Add(new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.EndTime)));
			return result;
		}

		private bool isInvalid(TimeOfDay early, TimeOfDay late)
		{
			return early.Time > late.ToTimeSpan(EndTimeNextDay);
		}
	}
}