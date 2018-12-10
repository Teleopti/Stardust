using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IScheduleDayForPerson
	{
		IScheduleDay ForPerson(IPerson person, DateOnly date);
	}
}