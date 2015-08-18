using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class SeatBookingByDateViewModel
	{
		public List<SeatBookingByTeamViewModel> Teams { get; set; }
		public DateOnly Date { get; set; }

		public SeatBookingByDateViewModel(IGrouping<DateOnly, IPersonScheduleWithSeatBooking> seatBookingsByDate, ISeatMapLocationRepository locationRepository)
		{
			Date = seatBookingsByDate.Key;
			var seatBookingsByTeam = from booking in seatBookingsByDate
				group booking by booking.TeamName
				into teamGroupedBookings
				select new SeatBookingByTeamViewModel(teamGroupedBookings, locationRepository);

			Teams = new List<SeatBookingByTeamViewModel>(seatBookingsByTeam);
		}
	}
}