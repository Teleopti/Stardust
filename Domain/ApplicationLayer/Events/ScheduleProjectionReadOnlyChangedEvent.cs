using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	[RemoveMeWithToggle(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
	public class ScheduleProjectionReadOnlyChangedEvent : Event
	{
		public Guid PersonId { get; set; }
	}
}