using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FullSchedulingResult
	{
		private readonly CheckScheduleHints _checkScheduleHints;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFindSchedulesForPersons _findSchedulesForPersons;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ICurrentScenario _currentScenario;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly AgentsWithWhiteSpots _agentsWithWhiteSpots;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		

		public FullSchedulingResult(Func<ISchedulerStateHolder> schedulerStateHolder, IFindSchedulesForPersons findSchedulesForPersons, 
			IUserTimeZone userTimeZone, ICurrentScenario currentScenario,  
			CheckScheduleHints checkScheduleHints, 
			IResourceCalculation resourceCalculation, 
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory, FillSchedulerStateHolder fillSchedulerStateHolder,
			AgentsWithWhiteSpots agentsWithWhiteSpots, IPlanningPeriodRepository planningPeriodRepository)
		{
			_checkScheduleHints = checkScheduleHints;
			_resourceCalculation = resourceCalculation;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_agentsWithWhiteSpots = agentsWithWhiteSpots;
			_planningPeriodRepository = planningPeriodRepository;
			_schedulerStateHolder = schedulerStateHolder;
			_findSchedulesForPersons = findSchedulesForPersons;
			_userTimeZone = userTimeZone;
			_currentScenario = currentScenario;
		}

		[TestLog]
		[UnitOfWork]
		public virtual FullSchedulingResultModel Create(DateOnlyPeriod period, HashSet<IPerson> selectedAgents, Guid planningPeriodId)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, period, null);
			var resultStateHolder = schedulerStateHolder.SchedulingResultState;
			var loadedSelectedAgents = resultStateHolder.LoadedAgents.Where(selectedAgents.Contains).ToArray();
			var planningGroupSkills = loadedSelectedAgents
				.SelectMany(person => person.PersonPeriods(period)).SelectMany(p => p.PersonSkillCollection.Select(s => s.Skill)).ToHashSet();
			using (_resourceCalculationContextFactory.Create(resultStateHolder, true, period.Inflate(1)))
			{
				_resourceCalculation.ResourceCalculate(period.Inflate(1),
					new ResourceCalculationData(resultStateHolder, false, false));
			}
			var allSkillsForAgentGroup = getAllSkillsForPlanningGroup(planningGroupSkills, resultStateHolder);
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			var preferenceValue = planningPeriod.PlanningGroup.Settings.PreferenceValue.Value;
			var scheduleOfSelectedPeople = _findSchedulesForPersons.FindSchedulesForPersons(_currentScenario.Current(), loadedSelectedAgents, 
				new ScheduleDictionaryLoadOptions(preferenceValue > 0, false, preferenceValue > 0), period.ToDateTimePeriod(_userTimeZone.TimeZone()), loadedSelectedAgents, true);
			var blockPreferenceProvider = new BlockPreferenceProviderUsingFilters(planningPeriod.PlanningGroup.Settings);
			var validationResults = _checkScheduleHints.Execute(new SchedulePostHintInput(scheduleOfSelectedPeople, loadedSelectedAgents, period, blockPreferenceProvider, preferenceValue)).InvalidResources;
			var nonScheduledAgents = _agentsWithWhiteSpots.Execute(scheduleOfSelectedPeople, loadedSelectedAgents, period);
			var result = new FullSchedulingResultModel
			{
				ScheduledAgentsCount = loadedSelectedAgents.Length - nonScheduledAgents.Count(),
				BusinessRulesValidationResults = validationResults
			};
			if (resultStateHolder.SkillDays != null)
			{
				result.Map(allSkillsForAgentGroup, period);
			}
			return result;
		}

		private static Dictionary<ISkill, IEnumerable<ISkillDay>> getAllSkillsForPlanningGroup(HashSet<ISkill> planningGroupSkills,
			ISchedulingResultStateHolder resultStateHolder)
		{
			var planningGroupSkillsDictionary =
				resultStateHolder.SkillDays.Where(skill => planningGroupSkills.Contains(skill.Key))
					.ToDictionary(skill => skill.Key, skill => skill.Value);
			return planningGroupSkillsDictionary;
		}
	}
}