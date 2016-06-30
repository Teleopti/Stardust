using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class FixReadModelsEvent : EventWithInfrastructureContext
	{
		public IList<ValidateReadModelType> Targets { get; set; }
	}
}
