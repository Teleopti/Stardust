using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ValidateReadModelsEventHandler : IHandleEvent<ValidateReadModelsEvent>,
		IRunOnStardust
	{
		private readonly IReadModelValidator _validator;
		private readonly IReadModelValidationResultPersister _readModelValidationResultPersister;

		public ValidateReadModelsEventHandler(IReadModelValidator validator, IReadModelValidationResultPersister readModelValidationResultPersister)
		{
			_validator = validator;
			_readModelValidationResultPersister = readModelValidationResultPersister;
		}

		public void Handle(ValidateReadModelsEvent @event)
		{
			Action<ReadModelValidationResult> action = result =>
			{
				switch (result.Type)
				{
					case ValidateReadModelType.ScheduleProjectionReadOnly:
						_readModelValidationResultPersister.SaveScheduleProjectionReadOnly(result);
						break;
					case ValidateReadModelType.PersonScheduleDay:
						_readModelValidationResultPersister.SavePersonScheduleDay(result);
						break;
					case ValidateReadModelType.ScheduleDay:
					        _readModelValidationResultPersister.SaveScheduleDay(result);
						break;

				}
			};
			_readModelValidationResultPersister.Reset(@event.Targets);

			_validator.SetTargetTypes(@event.Targets);
			_validator.Validate(@event.StartDate, @event.EndDate, action, true);
		}
	}
}