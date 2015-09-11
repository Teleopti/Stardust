using System;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatPlanPersister
	{
		void Persist(DateOnlyPeriod period, ISeatBookingRequestParameters seatBookingRequestParameters);
		void RemoveSeatBooking (Guid seatBookingId);
	}
}