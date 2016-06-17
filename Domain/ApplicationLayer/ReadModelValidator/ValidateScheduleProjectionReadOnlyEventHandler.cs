using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ValidateScheduleProjectionReadOnlyEventHandler : IHandleEvent<ValidateScheduleProjectionReadOnlyEvent>,
		IRunOnStardust
	{
		private readonly IReadModelValidator _validator;

		public ValidateScheduleProjectionReadOnlyEventHandler(IReadModelValidator validator)
		{
			_validator = validator;
		}

		public void Handle(ValidateScheduleProjectionReadOnlyEvent @event)
		{
			var result = _validator.Validate(@event.StartDate, @event.EndDate);
		}
	}
}