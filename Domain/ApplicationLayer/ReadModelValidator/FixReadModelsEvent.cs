using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class FixReadModelsEvent : EventWithInfrastructureContext
	{
		public ValidateReadModelType Targets { get; set; }
	}
}
