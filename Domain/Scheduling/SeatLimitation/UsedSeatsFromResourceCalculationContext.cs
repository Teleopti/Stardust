using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class UsedSeatsFromResourceCalculationContext : IUsedSeats
	{
		public double Fetch(ISkillStaffPeriod skillStaffPeriod)
		{
			return ResourceCalculationContext.Fetch().ActivityResourcesWhereSeatRequired(skillStaffPeriod.SkillDay.Skill, skillStaffPeriod.Period);
		}
	}
}