using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class DateOnlyAsDateTimePeriod : IDateOnlyAsDateTimePeriod
    {
        private readonly DateOnly _dateOnly;
        private readonly ICccTimeZoneInfo _sourceTimeZone;
        private DateTimePeriod? _period;

        public DateOnlyAsDateTimePeriod(DateOnly dateOnly, ICccTimeZoneInfo sourceTimeZone)
        {
            _dateOnly = dateOnly;
            _sourceTimeZone = sourceTimeZone;
        }

        public DateOnly DateOnly
        {
            get { return _dateOnly; }
        }

        public DateTimePeriod Period()
        {
            if(!_period.HasValue)
            {
                _period = new DateOnlyPeriod(_dateOnly, _dateOnly).ToDateTimePeriod(_sourceTimeZone);                
            }
            return _period.Value;
        }
    }

	public interface IDateOnlyPeriodAsDateTimePeriod
	{
		DateOnlyPeriod DateOnlyPeriod { get; }
		DateTimePeriod Period();
	}

	public class DateOnlyPeriodAsDateTimePeriod : IDateOnlyPeriodAsDateTimePeriod
	{
		private readonly DateOnlyPeriod _dateOnlyPeriod;
		private readonly ICccTimeZoneInfo _sourceTimeZone;
		private DateTimePeriod? _period;

		public DateOnlyPeriodAsDateTimePeriod(DateOnlyPeriod dateOnlyPeriod, ICccTimeZoneInfo sourceTimeZone)
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
	}
}