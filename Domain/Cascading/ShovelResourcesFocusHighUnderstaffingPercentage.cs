using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ShovelResourcesFocusHighUnderstaffingPercentage : IShovelResourcesPerActivityIntervalSkillGroup
	{
		private readonly AddResourcesToSubSkillsFocusHighUnderstaffingPercentage _addResourcesToSubSkillsFocusHighUnderstaffingPercentage;
		private readonly ReducePrimarySkillResourcesPercentageDistribution _reducePrimarySkillResourcesPercentageDistribution;

		public ShovelResourcesFocusHighUnderstaffingPercentage(AddResourcesToSubSkillsFocusHighUnderstaffingPercentage addResourcesToSubSkillsFocusHighUnderstaffingPercentage,
																ReducePrimarySkillResourcesPercentageDistribution reducePrimarySkillResourcesPercentageDistribution)
		{
			_addResourcesToSubSkillsFocusHighUnderstaffingPercentage = addResourcesToSubSkillsFocusHighUnderstaffingPercentage;
			_reducePrimarySkillResourcesPercentageDistribution = reducePrimarySkillResourcesPercentageDistribution;
		}

		public void Execute(ShovelResourcesState state, ISkillStaffPeriodHolder skillStaffPeriodHolder, IActivity activity, DateTimePeriod interval, CascadingSkillGroup skillGroup)
		{
			_addResourcesToSubSkillsFocusHighUnderstaffingPercentage.Execute(state, skillStaffPeriodHolder, skillGroup, interval);
			_reducePrimarySkillResourcesPercentageDistribution.Execute(skillStaffPeriodHolder, skillGroup.PrimarySkills, interval, state.ResourcesMoved);
		}
	}
}