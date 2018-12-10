using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimePeriodValue
	{
		DateTimePeriod Period { get; }
		double Value { get; }
	}

	public class OvertimePeriodValue : IOvertimePeriodValue
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