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
			}
			var checkResult = lowestIntervalBalances != null && checkInternalBalanceRuleBreaks(lowestIntervalBalances, 1);
			result.Details.AppendLine("Details:");
			if (checkResult)
				result.Details.AppendLine("Lowest intra interval balance: OK");
			else
			{
				result.Details.AppendLine("Lowest intra interval balance: Fail");
				result.Result = SikuliValidationResult.ResultValue.Fail;
			}
			return result;
		}

		private bool checkInternalBalanceRuleBreaks(IEnumerable<double?> intervalBalances, int numberOfAllowedRuleBreaks)
		{
			int numberOfRuleBreaks = 0;
			const double limit = 0.8; 

			foreach (var intervalBalance in intervalBalances)
			{
				if (intervalBalance < limit)
					numberOfRuleBreaks++;
				if(numberOfRuleBreaks > numberOfAllowedRuleBreaks)
					return false;
			}
			return true;
		}
	}
}
