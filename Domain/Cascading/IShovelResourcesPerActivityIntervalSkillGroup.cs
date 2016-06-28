using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	//TODO: remove me when #39463 is released/confirmed to work
	public interface IShovelResourcesPerActivityIntervalSkillGroup
	{
		void Execute(ShovelResourcesState state, ISkillStaffPeriodHolder skillStaffPeriodHolder, IActivity activity, DateTimePeriod interval, CascadingSkillGroup skillGroup);
	}
}