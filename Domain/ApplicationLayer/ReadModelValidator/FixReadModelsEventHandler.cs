using System;
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
		private readonly IReadModelValidator _validator;
		private readonly IReadModelValidationResultPersister _persister;
		private readonly IProjectionVersionPersister _projectionVersionPersister;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly IPersonScheduleDayReadModelPersister _personScheduleDayReadModelPersister;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private readonly ICurrentScenario _currentScenario;

		public FixReadModelsEventHandler(IReadModelValidator validator,
			IReadModelValidationResultPersister persister, IProjectionVersionPersister projectionVersionPersister,
			IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, ICurrentScenario currentScenario, IPersonScheduleDayReadModelPersister personScheduleDayReadModelPersister, IPersonAssignmentRepository personAssignmentRepository, IScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_validator = validator;
			_persister = persister;
			_projectionVersionPersister = projectionVersionPersister;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
			_currentScenario = currentScenario;
			_personScheduleDayReadModelPersister = personScheduleDayReadModelPersister;
			_personAssignmentRepository = personAssignmentRepository;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		public void Handle(FixReadModelsEvent @event)
		{
			if (@event.Targets.Contains(ValidateReadModelType.ScheduleProjectionReadOnly))
			{
				var invalidRecords = _persister.LoadAllInvalidScheduleProjectionReadOnly();

				foreach(var record in invalidRecords)
				{
					var date = new DateOnly(record.Date);
					var version =
						_projectionVersionPersister.LockAndGetVersions(record.PersonId,date,date).FirstOrDefault()?.Version;

					_scheduleProjectionReadOnlyPersister.BeginAddingSchedule(date,_currentScenario.Current().Id.GetValueOrDefault(),
						record.PersonId,version ?? 0);

					var readModels = _validator.BuildReadModelScheduleProjectionReadOnly(record.PersonId,date);
					readModels.ForEach(_scheduleProjectionReadOnlyPersister.AddActivity);
				}
			}

			if (@event.Targets.Contains(ValidateReadModelType.PersonScheduleDay))
			{
				var invalidRecords = _persister.LoadAllInvalidPersonScheduleDay();

				foreach (var record in invalidRecords)
				{
					var date = new DateOnly(record.Date);
					var readModel = _validator.BuildReadModelPersonScheduleDay(record.PersonId, date);
					readModel.ScheduleLoadTimestamp = _personAssignmentRepository.GetScheduleLoadedTime();
					_personScheduleDayReadModelPersister.SaveReadModel(readModel, false);
				}
			}

			if(@event.Targets.Contains(ValidateReadModelType.ScheduleDay))
			{
				var invalidRecords = _persister.LoadAllInvalidPersonScheduleDay();

				foreach (var record in invalidRecords)
				{
					var date = new DateOnly(record.Date);
					var readModel = _validator.BuildReadModelScheduleDay(record.PersonId, date);
					_scheduleDayReadModelRepository.SaveReadModel(readModel);
				}
			}
		}
	}
}
