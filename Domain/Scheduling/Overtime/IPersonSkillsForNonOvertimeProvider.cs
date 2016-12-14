using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IPersonSkillsForNonOvertimeProvider
	{
		IGroupPersonSkillAggregator SkillAggregator();
	}
}