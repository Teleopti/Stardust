using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatBookingRequestParameters
	{
		IList<ITeamGroupedBooking> TeamGroupedBookings { get; set; }
		IList<ISeatBooking> ExistingSeatBookings { get; set; }
	}
}