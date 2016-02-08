using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleRangeConflictCollector : IScheduleRangeConflictCollector
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IReassociateDataForSchedules _reassociateDataForSchedules;
		private readonly ILazyLoadingManager _lazyLoadingManager;

		public ScheduleRangeConflictCollector(
			IScheduleStorage scheduleStorage,
			IPersonAssignmentRepository personAssignmentRepository,
			IReassociateDataForSchedules reassociateDataForSchedules,
			ILazyLoadingManager lazyLoadingManager)
		{
			_scheduleStorage = scheduleStorage;
			_personAssignmentRepository = personAssignmentRepository;
			_reassociateDataForSchedules = reassociateDataForSchedules;
			_lazyLoadingManager = lazyLoadingManager;
		}

		public IEnumerable<PersistConflict> GetConflicts(IDifferenceCollection<IPersistableScheduleData> differences, IScheduleParameters scheduleParameters)
		{
			_reassociateDataForSchedules.ReassociateDataFor(scheduleParameters.Person);
			var dateOnlyPeriod = scheduleParameters.Period.ToDateOnlyPeriod(scheduleParameters.Person.PermissionInformation.DefaultTimeZone());
			var personAssignmentsInDb = _personAssignmentRepository.FetchDatabaseVersions(dateOnlyPeriod, scheduleParameters.Scenario, scheduleParameters.Person);

			var uow = _scheduleStorage.UnitOfWork;

			var modifiedAndDeletedEntities = from e in differences
			                                 where e.Status != DifferenceStatus.Added
			                                 select e;
			var persistConflicts = new List<PersistConflict>();
			foreach (var diffItem in modifiedAndDeletedEntities)
			{
				var inMemoryEntity = diffItem.OriginalItem;
				var inMemoryVersion = inMemoryEntity as IVersioned;
				if(inMemoryVersion==null)
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
					var databaseEntity = _scheduleStorage.LoadScheduleDataAggregate(inMemoryEntity.GetType(), inMemoryEntity.Id.Value);
					persistConflicts.Add(makePersistConflict(diffItem, databaseEntity));
				}
			}

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
							persistConflicts.Add(makePersistConflict(diffItem, _scheduleStorage.LoadScheduleDataAggregate(typeof(IPersonAssignment), assignmentInDb.Id)));
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