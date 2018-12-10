using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatPlanPersister
	{
		void Persist(DateOnlyPeriod period, ISeatBookingRequestParameters seatBookingRequestParameters);
		void RemoveSeatBooking (Guid seatBookingId);
	}
}