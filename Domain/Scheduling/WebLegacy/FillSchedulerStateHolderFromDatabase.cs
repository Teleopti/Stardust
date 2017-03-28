using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class FillSchedulerStateHolderFromDatabase : FillSchedulerStateHolder
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IFindSchedulesForPersons _findSchedulesForPersons;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IUserTimeZone _userTimeZone;

		public FillSchedulerStateHolderFromDatabase(PersonalSkillsProvider personalSkillsProvider,
					IScenarioRepository scenarioRepository,
					ISkillDayLoadHelper skillDayLoadHelper,
					IFindSchedulesForPersons findSchedulesForPersons,
					IPersonAbsenceAccountRepository personAbsenceAccountRepository,
					IRepositoryFactory repositoryFactory,
					IPersonRepository personRepository,
					ISkillRepository skillRepository,
					ICurrentUnitOfWork currentUnitOfWork,
					IUserTimeZone userTimeZone) : base(personalSkillsProvider)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_findSchedulesForPersons = findSchedulesForPersons;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_repositoryFactory = repositoryFactory;
			_personRepository = personRepository;
			_skillRepository = skillRepository;
			_currentUnitOfWork = currentUnitOfWork;
			_userTimeZone = userTimeZone;
		}

		protected override IScenario FetchScenario()
		{
			return _scenarioRepository.LoadDefaultScenario();
		}

		protected override void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			var allPeople = _personRepository.FindPeopleInOrganizationLight(period);
			schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization = allPeople.ToList();
			schedulerStateHolderTo.AllPermittedPersons.Clear();
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
			var dateTimePeriod = period.ToDateTimePeriod(_userTimeZone.TimeZone());
			schedulerStateHolderTo.SetRequestedScenario(scenario);
			var personProvider = new PersonsInOrganizationProvider(agents) {DoLoadByPerson = true }; //TODO: this is experimental
			schedulerStateHolderTo.LoadSchedules(_findSchedulesForPersons, personProvider,
				new ScheduleDictionaryLoadOptions(true, false, false),
				new ScheduleDateTimePeriod(dateTimePeriod, agents, new SchedulerRangeToLoadCalculator(dateTimePeriod)));
		}

		protected override void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.LoadCommonState(_currentUnitOfWork.Current(), _repositoryFactory);
			_skillRepository.FindAllWithSkillDays(period); //perf hack to prevent working with skill proxies when doing calculation
		}

		protected override void PostFill(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, _userTimeZone.TimeZone());
			schedulerStateHolderTo.SchedulingResultState.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(agents);
		}
	}
}