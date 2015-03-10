using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class IntervalBalanceBeforeValidator : IRootValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public IntervalBalanceBeforeValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public string Description 
		{ 
			get { return "At least one 'lowest intra interval balance' value must be between 0 and 0.8 to have a good start point for optimization."; } 
		}

		public SikuliValidationResult Validate(ITestDuration duration)
		{

			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var lowestIntervalBalances = ValidatorHelper.GetDailyLowestIntraIntervalBalanceForPeriod(_schedulerState, _totalSkill.AggregateSkills[1]);
			if (lowestIntervalBalances == null)
			{
				result.Result = SikuliValidationResult.ResultValue.Fail;
				result.Details.AppendLine("Validator failure");
				return result;
			}
			var checkResult = lowestIntervalBalances.Any(c => c > 0 && c < 0.8);
			if (!checkResult)
				result.Result = SikuliValidationResult.ResultValue.Fail;
			result.Details.AppendLine(string.Format("Lowest intra interval balance: {0}", result.Result));
			return result;
		}
	}
}
