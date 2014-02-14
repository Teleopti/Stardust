using System;
using System.Data;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
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

		public static DataRow FillDataRow(DataRow dataRow, IPreferenceRestriction preferenceRestriction,
		                                  IScheduleDay schedulePart)
		{
			dataRow["person_restriction_code"] = preferenceRestriction.Id;
			var preferenceDay = (IPreferenceDay) preferenceRestriction.Parent;

			var restrictionDate = preferenceDay.RestrictionDate;
			dataRow["restriction_date"] = restrictionDate.Date;
			dataRow["person_code"] = schedulePart.Person.Id;
			dataRow["scenario_code"] = schedulePart.Scenario.Id;

			if (isShiftCategorySet(preferenceRestriction))
				dataRow["shift_category_code"] = preferenceRestriction.ShiftCategory.Id;

			if (preferenceRestriction.DayOffTemplate != null)
			{
				dataRow["day_off_code"] = preferenceRestriction.DayOffTemplate.Id;
				dataRow["day_off_name"] = preferenceRestriction.DayOffTemplate.Description.Name;
				dataRow["day_off_shortname"] = preferenceRestriction.DayOffTemplate.Description.ShortName;
			}

			// ReSharper disable PossibleInvalidOperationException
			if (isStartTimeEarlySet(preferenceRestriction))
				dataRow["StartTimeMinimum"] = preferenceRestriction.StartTimeLimitation.StartTime.Value.TotalMinutes;

			if (isStartTimeLateSet(preferenceRestriction))
				dataRow["StartTimeMaximum"] = preferenceRestriction.StartTimeLimitation.EndTime.Value.TotalMinutes;

			if (isEndTimeEarlySet(preferenceRestriction))
				dataRow["endTimeMinimum"] = preferenceRestriction.EndTimeLimitation.StartTime.Value.TotalMinutes;

			if (isEndTimeLateSet(preferenceRestriction))
				dataRow["endTimeMaximum"] = preferenceRestriction.EndTimeLimitation.EndTime.Value.TotalMinutes;

			if (isWorkTimeMinSet(preferenceRestriction))
				dataRow["WorkTimeMinimum"] = preferenceRestriction.WorkTimeLimitation.StartTime.Value.TotalMinutes;

			if (isWorkTimeMaxSet(preferenceRestriction))
				dataRow["WorkTimeMaximum"] = preferenceRestriction.WorkTimeLimitation.EndTime.Value.TotalMinutes;

			if (preferenceRestriction.ActivityRestrictionCollection.Count > 0)
				dataRow["activity_code"] = preferenceRestriction.ActivityRestrictionCollection[0].Activity.Id;
			// ReSharper restore PossibleInvalidOperationException

			dataRow["must_have"] = preferenceRestriction.MustHave ? 1 : 0;

			if (preferenceRestriction.Absence != null)
				dataRow["absence_code"] = preferenceRestriction.Absence.Id;
			else
				dataRow["absence_code"] = DBNull.Value;

			var restrictionChecker = new RestrictionChecker();
			var permissionState = restrictionChecker.CheckPreference(schedulePart);
			if (permissionState == PermissionState.Satisfied)
			{
				dataRow["preference_fulfilled"] = 1;
				dataRow["preference_unfulfilled"] = 0;
			}

			else
			{
				dataRow["preference_fulfilled"] = 0;
				dataRow["preference_unfulfilled"] = 1;
			}

			dataRow["business_unit_code"] = schedulePart.Scenario.BusinessUnit.Id;
			return dataRow;
		}
	}
}
