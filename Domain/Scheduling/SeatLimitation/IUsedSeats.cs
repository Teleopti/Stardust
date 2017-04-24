using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public interface IUsedSeats
	{
		double Fetch(ISkillStaffPeriod skillStaffPeriod);
	}
}