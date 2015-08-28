using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.SeatManagement;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class SeatBookingReportProvider : ISeatBookingReportProvider
	{

		private readonly ISeatBookingRepository _seatBookingRepository;
		private readonly ISeatMapLocationRepository _locationRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IUserTimeZone _userTimeZone;

		public SeatBookingReportProvider (ISeatBookingRepository seatBookingRepository, ISeatMapLocationRepository locationRepository, ITeamRepository teamRepository, IUserTimeZone userTimeZone)
		{
			_seatBookingRepository = seatBookingRepository;
			_locationRepository = locationRepository;
			_teamRepository = teamRepository;
			_userTimeZone = userTimeZone;
		}

		public SeatBookingReportViewModel Get (SeatBookingReportCommand command)
		{
			var crtieria = new SeatBookingReportCriteria()
			{
				Teams = command.Teams.IsNullOrEmpty()? null : _teamRepository.FindTeams (command.Teams),
				Locations = command.Locations.IsNullOrEmpty()? null : _locationRepository.FindLocations (command.Locations.ToList()),
				Period = new DateOnlyPeriod (new DateOnly (command.StartDate), new DateOnly (command.EndDate))
			};

			if (command.Take != 0)
			{
				return Get(crtieria, new Paging() { Skip = command.Skip, Take = command.Take });
			};

			return Get (crtieria);

		}

		public SeatBookingReportViewModel Get (SeatBookingReportCriteria criteria, Paging paging = null)
		{
			var seatBookingData = _seatBookingRepository.LoadSeatBookingsReport (criteria, paging);
			return groupBookings (seatBookingData);
		}

		private SeatBookingReportViewModel groupBookings (ISeatBookingReportModel seatBookingReportModel)
		{
			var dateGroupedSeatBookings =
				from seatBooking in seatBookingReportModel.SeatBookings
				group seatBooking by seatBooking.BelongsToDate
				into groupedSeatBookings
					select new SeatBookingByDateViewModel(groupedSeatBookings, _locationRepository, _userTimeZone);

			return new SeatBookingReportViewModel (dateGroupedSeatBookings, seatBookingReportModel.RecordCount);
		}
	}
}
