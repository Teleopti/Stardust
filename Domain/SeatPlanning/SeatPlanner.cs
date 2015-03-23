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
		private readonly Dictionary<ITeam, IList<ISeatBooking>> _bookingsByTeam = new Dictionary<ITeam, IList<ISeatBooking>>();
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
			_existingSeatBookings = _seatBookingRepository.LoadSeatBookingsForDateOnlyPeriod(period.StartDate, period.EndDate);
			groupBookings(period, people);
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

		private void groupBookings(DateOnlyPeriod period, List<IPerson> people)
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
			var shiftPeriod = scheduleDay.PersonAssignment().Period;
			var date = new DateOnly(shiftPeriod.StartDateTime);
			var team = person.MyTeam(date);

			removeExistingBookingForPersonOnThisDay(person, date);

			addBooking(team, new SeatBooking(person, shiftPeriod.StartDateTime, shiftPeriod.EndDateTime));
		}

		private void removeExistingBookingForPersonOnThisDay (IPerson person, DateOnly date)
		{
			var existingBooking = _existingSeatBookings.SingleOrDefault (booking => (booking.StartDateTime.Date == date && booking.Person == person));
			if (existingBooking != null)
			{
				_existingSeatBookings.Remove (existingBooking);
				_seatBookingRepository.Remove (existingBooking);
			}
		}

		private void addBooking(ITeam team, ISeatBooking booking)
		{
			if (_bookingsByTeam.ContainsKey(team))
			{
				_bookingsByTeam[team].Add(booking);
			}
			else
			{
				_bookingsByTeam.Add(team, new List<ISeatBooking>() { booking });
			}
		}

		private IScheduleDictionary getScheduleDaysForPeriod(DateOnlyPeriod period, IEnumerable<IPerson> people, IScenario currentScenario)
		{
			return _scheduleRepository.FindSchedulesForPersonsOnlyInGivenPeriod(people, new ScheduleDictionaryLoadOptions(false, false), period, currentScenario);
		}

		private void loadExistingSeatBookings(ISeatMapLocation rootSeatMapLocation)
		{
			foreach (var seat in rootSeatMapLocation.Seats)
			{
				var seatBookings = _existingSeatBookings.Where(booking => Equals(booking.Seat, seat)).ToList();
				seat.AddSeatBookings(seatBookings);
			}
		}
		
		private void allocateSeats(SeatAllocator seatAllocator)
		{
			var bookings = _bookingsByTeam.Select(group => group.Value).ToArray();
			var bookingRequests = bookings.Select(bookingList => new SeatBookingRequest(bookingList.ToArray()));
			
			seatAllocator.AllocateSeats(bookingRequests.ToArray());

			foreach (var booking in bookings)
			{
				persistBookings(booking);
				_seatBookingRepository.AddRange(booking.Where(shift => shift.Seat != null && !_existingSeatBookings.Contains(shift)));
			}

		}

		private void persistBookings(IEnumerable<ISeatBooking> seatBookings)
		{
			foreach (var booking in seatBookings)
			{
				var date = booking.Date;
				var lang = Thread.CurrentThread.CurrentUICulture;
				//Robtodo: revisit seat name display...how/should we use seat.Name?
				var description = booking.Seat != null
					? String.Format(Resources.YouHaveBeenAllocatedSeat, date.ToShortDateString(lang), ((SeatMapLocation)booking.Seat.Parent).Name + " Seat #" + booking.Seat.Priority)
					: String.Format(Resources.YouHaveNotBeenAllocatedSeat, date.ToShortDateString(lang));

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
		}
	}
}
