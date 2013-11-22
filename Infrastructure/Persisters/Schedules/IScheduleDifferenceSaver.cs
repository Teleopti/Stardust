using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public interface IScheduleDifferenceSaver
	{
		void SaveChanges(IDifferenceCollection<IPersistableScheduleData> scheduleChanges, IUnvalidatedScheduleRangeUpdate stateInMemoryUpdater);
	}
}