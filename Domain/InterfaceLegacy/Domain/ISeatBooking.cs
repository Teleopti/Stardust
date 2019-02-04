using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatBooking : IAggregateRoot, IPublishEvents
	{
		IPerson Person { get; set; }
		DateOnly BelongsToDate { get; set; }
		DateTime StartDateTime { get; set; }
		DateTime EndDateTime { get; set; }

		ISeat Seat { get; set; }
		bool Intersects(ISeatBooking booking);
		bool Intersects(DateTimePeriod period);
		void Book(ISeat seat);
	}
}