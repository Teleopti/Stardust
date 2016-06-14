using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Domain.Infrastructure.Events
{
	public class EtlNightlyEndEvent : EventWithInfrastructureContext
	{
		public bool Success { get; set; }
	}
}