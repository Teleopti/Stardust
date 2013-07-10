using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface ISkillIntervalData
	{
		DateTimePeriod Period { get; }
		double RelativeDifference { get; }
	}

	public class SkillIntervalData : ISkillIntervalData
	{
		public DateTimePeriod Period { get; private set; }
		public double RelativeDifference { get; private set; }

		public SkillIntervalData(DateTimePeriod dateTimePeriod, double relativeDifference)
		{
			Period = dateTimePeriod;
			RelativeDifference = relativeDifference;
		}
	}
}