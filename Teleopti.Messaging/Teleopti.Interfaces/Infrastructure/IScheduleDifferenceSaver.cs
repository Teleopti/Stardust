using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IScheduleDifferenceSaver
	{
		void SaveChanges(IDifferenceCollection<IPersistableScheduleData> scheduleChanges, IUnvalidatedScheduleRangeUpdate stateInMemoryUpdater);
	}
}