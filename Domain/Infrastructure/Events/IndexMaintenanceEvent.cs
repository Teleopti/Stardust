using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.Infrastructure.Events
{
	public class IndexMaintenanceEvent : StardustJobInfo
	{
		public bool AllStepsSuccess { get; set; }
	}
}