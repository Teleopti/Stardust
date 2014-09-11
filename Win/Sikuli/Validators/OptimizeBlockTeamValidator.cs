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
			SikuliValidationResult result = new SikuliValidationResult(true);
			var std = ValidatorHelper.GetStandardDeviationForPeriod(_schedulerState, _totalSkill);
			result.Details.AppendLine("Details:");
			result.AppendLimitValueLine("Period StdDev", "0,08", std.ToString());
			if (!std.HasValue || std.Value > 0.08)
			{
				result.Result = false;
			}
			return result;
		}
	}
}
