using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationResult
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFindSchedulesForPersons _findSchedulesForPersons;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ICurrentScenario _currentScenario;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public OptimizationResult(Func<ISchedulerStateHolder> schedulerStateHolder, IFindSchedulesForPersons findSchedulesForPersons, IUserTimeZone userTimeZone, ICurrentScenario currentScenario, ICurrentUnitOfWork currentUnitOfWork)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_findSchedulesForPersons = findSchedulesForPersons;
			_userTimeZone = userTimeZone;
			_currentScenario = currentScenario;
			_currentUnitOfWork = currentUnitOfWork;
		}

		[TestLog]
		[UnitOfWork]
		public virtual OptimizationResultModel Create(DateOnlyPeriod period, IEnumerable<IPerson> fixedStaffPeople)
		{
			//some hack to get rid of lazy load ex
			var uow = _currentUnitOfWork.Current();
			uow.Reassociate(fixedStaffPeople);
			//

			var resultStateHolder = _schedulerStateHolder().SchedulingResultState;
			var personsProvider = new PersonsInOrganizationProvider(fixedStaffPeople) { DoLoadByPerson = true };
			var scheduleOfSelectedPeople = _findSchedulesForPersons.FindSchedulesForPersons(new ScheduleDateTimePeriod(period.ToDateTimePeriod(_userTimeZone.TimeZone()), fixedStaffPeople), _currentScenario.Current(), personsProvider, new ScheduleDictionaryLoadOptions(false, false, false), fixedStaffPeople);

			var violatedBusinessRules = new List<BusinessRulesValidationResult>();

			var result = new OptimizationResultModel()
			{
				ScheduledAgentsCount = successfulScheduledAgents(scheduleOfSelectedPeople, period),
				BusinessRulesValidationResults = violatedBusinessRules
			};
			
			if (resultStateHolder.SkillDays != null)
			{
				result.Map(resultStateHolder.SkillDays, period);
			}

			return result;
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