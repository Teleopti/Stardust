using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatBookingRequestAssembler
	{
		ISeatBookingRequestParameters CreateSeatBookingRequests (IList<IPerson> people, DateOnlyPeriod period);
	}
}

	