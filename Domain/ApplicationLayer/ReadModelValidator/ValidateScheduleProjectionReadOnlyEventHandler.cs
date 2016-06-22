using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ValidateScheduleProjectionReadOnlyEventHandler : IHandleEvent<ValidateScheduleProjectionReadOnlyEvent>,
		IRunOnStardust
	{
		private readonly IReadModelValidator _validator;
		private readonly IScheduleProjectionReadOnlyCheckResultPersister _persister;

		public ValidateScheduleProjectionReadOnlyEventHandler(IReadModelValidator validator, IScheduleProjectionReadOnlyCheckResultPersister persister)
		{
			_validator = validator;
			_persister = persister;
		}

		public void Handle(ValidateScheduleProjectionReadOnlyEvent @event)
		{
				_persister.Reset();
				_validator.Validate(@event.StartDate, @event.EndDate, _persister.Save, true);
		}
	}
}