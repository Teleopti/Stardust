using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IPersonSkillsForNonOvertimeProvider
	{
		IGroupPersonSkillAggregator SkillAggregator(IOvertimePreferences overtimePreferences);
	}
}