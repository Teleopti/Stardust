using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleRangePersisterWithConflictSolve : IScheduleRangePersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IScheduleRangeConflictCollector _scheduleRangeConflictCollector;
		private readonly IInitiatorIdentifier _initiatorIdentifier;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IClearScheduleEvents _clearScheduleEvents;
		private readonly PersistScheduleChanges _persistScheduleChanges;

		public ScheduleRangePersisterWithConflictSolve(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService, IScheduleRangeConflictCollector scheduleRangeConflictCollector, IInitiatorIdentifier initiatorIdentifier, IScheduleStorage scheduleStorage, IClearScheduleEvents clearScheduleEvents, PersistScheduleChanges persistScheduleChanges)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_differenceCollectionService = differenceCollectionService;
			_scheduleRangeConflictCollector = scheduleRangeConflictCollector;
			_initiatorIdentifier = initiatorIdentifier;
			_scheduleStorage = scheduleStorage;
			_clearScheduleEvents = clearScheduleEvents;
			_persistScheduleChanges = persistScheduleChanges;
		}

		public SchedulePersistResult Persist(IScheduleRange scheduleRange, DateOnlyPeriod period)
		{
			var diff = scheduleRange.DifferenceSinceSnapshot(_differenceCollectionService, period);
			if (diff.IsEmpty())
				return new SchedulePersistResult
				{
					InitiatorIdentifier = _initiatorIdentifier
				};
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(scheduleRange.Person);
				var solved = solveConflicts(uow, diff, scheduleRange);
				var unsavedWithoutConflicts = new DifferenceCollection<IPersistableScheduleData>();
				diff.Except(solved).ForEach(x => unsavedWithoutConflicts.Add(x));

				_clearScheduleEvents.Execute(diff);
				_persistScheduleChanges.Execute(unsavedWithoutConflicts, (IUnvalidatedScheduleRangeUpdate) scheduleRange);
				var modifiedRoots = uow.PersistAll(_initiatorIdentifier);

				return new SchedulePersistResult
				{
					InitiatorIdentifier = _initiatorIdentifier,
					PersistConflicts = new List<PersistConflict>(),
					ModifiedRoots = modifiedRoots
				};
			}
		}

		private IEnumerable<DifferenceCollectionItem<IPersistableScheduleData>> solveConflicts(IUnitOfWork uow, IDifferenceCollection<IPersistableScheduleData> diff, IScheduleRange scheduleRange)
		{
			var conflicts = _scheduleRangeConflictCollector.GetConflicts(diff, scheduleRange).ToArray();
			var result = new List<DifferenceCollectionItem<IPersistableScheduleData>>();
			foreach (var conflict in conflicts)
			{
				switch (conflict.ClientVersion.Status)
				{
					case DifferenceStatus.Added:
						if (conflict.DatabaseVersion != null) // Someone else added, apply our changes to that object
						{
							(scheduleRange as ScheduleRange)?.SolveConflictBecauseOfExternalInsert(conflict.DatabaseVersion, false);
							conflict.ClientVersion.CurrentItem.SetId(conflict.DatabaseVersion.Id);
							uow.Merge(conflict.ClientVersion.CurrentItem);
						}
						break;
					case DifferenceStatus.Modified:
						if (conflict.DatabaseVersion != null) // Someone else modified, apply our changes to that object
						{
							(scheduleRange as ScheduleRange)?.SolveConflictBecauseOfExternalUpdate(conflict.DatabaseVersion, false);
							uow.Merge(conflict.ClientVersion.CurrentItem);
						}
						else // Someone deleted, we need to add instead of modify
						{
							(scheduleRange as ScheduleRange)?.SolveConflictBecauseOfExternalDeletion(conflict.ClientVersion.CurrentItem.Id.Value, false);
							conflict.ClientVersion.CurrentItem.SetId(null);
							_scheduleStorage.Add(conflict.ClientVersion.CurrentItem);
						}
						break;
					case DifferenceStatus.Deleted:
						if (conflict.DatabaseVersion != null) // Someone else modified, remove it
						{
							(scheduleRange as ScheduleRange)?.SolveConflictBecauseOfExternalUpdate(conflict.DatabaseVersion, false);
							_scheduleStorage.Remove(conflict.DatabaseVersion);
						}
						break;
				}
				result.Add(conflict.ClientVersion);
			}
			return result;
		}
	}
}