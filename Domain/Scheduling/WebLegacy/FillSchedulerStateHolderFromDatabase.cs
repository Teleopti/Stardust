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
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class FillSchedulerStateHolderFromDatabase : FillSchedulerStateHolder
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IFindSchedulesForPersons _findSchedulesForPersons;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ISkillCombinationResourceBpoReader _skillCombinationResourceBpoReader;
		private readonly SkillCombinationToBpoResourceMapper _skillCombinationToBpoResourceMapper;

		public FillSchedulerStateHolderFromDatabase(PersonalSkillsProvider personalSkillsProvider,
					IScenarioRepository scenarioRepository,
					ISkillDayLoadHelper skillDayLoadHelper,
					IFindSchedulesForPersons findSchedulesForPersons,
					IRepositoryFactory repositoryFactory,
					IPersonRepository personRepository,
					ISkillRepository skillRepository,
					ICurrentUnitOfWork currentUnitOfWork,
					IUserTimeZone userTimeZone,
					//TODO: make one class of these 
					ISkillCombinationResourceBpoReader skillCombinationResourceBpoReader, 
					SkillCombinationToBpoResourceMapper skillCombinationToBpoResourceMapper) 
					//////////////////////////////
			: base(personalSkillsProvider)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_findSchedulesForPersons = findSchedulesForPersons;
			_repositoryFactory = repositoryFactory;
			_personRepository = personRepository;
			_skillRepository = skillRepository;
			_currentUnitOfWork = currentUnitOfWork;
			_userTimeZone = userTimeZone;
			_skillCombinationResourceBpoReader = skillCombinationResourceBpoReader;
			_skillCombinationToBpoResourceMapper = skillCombinationToBpoResourceMapper;
		}

		protected override void FillScenario(ISchedulerStateHolder schedulerStateHolderTo)
		{
			schedulerStateHolderTo.SetRequestedScenario(_scenarioRepository.LoadDefaultScenario());
		}

		protected override void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, IEnumerable<Guid> choosenAgentIds, DateOnlyPeriod period)
		{
			var allPeople = _personRepository.FindAllAgentsLight(period);
			schedulerStateHolderTo.SchedulingResultState.LoadedAgents = allPeople.ToList();
			schedulerStateHolderTo.ChoosenAgents.Clear();
			allPeople.Where(x => choosenAgentIds==null || choosenAgentIds.Contains(x.Id.Value))
				.ForEach(x => schedulerStateHolderTo.ChoosenAgents.Add(x));
		}

		protected override void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			var forecast = _skillDayLoadHelper.LoadSchedulerSkillDays(period, skills, scenario);
			schedulerStateHolderTo.SchedulingResultState.SkillDays = forecast;
			schedulerStateHolderTo.SchedulingResultState.AddSkills(skills.ToArray());
		}

		protected override void FillBpos(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			var dateTimePeriod = period.ToDateTimePeriod(_userTimeZone.TimeZone());
			//temp... make a seperate class of this 
			var skillCombinationResources = _skillCombinationResourceBpoReader.Execute(dateTimePeriod);
			var bpoResources = _skillCombinationToBpoResourceMapper.Execute(skillCombinationResources, skills);
			//
			schedulerStateHolderTo.SchedulingResultState.BpoResources = bpoResources;
		}

		protected override void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var dateTimePeriod = period.ToDateTimePeriod(_userTimeZone.TimeZone());
			var personProvider = new PersonProvider(agents) {DoLoadByPerson = true }; //TODO: this is experimental
			schedulerStateHolderTo.LoadSchedules(_findSchedulesForPersons, personProvider,
				new ScheduleDictionaryLoadOptions(true, false, false),
				dateTimePeriod);
		}

		protected override void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.LoadCommonState(_currentUnitOfWork.Current(), _repositoryFactory);
			_skillRepository.FindAllWithSkillDays(period); //perf hack to prevent working with skill proxies when doing calculation
		}

		protected override void PostFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, _userTimeZone.TimeZone());
			schedulerStateHolderTo.SchedulingResultState.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
		}
	}
}