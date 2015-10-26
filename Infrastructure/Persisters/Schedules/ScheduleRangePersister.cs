using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleRangePersister : IScheduleRangePersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IScheduleRangeConflictCollector _scheduleRangeConflictCollector;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IInitiatorIdentifier _initiatorIdentifier;

		public ScheduleRangePersister(
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService,
			IScheduleRangeConflictCollector scheduleRangeConflictCollector,
			IScheduleDifferenceSaver scheduleDifferenceSaver,
			IInitiatorIdentifier initiatorIdentifier)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_differenceCollectionService = differenceCollectionService;
			_scheduleRangeConflictCollector = scheduleRangeConflictCollector;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_initiatorIdentifier = initiatorIdentifier;
		}

		public SchedulePersistResult Persist(IScheduleRange scheduleRange)
		{
			var diff = scheduleRange.DifferenceSinceSnapshot(_differenceCollectionService);
			if (diff.IsEmpty())
			{
				return new SchedulePersistResult
				{
					InitiatorIdentifier = _initiatorIdentifier
				};
			}
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var conflicts = _scheduleRangeConflictCollector.GetConflicts(diff, scheduleRange);
				if (conflicts.IsEmpty())
				{
					_scheduleDifferenceSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate)scheduleRange);
				}
				var modifiedRoots = uow.PersistAll(_initiatorIdentifier);
				return new SchedulePersistResult()
				{
					InitiatorIdentifier = _initiatorIdentifier,
					PersistConflicts = conflicts,
					ModifiedRoots = modifiedRoots
				};
			}
		}
	}
}