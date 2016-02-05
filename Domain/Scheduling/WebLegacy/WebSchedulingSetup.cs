using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class WebSchedulingSetup
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly ISkillRepository _skillRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IPeopleAndSkillLoaderDecider _decider;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IFixedStaffLoader _fixedStaffLoader;

		public WebSchedulingSetup(IScenarioRepository scenarioRepository, 
					ISkillDayLoadHelper skillDayLoadHelper, 
					ISkillRepository skillRepository, 
					IScheduleRepository scheduleRepository, 
					IPersonAbsenceAccountRepository personAbsenceAccountRepository, 
					IPeopleAndSkillLoaderDecider decider, 
					ICurrentTeleoptiPrincipal principal, 
					ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
					Func<ISchedulerStateHolder> schedulerStateHolder,
					IRepositoryFactory repositoryFactory,
					IFixedStaffLoader fixedStaffLoader)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_skillRepository = skillRepository;
			_scheduleRepository = scheduleRepository;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_decider = decider;
			_principal = principal;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_schedulerStateHolder = schedulerStateHolder;
			_repositoryFactory = repositoryFactory;
			_fixedStaffLoader = fixedStaffLoader;
		}

		public WebSchedulingSetupResult Setup(DateOnlyPeriod period)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			schedulerStateHolder.LoadCommonState(_currentUnitOfWorkFactory.Current().CurrentUnitOfWork(), _repositoryFactory);
			var people = _fixedStaffLoader.Load(period);
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var timeZone = _principal.Current().Regional.TimeZone;
			
			var allSkills = _skillRepository.FindAllWithSkillDays(period).ToArray();
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);

			var deciderResult = _decider.Execute(scenario, dateTimePeriod, people.AllPeople);
			deciderResult.FilterPeople(people.AllPeople);

			var forecast = _skillDayLoadHelper.LoadSchedulerSkillDays(period, allSkills, scenario);

			var stateHolder = schedulerStateHolder.SchedulingResultState;
			stateHolder.PersonsInOrganization = people.AllPeople;
			stateHolder.SkillDays = forecast;
			stateHolder.AddSkills(allSkills);
			deciderResult.FilterSkills(allSkills,stateHolder.RemoveSkill, s => stateHolder.AddSkills(s));
			
			schedulerStateHolder.SetRequestedScenario(scenario);
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, timeZone);
			people.AllPeople.ForEach(schedulerStateHolder.AllPermittedPersons.Add);
			stateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(people.AllPeople);
			schedulerStateHolder.ResetFilteredPersons();
			schedulerStateHolder.LoadSchedules(_scheduleRepository, new PersonsInOrganizationProvider(people.AllPeople),
				new ScheduleDictionaryLoadOptions(true, false, false),
				new ScheduleDateTimePeriod(dateTimePeriod, people.AllPeople, new SchedulerRangeToLoadCalculator(dateTimePeriod)));
			return new WebSchedulingSetupResult(people, extractAllSchedules(schedulerStateHolder.SchedulingResultState, people, period));
		}

		private static IList<IScheduleDay> extractAllSchedules(ISchedulingResultStateHolder stateHolder, PeopleSelection people, DateOnlyPeriod period)
		{
			var allSchedules = new List<IScheduleDay>();
			foreach (var schedule in stateHolder.Schedules.Where(schedule => people.FixedStaffPeople.Contains(schedule.Key)))
			{
				allSchedules.AddRange(schedule.Value.ScheduledDayCollection(period));
			}
			return allSchedules;
		}
	}
}