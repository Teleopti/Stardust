using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public interface IScheduleRangePersister
	{
		SchedulePersistResult Persist(IScheduleRange scheduleRange, DateOnlyPeriod period);
	}
}