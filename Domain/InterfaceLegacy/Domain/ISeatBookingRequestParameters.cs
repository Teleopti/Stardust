using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatBookingRequestParameters
	{
		IList<ITeamGroupedBooking> TeamGroupedBookings { get; set; }
		IList<ISeatBooking> ExistingSeatBookings { get; set; }
		int NumberOfUnscheduledAgentDays { get; set; }
	}
}