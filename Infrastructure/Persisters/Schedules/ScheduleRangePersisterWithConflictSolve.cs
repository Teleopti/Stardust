using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleRangePersisterWithConflictSolve : IScheduleRangePersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IScheduleRangeConflictCollector _scheduleRangeConflictCollector;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IInitiatorIdentifier _initiatorIdentifier;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IClearScheduleEvents _clearScheduleEvents;

		public ScheduleRangePersisterWithConflictSolve(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService, IScheduleRangeConflictCollector scheduleRangeConflictCollector, IScheduleDifferenceSaver scheduleDifferenceSaver, IInitiatorIdentifier initiatorIdentifier, IScheduleStorage scheduleStorage, IClearScheduleEvents clearScheduleEvents)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_differenceCollectionService = differenceCollectionService;
			_scheduleRangeConflictCollector = scheduleRangeConflictCollector;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_initiatorIdentifier = initiatorIdentifier;
			_scheduleStorage = scheduleStorage;
			_clearScheduleEvents = clearScheduleEvents;
		}

		public SchedulePersistResult Persist(IScheduleRange scheduleRange)
		{
			var diff = scheduleRange.DifferenceSinceSnapshot(_differenceCollectionService);
			if (diff.IsEmpty())
				return new SchedulePersistResult
				{
					InitiatorIdentifier = _initiatorIdentifier
				};
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var solved = solveConflicts(uow, diff, scheduleRange);
				var unsavedWithoutConflicts = new DifferenceCollection<IPersistableScheduleData>();
				diff.Where(x => !solved.SolvedConflicts.Contains(x)).ForEach(x => unsavedWithoutConflicts.Add(x));

				_clearScheduleEvents.Execute(diff);
				_scheduleDifferenceSaver.SaveChanges(unsavedWithoutConflicts, (IUnvalidatedScheduleRangeUpdate)scheduleRange);
				var modifiedRoots = uow.PersistAll(_initiatorIdentifier);

				return new SchedulePersistResult
				{
					InitiatorIdentifier = _initiatorIdentifier,
					PersistConflicts = new List<PersistConflict>(),
					ModifiedRoots = modifiedRoots.Union(solved.ModifiedRoots)
				};
			}
		}

		private solvedConflictsResult solveConflicts(IUnitOfWork uow, IDifferenceCollection<IPersistableScheduleData> diff, IScheduleRange scheduleRange)
		{
			var conflicts = _scheduleRangeConflictCollector.GetConflicts(diff, scheduleRange).ToArray();
			var result = new solvedConflictsResult();
			foreach (var conflict in conflicts)
			{
				switch (conflict.ClientVersion.Status)
				{
					case DifferenceStatus.Added:
						if (conflict.DatabaseVersion != null) // Someone else added, remove and re-add
						{
							_scheduleStorage.Remove(conflict.DatabaseVersion);
							result.ModifiedRoots.AddRange(uow.PersistAll(_initiatorIdentifier));
						}
						_scheduleStorage.Add(conflict.ClientVersion.CurrentItem);
						break;
					case DifferenceStatus.Modified:
						if (conflict.DatabaseVersion != null) // Someone else modified, remove and re-add
						{
							_scheduleStorage.Remove(conflict.DatabaseVersion);
							result.ModifiedRoots.AddRange(uow.PersistAll(_initiatorIdentifier));
						}
						// Either someone deleted the entry, or we did just above, so re-add
						conflict.ClientVersion.CurrentItem.SetId(null);
						_scheduleStorage.Add(conflict.ClientVersion.CurrentItem);
						break;
					case DifferenceStatus.Deleted:
						if (conflict.DatabaseVersion != null) // Someone else modified, remove it
						{
							_scheduleStorage.Remove(conflict.DatabaseVersion);
						}
						break;
				}
				result.SolvedConflicts.Add(conflict.ClientVersion);
			}
			return result;
		}

		private class solvedConflictsResult
		{
			public List<DifferenceCollectionItem<IPersistableScheduleData>> SolvedConflicts { get; }
			public List<IRootChangeInfo> ModifiedRoots { get; }

			public solvedConflictsResult()
			{
				SolvedConflicts = new List<DifferenceCollectionItem<IPersistableScheduleData>>();
				ModifiedRoots = new List<IRootChangeInfo>();
			}
		}
	}
}