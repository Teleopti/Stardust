using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	public class OptimizerValidator : ISikuliValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public OptimizerValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public SikuliValidationResult Validate()
		{
			SikuliValidationResult result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var std = ValidatorHelper.GetStandardDeviationForPeriod(_schedulerState, _totalSkill);
			result.AppendLimitValueLineToDetails("Period StdDev", "0,2", std.ToString());
			if (!std.HasValue || std.Value > 0.2)
				result.Result = SikuliValidationResult.ResultValue.Fail;
			result.Details.AppendLine("Explanation: The period standard deviation must be under the limit.");
			return result;
		}
	}
}
