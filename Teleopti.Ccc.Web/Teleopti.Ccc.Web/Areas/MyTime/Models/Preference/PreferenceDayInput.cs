using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceDayInput : IValidatableObject
	{
		public DateOnly Date { get; set; }
		public Guid PreferenceId { get; set; }

		public TimeOfDay? EarliestStartTime { get; set; }
		public TimeOfDay? LatestStartTime { get; set; }

		public TimeOfDay? EarliestEndTime { get; set; }
		public TimeOfDay? LatestEndTime { get; set; }

		public TimeSpan? MinimumWorkTime { get; set; }
		public TimeSpan? MaximumWorkTime { get; set; }

		public Guid ActivityPreferenceId { get; set; }

		public TimeSpan? ActivityMinimumTime { get; set; }
		public TimeSpan? ActivityMaximumTime { get; set; }

		public TimeOfDay? ActivityEarliestStartTime { get; set; }
		public TimeOfDay? ActivityLatestStartTime { get; set; }

		public TimeOfDay? ActivityEarliestEndTime { get; set; }
		public TimeOfDay? ActivityLatestEndTime { get; set; }


		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var result = new List<ValidationResult>();
			if (ValidateTimeOfDay(EarliestStartTime, LatestStartTime))
				result.Add(new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.StartTime)));
			if (ValidateTimeOfDay(EarliestEndTime, LatestEndTime))
				result.Add(new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.EndTime)));
			if (ValidateTimeOfDay(ActivityEarliestStartTime, ActivityLatestStartTime))
				result.Add(new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.ActivityTime)));
			if (ValidateTimeOfDay(ActivityEarliestEndTime, ActivityLatestEndTime))
				result.Add(new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.ActivityTime)));
			if (ValidateTimeSpan(MinimumWorkTime, MaximumWorkTime))
				result.Add(new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.WorkTime)));
			if (ValidateTimeSpan(ActivityMinimumTime, ActivityMaximumTime))
				result.Add(new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.ActivityLength)));
			return result;
		}

		private static bool ValidateTimeSpan(TimeSpan? min, TimeSpan? max)
		{
			if (min.HasValue && max.HasValue)
			{
				return min.Value > max.Value;
			}
			return false;
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