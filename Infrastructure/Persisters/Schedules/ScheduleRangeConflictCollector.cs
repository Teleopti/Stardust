using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public interface IScheduleRangeConflictCollector
	{
		IEnumerable<PersistConflict> GetConflicts(IScheduleRange scheduleRange);
	}

	public class ScheduleRangeConflictCollector : IScheduleRangeConflictCollector
	{
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IScheduleRepository _scheduleRepository;

		public ScheduleRangeConflictCollector(IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService, IScheduleRepository scheduleRepository)
		{
			_differenceCollectionService = differenceCollectionService;
			_scheduleRepository = scheduleRepository;
		}

		public IEnumerable<PersistConflict> GetConflicts(IScheduleRange scheduleRange)
		{
			var uow = _scheduleRepository.UnitOfWork;
			var diff = scheduleRange.DifferenceSinceSnapshot(_differenceCollectionService);

			var modifiedAndDeletedEntities = from e in diff
																			 where e.Status != DifferenceStatus.Added
																			 select e;
			var conflictingEntities = from e in modifiedAndDeletedEntities
																let inMemoryEntity = e.OriginalItem
																let databaseVersion = uow.DatabaseVersion(inMemoryEntity)
																where inMemoryEntity.Version != databaseVersion
																select e;
			var persistConflicts = from e in conflictingEntities
														let inMemoryEntity = e.OriginalItem
														let databaseEntity = _scheduleRepository.LoadScheduleDataAggregate(inMemoryEntity.GetType(), inMemoryEntity.Id.Value)
														select makePersistConflict(e, databaseEntity);
			return persistConflicts.ToArray();
		}

		private static PersistConflict makePersistConflict(DifferenceCollectionItem<IPersistableScheduleData> clientVersion, IPersistableScheduleData databaseVersion)
		{
			//if (databaseVersion != null)
			//{
			//	_lazyLoadingManager.Initialize(databaseVersion.Person);
			//	_lazyLoadingManager.Initialize(databaseVersion.UpdatedBy);
			//}
			return new PersistConflict(clientVersion, databaseVersion);
		}
	}
}