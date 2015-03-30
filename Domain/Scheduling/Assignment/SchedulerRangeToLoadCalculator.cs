using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// Calculates the loading period for a person
    /// </summary>
    public class SchedulerRangeToLoadCalculator : ISchedulerRangeToLoadCalculator
    {
        private readonly DateTimePeriod _requestedDateTimePeriod;
        private readonly IDictionary<IPerson, DateTimePeriod> _cachedResult;
        private int _justiceValue = -28;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerRangeToLoadCalculator"/> class.
        /// </summary>
        /// <param name="requestedDateTimePeriod">The requested date time period.</param>
        /// Use : (int)StateHolderReader.Instance.StateReader.SessionScopeData.SystemSetting[SettingKeys.JusticePointWindow] to load
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-19
        /// </remarks>
        public SchedulerRangeToLoadCalculator(DateTimePeriod requestedDateTimePeriod)
        {
            _requestedDateTimePeriod = requestedDateTimePeriod;
            _cachedResult = new Dictionary<IPerson, DateTimePeriod>();
        }

        /// <summary>
        /// Gets the requested period.
        /// </summary>
        /// <value>The requested period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-20
        /// </remarks>
        public DateTimePeriod RequestedPeriod
        {
            get
            {
                return _requestedDateTimePeriod;
            }
        }

        /// <summary>
        /// Gets or sets the justice value.
        /// </summary>
        /// <value>The justice value.</value>
        public int JusticeValue
        {
            get { return _justiceValue; }
            set { _justiceValue = value;}
        }

        /// <summary>
        /// Gets the scheduler range to load.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-19
        /// </remarks>
        public DateTimePeriod SchedulerRangeToLoad(IPerson person)
        {
            DateTimePeriod res;
            if (_cachedResult.TryGetValue(person,out res))
                return res;

            TimeZoneInfo timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
            IList<ISchedulePeriod> validPeriods = person.PersonSchedulePeriods(_requestedDateTimePeriod.ToDateOnlyPeriod(timeZoneInfo));

            if (validPeriods.Count == 0)
            {
                res = _requestedDateTimePeriod;                
            }
            else
            {
                CultureInfo culture = person.PermissionInformation.Culture();
                
                ISchedulePeriod minSchedulePeriod =
                    new SchedulePeriod(new DateOnly(3000, 1, 1), SchedulePeriodType.Day, 1);
                ISchedulePeriod maxSchedulePeriod =
                    new SchedulePeriod(new DateOnly(1900, 1, 1), SchedulePeriodType.Day, 1);

                foreach (ISchedulePeriod schedulePeriod in validPeriods)
                {
                    if (schedulePeriod.DateFrom < minSchedulePeriod.DateFrom)
                        minSchedulePeriod = schedulePeriod;
                    if (schedulePeriod.DateFrom > maxSchedulePeriod.DateFrom)
                        maxSchedulePeriod = schedulePeriod;
                }


                DateTime firstPeriodsFirstDateLocal = getFirstPeriodsFirstDateLocal(minSchedulePeriod, culture, timeZoneInfo);
                //DateTime justiceStart = _requestedDateTimePeriod.LocalStartDateTime.AddDays(_justiceValue);
                //always load at least one week before
                if (_justiceValue > -7)
                    _justiceValue = -7;
                DateTime justiceStart =
                    TimeZoneHelper.ConvertFromUtc(_requestedDateTimePeriod.StartDateTime, timeZoneInfo).AddDays(_justiceValue);
                if (justiceStart < firstPeriodsFirstDateLocal)
                    firstPeriodsFirstDateLocal = justiceStart;

                // always load one week after
                DateTime lastPeriodsLastDateLocal = getLastPeriodsLastDateLocal(maxSchedulePeriod, culture,timeZoneInfo).AddDays(7);
                DateTime justiceEnd = _requestedDateTimePeriod.EndDateTimeLocal(timeZoneInfo);
                if (justiceEnd > lastPeriodsLastDateLocal)
                    lastPeriodsLastDateLocal = justiceEnd;

                res = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(firstPeriodsFirstDateLocal, lastPeriodsLastDateLocal, timeZoneInfo);
            }
            _cachedResult[person] = res;
            return res;
        }

        private DateTime getLastPeriodsLastDateLocal(ISchedulePeriod maxSchedulePeriod, CultureInfo culture, TimeZoneInfo timeZoneInfo)
        {
            DateTime lastPeriodsLastDate;
            DateOnlyPeriod? maxPeriod =
                maxSchedulePeriod.GetSchedulePeriod(
                    new DateOnly(TimeZoneHelper.ConvertFromUtc(_requestedDateTimePeriod.EndDateTime, timeZoneInfo).AddTicks(-1)));

            if (maxPeriod.HasValue)
            {
                lastPeriodsLastDate = DateHelper.GetLastDateInWeek(maxPeriod.Value.EndDate.Date, culture);
            }
            else
            {
                lastPeriodsLastDate = _requestedDateTimePeriod.EndDateTime.AddDays(1);
            }

            return DateTime.SpecifyKind(lastPeriodsLastDate.Date, DateTimeKind.Local).AddDays(1);
        }

        private DateTime getFirstPeriodsFirstDateLocal(ISchedulePeriod minSchedulePeriod, CultureInfo culture, TimeZoneInfo timeZoneInfo)
        {
            DateTime firstPeriodsFirstDate;

            DateOnlyPeriod? minPeriod =
                minSchedulePeriod.GetSchedulePeriod(
                    new DateOnly(TimeZoneHelper.ConvertFromUtc(_requestedDateTimePeriod.StartDateTime, timeZoneInfo)));
            if (minPeriod.HasValue)
            {
                firstPeriodsFirstDate = DateHelper.GetFirstDateInWeek(minPeriod.Value.StartDate.Date, culture);
            }
            else
            {
                firstPeriodsFirstDate = _requestedDateTimePeriod.StartDateTime.AddDays(-1);
            }

            return DateTime.SpecifyKind(firstPeriodsFirstDate.Date,DateTimeKind.Local);
        }

       
    }
}