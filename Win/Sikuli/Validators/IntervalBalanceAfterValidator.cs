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
			var result = new SikuliValidationResult(true);
			var lowestIntervalBalances = ValidatorHelper.GetDailyLowestIntraIntervalBalanceForPeriod(_schedulerState, _totalSkill);
			var checkResult = lowestIntervalBalances.All(c => c > 0.8);
			result.Details.AppendLine("Details:");
			if (checkResult)
				result.Details.AppendLine("Lowest intra interval balance: OK");
			else
			{
				result.Details.AppendLine("Lowest intra interval balance: Fail");
				result.Result = false;
			}
			return result;
		}
	}
}
