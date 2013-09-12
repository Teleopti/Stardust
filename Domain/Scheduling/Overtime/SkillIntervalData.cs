using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeSkillIntervalData
	{
		DateTimePeriod Period { get; }
		double RelativeDifference { get; set; }
	}

	public class OvertimeSkillIntervalData : IOvertimeSkillIntervalData
	{
		public DateTimePeriod Period { get; private set; }
		public double RelativeDifference { get; set; }

		public OvertimeSkillIntervalData(DateTimePeriod dateTimePeriod, double relativeDifference)
		{
			Period = dateTimePeriod;
			RelativeDifference = relativeDifference;
		}
	}
}