using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability
{
	public class StudentAvailabilityDayInput : IValidatableObject
	{
		public DateOnly Date { get; set; }
		public TimeOfDay StartTime { get; set; }
		public TimeOfDay EndTime { get; set; }
		public bool NextDay { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var result = new List<ValidationResult>();
			var end = EndTime.Time;
			if (NextDay)
				end = end.Add(TimeSpan.FromDays(1));
			if (StartTime.Time > end)
				result.Add(new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.EndTime)));
				
			return result;
		}
	}
}