using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Portal.Reports.Ccc;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class SeatBookingByTeamViewModel
	{
		public TeamViewModel Team { get; set; }
		public List<SeatBookingViewModel> SeatBookings { get; set; }

		public SeatBookingByTeamViewModel(IGrouping<ITeam, ISeatBooking> seatBookingReportModels)
		{
			var team = seatBookingReportModels.Key;

			SeatBookings = new List<SeatBookingViewModel>();
			Team = new TeamViewModel() {
				Id = team.Id.GetValueOrDefault().ToString(),
				Name = team.Description.Name
			};
			seatBookingReportModels.ForEach (addSeatBooking);
		}

		private void addSeatBooking (ISeatBooking seatBookingReportRowModel)
		{
			SeatBookings.Add (mapSeatBookingReportModelToViewModel (seatBookingReportRowModel));
		}

		private static SeatBookingViewModel mapSeatBookingReportModelToViewModel (ISeatBooking seatBookingReportRowModel)
		{
			var location = (ISeatMapLocation) seatBookingReportRowModel.Seat.Parent;

			return new SeatBookingViewModel()
			{
				PersonId = seatBookingReportRowModel.Person.Id.GetValueOrDefault(),
				FirstName = seatBookingReportRowModel.Person.Name.FirstName,
				LastName = seatBookingReportRowModel.Person.Name.LastName,
				LocationId = seatBookingReportRowModel.Seat.Parent.Id.GetValueOrDefault(),
				LocationName = location.Name,
				LocationPath = SeatMapProvider.GetLocationPath (location),
				BelongsToDate = seatBookingReportRowModel.BelongsToDate,
				StartDateTime = seatBookingReportRowModel.StartDateTime,
				EndDateTime = seatBookingReportRowModel.EndDateTime,
				SeatId = seatBookingReportRowModel.Seat.Id.GetValueOrDefault(),
				SeatName = seatBookingReportRowModel.Seat.Name
			};
		}
	}
}