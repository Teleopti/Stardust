using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public class OvertimePeriodValue
    {
        public DateTimePeriod Period { get; private set; }
        public double Value { get; private set; }

        public OvertimePeriodValue(DateTimePeriod period, double value)
        {
            Period = period;
            Value = value;
        }
    }
}