using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatBookingRequestAssembler
	{
		ISeatBookingRequestParameters AssembleAndGroupSeatBookingRequests(IList<IPerson> people, DateOnlyPeriod period);
	}
}