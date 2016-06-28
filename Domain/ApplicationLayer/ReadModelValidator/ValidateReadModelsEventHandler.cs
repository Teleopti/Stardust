using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ValidateReadModelsEventHandler : IHandleEvent<ValidateReadModelsEvent>,
		IRunOnStardust
	{
		private readonly IReadModelValidator _validator;
		private readonly IScheduleProjectionReadOnlyCheckResultPersister _scheduleProjectionReadOnlyCheckResultPersister;

		public ValidateReadModelsEventHandler(IReadModelValidator validator, IScheduleProjectionReadOnlyCheckResultPersister scheduleProjectionReadOnlyCheckResultPersister)
		{
			_validator = validator;
			_scheduleProjectionReadOnlyCheckResultPersister = scheduleProjectionReadOnlyCheckResultPersister;
		}

		public void Handle(ValidateReadModelsEvent @event)
		{
			Action<ReadModelValidationResult> action = result =>
			{
				switch (result.Type)
				{
					case ValidateReadModelType.ScheduleProjectionReadOnly:
						_scheduleProjectionReadOnlyCheckResultPersister.Save(result);
						break;
				}
			};
			_scheduleProjectionReadOnlyCheckResultPersister.Reset();

			_validator.SetTargetTypes(@event.Targets);
			_validator.Validate(@event.StartDate, @event.EndDate, action, true);
		}
	}
}