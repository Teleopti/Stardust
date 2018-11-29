using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatBookingRequestAssembler : ISeatBookingRequestAssembler
	{
	
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISeatBookingRepository _seatBookingRepository;
		private readonly ICurrentScenario _scenario;
		private IList<ITeamGroupedBooking> _bookingsWithDateAndTeam;
		private IList<ISeatBooking> _existingSeatBookings;
		private int _numberOfUnscheduledAgentDays;

		public SeatBookingRequestAssembler(IScheduleStorage scheduleStorage, ISeatBookingRepository seatBookingRepository, ICurrentScenario scenario)
		{
			_scheduleStorage = scheduleStorage;
			_seatBookingRepository = seatBookingRepository;
			_scenario = scenario;
		}

		public ISeatBookingRequestParameters CreateSeatBookingRequests(IList<IPerson> people, DateOnlyPeriod period)
		{
			_numberOfUnscheduledAgentDays = 0;
			_bookingsWithDateAndTeam = new List<ITeamGroupedBooking>();
			

			var bookingPeriodWithSurroundingDays = new DateOnlyPeriod(period.StartDate.AddDays(-1), period.EndDate.AddDays(1));
			_existingSeatBookings = _seatBookingRepository.LoadSeatBookingsForDateOnlyPeriod(bookingPeriodWithSurroundingDays);
			

			groupNewBookings(period, people);
			
			 return new SeatBookingRequestParameters
			 {
				 ExistingSeatBookings =  _existingSeatBookings,
				 TeamGroupedBookings = _bookingsWithDateAndTeam,
				 NumberOfUnscheduledAgentDays = _numberOfUnscheduledAgentDays
			 };
		}
		
		private void groupNewBookings(DateOnlyPeriod period, IList<IPerson> people)
		{
			var schedulesForPeople = getScheduleDaysForPeriod(period, people, _scenario.Current());
			foreach (var person in people)
			{
				removeExistingBookings(period, person);

				var scheduleDays = getScheduleDaysToPlanSeats(schedulesForPeople[person].ScheduledDayCollection(period)).ToArray();
				
				scheduleDays.ForEach (day => findOrCreateSeatBooking(day, person));
				_numberOfUnscheduledAgentDays += period.DayCount() - scheduleDays.Count();
			}
		}

		private void removeExistingBookings (DateOnlyPeriod period, IPerson person)
		{
			period.DayCollection().ForEach (day =>
			{
				//ROBTODO: perhaps it would be better to mark these for deletion, and only delete them
				// when the agent has successfully been allocated a new seat for this day.  Otherwise we could
				// remove the existing booking, but the new requested booking can fail.
				removeExistingBookingForPersonOnThisDay (person, day);
			});
		}

		private IEnumerable<IScheduleDay> getScheduleDaysToPlanSeats(IEnumerable<IScheduleDay> scheduleDays)
		{
			return scheduleDays != null ? scheduleDays.Where(s => s.IsScheduled() && s.SignificantPart() == SchedulePartView.MainShift) : null;
		}

		private void findOrCreateSeatBooking(IScheduleDay scheduleDay, IPerson person)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			var shiftPeriod = personAssignment.Period;
			var date = personAssignment.Date;
			var team = person.MyTeam(date);

			
			addBooking(team, new SeatBooking(person, date, shiftPeriod.StartDateTime, shiftPeriod.EndDateTime));
		}

		private void removeExistingBookingForPersonOnThisDay(IPerson person, DateOnly date)
		{
			var existingBooking = _existingSeatBookings.SingleOrDefault(booking => (booking.BelongsToDate == date && booking.Person == person));
			if (existingBooking != null)
			{
				existingBooking.Seat.RemoveSeatBooking(existingBooking);
				_existingSeatBookings.Remove(existingBooking);
				_seatBookingRepository.Remove(existingBooking);
			}
		}

		private void addBooking(ITeam team, ISeatBooking booking)
		{
			_bookingsWithDateAndTeam.Add(new TeamGroupedBooking(team, booking));
		}

		private IScheduleDictionary getScheduleDaysForPeriod(DateOnlyPeriod period, IEnumerable<IPerson> people, IScenario currentScenario)
		{
			return _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people, new ScheduleDictionaryLoadOptions(false, false), period, currentScenario);
		}
	}
}