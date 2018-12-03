using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleRangePersister : IScheduleRangePersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IScheduleRangeConflictCollector _scheduleRangeConflictCollector;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IInitiatorIdentifier _initiatorIdentifier;
		private readonly IClearScheduleEvents _clearScheduleEvents;

		public ScheduleRangePersister(
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService,
			IScheduleRangeConflictCollector scheduleRangeConflictCollector,
			IScheduleDifferenceSaver scheduleDifferenceSaver,
			IInitiatorIdentifier initiatorIdentifier,
			IClearScheduleEvents clearScheduleEvents)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_differenceCollectionService = differenceCollectionService;
			_scheduleRangeConflictCollector = scheduleRangeConflictCollector;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_initiatorIdentifier = initiatorIdentifier;
			_clearScheduleEvents = clearScheduleEvents;
		}

		public SchedulePersistResult Persist(IScheduleRange scheduleRange, DateOnlyPeriod period)
		{
			var diff = scheduleRange.DifferenceSinceSnapshot(_differenceCollectionService, period);
			if (diff.IsEmpty())
			{
				return new SchedulePersistResult
				{
					InitiatorIdentifier = _initiatorIdentifier
				};
			}
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var conflicts = _scheduleRangeConflictCollector.GetConflicts(diff, scheduleRange).ToArray();
				if (conflicts.IsEmpty())
				{
					_clearScheduleEvents.Execute(diff);
					_scheduleDifferenceSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate)scheduleRange);
				}
				var modifiedRoots = uow.PersistAll(_initiatorIdentifier);
				return new SchedulePersistResult
				{ 
					InitiatorIdentifier = _initiatorIdentifier,
					PersistConflicts = conflicts,
					ModifiedRoots = modifiedRoots
				};
			}
		}
	}

	
}