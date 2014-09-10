using System;
using System.Data;
using System.Linq;
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

			dataRow["preference_type_id"] = GetPreferenceTypeId(preferenceRestriction);

			// ReSharper disable PossibleInvalidOperationException
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
