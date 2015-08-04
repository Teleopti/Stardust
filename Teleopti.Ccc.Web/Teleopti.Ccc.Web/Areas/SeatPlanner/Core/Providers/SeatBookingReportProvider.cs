using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class SeatBookingReportProvider : ISeatBookingReportProvider
	{

		private readonly ISeatBookingRepository _seatBookingRepository;

		public SeatBookingReportProvider (ISeatBookingRepository seatBookingRepository)
		{
			_seatBookingRepository = seatBookingRepository;
		}

		public SeatBookingReportViewModel Get (SeatBookingReportCriteria criteria, Paging paging = null)
		{
			var seatBookingData = _seatBookingRepository.LoadSeatBookingsReport (criteria, paging);
			return groupBookings (seatBookingData);
		}

		private static SeatBookingReportViewModel groupBookings (ISeatBookingReportModel seatBookingReportModel)
		{
			var dateGroupedSeatBookings =
				from seatBooking in seatBookingReportModel.SeatBookings
				group seatBooking by seatBooking.BelongsToDate
				into groupedSeatBookings
				select new SeatBookingByDateViewModel (groupedSeatBookings);

			return new SeatBookingReportViewModel (dateGroupedSeatBookings, seatBookingReportModel.RecordCount);
		}
	}
}
