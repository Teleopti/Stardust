using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatBookingRequestParameters : ISeatBookingRequestParameters
	{
		public IList<ITeamGroupedBooking> TeamGroupedBookings { get; set; }
		public IList<ISeatBooking> ExistingSeatBookings { get; set; }
		public int NumberOfUnscheduledAgentDays { get; set; }
	}
}