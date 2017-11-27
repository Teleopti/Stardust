using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class BusinessRulesHint:IScheduleHint
	{
		private readonly DayOffBusinessRuleValidation _dayOffBusinessRuleValidation;

		public BusinessRulesHint(DayOffBusinessRuleValidation dayOffBusinessRuleValidation)
		{
			_dayOffBusinessRuleValidation = dayOffBusinessRuleValidation;
		}

		public void FillResult(HintResult validationResult, HintInput input)
		{
			var schedules = input.Schedules;
			var agents = input.People;
			var period = input.Period;
			if (schedules == null) return;
			foreach (var item in schedules)
			{
				if (!agents.Contains(item.Key)) continue;
				if (validationResult.InvalidResources.Any(x => item.Key.Id != null && x.ResourceId == item.Key.Id.Value)) continue;
				if (!_dayOffBusinessRuleValidation.Validate(item.Value, period))
				{
					validationResult.Add(new PersonHintError
					{
						PersonName = item.Key.Name.ToString(),
						PersonId = item.Key.Id.Value,
						ErrorResource = nameof(UserTexts.Resources.TargetDayOffNotFulfilledMessage),
						ErrorResourceData = new object [] { item.Value.CalculatedTargetScheduleDaysOff(period) }.ToList()
					}, GetType());
				}
				else if (!isAgentFulfillingContractTime(item.Value, period))
				{
					validationResult.Add(new PersonHintError
					{
						PersonName = item.Key.Name.ToString(),
						PersonId = item.Key.Id.Value,
						ErrorResource = nameof(UserTexts.Resources.TargetScheduleTimeNotFullfilled),
						ErrorResourceData = new object[] {DateHelper.HourMinutesString(
									item.Value.CalculatedTargetTimeHolder(period).GetValueOrDefault(TimeSpan.Zero).TotalMinutes)}.ToList()
					}, GetType());
				}
				else
				{
					var agentScheduleDaysWithoutSchedule = getAgentScheduleDaysWithoutSchedule(item.Value, period);
					if (agentScheduleDaysWithoutSchedule <= 0) continue;
					validationResult.Add(new PersonHintError
					{
						PersonName = item.Key.Name.ToString(),
						PersonId = item.Key.Id.Value,
						ErrorResource = nameof(UserTexts.Resources.AgentHasDaysWithoutAnySchedule),
						ErrorResourceData = new object[] { agentScheduleDaysWithoutSchedule }.ToList()
					}, GetType());
				}
			}
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
