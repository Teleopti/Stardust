using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IScheduleDifferenceSaver
	{
		void SaveChanges(IDifferenceCollection<IPersistableScheduleData> scheduleChanges,IUnvalidatedScheduleRangeUpdate stateInMemoryUpdater);
	}
}