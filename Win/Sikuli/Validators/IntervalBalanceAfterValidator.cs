using System.Collections.Generic;
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
			var resultValue = checkInternalBalanceRuleBreaks(lowestIntervalBalances, 1);
			result.Result = resultValue;
			result.Details.AppendLine(string.Format("Lowest intra interval balance: {0}", resultValue));
			return result;
		}

		private SikuliValidationResult.ResultValue checkInternalBalanceRuleBreaks(IEnumerable<double?> intervalBalances, int numberOfAllowedRuleBreaks)
		{
			int numberOfRuleBreaks = 0;
			const double limit = 0.8; 

			foreach (var intervalBalance in intervalBalances)
			{
				if (intervalBalance < limit)
					numberOfRuleBreaks++;
				if(numberOfRuleBreaks > numberOfAllowedRuleBreaks)
					return SikuliValidationResult.ResultValue.Fail;
			}
			if (numberOfRuleBreaks > 0)
				return SikuliValidationResult.ResultValue.Warn;
			return SikuliValidationResult.ResultValue.Pass;
		}
	}
}
