using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayViewModelMapper
	{
		private readonly IExtendedPreferencePredicate _extendedPreferencePredicate;

		public PreferenceDayViewModelMapper(IExtendedPreferencePredicate extendedPreferencePredicate)
		{
			_extendedPreferencePredicate = extendedPreferencePredicate;
		}

		public PreferenceDayViewModel Map(IPreferenceDay s)
		{
			return new PreferenceDayViewModel
			{
				Preference = preference(s),
				Color = color(s),
				Extended = _extendedPreferencePredicate.IsExtended(s),
				MustHave = s.Restriction.MustHave,
				ExtendedTitle = extendedTitle(s),
				StartTimeLimitation = s.Restriction.StartTimeLimitation.StartTimeString.ToLower() + "-" + s.Restriction.StartTimeLimitation.EndTimeString.ToLower(),
				EndTimeLimitation = s.Restriction.EndTimeLimitation.StartTimeString.ToLower() + "-" + s.Restriction.EndTimeLimitation.EndTimeString.ToLower(),
				WorkTimeLimitation = s.Restriction.WorkTimeLimitation.StartTimeString + "-" + s.Restriction.WorkTimeLimitation.EndTimeString,
				Activity = activity(s),
				ActivityStartTimeLimitation = GetActivityRestrictionValue(s, r => r.StartTimeLimitation.StartTimeString.ToLower() + "-" + r.StartTimeLimitation.EndTimeString.ToLower()),
				ActivityEndTimeLimitation = GetActivityRestrictionValue(s, r => r.EndTimeLimitation.StartTimeString.ToLower() + "-" + r.EndTimeLimitation.EndTimeString.ToLower()),
				ActivityTimeLimitation = GetActivityRestrictionValue(s, r => r.WorkTimeLimitation.StartTimeString + "-" + r.WorkTimeLimitation.EndTimeString)
			};
		}

		private string activity(IPreferenceDay s)
		{
			var activityName = GetActivityRestrictionValue(s, r => r.Activity.Name);
			return string.IsNullOrEmpty(activityName) ? "(" + Resources.NoActivity + ")" : activityName;
		}

		private static string extendedTitle(IPreferenceDay s)
		{
			if (s.Restriction == null) return string.Empty;
			if (s.Restriction.DayOffTemplate != null)
				return s.Restriction.DayOffTemplate.Description.Name;
			if (s.Restriction.Absence != null)
				return s.Restriction.Absence.Description.Name;
			if (s.Restriction.ShiftCategory != null)
				return s.Restriction.ShiftCategory.Description.Name;
			return Resources.Extended;
		}

		private static string color(IPreferenceDay s)
		{
			if (s.Restriction == null) return string.Empty;
			if (s.Restriction.DayOffTemplate != null)
				return s.Restriction.DayOffTemplate.DisplayColor.ToCSV();
			if (s.Restriction.Absence != null)
				return s.Restriction.Absence.DisplayColor.ToCSV();
			if (s.Restriction.ShiftCategory != null)
				return s.Restriction.ShiftCategory.DisplayColor.ToCSV();
			return null;
		}

		private string preference(IPreferenceDay s)
		{
			if (s.TemplateName != null)
				return s.TemplateName;
			if (s.Restriction == null)
				return string.Empty;
			if (s.Restriction.DayOffTemplate != null)
				return s.Restriction.DayOffTemplate.Description.Name;
			if (s.Restriction.Absence != null)
				return s.Restriction.Absence.Description.Name;
			if (s.Restriction.ShiftCategory != null)
				return s.Restriction.ShiftCategory.Description.Name;
			if (_extendedPreferencePredicate.IsExtended(s))
				return Resources.Extended;
			return null;
		}

		private T GetActivityRestrictionValue<T>(IPreferenceDay preferenceDay, Func<IActivityRestriction, T> getter)
		{
			if (preferenceDay.Restriction.ActivityRestrictionCollection.IsEmpty())
				return default(T);
			return getter.Invoke(preferenceDay.Restriction.ActivityRestrictionCollection.Single());
		}
	}
}