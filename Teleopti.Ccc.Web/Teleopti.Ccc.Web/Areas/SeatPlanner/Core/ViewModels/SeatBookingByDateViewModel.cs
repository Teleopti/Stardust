using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class SeatBookingByDateViewModel
	{
		public List<SeatBookingByTeamViewModel> Teams { get; set; }
		public DateOnly Date { get; set; }

		public SeatBookingByDateViewModel(IGrouping<DateOnly, IPersonScheduleWithSeatBooking> seatBookingsByDate, ISeatMapLocationRepository locationRepository, IUserTimeZone userTimeZone)
		{
			Date = seatBookingsByDate.Key;
			var seatBookingsByTeam = from booking in seatBookingsByDate 
				orderby booking.SiteName, booking.TeamName
				group booking by booking.TeamName
				into teamGroupedBookings
				select new SeatBookingByTeamViewModel(teamGroupedBookings, locationRepository, userTimeZone);

			Teams = new List<SeatBookingByTeamViewModel>(seatBookingsByTeam);
		}
	}
}