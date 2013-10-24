using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleRangeConflictCollector : IScheduleRangeConflictCollector
	{
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IReassociateDataForSchedules _reassociateDataForSchedules;
		private readonly ILazyLoadingManager _lazyLoadingManager;

		public ScheduleRangeConflictCollector(IScheduleRepository scheduleRepository,
																					IPersonAssignmentRepository personAssignmentRepository,
																					IReassociateDataForSchedules reassociateDataForSchedules,
																					ILazyLoadingManager lazyLoadingManager)
		{
			_scheduleRepository = scheduleRepository;
			_personAssignmentRepository = personAssignmentRepository;
			_reassociateDataForSchedules = reassociateDataForSchedules;
			_lazyLoadingManager = lazyLoadingManager;
		}

		public IEnumerable<PersistConflict> GetConflicts(IDifferenceCollection<IPersistableScheduleData> differences, IScenario scenario, DateOnlyPeriod period)
		{
			_reassociateDataForSchedules.ReassociateDataForAllPeople();
			var uow = _scheduleRepository.UnitOfWork;

			var modifiedAndDeletedEntities = from e in differences
																			 where e.Status != DifferenceStatus.Added
																			 select e;
			var conflictingEntities = from e in modifiedAndDeletedEntities
																let inMemoryEntity = e.OriginalItem
																let databaseVersion = uow.DatabaseVersion(inMemoryEntity)
																where inMemoryEntity.Version != databaseVersion
																select e;
			var persistConflicts = (from e in conflictingEntities
														let inMemoryEntity = e.OriginalItem
														let databaseEntity = _scheduleRepository.LoadScheduleDataAggregate(inMemoryEntity.GetType(), inMemoryEntity.Id.Value)
														select makePersistConflict(e, databaseEntity)).ToList();

			//reuse these also above
			var personAssignmentsInDb = _personAssignmentRepository.FetchDatabaseVersions(period, scenario);
			foreach (var diffItem in differences)
			{
				if (diffItem.Status == DifferenceStatus.Added)
				{
					var currentAss = diffItem.CurrentItem as IPersonAssignment;
					if(currentAss==null)
						continue;
					foreach (var assignmentInDb in personAssignmentsInDb)
					{
						if (assignmentInDb.EqualWith(currentAss))
						{
							persistConflicts.Add(makePersistConflict(diffItem, _scheduleRepository.LoadScheduleDataAggregate(typeof(IPersonAssignment), assignmentInDb.Id)));
						}
					}
				}
			}

			return persistConflicts;
		}

		private PersistConflict makePersistConflict(DifferenceCollectionItem<IPersistableScheduleData> clientVersion, IPersistableScheduleData databaseVersion)
		{
			if (databaseVersion != null)
			{
				_lazyLoadingManager.Initialize(databaseVersion.Person);
				_lazyLoadingManager.Initialize(databaseVersion.UpdatedBy);
			}
			return new PersistConflict(clientVersion, databaseVersion);
		}
	}
}