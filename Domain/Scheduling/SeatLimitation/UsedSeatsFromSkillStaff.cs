using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class UsedSeatsFromSkillStaff : IUsedSeats
	{
		public double Fetch(ISkillStaffPeriod skillStaffPeriod)
		{
			return skillStaffPeriod.Payload.CalculatedUsedSeats;
		}
	}
}