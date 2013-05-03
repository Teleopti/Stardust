using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
	public class IntradaySchedulePreferenceTransformer : IIntradaySchedulePreferenceTransformer
    {
        private readonly int _intervalsPerDay;

       
		public IntradaySchedulePreferenceTransformer(int intervalsPerDay)
        {
            _intervalsPerDay = intervalsPerDay;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Transform(IEnumerable<IPreferenceDay> rootList, DataTable table, ICommonStateHolder stateHolder, IScenario scenario)
        {
            InParameter.NotNull("rootList", rootList);
            InParameter.NotNull("table", table);

            foreach (var preferenceDay in rootList)
            { 
				if (CheckIfPreferenceIsValid(preferenceDay.Restriction))
				{
					var persons = stateHolder.PersonsWithIds(new List<Guid> {preferenceDay.Person.Id.GetValueOrDefault()});
					if (!persons.Any()) return;
                    var newDataRow = table.NewRow();
					var schedulePart = stateHolder.GetSchedulePartOnPersonAndDate(persons[0], preferenceDay.RestrictionDate, scenario);
					newDataRow = fillDataRow(newDataRow, preferenceDay.Restriction, schedulePart, _intervalsPerDay);
                    table.Rows.Add(newDataRow);
                }
                
            }
        }

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
            if (preferenceRestriction.StartTimeLimitation.HasValue())
            {
                if (preferenceRestriction.StartTimeLimitation.StartTime != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool isStartTimeLateSet(IRestrictionBase preferenceRestriction)
        {
            if (preferenceRestriction.StartTimeLimitation.HasValue())
            {
                if (preferenceRestriction.StartTimeLimitation.EndTime != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool isEndTimeEarlySet(IRestrictionBase preferenceRestriction)
        {
            if (preferenceRestriction.EndTimeLimitation.HasValue())
            {
                if (preferenceRestriction.EndTimeLimitation.StartTime != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool isEndTimeLateSet(IRestrictionBase preferenceRestriction)
        {
            if (preferenceRestriction.EndTimeLimitation.HasValue())
            {
                if (preferenceRestriction.EndTimeLimitation.EndTime != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool isWorkTimeMinSet(IRestrictionBase preferenceRestriction)
        {
            if (preferenceRestriction.WorkTimeLimitation.HasValue())
            {
                if (preferenceRestriction.WorkTimeLimitation.StartTime != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool isWorkTimeMaxSet(IRestrictionBase preferenceRestriction)
        {
            if (preferenceRestriction.WorkTimeLimitation.HasValue())
            {
                if (preferenceRestriction.WorkTimeLimitation.EndTime != null)
                {
                    return true;
                }
            }
            return false;
        }


        public bool CheckIfPreferenceIsValid(IPreferenceRestriction preferenceRestriction)
        {
            if (preferenceRestriction == null)
                return false;

            if (isShiftCategorySet(preferenceRestriction))
            {
                return true;
            }
            if (isDayOffSet(preferenceRestriction))
            {
                return true;
            }
            if (isStartTimeEarlySet(preferenceRestriction))
            {
                return true;
            }
            if (isStartTimeLateSet(preferenceRestriction))
            {
                return true;
            }
            if (isEndTimeEarlySet(preferenceRestriction))
            {
                return true;
            }
            if (isEndTimeLateSet(preferenceRestriction))
            {
                return true;
            }

            if (isWorkTimeMinSet(preferenceRestriction))
            {
                return true;
            }
            if (isWorkTimeMaxSet(preferenceRestriction))
            {
                return true;
            }

			if (preferenceRestriction.Absence != null)
				return true;

			if(preferenceRestriction.ActivityRestrictionCollection.Count > 0)
			{
				return true;
			}

            return false;
        }

        private static DataRow fillDataRow(DataRow dataRow, IPreferenceRestriction preferenceRestriction, IScheduleDay schedulePart, int intervalsPerDay)
        {
            dataRow["person_restriction_code"] = preferenceRestriction.Id;
            var preferenceDay = (IPreferenceDay)preferenceRestriction.Parent;
			
            var utcRestrictionDateTime = TimeZoneHelper.ConvertToUtc(preferenceDay.RestrictionDate,
																	  preferenceDay.Person.PermissionInformation.
                                                                          DefaultTimeZone());
            dataRow["restriction_date"] = utcRestrictionDateTime.Date;
			dataRow["person_code"] = schedulePart.Person.Id;
            dataRow["interval_id"] = new IntervalBase(utcRestrictionDateTime, intervalsPerDay).Id;
			dataRow["scenario_code"] = schedulePart.Scenario.Id;
			
            if (isShiftCategorySet(preferenceRestriction))
            {
                dataRow["shift_category_code"] = preferenceRestriction.ShiftCategory.Id;
            }
            if (preferenceRestriction.DayOffTemplate != null)
            {
                dataRow["day_off_code"] = preferenceRestriction.DayOffTemplate.Id;
                //dataRow["day_off_name"] = "PrefsDayOff"; //Get from domain
                dataRow["day_off_name"] = preferenceRestriction.DayOffTemplate.Description.Name;
                dataRow["day_off_shortname"] = preferenceRestriction.DayOffTemplate.Description.ShortName;
            }

            if (isStartTimeEarlySet(preferenceRestriction))
            {
                dataRow["StartTimeMinimum"] = preferenceRestriction.StartTimeLimitation.StartTime.Value.TotalMinutes;
            }
            if (isStartTimeLateSet(preferenceRestriction))
            {
                dataRow["StartTimeMaximum"] = preferenceRestriction.StartTimeLimitation.EndTime.Value.TotalMinutes;
            }

            if (isEndTimeEarlySet(preferenceRestriction))
            {
                dataRow["endTimeMinimum"] = preferenceRestriction.EndTimeLimitation.StartTime.Value.TotalMinutes;
            }
            if (isEndTimeLateSet(preferenceRestriction))
            {
                dataRow["endTimeMaximum"] = preferenceRestriction.EndTimeLimitation.EndTime.Value.TotalMinutes;
            }

            if (isWorkTimeMinSet(preferenceRestriction))
            {
                dataRow["WorkTimeMinimum"] = preferenceRestriction.WorkTimeLimitation.StartTime.Value.TotalMinutes;
            }
            if (isWorkTimeMaxSet(preferenceRestriction))
            {
                dataRow["WorkTimeMaximum"] = preferenceRestriction.WorkTimeLimitation.EndTime.Value.TotalMinutes;
            }

			if(preferenceRestriction.ActivityRestrictionCollection.Count > 0)
			{
				dataRow["activity_code"] = preferenceRestriction.ActivityRestrictionCollection[0].Activity.Id;
			}

			dataRow["must_have"] = preferenceRestriction.MustHave ? 1 : 0;

			if (preferenceRestriction.Absence != null)
				dataRow["absence_code"] = preferenceRestriction.Absence.Id;
			else
				dataRow["absence_code"] = DBNull.Value;

            RestrictionChecker restrictionChecker = new RestrictionChecker(schedulePart);
            PermissionState permissionState = restrictionChecker.CheckPreference();
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
            //dataRow["datasource_id"] = 1;
            //dataRow["datasource_update_date"] = 

            return dataRow;
        }



    }
}
