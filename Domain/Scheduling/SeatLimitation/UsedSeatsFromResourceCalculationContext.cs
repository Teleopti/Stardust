using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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