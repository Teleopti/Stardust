using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class FixScheduleProjectionReadOnlyEvent : EventWithInfrastructureContext
	{
		public IList<ValidateReadModelType> Targets { get; set; }
	}
}
