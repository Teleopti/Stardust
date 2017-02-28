using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IScheduleRangeForPerson
	{
		IScheduleRange ForPerson(IPerson person);
	}
}