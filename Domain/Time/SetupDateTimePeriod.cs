using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodToSchedulesIfTheyExist:ISetupDateTimePeriod
    {
        private readonly IList<IScheduleDay> _scheduleDays;
        private ISetupDateTimePeriod _fallback;


        public SetupDateTimePeriodToSchedulesIfTheyExist(IList<IScheduleDay> scheduleDays,ISetupDateTimePeriod fallback)
        {
            _fallback = fallback;
            _scheduleDays = scheduleDays;
        }

        public DateTimePeriod Period
        {
            get
            {
                if (_scheduleDays == null || 
                    _scheduleDays.Count == 0 || 
                    (_scheduleDays[0].PersistableScheduleDataCollection().IsEmpty() && _scheduleDays.Count==1))
                {
                    return _fallback.Period;
                }

                return new SetupDateTimePeriodToSelectedSchedules(_scheduleDays).Period;
            }
        }
    }
   
    //Sets the period to the total time of all scheduleparts (or default if no scheduleparts)
    public class SetupDateTimePeriodToSelectedSchedules : ISetupDateTimePeriod
    {
        private readonly DateTimePeriod _period;

        public SetupDateTimePeriodToSelectedSchedules(IList<IScheduleDay> scheduleDays) : this(scheduleDays, null){}

        public SetupDateTimePeriodToSelectedSchedules(IList<IScheduleDay> scheduleDays, DateTimePeriod? defaultPeriod)
        {
            if (defaultPeriod != null) _period = defaultPeriod.Value;
            else _period = GetPeriodFromScheduleDays(scheduleDays);
        }

        private static DateTimePeriod GetPeriodFromScheduleDays(IList<IScheduleDay> scheduleDays)
        {
            
            if (scheduleDays.Count == 1 && scheduleDays[0].HasProjection)
            {
                if (scheduleDays[0].PersonAssignmentCollection().Count > 0)
                {
                    var p =scheduleDays[0].PersonAssignmentCollection().First().MainShift.ProjectionService().CreateProjection().Period();
                    if (p != null) return p.Value;
                }
            }
            

            IList<DateTimePeriod> periods = new List<DateTimePeriod>();
            foreach (IScheduleDay scheduleDay in scheduleDays)
            {
                periods.Add(scheduleDay.Period);
            }

            return new DateTimePeriod(periods.Min(p => p.StartDateTime),periods.Max(p=>p.EndDateTime.Subtract(TimeSpan.FromMinutes(1))));
        }


        public DateTimePeriod Period
        {
            get { return _period; }
        }
    }

    /// <summary>
    /// Sets to 8-17 local on startdate
    /// </summary>
    public class SetupDateTimePeriodToDefaultLocalHours : ISetupDateTimePeriod
    {
        private DateTimePeriod _period;

        public SetupDateTimePeriodToDefaultLocalHours(TimePeriod defaultLocal, IScheduleDay scheduleDay, DateTimePeriod period, ICccTimeZoneInfo info)
        {
            DateTime date = period.StartDateTimeLocal(info).Date;

            var start = date.Add(defaultLocal.StartTime);
            var end = date.Add(defaultLocal.EndTime);

            var startUtc = TimeZoneHelper.ConvertToUtc(start, info);
            var endUtc = TimeZoneHelper.ConvertToUtc(end, info);

            _period = new DateTimePeriod(startUtc, endUtc);

            createFromScheduleDay(scheduleDay);
        }

        private void createFromScheduleDay(IScheduleDay scheduleDay)
        {
            if (scheduleDay != null && scheduleDay.PersonAssignmentCollection().Count > 0)
            {
                var timePeriod = scheduleDay.PersonAssignmentCollection().First().Period;
                _period = new DateTimePeriod(timePeriod.EndDateTime, timePeriod.EndDateTime.AddHours(1));
            }
        }

        public DateTimePeriod Period
        {
            get { return _period; }
        }


    }


    public class SetupDateTimePeriodToDefaultPeriod:ISetupDateTimePeriod
    {
        private readonly DateTimePeriod? _period;


        public SetupDateTimePeriodToDefaultPeriod(DateTimePeriod? period)
        {
            _period = period;
        }

        public DateTimePeriod Period
        {
            get
            {
                if (_period != null) return _period.Value;
                return createDefaultperiod();

            }
        }

        private static DateTimePeriod createDefaultperiod()
        {
            return new DateTimePeriod(DateTime.UtcNow,DateTime.UtcNow.AddHours(1));
        }
    }
    
    /// <summary>
    /// Used for setting a default period based on inputs
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-09-07
    /// </remarks>
    public interface ISetupDateTimePeriod
    {
        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-09-07
        /// </remarks>
        DateTimePeriod Period { get; }

      
    }
}
