using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public interface IShovelResourcesPerActivityIntervalSkillGroup
	{
		void Execute(ShovelResourcesState state, ISkillStaffPeriodHolder skillStaffPeriodHolder, IActivity activity, DateTimePeriod interval, CascadingSkillGroup skillGroup);
	}
}