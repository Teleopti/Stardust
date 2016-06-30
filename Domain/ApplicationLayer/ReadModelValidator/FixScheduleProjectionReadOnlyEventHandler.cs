using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class FixScheduleProjectionReadOnlyEventHandler : IHandleEvent<FixScheduleProjectionReadOnlyEvent>,
		IRunOnStardust
	{
		private readonly IReadModelValidator _validator;
		private readonly IReadModelValidationResultPersister _persister;
		private readonly IProjectionVersionPersister _projectionVersionPersister;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly ICurrentScenario _currentScenario;

		public FixScheduleProjectionReadOnlyEventHandler(IReadModelValidator validator,
			IReadModelValidationResultPersister persister, IProjectionVersionPersister projectionVersionPersister,
			IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, ICurrentScenario currentScenario)
		{
			_validator = validator;
			_persister = persister;
			_projectionVersionPersister = projectionVersionPersister;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
			_currentScenario = currentScenario;
		}

		public void Handle(FixScheduleProjectionReadOnlyEvent @event)
		{
			if (@event.Targets.Contains(ValidateReadModelType.ScheduleProjectionReadOnly))
			{
				var invalidRecords = _persister.LoadAllInvalid();

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
				throw new NotImplementedException();
			}

			if(@event.Targets.Contains(ValidateReadModelType.ScheduleDay))
			{
				throw new NotImplementedException();
			}


		}
	}
}
