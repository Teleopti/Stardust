using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_CascadingScheduleOvertimeOnPrimary_41318)]
	public class AllPersonSkillForNonOvertimeProvider : IPersonSkillsForNonOvertimeProvider
	{
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public AllPersonSkillForNonOvertimeProvider(IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}

		public IGroupPersonSkillAggregator SkillAggregator()
		{
			return _groupPersonSkillAggregator;
		}
	}
}