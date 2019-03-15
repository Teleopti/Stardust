using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators
{
	internal class DailyStandardDeviationValidator : IAtomicValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;
		private readonly double _limit;

		public DailyStandardDeviationValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill, double limit)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
			_limit = limit;
		}

		public string Description
		{
			get { return "The daily's standard deviation must be under the limit."; }
		}

		public SikuliValidationResult Validate(ITimeZoneGuard timeZoneGuard)
		{
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var sumOfStandardDeviations = ValidatorHelperMethods.GetDailySumOfStandardDeviationsFullPeriod(_schedulerState, _totalSkill, timeZoneGuard);

			if (sumOfStandardDeviations > _limit)
				result.Result = SikuliValidationResult.ResultValue.Fail;
			result.AppendResultLine("Daily StdDev sum", _limit.ToString(CultureInfo.CurrentCulture), sumOfStandardDeviations.ToString(CultureInfo.CurrentCulture), result.Result);
			return result;
		}
	}
}
