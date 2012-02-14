using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TimeLayer
{
    public class DayOfWeekMultiplicatorDefinition : MultiplicatorDefinition
    {
        private TimePeriod _period;
        private DayOfWeek _dayOfWeek;

        protected DayOfWeekMultiplicatorDefinition()
        {
        }

        public DayOfWeekMultiplicatorDefinition(IMultiplicator multiplicator, DayOfWeek dayOfWeek, TimePeriod period)
            : base(multiplicator)
        {
            _period = period;
            _dayOfWeek = dayOfWeek;
        }

        public virtual DayOfWeek DayOfWeek
        {
            get { return _dayOfWeek; }
            set { _dayOfWeek = value; }
        }

        public virtual TimePeriod Period
        {
            get { return _period; }
            set { _period = value; }
        }

        public virtual DateTimePeriod ConvertToDateTimePeriod(DateOnly givenDate, ICccTimeZoneInfo timeZoneInfo)
        {
            if (givenDate.DayOfWeek != DayOfWeek)
                throw new ArgumentException("Day of week for date givenDate correspond to this instance day of week", "givenDate");

            DateTime localMidnight = givenDate;
            return
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                    localMidnight.Add(Period.StartTime),
                    localMidnight.Add(Period.EndTime), timeZoneInfo);

        }

        public virtual IMultiplicatorLayer ConvertToLayer(DateOnly givenDate, ICccTimeZoneInfo timeZoneInfo)
        {
            return new MultiplicatorLayer((IMultiplicatorDefinitionSet) Parent, Multiplicator,ConvertToDateTimePeriod(givenDate, timeZoneInfo));
        }

        public override IList<IMultiplicatorLayer> GetLayersForPeriod(DateOnly startDate, DateOnly endDate, ICccTimeZoneInfo timeZoneInfo)
        {
        	var dayCollection = new DateOnlyPeriod(startDate, endDate).DayCollection();

        	return (from dateOnly in dayCollection
        	        where dateOnly.DayOfWeek == DayOfWeek
        	        select ConvertToLayer(dateOnly, timeZoneInfo)).ToList();
        }
    }
}
