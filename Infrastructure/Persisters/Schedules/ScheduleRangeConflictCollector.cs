using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleRangeConflictCollector : IScheduleRangeConflictCollector
	{
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IOwnMessageQueue _ownMessageQueue;
		private readonly ILazyLoadingManager _lazyLoadingManager;

		public ScheduleRangeConflictCollector(IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService, 
																					IScheduleRepository scheduleRepository,
																					IPersonAssignmentRepository personAssignmentRepository,
																					IOwnMessageQueue ownMessageQueue,
																					ILazyLoadingManager lazyLoadingManager)
		{
			_differenceCollectionService = differenceCollectionService;
			_scheduleRepository = scheduleRepository;
			_personAssignmentRepository = personAssignmentRepository;
			_ownMessageQueue = ownMessageQueue;
			_lazyLoadingManager = lazyLoadingManager;
		}


		//TA IN DIFFEN HÄR ISTÄLLET SEN!
		public IEnumerable<PersistConflict> GetConflicts(IScheduleRange scheduleRange)
		{
			_ownMessageQueue.ReassociateDataWithAllPeople();
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
			var persistConflicts = (from e in conflictingEntities
														let inMemoryEntity = e.OriginalItem
														let databaseEntity = _scheduleRepository.LoadScheduleDataAggregate(inMemoryEntity.GetType(), inMemoryEntity.Id.Value)
														select makePersistConflict(e, databaseEntity)).ToList();

			//reuse these also above
			var personAssignmentsInDb = _personAssignmentRepository.FetchDatabaseVersions(scheduleRange.Period.ToDateOnlyPeriod(scheduleRange.Person.PermissionInformation.DefaultTimeZone()), scheduleRange.Scenario);
			foreach (var diffItem in diff)
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