using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	internal class OptimizeBlockTeamValidator : IRootValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public OptimizeBlockTeamValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public string Description
		{
			get { return "The period's standard deviation must be under the limit."; }
		}

		public SikuliValidationResult Validate(ITestDuration duration)
		{
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var std = ValidatorHelper.GetStandardDeviationForPeriod(_schedulerState, _totalSkill);
			const double limit = 0.08d;
			result.AppendLimitValueLineToDetails("Period StdDev", limit.ToString(CultureInfo.CurrentCulture), std.Value.ToString(CultureInfo.CurrentCulture));
			if (std.Value > limit)
				result.Result = SikuliValidationResult.ResultValue.Fail;
			return result;
		}
	}
}
