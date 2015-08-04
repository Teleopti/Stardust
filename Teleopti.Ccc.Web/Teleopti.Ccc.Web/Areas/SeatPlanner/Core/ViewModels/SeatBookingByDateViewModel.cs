using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class SeatBookingByDateViewModel
	{
		public List<SeatBookingByTeamViewModel> Teams { get; set; }
		public DateOnly Date { get; set; }

		public SeatBookingByDateViewModel(IGrouping<DateOnly, ISeatBooking> seatBookingsByDate)
		{
			Date = seatBookingsByDate.Key;
			var seatBookingsByTeam = from booking in seatBookingsByDate
				group booking by booking.Person.MyTeam (booking.BelongsToDate)
				into teamGroupedBookings
				select new SeatBookingByTeamViewModel(teamGroupedBookings);

			Teams = new List<SeatBookingByTeamViewModel>(seatBookingsByTeam);
		}
	}
}