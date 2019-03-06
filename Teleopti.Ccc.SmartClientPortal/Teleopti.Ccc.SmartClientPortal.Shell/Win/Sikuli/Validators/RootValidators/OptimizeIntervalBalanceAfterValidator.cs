using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeIntervalBalanceAfterValidator : RootValidator
	{
		private const double _limit = 0.8d;
		private const int _maxRuleBreaks = 10;
		private TimeSpan _durationLimit = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(07));
		private DurationValidator _durationValidator;

		public OptimizeIntervalBalanceAfterValidator()
		{
			_durationValidator = new DurationValidator(_durationLimit);
		}

		public override string Description
		{
			get
			{
				return string.Format(
					"Only {0} 'lowest intra interval balance' value can be under {1}. Duration must be under {2}.",
					_maxRuleBreaks, _limit, _durationLimit.ToString(@"mm\:ss"));
			}
		}

		public override SikuliValidationResult Validate(object data, ITimeZoneGuard timeZoneGuard)
		{
			var scheduleTestData = data as SchedulerTestData;
			if (scheduleTestData == null)
			{
				var testDataFail = new SikuliValidationResult(SikuliValidationResult.ResultValue.Fail);
				testDataFail.Details.AppendLine("Sikuli testdata failure.");
				return testDataFail;
			}

			var intradayValidationResult = intradayBalanceValidationResult(scheduleTestData, timeZoneGuard);

			var durationValidatorResult = _durationValidator.Validate(timeZoneGuard);

			intradayValidationResult.CombineResultValue(durationValidatorResult);
			intradayValidationResult.CombineDetails(durationValidatorResult);

			return intradayValidationResult;
		}

		private SikuliValidationResult intradayBalanceValidationResult(SchedulerTestData schedulerData, ITimeZoneGuard timeZoneGuard)
		{
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var lowestIntervalBalances = ValidatorHelperMethods.GetDailyLowestIntraIntervalBalanceForPeriod(schedulerData.SchedulerState,
				schedulerData.TotalSkill.AggregateSkills[1], timeZoneGuard);
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
			if (ruleBreaks > _maxRuleBreaks)
				result.Result = SikuliValidationResult.ResultValue.Fail;
			result.Details.AppendLine(string.Format("Lowest intra interval balance: {0}", result.Result));
		}

		private int checkInternalBalanceRuleBreaks(IEnumerable<double?> intervalBalances)
		{
			int numberOfRuleBreaks = intervalBalances.Count(intervalBalance => intervalBalance < _limit);
			return numberOfRuleBreaks;
		}
	}
}
