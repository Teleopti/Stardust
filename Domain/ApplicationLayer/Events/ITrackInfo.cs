using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public interface ITrackInfo
	{
		Guid TrackId { get; set; }
	}
}