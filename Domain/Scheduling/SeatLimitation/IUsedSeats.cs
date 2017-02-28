using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public interface IUsedSeats
	{
		[RemoveMeWithToggle("Change parameter to ISkill and DateTimePeriod when toggle is removed", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		double Fetch(ISkillStaffPeriod skillStaffPeriod);
	}
}