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

		public virtual void Handle(ValidateReadModelsEvent @event)
		{
			var mode = @event.Reinitialize
				? ReadModelValidationMode.Reinitialize
				: @event.TriggerFix ? ReadModelValidationMode.ValidateAndFix : ReadModelValidationMode.Validate;

			_validator.Validate(@event.Targets,@event.StartDate,@event.EndDate, mode);	
		}
	}
}