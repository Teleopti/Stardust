using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators
{
	internal class PeriodStandardDeviationValidator : IAtomicValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;
		private double _limit;

		public PeriodStandardDeviationValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill, double limit)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
			_limit = limit;
		}

		public string Description
		{
			get { return "The period's standard deviation must be under the limit."; }
		}

		public SikuliValidationResult Validate()
		{
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var std = ValidatorHelper.GetStandardDeviationForPeriod(_schedulerState, _totalSkill);
			result.AppendLimitValueLineToDetails("Period StdDev", _limit.ToString(CultureInfo.CurrentCulture), std.Value.ToString(CultureInfo.CurrentCulture));
			if (std.Value > _limit)
				result.Result = SikuliValidationResult.ResultValue.Fail;
			return result;
		}
	}
}
