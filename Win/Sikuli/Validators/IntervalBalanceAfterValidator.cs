﻿using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	public class IntervalBalanceAfterValidator : ISikuliValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public IntervalBalanceAfterValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public SikuliValidationResult Validate()
		{
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var lowestIntervalBalances = ValidatorHelper.GetDailyLowestIntraIntervalBalanceForPeriod(_schedulerState, _totalSkill.AggregateSkills[1]);
			if (lowestIntervalBalances == null)
			{
				result.Result = SikuliValidationResult.ResultValue.Fail;
				result.Details.AppendLine("Validator failure");
				return result;
			}
			var ruleBreaks = checkInternalBalanceRuleBreaks(lowestIntervalBalances, 1);
			if (ruleBreaks > 0)
			{
				result.Result = SikuliValidationResult.ResultValue.Warn;
				result.Details.AppendLine(string.Format("Broken rules: {0}", ruleBreaks));
			}
			const int maxRuleBreaks = 1;
			if (ruleBreaks > maxRuleBreaks)
				result.Result = SikuliValidationResult.ResultValue.Fail;
			result.Details.AppendLine(string.Format("Lowest intra interval balance: {0}", result.Result));
			return result;
		}

		private int checkInternalBalanceRuleBreaks(IEnumerable<double?> intervalBalances, int numberOfAllowedRuleBreaks)
		{
			const double limit = 0.8;
			int numberOfRuleBreaks = intervalBalances.Count(intervalBalance => intervalBalance < limit);
			return numberOfRuleBreaks;
		}
	}
}
