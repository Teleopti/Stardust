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
	public class FillSchedulerStateHolderFromDatabase : FillSchedulerStateHolder
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly ISkillRepository _skillRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IPeopleAndSkillLoaderDecider _decider;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IFixedStaffLoader _fixedStaffLoader;

		public FillSchedulerStateHolderFromDatabase(IScenarioRepository scenarioRepository,
					ISkillDayLoadHelper skillDayLoadHelper,
					ISkillRepository skillRepository,
					IScheduleStorage scheduleStorage,
					IPersonAbsenceAccountRepository personAbsenceAccountRepository,
					IPeopleAndSkillLoaderDecider decider,
					ICurrentTeleoptiPrincipal principal,
					ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
					IRepositoryFactory repositoryFactory,
					IFixedStaffLoader fixedStaffLoader)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_skillRepository = skillRepository;
			_scheduleStorage = scheduleStorage;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_decider = decider;
			_principal = principal;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_repositoryFactory = repositoryFactory;
			_fixedStaffLoader = fixedStaffLoader;
		}

		protected override IEnumerable<IPerson> FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var timeZone = _principal.Current().Regional.TimeZone;
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);
			var people = _fixedStaffLoader.Load(period);
			var deciderResult = _decider.Execute(scenario, dateTimePeriod, people.AllPeople);
			deciderResult.FilterPeople(people.AllPeople);
			schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization = people.AllPeople;
			people.AllPeople.ForEach(schedulerStateHolderTo.AllPermittedPersons.Add);
			schedulerStateHolderTo.ResetFilteredPersons();
			return people.AllPeople;
		}

		protected override void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			//TODO: see if we can reuse from fillagents
			var scenario = _scenarioRepository.LoadDefaultScenario();
			//

			var allSkills = _skillRepository.FindAllWithSkillDays(period).ToArray();
			var forecast = _skillDayLoadHelper.LoadSchedulerSkillDays(period, allSkills, scenario);
			schedulerStateHolderTo.SchedulingResultState.SkillDays = forecast;
			schedulerStateHolderTo.SchedulingResultState.AddSkills(allSkills);

			//TODO: see if we can reuse from fillagents
			var timeZone = _principal.Current().Regional.TimeZone;
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);
			var deciderResult = _decider.Execute(scenario, dateTimePeriod, schedulerStateHolderTo.AllPermittedPersons);
			//
			deciderResult.FilterSkills(allSkills, schedulerStateHolderTo.SchedulingResultState.RemoveSkill, s => schedulerStateHolderTo.SchedulingResultState.AddSkills(s));
		}

		protected override void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			//TODO: see if we can reuse from fillagents
			var scenario = _scenarioRepository.LoadDefaultScenario();
			//
			var timeZone = _principal.Current().Regional.TimeZone;
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);
			schedulerStateHolderTo.SetRequestedScenario(scenario);
			schedulerStateHolderTo.LoadSchedules(_scheduleStorage, new PersonsInOrganizationProvider(agents),
				new ScheduleDictionaryLoadOptions(true, false, false),
				new ScheduleDateTimePeriod(dateTimePeriod, agents, new SchedulerRangeToLoadCalculator(dateTimePeriod)));
		}

		protected override void PreFill(ISchedulerStateHolder schedulerStateHolderTo)
		{
			schedulerStateHolderTo.LoadCommonState(_currentUnitOfWorkFactory.Current().CurrentUnitOfWork(), _repositoryFactory);
		}

		protected override void PostFill(ISchedulerStateHolder schedulerStateHolder, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var timeZone = _principal.Current().Regional.TimeZone;
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, timeZone);
			schedulerStateHolder.SchedulingResultState.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(agents);
		}
	}
}