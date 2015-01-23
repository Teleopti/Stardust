using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	public class OptimizeBlockTeamValidator : ISikuliValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public OptimizeBlockTeamValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public SikuliValidationResult Validate()
		{
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var std = ValidatorHelper.GetStandardDeviationForPeriod(_schedulerState, _totalSkill);
			result.AppendLimitValueLineToDetails("Period StdDev", "0,08", std.ToString());
			if (!std.HasValue || std.Value > 0.08)
				result.Result = SikuliValidationResult.ResultValue.Fail;
			result.Details.AppendLine("Explanation: The period standard deviation must be under the limit.");
			return result;
		}
	}
}
