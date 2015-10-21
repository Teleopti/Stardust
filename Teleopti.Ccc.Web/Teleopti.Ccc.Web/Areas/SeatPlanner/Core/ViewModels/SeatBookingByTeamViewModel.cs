using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Reporting.Reports.CCC;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class SeatBookingByTeamViewModel
	{
		private readonly ISeatMapLocationRepository _locationRepository;
		private readonly IUserTimeZone _userTimeZone;
		public String Name { get; set; }
		public List<SeatBookingViewModel> SeatBookings { get; set; }

		public SeatBookingByTeamViewModel(IGrouping<string, IPersonScheduleWithSeatBooking> seatBookingReportModels, ISeatMapLocationRepository locationRepository, IUserTimeZone userTimeZone)
		{
			_locationRepository = locationRepository;
			_userTimeZone = userTimeZone;
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
			
			var shiftReadModel = personScheduleWithSeatBooking.PersonScheduleModelSerialized!= null
				? JsonConvert.DeserializeObject<Model>(personScheduleWithSeatBooking.PersonScheduleModelSerialized)
				: null;

			return new SeatBookingViewModel()
			{
				PersonId = personScheduleWithSeatBooking.PersonId,
				FirstName = personScheduleWithSeatBooking.FirstName,
				LastName = personScheduleWithSeatBooking.LastName,
				LocationId = personScheduleWithSeatBooking.LocationId,
				LocationName = personScheduleWithSeatBooking.LocationName,
				LocationPath = location != null ? SeatMapProvider.GetLocationPath (location) : null,
				BelongsToDate = personScheduleWithSeatBooking.BelongsToDate,
				StartDateTime = convertTimeToLocal(personScheduleWithSeatBooking.SeatBookingStart ?? personScheduleWithSeatBooking.PersonScheduleStart),
				EndDateTime = convertTimeToLocal(personScheduleWithSeatBooking.SeatBookingEnd ?? personScheduleWithSeatBooking.PersonScheduleEnd),
				SeatId = personScheduleWithSeatBooking.SeatId,
				SeatName = personScheduleWithSeatBooking.SeatName,
				SiteId = personScheduleWithSeatBooking.SiteId,
				SiteName = personScheduleWithSeatBooking.SiteName,
				IsDayOff = personScheduleWithSeatBooking.IsDayOff,
				IsFullDayAbsence = shiftReadModel != null && shiftReadModel.Shift.IsFullDayAbsence
			};
		}


		private DateTime convertTimeToLocal(DateTime dateTime)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _userTimeZone.TimeZone());

		}

	}
}