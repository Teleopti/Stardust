using System;
using System.Collections.Generic;
using System.Linq;
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
	public class FillSchedulerStateHolderForResourceCalculation : FillSchedulerStateHolder
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;

		public FillSchedulerStateHolderForResourceCalculation(IScenarioRepository scenarioRepository,
					ISkillDayLoadHelper skillDayLoadHelper,
					IScheduleStorage scheduleStorage,
					IPersonAbsenceAccountRepository personAbsenceAccountRepository,
					ICurrentTeleoptiPrincipal principal,
					ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
					IRepositoryFactory repositoryFactory,
					IPersonRepository personRepository,
					ISkillRepository skillRepository)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_scheduleStorage = scheduleStorage;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_principal = principal;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_repositoryFactory = repositoryFactory;
			_personRepository = personRepository;
			_skillRepository = skillRepository;
		}

		protected override IScenario FetchScenario()
		{
			return _scenarioRepository.LoadDefaultScenario();
		}

		protected override void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			var allPeople = _personRepository.FindPeopleInOrganizationQuiteLight(period);
			schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization = allPeople.ToList();
			allPeople.ForEach(x => schedulerStateHolderTo.AllPermittedPersons.Add(x));
		}

		protected override void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			var forecast = _skillDayLoadHelper.LoadSchedulerSkillDays(period, skills, scenario);
			schedulerStateHolderTo.SchedulingResultState.SkillDays = forecast;
			schedulerStateHolderTo.SchedulingResultState.AddSkills(skills.ToArray());
		}

		protected override void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var timeZone = _principal.Current().Regional.TimeZone;
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);
			schedulerStateHolderTo.SetRequestedScenario(scenario);
			var personProvider = new PersonsInOrganizationProvider(agents) {DoLoadByPerson = true }; //TODO: this is experimental
			schedulerStateHolderTo.LoadSchedules(_scheduleStorage, personProvider,
				new ScheduleDictionaryLoadOptions(false, false, false),
				new ScheduleDateTimePeriod(dateTimePeriod, agents, new SchedulerRangeToLoadCalculator(dateTimePeriod)));
		}

		protected override void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			//just temporary till we know why it doesn't work without it
			_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
			schedulerStateHolderTo.LoadCommonStateForResourceCalculationOnly(_currentUnitOfWorkFactory.Current().CurrentUnitOfWork(), _repositoryFactory);
			_skillRepository.FindAllWithSkillDays(period); //perf hack to prevent working with skill proxies when doing calculation
		}

		protected override void PostFill(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var timeZone = _principal.Current().Regional.TimeZone;
			schedulerStateHolderTo.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, timeZone);
			//schedulerStateHolderTo.SchedulingResultState.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(agents);
		}
	}
}