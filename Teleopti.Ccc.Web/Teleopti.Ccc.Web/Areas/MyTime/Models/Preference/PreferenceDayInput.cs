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
		public Guid? PreferenceId { get; set; }

		public TimeOfDay? EarliestStartTime { get; set; }
		public TimeOfDay? LatestStartTime { get; set; }

		public TimeOfDay? EarliestEndTime { get; set; }
		public bool EarliestEndTimeNextDay { get; set; }
		public TimeOfDay? LatestEndTime { get; set; }
		public bool LatestEndTimeNextDay { get; set; }

		public TimeSpan? MinimumWorkTime { get; set; }
		public TimeSpan? MaximumWorkTime { get; set; }

		public Guid? ActivityPreferenceId { get; set; }

		public TimeSpan? ActivityMinimumTime { get; set; }
		public TimeSpan? ActivityMaximumTime { get; set; }

		public TimeOfDay? ActivityEarliestStartTime { get; set; }
		public TimeOfDay? ActivityLatestStartTime { get; set; }

		public TimeOfDay? ActivityEarliestEndTime { get; set; }
		public TimeOfDay? ActivityLatestEndTime { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var result = new List<ValidationResult>();
			if (IsEmptyForm())
				result.Add(new ValidationResult(string.Format(Resources.EmptyRequest, Resources.ExtendedPreferences)));
			if (ValidateTimeOfDay(EarliestStartTime, LatestStartTime))
				result.Add(new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.StartTime)));
			if (EarliestEndTime.HasValue && LatestEndTime.HasValue)
			{
				var earliest = EarliestEndTimeNextDay
					               ? EarliestEndTime.Value.Time.Add(TimeSpan.FromDays(1))
					               : EarliestEndTime.Value.Time;
				var latest = LatestEndTimeNextDay ? LatestEndTime.Value.Time.Add(TimeSpan.FromDays(1)) : LatestEndTime.Value.Time;
				if (earliest > latest)
				{
					result.Add(new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.EndTime)));
				}
			}
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

		private bool IsEmptyForm()
		{
			return !PreferenceId.HasValue &&
			       !EarliestStartTime.HasValue &&
			       !LatestStartTime.HasValue &&
			       !EarliestEndTime.HasValue &&
			       !LatestEndTime.HasValue &&
			       !MinimumWorkTime.HasValue &&
			       !MaximumWorkTime.HasValue &&
			       !ActivityPreferenceId.HasValue &&
			       !ActivityMinimumTime.HasValue &&
			       !ActivityMaximumTime.HasValue &&
			       !ActivityEarliestStartTime.HasValue &&
			       !ActivityLatestStartTime.HasValue &&
			       !ActivityEarliestEndTime.HasValue &&
			       !ActivityLatestEndTime.HasValue;
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