using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatBookingRequestAssembler
	{
		ISeatBookingRequestParameters CreateSeatBookingRequests (IList<IPerson> people, DateOnlyPeriod period);
	}
}

	