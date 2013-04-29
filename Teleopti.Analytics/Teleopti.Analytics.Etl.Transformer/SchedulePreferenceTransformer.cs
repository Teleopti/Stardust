﻿using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class SchedulePreferenceTransformer : ISchedulePreferenceTransformer
    {
        private readonly int _intervalsPerDay;

        private SchedulePreferenceTransformer() { }

        public SchedulePreferenceTransformer(int intervalsPerDay)
            : this()
        {
            _intervalsPerDay = intervalsPerDay;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Transform(IEnumerable<IScheduleDay> rootList, DataTable table)
        {
            InParameter.NotNull("rootList", rootList);
            InParameter.NotNull("table", table);

            foreach (IScheduleDay schedulePart in rootList)
            {
                IEnumerable<IRestrictionBase> restrictionBases = schedulePart.RestrictionCollection();
                foreach (IRestrictionBase restrictionBase in restrictionBases)
                {
                    IPreferenceRestriction preferenceRestriction = restrictionBase as IPreferenceRestriction;
                    if (CheckIfPreferenceIsValid(preferenceRestriction))
                    {
                        DataRow newDataRow = table.NewRow();
                        newDataRow = fillDataRow(newDataRow, preferenceRestriction, schedulePart, _intervalsPerDay);
                        table.Rows.Add(newDataRow);
                    }
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

        /// <summary>
        /// Checks if preferences is set.
        /// </summary>
        /// <param name="preferenceRestriction">The preference restriction.</param>
        /// <returns></returns>
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

			if(preferenceRestriction.ActivityRestrictionCollection.Count > 0)
			{
				return true;
			}

            return false;
        }

        /// <summary>
        /// Fills the data row.
        /// </summary>
        /// <param name="dataRow">The data row.</param>
        /// <param name="preferenceRestriction">The preference restriction.</param>
        /// <param name="schedulePart">The schedule part.</param>
        /// <param name="intervalsPerDay">The intervals per day.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Henryg
        /// Created date: 2009-11-24
        /// </remarks>
        private static DataRow fillDataRow(DataRow dataRow, IPreferenceRestriction preferenceRestriction, IScheduleDay schedulePart, int intervalsPerDay)
        {
            dataRow["person_restriction_code"] = preferenceRestriction.Id;
            IPreferenceDay preferenceDay = (IPreferenceDay)preferenceRestriction.Parent;
            DateTime utcRestrictionDateTime = TimeZoneHelper.ConvertToUtc(preferenceDay.RestrictionDate,
                                                                      schedulePart.Person.PermissionInformation.
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

            dataRow["absence_code"] = preferenceRestriction.Absence.Id;

            RestrictionChecker restrictionChecker = new RestrictionChecker(schedulePart);
            PermissionState permissionState = restrictionChecker.CheckPreference();
            if (permissionState == PermissionState.Satisfied)
            {
                dataRow["preference_accepted"] = 1;
                dataRow["preference_declined"] = 0;
            }
            else
            {
                dataRow["preference_accepted"] = 0;
                dataRow["preference_declined"] = 1;
            }

            dataRow["business_unit_code"] = schedulePart.Scenario.BusinessUnit.Id;
            //dataRow["datasource_id"] = 1;
            //dataRow["datasource_update_date"] = 

            return dataRow;
        }



    }
}
