using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Domain.Infrastructure.Events
{
	public class IndexMaintenanceEvent : EventWithInfrastructureContext
	{
		public bool AllStepsSuccess { get; set; }
	}
}