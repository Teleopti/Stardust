using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class PrimaryOrAllPersonSkillForNonOvertimeProvider
	{
		private readonly PrimaryGroupPersonSkillAggregator _primaryGroupPersonSkillAggregator;
		private readonly IGroupPersonSkillAggregator _allGroupPersonSkillAggregator;

		public PrimaryOrAllPersonSkillForNonOvertimeProvider(PrimaryGroupPersonSkillAggregator primaryGroupPersonSkillAggregator, IGroupPersonSkillAggregator allGroupPersonSkillAggregator)
		{
			_primaryGroupPersonSkillAggregator = primaryGroupPersonSkillAggregator;
			_allGroupPersonSkillAggregator = allGroupPersonSkillAggregator;
		}

		public IGroupPersonSkillAggregator SkillAggregator(IOvertimePreferences overtimePreferences)
		{
			return overtimePreferences.UseSkills == UseSkills.All ? 
				_allGroupPersonSkillAggregator : 
				_primaryGroupPersonSkillAggregator;
		}
	}
}