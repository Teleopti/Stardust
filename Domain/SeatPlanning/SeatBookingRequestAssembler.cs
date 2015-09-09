using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatBookingRequestAssembler
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ISeatBookingRepository _seatBookingRepository;
		private readonly IScenario _scenario;
		private readonly List<TeamGroupedBooking> _bookingsWithDateAndTeam = new List<TeamGroupedBooking>();
		private IList<ISeatBooking> _existingSeatBookings;

		public SeatBookingRequestAssembler(IPersonRepository personRepository, IScheduleRepository scheduleRepository, ISeatBookingRepository seatBookingRepository, IScenario scenario)
		{
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_seatBookingRepository = seatBookingRepository;
			_scenario = scenario;
		}

		public SeatBookingRequestParameters AssembleAndGroupSeatBookingRequests(SeatMapLocation rootSeatMapLocation, ICollection<ITeam> teams, DateOnlyPeriod period)
		{

			var people = getPeople(teams, period);
			var bookingPeriodWithSurroundingDays = new DateOnlyPeriod(period.StartDate.AddDays(-1), period.EndDate.AddDays(1));

			_existingSeatBookings = _seatBookingRepository.LoadSeatBookingsForDateOnlyPeriod(bookingPeriodWithSurroundingDays);
			groupNewBookings(period, people);
			loadExistingSeatBookings(rootSeatMapLocation);

			 return new SeatBookingRequestParameters()
			 {
				 ExistingSeatBookings =  _existingSeatBookings,
				 TeamGroupedBookings = _bookingsWithDateAndTeam
			 };
		}

		private List<IPerson> getPeople(IEnumerable<ITeam> teams, DateOnlyPeriod period)
		{
			var people = new List<IPerson>();
			foreach (var team in teams)
			{
				//RobTodo: review: Try to use personscheduledayreadmodel to improve performance
				people.AddRange(_personRepository.FindPeopleBelongTeamWithSchedulePeriod(team, period));
			}

			return people;
		}

		private void groupNewBookings(DateOnlyPeriod period, List<IPerson> people)
		{
			var schedulesForPeople = getScheduleDaysForPeriod(period, people, _scenario);
			foreach (var person in people)
			{
				var scheduleDays = getScheduleDaysToPlanSeats(schedulesForPeople[person].ScheduledDayCollection(period));
				scheduleDays.ForEach (day => findOrCreateSeatBooking(day, person));
			}
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

			removeExistingBookingForPersonOnThisDay(person, date);

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
			return _scheduleRepository.FindSchedulesForPersonsOnlyInGivenPeriod(people, new ScheduleDictionaryLoadOptions(false, false), period, currentScenario);
		}

		private void loadExistingSeatBookings(SeatMapLocation rootSeatMapLocation)
		{
			foreach (var seat in rootSeatMapLocation.Seats)
			{
				var seatBookings = _existingSeatBookings.Where(booking => Equals(booking.Seat, seat)).ToList();
				seat.AddSeatBookings(seatBookings);
			}

			rootSeatMapLocation.ChildLocations.ForEach(loadExistingSeatBookings);
		}
	}
	
	public class TeamGroupedBooking
	{
		public ITeam Team { get; private set; }
		public ISeatBooking SeatBooking { get; private set; }

		public TeamGroupedBooking(ITeam team, ISeatBooking seatBooking)
		{
			Team = team;
			SeatBooking = seatBooking;
		}
	}

	public class SeatBookingRequestParameters 
	{
		public IList<TeamGroupedBooking> TeamGroupedBookings { get; set; }
		public IList<ISeatBooking> ExistingSeatBookings { get; set; }
	}
}