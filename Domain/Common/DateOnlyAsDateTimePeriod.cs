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
}