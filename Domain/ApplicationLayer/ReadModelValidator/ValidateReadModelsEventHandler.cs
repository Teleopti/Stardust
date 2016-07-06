using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ValidateReadModelsEventHandler : IHandleEvent<ValidateReadModelsEvent>, IRunOnStardust
	{
		private readonly IReadModelValidator _validator;

		public ValidateReadModelsEventHandler(IReadModelValidator validator)
		{
			_validator = validator;
		}

		public void Handle(ValidateReadModelsEvent @event)
		{
			_validator.Validate(@event.Targets,@event.StartDate,@event.EndDate);	
		}
	}
}