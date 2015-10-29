using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatBookingRequestAssembler
	{
		ISeatBookingRequestParameters CreateSeatBookingRequests(IList<IPerson> people, DateOnlyPeriod period);
	}
}