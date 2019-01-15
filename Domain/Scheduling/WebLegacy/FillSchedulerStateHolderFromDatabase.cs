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
		private readonly ExternalStaffProvider _externalStaffProvider;
		private readonly AddReducedSkillDaysToStateHolder _addReducedSkillDaysToStateHolder;
		private readonly ReducedSkillsProvider _reducedSkillsProvider;

		public FillSchedulerStateHolderFromDatabase(PersonalSkillsProvider personalSkillsProvider,
					IScenarioRepository scenarioRepository,
					ISkillDayLoadHelper skillDayLoadHelper,
					IFindSchedulesForPersons findSchedulesForPersons,
					IRepositoryFactory repositoryFactory,
					IPersonRepository personRepository,
					ISkillRepository skillRepository,
					ICurrentUnitOfWork currentUnitOfWork,
					IUserTimeZone userTimeZone,
					ExternalStaffProvider externalStaffProvider,
					AddReducedSkillDaysToStateHolder addReducedSkillDaysToStateHolder,
					ReducedSkillsProvider reducedSkillsProvider)
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
			_externalStaffProvider = externalStaffProvider;
			_addReducedSkillDaysToStateHolder = addReducedSkillDaysToStateHolder;
			_reducedSkillsProvider = reducedSkillsProvider;
		}

		protected override void FillScenario(ISchedulerStateHolder schedulerStateHolderTo)
		{
			schedulerStateHolderTo.SetRequestedScenario(_scenarioRepository.LoadDefaultScenario());
		}

		protected override void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			var allPeople = _personRepository.FindAllAgentsLight(period);
			ICollection<IPerson> allAgentsInIsland;
			if (agentIds == null)
			{
				allAgentsInIsland = allPeople;
			}
			else
			{
				var agentIdHashSet = agentIds.ToHashSet();
				allAgentsInIsland = allPeople.Where(x => agentIdHashSet.Contains(x.Id.Value)).ToArray();
			}

			schedulerStateHolderTo.SchedulingResultState.LoadedAgents = allAgentsInIsland.ToArray();
			schedulerStateHolderTo.ChoosenAgents.Clear();
			allAgentsInIsland.ToArray().ForEach(x => schedulerStateHolderTo.ChoosenAgents.Add(x));
		}

		protected override void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			var forecast = _skillDayLoadHelper.LoadSchedulerSkillDays(period, skills, scenario);
			schedulerStateHolderTo.SchedulingResultState.SkillDays = forecast;
			schedulerStateHolderTo.SchedulingResultState.Skills = new HashSet<ISkill>(skills);
		}

		protected override void AddSkillDaysForReducedSkills(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			var reducedSkills = _reducedSkillsProvider.Execute(schedulerStateHolderTo, period);
			var skillDaysContainingReducedSkills = _skillDayLoadHelper.LoadSchedulerSkillDays(period, reducedSkills, schedulerStateHolderTo.RequestedScenario);

			_addReducedSkillDaysToStateHolder.Execute(schedulerStateHolderTo, period, reducedSkills, skillDaysContainingReducedSkills);
		}

		protected override void FillBpos(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			var dateTimePeriod = period.ToDateTimePeriod(_userTimeZone.TimeZone());
			schedulerStateHolderTo.SchedulingResultState.ExternalStaff = _externalStaffProvider.Fetch(skills, dateTimePeriod);
		}

		protected override void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var dateTimePeriod = period.ToDateTimePeriod(_userTimeZone.TimeZone());
			schedulerStateHolderTo.LoadSchedules(_findSchedulesForPersons, agents,
				new ScheduleDictionaryLoadOptions(true, false, false),
				dateTimePeriod);
		}

		protected override void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.LoadCommonState(_currentUnitOfWork.Current(), _repositoryFactory);
			_skillRepository.FindAllWithSkillDays(period); //perf hack to prevent working with skill proxies when doing calculation
			schedulerStateHolderTo.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, _userTimeZone.TimeZone());
			schedulerStateHolderTo.SchedulingResultState.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
		}
	}
}