using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
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
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFindSchedulesForPersons _findSchedulesForPersons;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ICurrentScenario _currentScenario;

		public OptimizationResult(Func<ISchedulerStateHolder> schedulerStateHolder, IFindSchedulesForPersons findSchedulesForPersons, 
			IUserTimeZone userTimeZone, ICurrentScenario currentScenario,  
			SchedulingValidator schedulingValidator)
		{
			_schedulingValidator = schedulingValidator;
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
				ScheduledAgentsCount = successfulScheduledAgents(scheduleOfSelectedPeople, period),
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
			var planningGroupSkills = new List<ISkill>();
			foreach (var person in fixedStaffPeople)
			{
				for (var i = person.PersonPeriodCollection.Count - 1; i >= 0; i--)
				{
					if (person.PersonPeriodCollection[i].StartDate > period.EndDate) continue;
					foreach (var skill in person.PersonPeriodCollection[i].PersonSkillCollection)
					{
						if (!planningGroupSkills.Contains(skill.Skill))
							planningGroupSkills.Add(skill.Skill);
					}
					if (person.PersonPeriodCollection[i].StartDate <= period.StartDate)
						break;
				}
			}

			var planningGroupSkillsDictionary =
				resultStateHolder.SkillDays.Where(skill => planningGroupSkills.Contains(skill.Key))
					.ToDictionary(skill => skill.Key, skill => skill.Value);
			return planningGroupSkillsDictionary;
		}
		
		private static int successfulScheduledAgents(IEnumerable<KeyValuePair<IPerson, IScheduleRange>> schedules, DateOnlyPeriod periodToCheck)
		{
			return schedules.Count(x => isAgentScheduled(x.Value, periodToCheck));
		}

		private static bool isAgentScheduled(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			return isAgentFulfillingContractTime(scheduleRange, periodToCheck) &&
				   getAgentScheduleDaysWithoutSchedule(scheduleRange, periodToCheck) == 0;
		}

		private static bool isAgentFulfillingContractTime(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			var targetSummary = scheduleRange.CalculatedTargetTimeSummary(periodToCheck);
			var scheduleSummary = scheduleRange.CalculatedCurrentScheduleSummary(periodToCheck);
			return targetSummary.TargetTime.HasValue &&
				   targetSummary.TargetTime - targetSummary.NegativeTargetTimeTolerance <= scheduleSummary.ContractTime &&
				   targetSummary.TargetTime + targetSummary.PositiveTargetTimeTolerance >= scheduleSummary.ContractTime;
		}

		private static int getAgentScheduleDaysWithoutSchedule(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			var scheduleSummary = scheduleRange.CalculatedCurrentScheduleSummary(periodToCheck);
			return scheduleSummary.DaysWithoutSchedule;
		}
	}
}