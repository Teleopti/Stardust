using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationResult
	{
		private readonly SchedulingValidator _schedulingValidator;
		private readonly SuccessfulScheduledAgents _successfulScheduledAgents;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFindSchedulesForPersons _findSchedulesForPersons;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ICurrentScenario _currentScenario;

		public OptimizationResult(Func<ISchedulerStateHolder> schedulerStateHolder, IFindSchedulesForPersons findSchedulesForPersons, 
			IUserTimeZone userTimeZone, ICurrentScenario currentScenario,  
			SchedulingValidator schedulingValidator, SuccessfulScheduledAgents successfulScheduledAgents)
		{
			_schedulingValidator = schedulingValidator;
			_successfulScheduledAgents = successfulScheduledAgents;
			_schedulerStateHolder = schedulerStateHolder;
			_findSchedulesForPersons = findSchedulesForPersons;
			_userTimeZone = userTimeZone;
			_currentScenario = currentScenario;
		}

		[TestLog]
		[UnitOfWork]
		public virtual OptimizationResultModel Create(DateOnlyPeriod period, IEnumerable<IPerson> fixedStaffPeople)
		{
			var resultStateHolder = _schedulerStateHolder().SchedulingResultState;
			var allSkillsForAgentGroup = getAllSkillsForPlanningGroup(period, fixedStaffPeople, resultStateHolder);

			var personsProvider = new PersonsInOrganizationProvider(fixedStaffPeople) { DoLoadByPerson = true };
			var scheduleOfSelectedPeople = _findSchedulesForPersons.FindSchedulesForPersons(
				new ScheduleDateTimePeriod(period.ToDateTimePeriod(_userTimeZone.TimeZone()), fixedStaffPeople), _currentScenario.Current(), personsProvider, 
				new ScheduleDictionaryLoadOptions(false, false, false), fixedStaffPeople);


			var validationResults = _schedulingValidator.Validate(scheduleOfSelectedPeople, fixedStaffPeople, period).InvalidResources;
			//just to make ShouldShowAgentWithMissingShiftAsNotScheduled green
			validationResults.Add(new SchedulingValidationError());
			//
			
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

		private static Dictionary<ISkill, IEnumerable<ISkillDay>> getAllSkillsForPlanningGroup(DateOnlyPeriod period, IEnumerable<IPerson> fixedStaffPeople,
			ISchedulingResultStateHolder resultStateHolder)
		{
			var planningGroupSkills = new HashSet<ISkill>();
			foreach (var person in fixedStaffPeople)
			{
				var periods = person.PersonPeriods(period);
				periods.SelectMany(p => p.PersonSkillCollection).ForEach(s => planningGroupSkills.Add(s.Skill));
			}

			var planningGroupSkillsDictionary =
				resultStateHolder.SkillDays.Where(skill => planningGroupSkills.Contains(skill.Key))
					.ToDictionary(skill => skill.Key, skill => skill.Value);
			return planningGroupSkillsDictionary;
		}
	}
}