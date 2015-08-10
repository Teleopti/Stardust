using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Portal.Reports.Ccc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class SeatBookingByTeamViewModel
	{
		private readonly ISeatMapLocationRepository _locationRepository;
		public String Name { get; set; }
		public List<SeatBookingViewModel> SeatBookings { get; set; }

		public SeatBookingByTeamViewModel(IGrouping<string, IPersonScheduleWithSeatBooking> seatBookingReportModels, ISeatMapLocationRepository locationRepository)
		{
			_locationRepository = locationRepository;
			Name = seatBookingReportModels.Key;
			SeatBookings = new List<SeatBookingViewModel>();
			
			seatBookingReportModels.ForEach (addSeatBooking);
		}

		private void addSeatBooking (IPersonScheduleWithSeatBooking personScheduleWithSeatBooking)
		{
			SeatBookings.Add(mapSeatBookingReportModelToViewModel(personScheduleWithSeatBooking));
		}

		private SeatBookingViewModel mapSeatBookingReportModelToViewModel(IPersonScheduleWithSeatBooking personScheduleWithSeatBooking)
		{
			var location = _locationRepository.Get(personScheduleWithSeatBooking.LocationId);

			return new SeatBookingViewModel()
			{
				PersonId = personScheduleWithSeatBooking.PersonId,
				FirstName = personScheduleWithSeatBooking.FirstName,
				LastName = personScheduleWithSeatBooking.LastName,
				LocationId = personScheduleWithSeatBooking.LocationId,
				LocationName = personScheduleWithSeatBooking.LocationName,
				LocationPath = location != null ? SeatMapProvider.GetLocationPath (location) : null,
				BelongsToDate = personScheduleWithSeatBooking.BelongsToDate,
				StartDateTime = personScheduleWithSeatBooking.SeatBookingStart ?? personScheduleWithSeatBooking.PersonScheduleStart,
				EndDateTime = personScheduleWithSeatBooking.SeatBookingEnd ?? personScheduleWithSeatBooking.PersonScheduleEnd,
				SeatId = personScheduleWithSeatBooking.SeatId,
				SeatName = personScheduleWithSeatBooking.SeatName
			};
		}
	}
}