using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class DateOnlyAsDateTimePeriod : IDateOnlyAsDateTimePeriod
    {
        private readonly DateOnly _dateOnly;
        private readonly TimeZoneInfo _sourceTimeZone;
        private DateTimePeriod? _period;

        public DateOnlyAsDateTimePeriod(DateOnly dateOnly, TimeZoneInfo sourceTimeZone)
        {
            _dateOnly = dateOnly;
            _sourceTimeZone = sourceTimeZone;
        }

        public DateOnly DateOnly => _dateOnly;

	    public DateTimePeriod Period()
        {
            if(!_period.HasValue)
            {
				_period = _dateOnly.ToDateTimePeriod(_sourceTimeZone);                
            }
            return _period.Value;
        }

	    public TimeZoneInfo TimeZone()
	    {
		    return _sourceTimeZone;
	    }

	    public bool Equals(IDateOnlyAsDateTimePeriod other)
	    {
			return other is DateOnlyAsDateTimePeriod otherCasted && otherCasted._dateOnly == _dateOnly &&
				   otherCasted._sourceTimeZone.Id == _sourceTimeZone.Id;
	    }
    }

	public interface IDateOnlyPeriodAsDateTimePeriod
	{
		DateOnlyPeriod DateOnlyPeriod { get; }
		DateTimePeriod Period();
		DateTimePeriod Period(TimeZoneInfo sourceTimeZone);
	}

	public class DateOnlyPeriodAsDateTimePeriod : IDateOnlyPeriodAsDateTimePeriod
	{
		private readonly DateOnlyPeriod _dateOnlyPeriod;
		private readonly TimeZoneInfo _sourceTimeZone;
		private DateTimePeriod? _period;

		public DateOnlyPeriodAsDateTimePeriod(DateOnlyPeriod dateOnlyPeriod, TimeZoneInfo sourceTimeZone)
		{
			_dateOnlyPeriod = dateOnlyPeriod;
			_sourceTimeZone = sourceTimeZone;
		}

		public DateOnlyPeriod DateOnlyPeriod
		{
			get { return _dateOnlyPeriod; }
		}

		public DateTimePeriod Period()
		{
			if (!_period.HasValue)
			{
				_period = _dateOnlyPeriod.ToDateTimePeriod(_sourceTimeZone);
			}
			return _period.Value;
		}

         public DateTimePeriod Period(TimeZoneInfo sourceTimeZone)
        {
            _period = _dateOnlyPeriod.ToDateTimePeriod(sourceTimeZone);
            return _period.Value;
        }
	}
}