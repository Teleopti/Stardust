using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class BusinessRulesHint : ISchedulePostHint
	{
		private readonly DayOffBusinessRuleValidation _dayOffBusinessRuleValidation;

		public BusinessRulesHint(DayOffBusinessRuleValidation dayOffBusinessRuleValidation)
		{
			_dayOffBusinessRuleValidation = dayOffBusinessRuleValidation;
		}

		public void FillResult(HintResult validationResult, SchedulePostHintInput input)
		{
			var schedules = input.Schedules;
			var agents = input.People;
			var period = input.Period;
			foreach (var item in schedules)
			{
				if (!agents.Contains(item.Key)) continue;
				var personPeriods = item.Key.PersonPeriods(input.Period);
				if (personPeriods.Any(x => x.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff)) continue;

				if (validationResult.InvalidResources.Any(x => item.Key.Id != null && x.ResourceId == item.Key.Id.Value)) continue;

				var targetSummary = new TargetScheduleSummaryCalculator().GetTargets(item.Value, period);
				var selectedDaysSummary = new CurrentScheduleSummaryCalculator().GetCurrent(item.Value, period);
				var agentScheduleDaysWithoutSchedule = selectedDaysSummary.DaysWithoutSchedule;
				var totalSchedulePeriod = getAffectedSchedulePeriod(period, item);
				var currentSummary = new CurrentScheduleSummaryCalculator().GetCurrent(item.Value, totalSchedulePeriod);

				if (agentScheduleDaysWithoutSchedule == 0) continue;
				if (!_dayOffBusinessRuleValidation.Validate(targetSummary, currentSummary))
				{
					validationResult.Add(new PersonHintError
					{
						PersonName = item.Key.Name.ToString(),
						PersonId = item.Key.Id.Value,
						ErrorResource = nameof(UserTexts.Resources.TargetDayOffNotFulfilledMessage),
						ErrorResourceData = new object[] {item.Value.CalculatedTargetScheduleDaysOff(period)}.ToList()
					}, GetType());
				}
				else if (!isAgentFulfillingContractTime(targetSummary, currentSummary))
				{
					validationResult.Add(new PersonHintError
					{
						PersonName = item.Key.Name.ToString(),
						PersonId = item.Key.Id.Value,
						ErrorResource = nameof(UserTexts.Resources.TargetScheduleTimeNotFullfilled),
						ErrorResourceData = new object[]
						{
							DateHelper.HourMinutesString(
								item.Value.CalculatedTargetTime(period).GetValueOrDefault(TimeSpan.Zero).TotalMinutes)
						}.ToList()
					}, GetType());
				}
				else
				{
					validationResult.Add(new PersonHintError
					{
						PersonName = item.Key.Name.ToString(),
						PersonId = item.Key.Id.Value,
						ErrorResource = nameof(UserTexts.Resources.AgentHasDaysWithoutAnySchedule),
						ErrorResourceData = new object[] {agentScheduleDaysWithoutSchedule}.ToList()
					}, GetType());
				}
			}
		}

		private static DateOnlyPeriod getAffectedSchedulePeriod(DateOnlyPeriod period,
			KeyValuePair<IPerson, IScheduleRange> item)
		{
			var virtualPeriods = new HashSet<IVirtualSchedulePeriod>();
			foreach (var dateOnly in period.DayCollection())
			{
				virtualPeriods.Add(item.Key.VirtualSchedulePeriod(dateOnly));
			}
			var totalSchedulePeriod = new DateOnlyPeriod(virtualPeriods.Min(x => x.DateOnlyPeriod.StartDate),
				virtualPeriods.Max(x => x.DateOnlyPeriod.EndDate));
			return totalSchedulePeriod;
		}

		private static bool isAgentFulfillingContractTime(TargetScheduleSummary targetSummary,
			CurrentScheduleSummary currentSummary)
		{
			return targetSummary.TargetTime.HasValue &&
				   targetSummary.TargetTime - targetSummary.NegativeTargetTimeTolerance <= currentSummary.ContractTime &&
				   targetSummary.TargetTime + targetSummary.PositiveTargetTimeTolerance >= currentSummary.ContractTime;
		}
	}
}
