using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationResult
	{
		private readonly CheckScheduleHints _checkScheduleHints;
		private readonly SuccessfulScheduledAgents _successfulScheduledAgents;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFindSchedulesForPersons _findSchedulesForPersons;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ICurrentScenario _currentScenario;
		private readonly BlockPreferenceProviderUsingFiltersFactory _blockPreferenceProviderUsingFiltersFactory;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		public OptimizationResult(Func<ISchedulerStateHolder> schedulerStateHolder, IFindSchedulesForPersons findSchedulesForPersons, 
			IUserTimeZone userTimeZone, ICurrentScenario currentScenario,  
			CheckScheduleHints checkScheduleHints, SuccessfulScheduledAgents successfulScheduledAgents, 
			BlockPreferenceProviderUsingFiltersFactory blockPreferenceProviderUsingFiltersFactory, 
			ICurrentUnitOfWork currentUnitOfWork, IResourceCalculation resourceCalculation, 
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory, FillSchedulerStateHolder fillSchedulerStateHolder)
		{
			_checkScheduleHints = checkScheduleHints;
			_successfulScheduledAgents = successfulScheduledAgents;
			_blockPreferenceProviderUsingFiltersFactory = blockPreferenceProviderUsingFiltersFactory;
			_currentUnitOfWork = currentUnitOfWork;
			_resourceCalculation = resourceCalculation;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_findSchedulesForPersons = findSchedulesForPersons;
			_userTimeZone = userTimeZone;
			_currentScenario = currentScenario;
		}

		[TestLog]
		[UnitOfWork]
		public virtual OptimizationResultModel Create(DateOnlyPeriod period, IEnumerable<IPerson> fixedStaffPeople, IPlanningGroup planningGroup, bool usePreferences)
		{
			//TODO: investigate, hackelihack
			_currentUnitOfWork.Current().Reassociate(fixedStaffPeople);
			var planningGroupSkills = fixedStaffPeople.SelectMany(person => person.PersonPeriods(period)).SelectMany(p => p.PersonSkillCollection.Select(s => s.Skill)).Distinct().ToArray();
			_currentUnitOfWork.Current().Reassociate(planningGroupSkills);
			//

			var schedulerStateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, period);
			var resultStateHolder = schedulerStateHolder.SchedulingResultState;
			using (_resourceCalculationContextFactory.Create(resultStateHolder, true, period.Inflate(1)))
			{
				_resourceCalculation.ResourceCalculate(period.Inflate(1),
					new ResourceCalculationData(resultStateHolder, false, false));
			}
			var allSkillsForAgentGroup = getAllSkillsForPlanningGroup(planningGroupSkills, resultStateHolder);
			var scheduleOfSelectedPeople = _findSchedulesForPersons.FindSchedulesForPersons(_currentScenario.Current(), fixedStaffPeople, 
				new ScheduleDictionaryLoadOptions(usePreferences, false, usePreferences), period.ToDateTimePeriod(_userTimeZone.TimeZone()), fixedStaffPeople, true);
			var validationResults = _checkScheduleHints.Execute(new HintInput(scheduleOfSelectedPeople, fixedStaffPeople, period, _blockPreferenceProviderUsingFiltersFactory.Create(planningGroup), usePreferences)).InvalidResources;
			var result = new OptimizationResultModel
			{
				ScheduledAgentsCount = _successfulScheduledAgents.Execute(scheduleOfSelectedPeople, period),
				BusinessRulesValidationResults = validationResults
			};
			if (resultStateHolder.SkillDays != null)
			{
				result.Map(allSkillsForAgentGroup, period);
			}
			return result;
		}

		private static Dictionary<ISkill, IEnumerable<ISkillDay>> getAllSkillsForPlanningGroup(ISkill[] planningGroupSkills,
			ISchedulingResultStateHolder resultStateHolder)
		{
			var planningGroupSkillsDictionary =
				resultStateHolder.SkillDays.Where(skill => planningGroupSkills.Contains(skill.Key))
					.ToDictionary(skill => skill.Key, skill => skill.Value);
			return planningGroupSkillsDictionary;
		}
	}
}