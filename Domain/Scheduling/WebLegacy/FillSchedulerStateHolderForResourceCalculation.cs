using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class FillSchedulerStateHolderForResourceCalculation
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;

		public FillSchedulerStateHolderForResourceCalculation(IScenarioRepository scenarioRepository,
					ISkillDayLoadHelper skillDayLoadHelper,
					IScheduleStorage scheduleStorage,
					ICurrentTeleoptiPrincipal principal,
					ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
					IRepositoryFactory repositoryFactory,
					IPersonRepository personRepository,
					ISkillRepository skillRepository)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_scheduleStorage = scheduleStorage;
			_principal = principal;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_repositoryFactory = repositoryFactory;
			_personRepository = personRepository;
			_skillRepository = skillRepository;
		}

		[LogTime]
		public virtual void Fill(ISchedulerStateHolder schedulerStateHolderTo,  DateOnlyPeriod period)
		{
			PreFill(schedulerStateHolderTo, period);
			var scenario = FetchScenario();
			FillAgents(schedulerStateHolderTo, period);
			//removeUnwantedAgents(schedulerStateHolderTo, null);
			var skills = skillsToUse(schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization, period).ToList();
			FillSkillDays(schedulerStateHolderTo, scenario, skills, period);
			//removeUnwantedSkillDays(schedulerStateHolderTo, skills);
			FillSchedules(schedulerStateHolderTo, scenario, schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization, period);
			//removeUnwantedScheduleRanges(schedulerStateHolderTo);
			//PostFill(schedulerStateHolderTo, schedulerStateHolderTo.AllPermittedPersons, period);
			//schedulerStateHolderTo.ResetFilteredPersons();
		}

		protected IScenario FetchScenario()
		{
			return _scenarioRepository.LoadDefaultScenario();
		}

		[LogTime]
		protected virtual void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			var allPeople = _personRepository.FindPeopleInOrganizationQuiteLight(period);
			schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization = allPeople.ToList();
			allPeople.ForEach(x => schedulerStateHolderTo.AllPermittedPersons.Add(x));
		}

		[LogTime]
		protected virtual void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			var forecast = _skillDayLoadHelper.LoadSchedulerSkillDays(period, skills, scenario);
			schedulerStateHolderTo.SchedulingResultState.SkillDays = forecast;
			schedulerStateHolderTo.SchedulingResultState.AddSkills(skills.ToArray());
		}

		[LogTime]
		protected virtual void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var timeZone = _principal.Current().Regional.TimeZone;
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);
			schedulerStateHolderTo.SetRequestedScenario(scenario);
			var personProvider = new PersonsInOrganizationProvider(agents) { DoLoadByPerson = false }; //TODO: this is experimental
			//schedulerStateHolderTo.SchedulingResultState.Schedules =
			//	_scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(agents,
			//		new ScheduleDictionaryLoadOptions(false, false, false), period, scenario);
			schedulerStateHolderTo.LoadSchedules(_scheduleStorage, personProvider,
				new ScheduleDictionaryLoadOptions(false, false, false) {LoadAgentDayScheduleTags = false},
				new ScheduleDateTimePeriod(dateTimePeriod, agents, new ResourceCalculateRangeToLoadCalculator(dateTimePeriod)));
		}

		[LogTime]
		protected virtual void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.LoadCommonStateForResourceCalculationOnly(_currentUnitOfWorkFactory.Current().CurrentUnitOfWork(), _repositoryFactory);
			_skillRepository.FindAllWithSkillDays(period); //perf hack to prevent working with skill proxies when doing calculation
		}

		protected void PostFill(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var timeZone = _principal.Current().Regional.TimeZone;
			schedulerStateHolderTo.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, timeZone);
		}

		//private static void removeUnwantedAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIdsToKeep)
		//{
		//	if (agentIdsToKeep != null) //remove this when also scheduling is converted to "events"
		//	{
		//		foreach (var agent in schedulerStateHolderTo.AllPermittedPersons.ToList().Where(agent => !agentIdsToKeep.Contains(agent.Id.Value)))
		//		{
		//			schedulerStateHolderTo.AllPermittedPersons.Remove(agent);
		//		}
		//		foreach (var agent in schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.ToList().Where(agent => !agentIdsToKeep.Contains(agent.Id.Value)))
		//		{
		//			schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.Remove(agent);
		//		}
		//	}
		//}

		//private static void removeUnwantedSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<ISkill> skillsToKeep)
		//{
		//	foreach (var skill in schedulerStateHolderTo.SchedulingResultState.Skills.ToList().Where(skill => !skillsToKeep.Contains(skill)))
		//	{
		//		schedulerStateHolderTo.SchedulingResultState.RemoveSkill(skill);
		//	}
		//	foreach (var skillDay in schedulerStateHolderTo.SchedulingResultState.SkillDays.ToList().Where(skillDay => !skillsToKeep.Contains(skillDay.Key)))
		//	{
		//		schedulerStateHolderTo.SchedulingResultState.SkillDays.Remove(skillDay.Key);
		//	}
		//}

		//private static void removeUnwantedScheduleRanges(ISchedulerStateHolder schedulerStateHolderTo)
		//{
		//	foreach (var person in schedulerStateHolderTo.Schedules.Keys.Where(person => !schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.Contains(person)).ToList())
		//	{
		//		schedulerStateHolderTo.Schedules.Remove(person);
		//	}
		//}


		private IEnumerable<ISkill> skillsToUse(IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var agentSkills = new HashSet<ISkill>();
			foreach (var agent in agents)
			{
				foreach (var personPeriod in agent.PersonPeriods(period))
				{
					foreach (var personSkill in personPeriod.PersonSkillCollection)
					{
						if (personSkill.Active)
						{
							agentSkills.Add(personSkill.Skill);
						}
					}

				}
			}

			return agentSkills;
		}
	}

	//dont mess with the period on calculation only
	public class ResourceCalculateRangeToLoadCalculator : ISchedulerRangeToLoadCalculator
	{
		private readonly DateTimePeriod _requestedDateTimePeriod;

		public ResourceCalculateRangeToLoadCalculator(DateTimePeriod requestedDateTimePeriod)
		{
			_requestedDateTimePeriod = requestedDateTimePeriod;
			
		}
		public DateTimePeriod SchedulerRangeToLoad(IPerson person)
		{
			return _requestedDateTimePeriod;
		}

		public DateTimePeriod RequestedPeriod {
			get
			{
				return _requestedDateTimePeriod;
			}
		}
	}
}