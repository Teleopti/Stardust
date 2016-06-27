using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	//TODO: can be removed when we know what impl to use
	public interface IShovelResourcesPerActivityIntervalSkillGroup
	{
		void Execute(ShovelResourcesState state, ISkillStaffPeriodHolder skillStaffPeriodHolder, IActivity activity, DateTimePeriod interval, CascadingSkillGroup skillGroup);
	}
}