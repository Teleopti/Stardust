using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters 
{
	public class ScheduleDictionaryConflictCollector : IScheduleDictionaryConflictCollector 
	{
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ILazyLoadingManager _lazyLoadingManager;

		public ScheduleDictionaryConflictCollector(IScheduleRepository scheduleRepository, ILazyLoadingManager lazyLoadingManager)
		{

			_scheduleRepository = scheduleRepository;
			_lazyLoadingManager = lazyLoadingManager;
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

			return conflictObjects.ToArray();
		}

		private IPersistConflict MakePersistConflict(DifferenceCollectionItem<IPersistableScheduleData> clientVersion, IPersistableScheduleData databaseVersion)
		{
			if (databaseVersion != null)
			{
				_lazyLoadingManager.Initialize(databaseVersion.Person);
				_lazyLoadingManager.Initialize(databaseVersion.UpdatedBy);
				_lazyLoadingManager.Initialize(databaseVersion.CreatedBy);
			}
			return new PersistConflict { ClientVersion = clientVersion, DatabaseVersion = databaseVersion };
		}
	}
}