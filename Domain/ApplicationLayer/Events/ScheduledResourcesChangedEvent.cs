using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	/// <summary>
	/// Denormalized schedule for normal usage after the scheduled resources was updated
	/// 
	/// Stays here for legacy reasons only! By not deleting this one we won't have issues during patch with errors in log files.
	/// </summary>
	public class ScheduledResourcesChangedEvent : ProjectionChangedEventBase
	{
	}
}