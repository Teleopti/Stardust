using System;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatBooking : IAggregateRootWithEvents
	{
		IPerson Person { get; set; }
		DateTime StartDateTime { get; set; }
		DateTime EndDateTime { get; set; }
		
		ISeat Seat { get; set; }
		bool Intersects (ISeatBooking booking);
		bool Intersects (DateTimePeriod period);
		void Book (ISeat seat);
	}
}