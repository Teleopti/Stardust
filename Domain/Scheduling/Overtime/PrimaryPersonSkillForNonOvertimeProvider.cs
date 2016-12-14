using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class PrimaryPersonSkillForNonOvertimeProvider : IPersonSkillsForNonOvertimeProvider
	{
		private readonly PrimaryGroupPersonSkillAggregator _primaryGroupPersonSkillAggregator;

		public PrimaryPersonSkillForNonOvertimeProvider(PrimaryGroupPersonSkillAggregator primaryGroupPersonSkillAggregator)
		{
			_primaryGroupPersonSkillAggregator = primaryGroupPersonSkillAggregator;
		}

		public IGroupPersonSkillAggregator SkillAggregator()
		{
			return _primaryGroupPersonSkillAggregator;
		}
	}
}