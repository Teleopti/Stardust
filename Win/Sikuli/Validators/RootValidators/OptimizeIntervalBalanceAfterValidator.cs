using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeIntervalBalanceAfterValidator : IRootValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		private const double limit = 0.8d;
		private const int maxRuleBreaks = 3;
		private TimeSpan durationLimit;

		public OptimizeIntervalBalanceAfterValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
			durationLimit = TimeSpan.FromMinutes(1);
		}

		public string Description
		{
			get
			{
				return string.Format(
					"Only {0} 'lowest intra interval balance' value can be under {1}. Duration must be under {2}.",
					maxRuleBreaks, limit, durationLimit.ToString(@"mm\:ss"));
			}
		}

		public SikuliValidationResult Validate(ITestDuration duration)
		{
			var intradayValidationResult = intradayBalanceValidationResult();

			var durationValidator = new DurationValidator(TimeSpan.FromMinutes(1), duration);
			var durationValidatorResult = durationValidator.Validate();

			intradayValidationResult.CombineResultValue(durationValidatorResult);
			intradayValidationResult.CombineDetails(durationValidatorResult);

			return intradayValidationResult;
		}

		private SikuliValidationResult intradayBalanceValidationResult()
		{
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var lowestIntervalBalances = ValidatorHelper.GetDailyLowestIntraIntervalBalanceForPeriod(_schedulerState,
				_totalSkill.AggregateSkills[1]);
			if (lowestIntervalBalances == null)
			{
				result.Result = SikuliValidationResult.ResultValue.Fail;
				result.Details.AppendLine("Validator failure");
				return result;
			}
			var ruleBreaks = checkInternalBalanceRuleBreaks(lowestIntervalBalances);
			assertRuleBreaks(result, ruleBreaks);
			return result;
		}

		private static void assertRuleBreaks(SikuliValidationResult result, int ruleBreaks)
		{
			if (ruleBreaks > 0)
			{
				result.Result = SikuliValidationResult.ResultValue.Warn;
				result.Details.AppendLine(string.Format("Broken rules: {0}", ruleBreaks));
			}
			if (ruleBreaks > maxRuleBreaks)
				result.Result = SikuliValidationResult.ResultValue.Fail;
			result.Details.AppendLine(string.Format("Lowest intra interval balance: {0}", result.Result));
		}

		private int checkInternalBalanceRuleBreaks(IEnumerable<double?> intervalBalances)
		{
			int numberOfRuleBreaks = intervalBalances.Count(intervalBalance => intervalBalance < limit);
			return numberOfRuleBreaks;
		}
	}
}
