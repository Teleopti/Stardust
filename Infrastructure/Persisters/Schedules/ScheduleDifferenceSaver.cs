using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleDifferenceSaver : IScheduleDifferenceSaver
	{
		private readonly IScheduleDayDifferenceSaver _scheduleDayDifferenceSaver;
		private readonly PersistScheduleChanges _persistScheduleChanges;

		public ScheduleDifferenceSaver(IScheduleDayDifferenceSaver scheduleDayDifferenceSaver,
			PersistScheduleChanges persistScheduleChanges)
		{
			_scheduleDayDifferenceSaver = scheduleDayDifferenceSaver;
			_persistScheduleChanges = persistScheduleChanges;
		}

		public void SaveChanges(IDifferenceCollection<IPersistableScheduleData> scheduleChanges, IUnvalidatedScheduleRangeUpdate stateInMemoryUpdater)
		{
			_scheduleDayDifferenceSaver.SaveDifferences((IScheduleRange)stateInMemoryUpdater);
			_persistScheduleChanges.Execute(scheduleChanges, stateInMemoryUpdater);
		}
	}
}