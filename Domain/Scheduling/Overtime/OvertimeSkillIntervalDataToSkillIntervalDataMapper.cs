using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class OvertimeSkillIntervalDataToSkillIntervalDataMapper
	{
		public ISkillIntervalData Map(IOvertimeSkillIntervalData source)
		{
			var target = new SkillIntervalData(source.Period, source.ForecastedDemand, source.CurrentDemand, 0, null, null);
			return target;
		}

	}
}
