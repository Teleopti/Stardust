using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class BusinessRulesValidator:IScheduleValidator
	{
		private readonly DayOffBusinessRuleValidation _dayOffBusinessRuleValidation;

		public BusinessRulesValidator(DayOffBusinessRuleValidation dayOffBusinessRuleValidation)
		{
			_dayOffBusinessRuleValidation = dayOffBusinessRuleValidation;
		}

		public void FillResult(ValidationResult validationResult, IScheduleDictionary schedules, IEnumerable<IPerson> agents,
			DateOnlyPeriod period)
		{
			if (schedules == null) return;
			foreach (var item in schedules)
			{
				if (!agents.Contains(item.Key)) continue;
				if (validationResult.InvalidResources.Any(x => item.Key.Id != null && x.ResourceId == item.Key.Id.Value)) continue;
				if (!_dayOffBusinessRuleValidation.Validate(item.Value, period))
				{
					validationResult.Add(new PersonValidationError()
					{
						PersonName = item.Key.Name.ToString(),
						PersonId = item.Key.Id.Value,
						ValidationError = string.Format(UserTexts.Resources.TargetDayOffNotFulfilledMessage,
							item.Value.CalculatedTargetScheduleDaysOff(period))
					}, GetType());
				}
				else if (!isAgentFulfillingContractTime(item.Value, period))
				{
					validationResult.Add(new PersonValidationError()
					{
						PersonName = item.Key.Name.ToString(),
						PersonId = item.Key.Id.Value,
						ValidationError =
							string.Format(UserTexts.Resources.TargetScheduleTimeNotFullfilled,
								DateHelper.HourMinutesString(
									item.Value.CalculatedTargetTimeHolder(period).GetValueOrDefault(TimeSpan.Zero).TotalMinutes))
					}, GetType());
				}
				else
				{
					var agentScheduleDaysWithoutSchedule = getAgentScheduleDaysWithoutSchedule(item.Value, period);
					if (agentScheduleDaysWithoutSchedule <= 0) continue;
					validationResult.Add(new PersonValidationError()
					{
						PersonName = item.Key.Name.ToString(),
						PersonId = item.Key.Id.Value,
						ValidationError =
							string.Format(UserTexts.Resources.AgentHasDaysWithoutAnySchedule, agentScheduleDaysWithoutSchedule)
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
