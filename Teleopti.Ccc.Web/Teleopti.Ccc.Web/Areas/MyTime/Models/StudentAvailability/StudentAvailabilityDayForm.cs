using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability
{
	public class StudentAvailabilityDayForm : IValidatableObject
	{
		public DateOnly Date { get; set; }
		public TimeOfDay StartTime { get; set; }
		public TimeOfDay EndTime { get; set; }
		public bool NextDay { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var result = new List<ValidationResult>();
			if (ValidateTimeOfDay(StartTime, EndTime))
				result.Add(new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.EndTime)));
			return result;
		}

		private static bool ValidateTimeOfDay(TimeOfDay? early, TimeOfDay? late)
		{
			if (early.HasValue && late.HasValue)
			{
				return early.Value.Time > late.Value.Time;
			}
			return false;
		}
	}
}