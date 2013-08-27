using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	/// <summary>
	/// Denormalized schedule for normal usage after the scheduled resources was updated
	/// </summary>
	public class ScheduledResourcesChangedEvent : ProjectionChangedEventBase
	{
	}
}