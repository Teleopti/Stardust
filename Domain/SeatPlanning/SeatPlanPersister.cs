using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatPlanPersister
	{
		private readonly ISeatBookingRepository _seatBookingRepository;
		private readonly ISeatPlanRepository _seatPlanRepository;
		private readonly List<SeatPlan> _seatPlansToUpdate = new List<SeatPlan>();


		public SeatPlanPersister(ISeatBookingRepository seatBookingRepository, ISeatPlanRepository seatPlanRepository)
		{
			_seatBookingRepository = seatBookingRepository;
			_seatPlanRepository = seatPlanRepository;
		}

		public void Persist (DateOnlyPeriod period, SeatBookingRequestParameters seatBookingRequestParameters)
		{
			prepareSeatPlans(period);

			_seatBookingRepository.AddRange (seatBookingRequestParameters.TeamGroupedBookings
				.Where (
					groupedBookings =>
						groupedBookings.SeatBooking.Seat != null &&
						!seatBookingRequestParameters.ExistingSeatBookings.Contains (groupedBookings.SeatBooking))
				.Select (booking => booking.SeatBooking));


			updateSeatPlanStatus(seatBookingRequestParameters);
		}

		private void updateSeatPlanStatus(SeatBookingRequestParameters seatBookingRequestParameters)
		{
			foreach (var seatPlan in _seatPlansToUpdate )
			{
				var bookingsForDay = seatBookingRequestParameters.TeamGroupedBookings
					.Where (booking => booking.SeatBooking.BelongsToDate == seatPlan.Date)
					.Select (booking => booking.SeatBooking);
				
				seatPlan.Status = bookingsForDay.Any (booking => booking.Seat == null) ? SeatPlanStatus.InError : SeatPlanStatus.Ok;
				_seatPlanRepository.Update (seatPlan);
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
				_seatPlanRepository.Update (seatPlan);
			}

			return seatPlan as SeatPlan;
		}
	}
}