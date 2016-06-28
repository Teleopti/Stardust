using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	//TODO: remove me when #39463 is released/confirmed to work
	public class ShovelResourcesPercentageDistribution : IShovelResourcesPerActivityIntervalSkillGroup
	{
		private readonly AddResourcesToSubSkillsPercentageDistribution _addResourcesToSubSkillsPercentageDistribution;
		private readonly ReducePrimarySkillResourcesPercentageDistribution _reducePrimarySkillResourcesPercentageDistribution;

		public ShovelResourcesPercentageDistribution(AddResourcesToSubSkillsPercentageDistribution addResourcesToSubSkillsPercentageDistribution, 
			ReducePrimarySkillResourcesPercentageDistribution reducePrimarySkillResourcesPercentageDistribution)
		{
			_addResourcesToSubSkillsPercentageDistribution = addResourcesToSubSkillsPercentageDistribution;
			_reducePrimarySkillResourcesPercentageDistribution = reducePrimarySkillResourcesPercentageDistribution;
		}

		public void Execute(ShovelResourcesState state, ISkillStaffPeriodHolder skillStaffPeriodHolder, IActivity activity, DateTimePeriod interval, CascadingSkillGroup skillGroup)
		{
			_addResourcesToSubSkillsPercentageDistribution.Execute(state, skillStaffPeriodHolder, skillGroup, interval);
			_reducePrimarySkillResourcesPercentageDistribution.Execute(skillStaffPeriodHolder, skillGroup.PrimarySkills, interval, state.ResourcesMoved);
		}
	}
}