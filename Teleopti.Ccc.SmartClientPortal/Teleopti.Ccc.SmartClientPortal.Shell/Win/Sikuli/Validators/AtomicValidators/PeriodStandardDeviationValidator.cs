using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators
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

		public SikuliValidationResult Validate(ITimeZoneGuard timeZoneGuard)
		{
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var std = ValidatorHelperMethods.GetStandardDeviationForPeriod(_schedulerState, _totalSkill, timeZoneGuard);
			if (std.Value > _limit)
				result.Result = SikuliValidationResult.ResultValue.Fail;
			result.AppendResultLine("Period StdDev", _limit.ToString(CultureInfo.CurrentCulture), std.Value.ToString(CultureInfo.CurrentCulture), result.Result);

			return result;
		}
	}
}
