using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	

	public class SeatPlanPersister : ISeatPlanPersister
	{
		private readonly ISeatBookingRepository _seatBookingRepository;
		private readonly ISeatPlanRepository _seatPlanRepository;
		private List<SeatPlan> _seatPlansToUpdate;


		public SeatPlanPersister(ISeatBookingRepository seatBookingRepository, ISeatPlanRepository seatPlanRepository)
		{
			_seatBookingRepository = seatBookingRepository;
			_seatPlanRepository = seatPlanRepository;		
		}

		public void Persist (DateOnlyPeriod period, ISeatBookingRequestParameters seatBookingRequestParameters)
		{
			_seatPlansToUpdate = new List<SeatPlan>();

			prepareSeatPlans(period);

			_seatBookingRepository.AddRange (seatBookingRequestParameters.TeamGroupedBookings
				.Where (
					groupedBookings =>
						groupedBookings.SeatBooking.Seat != null &&
						!seatBookingRequestParameters.ExistingSeatBookings.Contains (groupedBookings.SeatBooking))
				.Select (booking => booking.SeatBooking));


			updateSeatPlanStatus(seatBookingRequestParameters);
		}

		private void updateSeatPlanStatus(ISeatBookingRequestParameters seatBookingRequestParameters)
		{
			foreach (var seatPlan in _seatPlansToUpdate )
			{
				var bookingsForDay = seatBookingRequestParameters.TeamGroupedBookings
					.Where (booking => booking.SeatBooking.BelongsToDate == seatPlan.Date)
					.Select (booking => booking.SeatBooking);
				
				seatPlan.Status = bookingsForDay.Any (booking => booking.Seat == null) ? SeatPlanStatus.InError : SeatPlanStatus.Ok;
			}
		}

		private void prepareSeatPlans(DateOnlyPeriod period)
		{
			foreach (var day in period.DayCollection())
			{
				_seatPlansToUpdate.Add(addOrUpdateSeatPlan(day));
			}
		}

		private SeatPlan addOrUpdateSeatPlan (DateOnly day)
		{
			var seatPlan = _seatPlanRepository.GetSeatPlanForDate (day);
			if (seatPlan == null)
			{
				seatPlan = new SeatPlan {Date = day, Status = SeatPlanStatus.InProgress};
				_seatPlanRepository.Add (seatPlan);
			}
			else
			{
				seatPlan.Status = SeatPlanStatus.InProgress;
			}

			return seatPlan as SeatPlan;
		}

		public void RemoveSeatBooking (Guid seatBookingId)
		{
			var seatBooking = _seatBookingRepository.Get (seatBookingId);
			if (seatBooking == null) return;

			var deleteSeatPlan = _seatBookingRepository.LoadSeatBookingsForDay (seatBooking.BelongsToDate).Count == 1;

			_seatBookingRepository.Remove (seatBooking);

			if (deleteSeatPlan)
			{
				_seatPlanRepository.RemoveSeatPlanForDate (seatBooking.BelongsToDate);
			}
		}
	}
}