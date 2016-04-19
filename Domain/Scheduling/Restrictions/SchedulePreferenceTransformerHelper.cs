using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public static class SchedulePreferenceTransformerHelper
	{
		private static bool isShiftCategorySet(IPreferenceRestriction preferenceRestriction)
		{
			return preferenceRestriction.ShiftCategory != null;
		}

		private static bool isDayOffSet(IPreferenceRestriction preferenceRestriction)
		{
			return preferenceRestriction.DayOffTemplate != null;
		}

		private static bool isStartTimeEarlySet(IRestrictionBase preferenceRestriction)
		{
			return preferenceRestriction.StartTimeLimitation.HasValue() &&
					 preferenceRestriction.StartTimeLimitation.StartTime != null;
		}

		private static bool isStartTimeLateSet(IRestrictionBase preferenceRestriction)
		{
			return preferenceRestriction.StartTimeLimitation.HasValue() &&
					 preferenceRestriction.StartTimeLimitation.EndTime != null;
		}

		private static bool isEndTimeEarlySet(IRestrictionBase preferenceRestriction)
		{
			return preferenceRestriction.EndTimeLimitation.HasValue() &&
					 preferenceRestriction.EndTimeLimitation.StartTime != null;
		}

		private static bool isEndTimeLateSet(IRestrictionBase preferenceRestriction)
		{
			return preferenceRestriction.EndTimeLimitation.HasValue() &&
					 preferenceRestriction.EndTimeLimitation.EndTime != null;
		}

		private static bool isWorkTimeMinSet(IRestrictionBase preferenceRestriction)
		{
			return preferenceRestriction.WorkTimeLimitation.HasValue() &&
					 preferenceRestriction.WorkTimeLimitation.StartTime != null;
		}

		private static bool isWorkTimeMaxSet(IRestrictionBase preferenceRestriction)
		{
			return preferenceRestriction.WorkTimeLimitation.HasValue() &&
					 preferenceRestriction.WorkTimeLimitation.EndTime != null;
		}

		private static bool isStartTimeSet(IRestrictionBase preferenceRestriction)
		{
			return isStartTimeEarlySet(preferenceRestriction) || isStartTimeLateSet(preferenceRestriction);
		}

		private static bool isEndTimeSet(IRestrictionBase preferenceRestriction)
		{
			return isEndTimeEarlySet(preferenceRestriction) || isEndTimeLateSet(preferenceRestriction);
		}

		private static bool isWorkTimeSet(IRestrictionBase preferenceRestriction)
		{
			return isWorkTimeMinSet(preferenceRestriction) || isWorkTimeMaxSet(preferenceRestriction);
		}

		public static int GetPreferenceTypeId(IPreferenceRestriction preferenceRestriction)
		{
			if (isStartTimeSet(preferenceRestriction) || isEndTimeSet(preferenceRestriction) ||
				 isWorkTimeSet(preferenceRestriction) || preferenceRestriction.ActivityRestrictionCollection.Any())
				return 3;

			if (preferenceRestriction.Absence != null)
				return 4;

			if (preferenceRestriction.DayOffTemplate != null)
				return 2;

			if (preferenceRestriction.ShiftCategory != null)
				return 1;

			throw new ArgumentException("Preference type could not be resolved");
		}

		public static bool CheckIfPreferenceIsValid(IPreferenceRestriction preferenceRestriction)
		{
			if (preferenceRestriction == null)
				return false;
			if (isShiftCategorySet(preferenceRestriction))
				return true;
			if (isDayOffSet(preferenceRestriction))
				return true;
			if (isStartTimeEarlySet(preferenceRestriction))
				return true;
			if (isStartTimeLateSet(preferenceRestriction))
				return true;
			if (isEndTimeEarlySet(preferenceRestriction))
				return true;
			if (isEndTimeLateSet(preferenceRestriction))
				return true;
			if (isWorkTimeMinSet(preferenceRestriction))
				return true;
			if (isWorkTimeMaxSet(preferenceRestriction))
				return true;
			if (preferenceRestriction.Absence != null)
				return true;

			return preferenceRestriction.ActivityRestrictionCollection.Count > 0;
		}
	}
}
