using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public interface ITrackableEvent
	{
		Guid TrackId { get; set; }
	}
}