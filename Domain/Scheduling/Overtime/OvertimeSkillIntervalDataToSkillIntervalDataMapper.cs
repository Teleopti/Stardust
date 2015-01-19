using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeSkillIntervalDataToSkillIntervalDataMapper
	{
		ISkillIntervalData Map(IOvertimeSkillIntervalData source);
	}

	public class OvertimeSkillIntervalDataToSkillIntervalDataMapper : IOvertimeSkillIntervalDataToSkillIntervalDataMapper
	{
		public ISkillIntervalData Map(IOvertimeSkillIntervalData source)
		{
			var target = new SkillIntervalData(source.Period, source.ForecastedDemand, source.CurrentDemand, 0, null, null);
			return target;
		}

	}
}
