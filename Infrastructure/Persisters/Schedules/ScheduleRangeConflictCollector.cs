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

		public IEnumerable<PersistConflict> GetConflicts(IDifferenceCollection<IPersistableScheduleData> differences, IScheduleParameters scheduleParameters)
		{
			_reassociateDataForSchedules.ReassociateDataFor(scheduleParameters.Person);
			var dateOnlyPeriod = scheduleParameters.Period.ToDateOnlyPeriod(scheduleParameters.Person.PermissionInformation.DefaultTimeZone());
			var personAssignmentsInDb = _personAssignmentRepository.FetchDatabaseVersions(dateOnlyPeriod, scheduleParameters.Scenario);

			var uow = _scheduleRepository.UnitOfWork;

			var modifiedAndDeletedEntities = from e in differences
			                                 where e.Status != DifferenceStatus.Added
			                                 select e;
			var persistConflicts = new List<PersistConflict>();
			foreach (var diffItem in modifiedAndDeletedEntities)
			{
				var inMemoryEntity = diffItem.OriginalItem as IPersistableScheduleData;
				if (inMemoryEntity == null) continue;
				var inMemoryVersion = inMemoryEntity as IVersioned;
				if(inMemoryEntity==null)
					continue;

				int? databaseVersion;
				var inMemoryEntityAsAssignment = inMemoryEntity as IPersonAssignment;
				if (inMemoryEntityAsAssignment != null)
				{
					var assInDbVersion = personAssignmentsInDb.FirstOrDefault(p => p.EqualWith(inMemoryEntityAsAssignment));
					if (assInDbVersion != null)
					{
						databaseVersion = assInDbVersion.Version;						
					}
					else
					{
						databaseVersion = null;
					}
				}
				else
				{
					databaseVersion = uow.DatabaseVersion(inMemoryEntity);
				}
				if (inMemoryVersion.Version != databaseVersion)
				{
					var databaseEntity = _scheduleRepository.LoadScheduleDataAggregate(inMemoryEntity.GetType(), inMemoryEntity.Id.Value);
					persistConflicts.Add(makePersistConflict(diffItem, databaseEntity));
				}
			}



			//var conflictingEntities = from e in modifiedAndDeletedEntities
			//													let inMemoryEntity = e.OriginalItem
			//													let databaseVersion = uow.DatabaseVersion(inMemoryEntity)
			//													where inMemoryEntity.Version != databaseVersion
			//													select e;
			//var persistConflicts = (from e in conflictingEntities
			//											let inMemoryEntity = e.OriginalItem
			//											let databaseEntity = _scheduleRepository.LoadScheduleDataAggregate(inMemoryEntity.GetType(), inMemoryEntity.Id.Value)
			//											select makePersistConflict(e, databaseEntity)).ToList();

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