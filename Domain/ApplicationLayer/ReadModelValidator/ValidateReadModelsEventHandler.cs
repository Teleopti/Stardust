using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ValidateReadModelsEventHandler : IHandleEvent<ValidateReadModelsEvent>,
		IRunOnStardust
	{
		private readonly IReadModelValidator _validator;
		private readonly IScheduleProjectionReadOnlyCheckResultPersister _persister;

		public ValidateReadModelsEventHandler(IReadModelValidator validator, IScheduleProjectionReadOnlyCheckResultPersister persister)
		{
			_validator = validator;
			_persister = persister;
		}

		public void Handle(ValidateReadModelsEvent @event)
		{
				_persister.Reset();
				_validator.Validate(@event.StartDate, @event.EndDate, _persister.Save, true);
		}
	}
}