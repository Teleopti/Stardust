using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.SeatPlanning
{

	[Serializable]
	public class SeatPlan : VersionedAggregateRoot, ISeatPlan, IDeleteTag
	{
		private readonly IScenario _scenario;
		private readonly IPublicNoteRepository _publicNoteRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly Dictionary<DateOnly, dayShift> _dayShifts = new Dictionary<DateOnly, dayShift>();

		public SeatPlan(IScenario currentScenario, IPublicNoteRepository publicNoteRepository, IPersonRepository personRepository, IScheduleRepository scheduleRepository)
		{
			_scenario = currentScenario;
			_publicNoteRepository = publicNoteRepository;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
		}

		public void CreateSeatPlan(Location rootLocation, ICollection<ITeam> teams, DateOnlyPeriod period, TrackedCommandInfo trackedCommandInfo)
		{
			//RobTodo: Fill in save routines when needed.
			var seatAllocator = new SeatAllocator(rootLocation);
			var people = getPeople(teams, period);
			createAgentShiftsFromSchedules(period, people);
			allocateSeats(seatAllocator);

			//RobTodo: Required by Seat Planner?
			//addSeatPlanAddedEvent (period, trackedCommandInfo);
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

		private void allocateSeats(SeatAllocator seatAllocator)
		{
			foreach (var dayShift in _dayShifts.Values)
			{
				foreach (var agentShifts in dayShift.GetShifts())
				{
					var bookingRequest = new SeatBookingRequest(agentShifts.ToArray());
					seatAllocator.AllocateSeats(bookingRequest);
					publishShiftInformation(agentShifts);
				}
			}
		}

		private void publishShiftInformation(IEnumerable<AgentShift> agentShifts)
		{
			foreach (var shift in agentShifts)
			{
				var date = shift.ScheduleDay.DateOnlyAsPeriod.DateOnly;
				var lang = Thread.CurrentThread.CurrentUICulture;
				var description = shift.Seat != null 
					? String.Format(Resources.YouHaveBeenAllocatedSeat, date.ToShortDateString(lang), shift.Seat.Name) 
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
				_dayShifts.Add(date, dayShift);
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
				foreach (var scheduleDay in scheduleDays.Where(s => s.IsScheduled() && s.DateOnlyAsPeriod.DateOnly == dayShift.date))
				{
					SchedulePartView partView = scheduleDay.SignificantPart();
					if (partView == SchedulePartView.MainShift || partView == SchedulePartView.ContractDayOff)
					{
						// Robtodo: Review....I am assumming that Contract Day Off will still require a seat
						/*partView == SchedulePartView.FullDayAbsence || partView == SchedulePartView.DayOff ||
							partView == SchedulePartView.ContractDayOff || partView == SchedulePartView.MainShift */
						createAgentShift(scheduleDay, person, dayShift);
					}
				}
			}
		}

		private void createAgentShift(IScheduleDay scheduleDay, IPerson person, dayShift dayShift)
		{
			var shiftPeriod = scheduleDay.PersonAssignment().Period;
			var team = person.MyTeam(new DateOnly(shiftPeriod.StartDateTime));
			var bookingPeriod = new BookingPeriod(shiftPeriod.StartDateTime, shiftPeriod.EndDateTime);
			dayShift.AddShift(new AgentShift(bookingPeriod, person, scheduleDay), team);
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

		private void addSeatPlanAddedEvent(DateOnlyPeriod period, TrackedCommandInfo trackedCommandInfo)
		{
			var seatPlanAddedEvent = new SeatPlanAddedEvent
			{
				StartDate = period.StartDate,
				EndDate = period.EndDate,
				ScenarioId = _scenario.Id.GetValueOrDefault(),
				BusinessUnitId = _scenario.BusinessUnit.Id.GetValueOrDefault()
			};
			if (trackedCommandInfo != null)
			{
				seatPlanAddedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
				seatPlanAddedEvent.TrackId = trackedCommandInfo.TrackId;
			}
			AddEvent(seatPlanAddedEvent);
		}

		#region IDeleteTag Implementation

		private bool _isDeleted;

		public bool IsDeleted
		{
			get { return _isDeleted; }
			private set { _isDeleted = value; }
		}

		public void SetDeleted()
		{
			_isDeleted = true;
		}

		#endregion

		private class dayShift
		{
			private readonly Dictionary<Guid, teamShift> _teamShifts = new Dictionary<Guid, teamShift>();
			public DateOnly date { get; set; }

			public dayShift(DateOnly date)
			{
				this.date = date;
			}

			public void AddShift(AgentShift agentShift, ITeam team)
			{

				teamShift teamShift;
				if (!_teamShifts.TryGetValue(team.Id.Value, out teamShift))
				{
					teamShift = addTeamShift(new teamShift(team));
				}
				teamShift.AddShift(agentShift);

			}

			public IEnumerable<IEnumerable<AgentShift>> GetShifts()
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

			private readonly List<AgentShift> _agentShifts = new List<AgentShift>();

			public List<AgentShift> AgentShifts
			{
				get { return _agentShifts; }
			}

			public teamShift(ITeam team)
			{
				Team = team;
			}

			public void AddShift(AgentShift agentShift)
			{
				_agentShifts.Add(agentShift);

			}

		}

	}
}
