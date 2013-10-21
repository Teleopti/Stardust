using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters 
{
	public class ScheduleDictionaryConflictCollector : IScheduleDictionaryConflictCollector 
	{
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly ILazyLoadingManager _lazyLoadingManager;
		private readonly IUserTimeZone _timeZone;

		public ScheduleDictionaryConflictCollector(
			IScheduleRepository scheduleRepository, 
			IPersonAssignmentRepository personAssignmentRepository,
			ILazyLoadingManager lazyLoadingManager,
			IUserTimeZone timeZone
			)
		{

			_scheduleRepository = scheduleRepository;
			_personAssignmentRepository = personAssignmentRepository;
			_lazyLoadingManager = lazyLoadingManager;
			_timeZone = timeZone;
		}

		public IEnumerable<IPersistConflict> GetConflicts(IScheduleDictionary scheduleDictionary, IOwnMessageQueue messageQueueUpdater)
		{
			messageQueueUpdater.ReassociateDataWithAllPeople();

			var entityDifferencesSinceLoad = scheduleDictionary.DifferenceSinceSnapshot();
			if (entityDifferencesSinceLoad == null)
			{
				return new IPersistConflict[] {};
			}

			var uow = _scheduleRepository.UnitOfWork;

			var modifiedAndDeletedEntities = from e in entityDifferencesSinceLoad
											 where e.Status != DifferenceStatus.Added
											 select e;

			var conflictingEntities = from e in modifiedAndDeletedEntities
									  let inMemoryEntity = e.OriginalItem
									  let databaseVersion = uow.DatabaseVersion(inMemoryEntity)
									  where inMemoryEntity.Version != databaseVersion
									  select e;

			var conflictObjects = from e in conflictingEntities
			                      let inMemoryEntity = e.OriginalItem
			                      let databaseEntity =
			                      	_scheduleRepository.LoadScheduleDataAggregate(inMemoryEntity.GetType(),
			                      	                                              inMemoryEntity.Id.Value)
								  select MakePersistConflict(e, databaseEntity);



			var personAssignmentsInDb = _personAssignmentRepository.Find(
				scheduleDictionary.Period.LoadedPeriod().ToDateOnlyPeriod(_timeZone.TimeZone()), 
				scheduleDictionary.Scenario);

			var addedPersonAssignments = from e in entityDifferencesSinceLoad
			                             let pa = e.CurrentItem as IPersonAssignment
			                             where
				                             e.Status == DifferenceStatus.Added &&
				                             pa != null
			                             select new
				                             {
					                             Diff = e, 
												 CurrentPersonAssignment = pa
				                             };

			var conflictingAddedPersonAssignments = from e in addedPersonAssignments
			                                        let inMemoryEntity = e.CurrentPersonAssignment
			                                        let databaseEntity = personAssignmentsInDb
				                                        .SingleOrDefault(pa => pa.Equals(e.CurrentPersonAssignment))
			                                        where databaseEntity != null
			                                        select MakePersistConflict(e.Diff, databaseEntity);

			return conflictObjects.Union(conflictingAddedPersonAssignments).ToArray();
		}

		private IPersistConflict MakePersistConflict(DifferenceCollectionItem<IPersistableScheduleData> clientVersion, IPersistableScheduleData databaseVersion)
		{
			if (databaseVersion != null)
			{
				_lazyLoadingManager.Initialize(databaseVersion.Person);
				_lazyLoadingManager.Initialize(databaseVersion.UpdatedBy);
			}
			return new PersistConflictOldAndWillBeDeleted { ClientVersion = clientVersion, DatabaseVersion = databaseVersion };
		}
	}
}