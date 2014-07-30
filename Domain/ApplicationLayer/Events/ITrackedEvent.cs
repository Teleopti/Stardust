using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public interface ITrackedEvent
	{
		Guid TrackId { get; set; }
	}
}