using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class FixReadModelsEventHandler : IHandleEvent<FixReadModelsEvent>,
		IRunOnStardust
	{
		private readonly IReadModelValidationResultPersister _persister;
		private readonly IProjectionVersionPersister _projectionVersionPersister;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly IPersonScheduleDayReadModelPersister _personScheduleDayReadModelPersister;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		private readonly IReadModelPersonScheduleDayValidator _readModelPersonScheduleDayValidator;
		private readonly IReadModelScheduleProjectionReadOnlyValidator _readModelScheduleProjectionReadOnlyValidator;
		private readonly IReadModelScheduleDayValidator _readModelScheduleDayValidator;

		public FixReadModelsEventHandler(IReadModelValidationResultPersister persister, IProjectionVersionPersister projectionVersionPersister,
			IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, ICurrentScenario currentScenario, IPersonScheduleDayReadModelPersister personScheduleDayReadModelPersister, IPersonAssignmentRepository personAssignmentRepository, IScheduleDayReadModelRepository scheduleDayReadModelRepository, IReadModelPersonScheduleDayValidator readModelPersonScheduleDayValidator, IReadModelScheduleProjectionReadOnlyValidator readModelScheduleProjectionReadOnlyValidator, IReadModelScheduleDayValidator readModelScheduleDayValidator, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_persister = persister;
			_projectionVersionPersister = projectionVersionPersister;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
			_currentScenario = currentScenario;
			_personScheduleDayReadModelPersister = personScheduleDayReadModelPersister;
			_personAssignmentRepository = personAssignmentRepository;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_readModelPersonScheduleDayValidator = readModelPersonScheduleDayValidator;
			_readModelScheduleProjectionReadOnlyValidator = readModelScheduleProjectionReadOnlyValidator;
			_readModelScheduleDayValidator = readModelScheduleDayValidator;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void Handle(FixReadModelsEvent @event)
		{
			if (@event.Targets.HasFlag(ValidateReadModelType.ScheduleProjectionReadOnly))
			{
				IList<ReadModelValidationResult> invalidRecords;
				using(var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					invalidRecords = _persister.LoadAllInvalidScheduleProjectionReadOnly().ToList();
				}

				foreach(var records in invalidRecords.Batch(50))
				{
					using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						foreach (var record in records)
						{
							var date = new DateOnly(record.Date);
							var version =
								_projectionVersionPersister.LockAndGetVersions(record.PersonId, date, date).FirstOrDefault()?.Version;

							_scheduleProjectionReadOnlyPersister.BeginAddingSchedule(date, _currentScenario.Current().Id.GetValueOrDefault(),
								record.PersonId, version ?? 0);

							var readModels = _readModelScheduleProjectionReadOnlyValidator.Build(record.PersonId, date);
							readModels.ForEach(_scheduleProjectionReadOnlyPersister.AddActivity);
						}
						uow.PersistAll();
					}
				}
			}

			if (@event.Targets.HasFlag(ValidateReadModelType.PersonScheduleDay))
			{				
				IList<ReadModelValidationResult> invalidRecords;
				using(var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					invalidRecords = _persister.LoadAllInvalidPersonScheduleDay().ToList();
				}

				foreach(var records in invalidRecords.Batch(50))
				{
					using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						foreach (var record in records)
						{
							var date = new DateOnly(record.Date);
							var readModel = _readModelPersonScheduleDayValidator.Build(record.PersonId, date);
							if (readModel == null)
							{
								_personScheduleDayReadModelPersister.DeleteReadModel(record.PersonId, date);
								continue;
							}
							readModel.ScheduleLoadTimestamp = _personAssignmentRepository.GetScheduleLoadedTime();
							_personScheduleDayReadModelPersister.SaveReadModel(readModel, false);
						}
						uow.PersistAll();
					}
				}
			}

			if(@event.Targets.HasFlag(ValidateReadModelType.ScheduleDay))
			{				
				IList<ReadModelValidationResult> invalidRecords;
				using(var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					invalidRecords = _persister.LoadAllInvalidScheduleDay().ToList();
				}

				foreach(var records in invalidRecords.Batch(50))
				{
					using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						foreach (var record in records)
						{
							var date = new DateOnly(record.Date);
							var readModel = _readModelScheduleDayValidator.Build(record.PersonId, date);
							if (readModel == null)
							{
								_scheduleDayReadModelRepository.ClearPeriodForPerson(new DateOnlyPeriod(date, date), record.PersonId);
								continue;
							}
							_scheduleDayReadModelRepository.SaveReadModel(readModel);
						}
						uow.PersistAll();
					}
				}
			}
		}
	}
}
