using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatPlanner : ISeatPlanner
	{
		private readonly IScenario _scenario;
		private readonly IPublicNoteRepository _publicNoteRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ISeatBookingRepository _seatBookingRepository;
		private readonly List<teamGroupedBooking> _bookingsWithDateAndTeam = new List<teamGroupedBooking>();
		private IList<ISeatBooking> _existingSeatBookings;

		public SeatPlanner(IScenario currentScenario,
							IPublicNoteRepository publicNoteRepository,
							IPersonRepository personRepository,
							IScheduleRepository scheduleRepository,
							ISeatBookingRepository seatBookingRepository)
		{
			_scenario = currentScenario;
			_publicNoteRepository = publicNoteRepository;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_seatBookingRepository = seatBookingRepository;
		}

		public void CreateSeatPlan(SeatMapLocation rootSeatMapLocation, ICollection<ITeam> teams, DateOnlyPeriod period, TrackedCommandInfo trackedCommandInfo)
		{
			var people = getPeople(teams, period);
			var bookingPeriodWithSurroundingDays = new DateOnlyPeriod(period.StartDate.AddDays(-1), period.EndDate.AddDays(1));

			_existingSeatBookings = _seatBookingRepository.LoadSeatBookingsForDateOnlyPeriod(bookingPeriodWithSurroundingDays);
			groupNewBookings(period, people);
			loadExistingSeatBookings(rootSeatMapLocation);
			allocateSeats(new SeatAllocator(rootSeatMapLocation));
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
				scheduleDays.ForEach(day => findOrCreateSeatBooking(day, person));
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
				existingBooking.Seat.RemoveSeatBooking (existingBooking);
				_existingSeatBookings.Remove(existingBooking);
				_seatBookingRepository.Remove(existingBooking);
			}
		}

		private void addBooking(ITeam team, ISeatBooking booking)
		{
			_bookingsWithDateAndTeam.Add(new teamGroupedBooking(team, booking));
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

			rootSeatMapLocation.ChildLocations.ForEach (loadExistingSeatBookings);
			
		}

		private void allocateSeats(SeatAllocator seatAllocator)
		{
			seatAllocator.AllocateSeats(getSeatBookingRequests().ToArray());
		
			persistBookings(_bookingsWithDateAndTeam);

			_seatBookingRepository.AddRange(_bookingsWithDateAndTeam
				.Where(groupedBookings => groupedBookings.SeatBooking.Seat != null && !_existingSeatBookings.Contains(groupedBookings.SeatBooking))
				.Select (booking => booking.SeatBooking));
		}

		private IEnumerable<SeatBookingRequest> getSeatBookingRequests()
		{
		
			var seatBookingsByDateAndTeam = _bookingsWithDateAndTeam
				.GroupBy (booking => booking.SeatBooking.BelongsToDate)
				.Select (x => new
				{
					Category = x.Key,
					TeamGroups = x.ToList()
						.GroupBy (y => y.Team)
				});

			var seatBookingRequests =
				from day in seatBookingsByDateAndTeam
				from teamGroups in day.TeamGroups
				select teamGroups.Select (team => team.SeatBooking)
				into teamBookingsforDay
				select new SeatBookingRequest (teamBookingsforDay.ToArray());

			return seatBookingRequests;
		}

		private void persistBookings(IEnumerable<teamGroupedBooking> seatBookings)
		{
			
			foreach (var bookingWithDateAndTeam in seatBookings)
			{
				var booking = bookingWithDateAndTeam.SeatBooking;
				var date = bookingWithDateAndTeam.SeatBooking.BelongsToDate;
				var lang = Thread.CurrentThread.CurrentUICulture;
				//Robtodo: revisit seat name display...how/should we use seat.Name?
				var description = booking.Seat != null
					? String.Format(Resources.YouHaveBeenAllocatedSeat, date.ToShortDateString(lang), ((SeatMapLocation)booking.Seat.Parent).Name + " Seat #" + booking.Seat.Priority)
					: String.Format(Resources.YouHaveNotBeenAllocatedSeat, date.ToShortDateString(lang));

				tempStoreBookingInfoInPublicNote(date, booking, description);
			}
		}

		//Robtodo: WIP: Temporary mechanism to store booking information
		private void tempStoreBookingInfoInPublicNote(DateOnly date, ISeatBooking booking, string description)
		{
			var existingNote = _publicNoteRepository.Find(date, booking.Person, _scenario);
			if (existingNote != null)
			{
				_publicNoteRepository.Remove(existingNote);
			}
			var publicNote = new PublicNote(booking.Person,
				date,
				_scenario,
				description);

			_publicNoteRepository.Add(publicNote);
		}


		private class teamGroupedBooking
		{
			public ITeam Team { get; private set; }
			public ISeatBooking SeatBooking { get; private set; }

			public teamGroupedBooking(ITeam team, ISeatBooking seatBooking)
			{
				Team = team;
				SeatBooking = seatBooking;
			}
		}
	}


}
