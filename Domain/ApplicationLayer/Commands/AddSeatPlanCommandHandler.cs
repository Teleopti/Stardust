using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddSeatPlanCommandHandler : IHandleCommand<AddSeatPlanCommand>
	{
		//private readonly IWriteSideRepository<ISeatPlan> _seatPlanRepository;
		private readonly ICurrentScenario _scenario;
		//private readonly IProxyForId<IPerson> _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IPublicNoteRepository _publicNoteRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;


		//RobTodo: When we need to store Seat Plans, we need to create a SeatPlanRepository, eg:
		// public class SeatPlanRepository : Repository<ISeatPlan>, ISeatPlanRepository, IWriteSideRepository<ISeatPlan>, IProxyForId<ISeatPlan>
		// then it can be added to the constructor ready for autofac injection
		//public AddSeatPlanCommandHandler(IWriteSideRepository<ISeatPlan>seatPlanRepository, ICurrentScenario scenario)
		public AddSeatPlanCommandHandler(IScheduleRepository scheduleRepository, ITeamRepository teamRepository, IPersonRepository personRepository, ICurrentScenario scenario, IPublicNoteRepository publicNoteRepository, IBusinessUnitRepository businessUnitRepository)
		{
			//_seatPlanRepository = seatPlanRepository;
			_scenario = scenario;
			_publicNoteRepository = publicNoteRepository;
			_businessUnitRepository = businessUnitRepository;
			_scheduleRepository = scheduleRepository;
			_teamRepository = teamRepository;
			_personRepository = personRepository;
		}

		public void Handle(AddSeatPlanCommand command)
		{
			var period = new DateOnlyPeriod(new DateOnly(command.StartDate), new DateOnly(command.EndDate));
			var people = new List<IPerson>();
			var currentScenario = _scenario.Current();
			
			
			var teams = _teamRepository.FindTeams (command.Teams);
			foreach (var team in teams)
			{
				//RobTodo: review: Try to use personscheduledayreadmodel to improve performance
				people.AddRange(_personRepository.FindPeopleBelongTeamWithSchedulePeriod(team, period));
			}
			var schedulesForPeople = getScheduleDaysForPeriod (period, people, currentScenario);

			foreach (var person in people)
			{
				var scheduleDays = schedulesForPeople[person].ScheduledDayCollection (period);
				if (scheduleDays != null)
				{
					foreach (var scheduleDay in scheduleDays)
					{
						var publicNote = new PublicNote(person, scheduleDay.DateOnlyAsPeriod.DateOnly, currentScenario, "Hey");
						_publicNoteRepository.Add (publicNote);
					}
				}
			}

			//Robtodo: Create Seat Plan later..
			//var seatPlan = new SeatPlan(_scenario.Current());
			//seatPlan.CreateSeatPlan(period, command.TrackedCommandInfo);
			//_seatPlanRepository.Add(seatPlan);

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

	}
}


