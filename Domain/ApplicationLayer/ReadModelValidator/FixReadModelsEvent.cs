using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class FixReadModelsEvent : EventWithInfrastructureContext
	{
		public ValidateReadModelType Targets { get; set; }
	}
}
