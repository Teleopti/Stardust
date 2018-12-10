using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class ExtendedPreferenceTemplateMapper
	{
		public PreferenceTemplateViewModel Map(IExtendedPreferenceTemplate extendedPreferenceTemplate)
		{
			return new PreferenceTemplateViewModel
			{
				Value = extendedPreferenceTemplate.Id.ToString(),
				Text = extendedPreferenceTemplate.Name,
				Color = extendedPreferenceTemplate.DisplayColor.ToHtml(),

				PreferenceId = findPreferenceId(extendedPreferenceTemplate),
				EarliestStartTime = extendedPreferenceTemplate.Restriction.StartTimeLimitation.StartTimeString,
				LatestStartTime = extendedPreferenceTemplate.Restriction.StartTimeLimitation.EndTimeString,
				EarliestEndTime = earliestEndtime(extendedPreferenceTemplate),
				EarliestEndTimeNextDay =
					extendedPreferenceTemplate.Restriction.EndTimeLimitation.StartTime.HasValue &&
					extendedPreferenceTemplate.Restriction.EndTimeLimitation.StartTime.Value >= TimeSpan.FromDays(1),
				LatestEndTime = latestEndTime(extendedPreferenceTemplate),
				LatestEndTimeNextDay =
					extendedPreferenceTemplate.Restriction.EndTimeLimitation.EndTime.HasValue &&
					extendedPreferenceTemplate.Restriction.EndTimeLimitation.EndTime.Value >= TimeSpan.FromDays(1),
				MinimumWorkTime = extendedPreferenceTemplate.Restriction.WorkTimeLimitation.StartTimeString,
				MaximumWorkTime = extendedPreferenceTemplate.Restriction.WorkTimeLimitation.EndTimeString,
				ActivityPreferenceId = GetActivityRestrictionValue(extendedPreferenceTemplate, r => r.Activity?.Id),
				ActivityEarliestStartTime =
					GetActivityRestrictionValue(extendedPreferenceTemplate, r => r.StartTimeLimitation.StartTimeString),
				ActivityLatestStartTime =
					GetActivityRestrictionValue(extendedPreferenceTemplate, r => r.StartTimeLimitation.EndTimeString),
				ActivityEarliestEndTime =
					GetActivityRestrictionValue(extendedPreferenceTemplate, r => r.EndTimeLimitation.StartTimeString),
				ActivityLatestEndTime =
					GetActivityRestrictionValue(extendedPreferenceTemplate, r => r.EndTimeLimitation.EndTimeString),
				ActivityMinimumTime =
					GetActivityRestrictionValue(extendedPreferenceTemplate, r => r.WorkTimeLimitation.StartTimeString),
				ActivityMaximumTime =
					GetActivityRestrictionValue(extendedPreferenceTemplate, r => r.WorkTimeLimitation.EndTimeString)
			};
		}

		private static string latestEndTime(IExtendedPreferenceTemplate s)
		{
			if (s.Restriction.EndTimeLimitation.EndTime.HasValue &&
				s.Restriction.EndTimeLimitation.EndTime.Value >= TimeSpan.FromDays(1))
			{
				return TimeHelper.TimeOfDayFromTimeSpan(s.Restriction.EndTimeLimitation.EndTime.Value.Add(TimeSpan.FromDays(-1)),
					CultureInfo.CurrentCulture);
			}
			return s.Restriction.EndTimeLimitation.EndTimeString;
		}

		private static string earliestEndtime(IExtendedPreferenceTemplate s)
		{
			if (s.Restriction.EndTimeLimitation.StartTime.HasValue &&
				s.Restriction.EndTimeLimitation.StartTime.Value >= TimeSpan.FromDays(1))
			{
				return TimeHelper.TimeOfDayFromTimeSpan(s.Restriction.EndTimeLimitation.StartTime.Value.Add(TimeSpan.FromDays(-1)),
					CultureInfo.CurrentCulture);
			}
			return s.Restriction.EndTimeLimitation.StartTimeString;
		}

		private Guid? findPreferenceId(IExtendedPreferenceTemplate s)
		{
			if (s.Restriction?.DayOffTemplate != null)
				return s.Restriction.DayOffTemplate.Id;
			if (s.Restriction?.Absence != null)
				return s.Restriction.Absence.Id;

			return s.Restriction?.ShiftCategory?.Id;
		}

		private T GetActivityRestrictionValue<T>(IExtendedPreferenceTemplate template, Func<IActivityRestriction, T> getter)
		{
			if (template.Restriction.ActivityRestrictionCollection.IsEmpty())
				return default(T);
			return getter.Invoke(template.Restriction.ActivityRestrictionCollection.Single());
		}
	}
}