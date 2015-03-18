using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{


	public class SeatPlan : ISeatPlan
	{
		private readonly IScenario _scenario;
		private readonly IPublicNoteRepository _publicNoteRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ISeatBookingRepository _seatBookingRepository;
		private readonly List<dayShift> _shifts = new List<dayShift>();

		public SeatPlan(IScenario currentScenario, IPublicNoteRepository publicNoteRepository, IPersonRepository personRepository, IScheduleRepository scheduleRepository, ISeatBookingRepository seatBookingRepository)
		{
			_scenario = currentScenario;
			_publicNoteRepository = publicNoteRepository;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_seatBookingRepository = seatBookingRepository;
		}

		public IList<ISeatBooking> CreateSeatPlan(SeatMapLocation rootSeatMapLocation, ICollection<ITeam> teams, DateOnlyPeriod period, TrackedCommandInfo trackedCommandInfo)
		{
			var seatAllocator = new SeatAllocator(rootSeatMapLocation);
			var people = getPeople(teams, period);
			createAgentShiftsFromSchedules(period, people);
			return allocateSeats(seatAllocator);

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

		private IList<ISeatBooking> allocateSeats(SeatAllocator seatAllocator)
		{
			var seatBookingRequests = new List<SeatBookingRequest>();

			//Robtodo: review how the grouping of teams is done, and try
			// to simplify, reducing the number of iterations...

			foreach (var shift in _shifts)
			{
				seatBookingRequests.AddRange(shift.GetShifts().Select(agentShifts => new SeatBookingRequest(agentShifts.ToArray())));
			}

			seatAllocator.AllocateSeats(seatBookingRequests.ToArray());

			var seatBookings = new List<ISeatBooking>();
			var shifts = _shifts.SelectMany(shift => shift.GetShifts());

			foreach (var agentShifts in shifts)
			{
				publishShiftInformation(agentShifts);
				seatBookings.AddRange(agentShifts.Where(shift => shift.Seat != null));

			}

			return seatBookings;

		}

		private void publishShiftInformation(IEnumerable<ISeatBooking> seatBookings)
		{
			foreach (var shift in seatBookings)
			{
				var date = shift.Date;
				var lang = Thread.CurrentThread.CurrentUICulture;
				//Robtodo: revisit seat name display...how/should we use seat.Name?
				var description = shift.Seat != null
					? String.Format(Resources.YouHaveBeenAllocatedSeat, date.ToShortDateString(lang), ((SeatMapLocation)shift.Seat.Parent).Name + " " + shift.Seat.Priority)
					: String.Format(Resources.YouHaveNotBeenAllocatedSeat, date.ToShortDateString(lang));

				var existingNote = _publicNoteRepository.Find(date, shift.Person, _scenario);
				if (existingNote != null)
				{
					_publicNoteRepository.Remove(existingNote);
				}
				var publicNote = new PublicNote(shift.Person,
													date,
													_scenario,
													description);

				_publicNoteRepository.Add(publicNote);
			}
		}

		private void createAgentShiftsFromSchedules(DateOnlyPeriod period, List<IPerson> people)
		{
			var schedulesForPeople = getScheduleDaysForPeriod(period, people, _scenario);

			foreach (var date in period.DayCollection())
			{
				var dayShift = new dayShift(date);
				_shifts.Add(dayShift);
				foreach (var person in people)
				{
					createAgentShiftsFromScheduleDays(schedulesForPeople[person].ScheduledDayCollection(period), person, dayShift);
				}
			}
		}

		private void createAgentShiftsFromScheduleDays(IEnumerable<IScheduleDay> scheduleDays, IPerson person, dayShift dayShift)
		{
			if (scheduleDays != null)
			{
				foreach (var scheduleDay in scheduleDays.Where(s => s.IsScheduled()
																&& s.DateOnlyAsPeriod.DateOnly == dayShift.date
																&& s.SignificantPart() == SchedulePartView.MainShift))
				{
					createAgentShift(scheduleDay, person, dayShift);
				}
			}
		}

		private void createAgentShift(IScheduleDay scheduleDay, IPerson person, dayShift dayShift)
		{
			var shiftPeriod = scheduleDay.PersonAssignment().Period;
			var team = person.MyTeam(new DateOnly(shiftPeriod.StartDateTime));
			//Robtodo: check - if we find an existing booking for this person on this date/time then use that.
			var existingBooking = _seatBookingRepository.LoadSeatBookingForPerson(new DateOnly(shiftPeriod.StartDateTime), person);
			if (existingBooking != null)
			{
				existingBooking.Seat = null;
				dayShift.AddShift (existingBooking, team);
			}
			else
			{
				dayShift.AddShift(new SeatBooking(person, shiftPeriod.StartDateTime, shiftPeriod.EndDateTime), team);	
			}
		}

		private IScheduleDictionary getScheduleDaysForPeriod(DateOnlyPeriod period, IEnumerable<IPerson> people, IScenario currentScenario)
		{
			var dictionary = _scheduleRepository.FindSchedulesForPersonsOnlyInGivenPeriod(
				people,
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				currentScenario);

			return dictionary;
		}

		private class dayShift
		{
			private readonly Dictionary<Guid, teamShift> _teamShifts = new Dictionary<Guid, teamShift>();
			public DateOnly date { get; set; }

			public dayShift(DateOnly date)
			{
				this.date = date;
			}

			public void AddShift(ISeatBooking seatBooking, ITeam team)
			{

				teamShift teamShift;
				if (!_teamShifts.TryGetValue(team.Id.Value, out teamShift))
				{
					teamShift = addTeamShift(new teamShift(team));
				}
				teamShift.AddShift(seatBooking);

			}

			public IEnumerable<IEnumerable<ISeatBooking>> GetShifts()
			{
				return _teamShifts.Values.Select(shift => shift.AgentShifts);
			}

			private teamShift addTeamShift(teamShift teamShift)
			{
				if (!containsTeamShift(teamShift))
				{
					_teamShifts.Add(teamShift.Team.Id.Value, teamShift);
				}

				return teamShift;
			}

			private Boolean containsTeamShift(teamShift teamShift)
			{
				return _teamShifts.ContainsKey(teamShift.Team.Id.Value);
			}
		}

		private class teamShift
		{
			public ITeam Team { get; private set; }

			private readonly List<ISeatBooking> _agentShifts = new List<ISeatBooking>();

			public List<ISeatBooking> AgentShifts
			{
				get { return _agentShifts; }
			}

			public teamShift(ITeam team)
			{
				Team = team;
			}

			public void AddShift(ISeatBooking seatBooking)
			{
				_agentShifts.Add(seatBooking);

			}
		}
	}
}
