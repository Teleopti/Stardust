using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TimeLayer
{
    public class DateTimeMultiplicatorDefinition : MultiplicatorDefinition
    {
        private DateOnly _startDate;
        private DateOnly _endDate;
        private TimeSpan _startTime;
        private TimeSpan _endTime;

        protected DateTimeMultiplicatorDefinition() { }

        public DateTimeMultiplicatorDefinition(IMultiplicator multiplicator, DateOnly startDate, DateOnly endDate, TimeSpan startTime, TimeSpan endTime)
            : base(multiplicator)
        {
            validatePeriod(startDate, endDate, startTime, endTime, "value");
            _startDate = startDate;
            _startTime = startTime;
            _endDate = endDate;
            _endTime = endTime;
        }

        public override IList<IMultiplicatorLayer> GetLayersForPeriod(DateOnlyPeriod period, TimeZoneInfo timeZoneInfo)
        {
			validatePeriod(_startDate, _endDate, _startTime, _endTime, "value");

			IList<IMultiplicatorLayer> ret = new List<IMultiplicatorLayer>();

			DateTime localStart = _startDate.Date.Add(_startTime);
			DateTime localEnd = _endDate.Date.Add(_endTime);

			DateTimePeriod definitionPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localStart, localEnd, timeZoneInfo);
			DateTimePeriod viewPeriod = period.ToDateTimePeriod(timeZoneInfo);

			var intersection = definitionPeriod.Intersection(viewPeriod);
			if (intersection.HasValue)
			{
				IMultiplicatorLayer layer = new MultiplicatorLayer((IMultiplicatorDefinitionSet)Parent, Multiplicator, intersection.Value);
				ret.Add(layer);
			}
			return ret;
        }

        public virtual DateOnly StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
            }
        }

        public virtual DateOnly EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
            }
        }

        public virtual TimeSpan StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
            }
        }

        public virtual TimeSpan EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
            }
        }

        public virtual IMultiplicatorLayer ConvertToLayer(TimeZoneInfo timeZoneInfo)
        {
			validatePeriod(StartDate, EndDate, StartTime, EndTime, "value");
            DateTimePeriod period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(StartDate.Date.Add(StartTime), EndDate.Date.Add(EndTime), timeZoneInfo);
            return new MultiplicatorLayer((IMultiplicatorDefinitionSet) Parent, Multiplicator, period);
        }
		
		private static void validatePeriod(DateOnly startDate, DateOnly endDate, TimeSpan startTime, TimeSpan endTime, string parameter)
        {
            if (startDate < endDate)
                return;
                
            if (startDate == endDate)
            {
                if (startTime <= endTime)
                    return;
            }

            throw new ArgumentOutOfRangeException(parameter, "End must be greater or equal to start");
        }
    }
}
