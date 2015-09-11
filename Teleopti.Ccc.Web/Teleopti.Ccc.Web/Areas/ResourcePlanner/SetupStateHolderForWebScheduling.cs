using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class SetupStateHolderForWebScheduling
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

		public SetupStateHolderForWebScheduling(IScenarioRepository scenarioRepository, ISkillDayLoadHelper skillDayLoadHelper, ISkillRepository skillRepository, IScheduleRepository scheduleRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, IPeopleAndSkillLoaderDecider decider, ICurrentTeleoptiPrincipal principal, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, Func<ISchedulerStateHolder> schedulerStateHolder)
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
		}

		public void Setup(DateOnlyPeriod period, PeopleSelection people)
		{
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var timeZone = _principal.Current().Regional.TimeZone;
			
			var allSkills = _skillRepository.FindAllWithSkillDays(period);
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);

			var deciderResult = _decider.Execute(scenario, dateTimePeriod, people.AllPeople);
			deciderResult.FilterSkills(allSkills);
			deciderResult.FilterPeople(people.AllPeople);

			var forecast = _skillDayLoadHelper.LoadSchedulerSkillDays(period, allSkills, scenario);

			var schedulerStateHolder = _schedulerStateHolder();
			var stateHolder = schedulerStateHolder.SchedulingResultState;
			stateHolder.PersonsInOrganization = people.AllPeople;
			stateHolder.SkillDays = forecast;
			allSkills.ForEach(stateHolder.Skills.Add);
			schedulerStateHolder.SetRequestedScenario(scenario);
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, timeZone);
			people.AllPeople.ForEach(schedulerStateHolder.AllPermittedPersons.Add);
			schedulerStateHolder.LoadCommonState(_currentUnitOfWorkFactory.Current().CurrentUnitOfWork(),
				new RepositoryFactory());
			stateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(people.AllPeople);
			schedulerStateHolder.ResetFilteredPersons();
			schedulerStateHolder.LoadSchedules(_scheduleRepository, new PersonsInOrganizationProvider(people.AllPeople),
				new ScheduleDictionaryLoadOptions(true, false, false),
				new ScheduleDateTimePeriod(dateTimePeriod, people.AllPeople, new SchedulerRangeToLoadCalculator(dateTimePeriod)));
		}
	}
}