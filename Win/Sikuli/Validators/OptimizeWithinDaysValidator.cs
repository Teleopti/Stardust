using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	public class OptimizeWithinDaysValidator : ISikuliValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public OptimizeWithinDaysValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public SikuliValidationResult Validate()
		{
			SikuliValidationResult result = new SikuliValidationResult(true);
			var std = ValidatorHelper.GetDailySumOfStandardDeviationsFullPeriod(_schedulerState, _totalSkill);
			result.Details.AppendLine("Details:");
			result.AppendLimitValueLine("Period daily StdDev sum", "4.6", std.ToString());
			if (std > 4.6)
			{
				result.Result = false;
			}
			return result;
		}
	}
}
