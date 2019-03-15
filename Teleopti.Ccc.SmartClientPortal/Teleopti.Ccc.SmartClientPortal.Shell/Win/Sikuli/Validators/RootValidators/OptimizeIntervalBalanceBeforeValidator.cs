using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal class OptimizeIntervalBalanceBeforeValidator : RootValidator
	{

		public override string Description 
		{ 
			get { return "At least one 'lowest intra interval balance' value must be between 0 and 0.8 to have a good start point for optimization."; } 
		}

		public override SikuliValidationResult Validate(object data, ITimeZoneGuard timeZoneGuard)
		{
			var scheduleTestData = data as SchedulerTestData;
			if (scheduleTestData == null)
			{
				var testDataFail = new SikuliValidationResult(SikuliValidationResult.ResultValue.Fail);
				testDataFail.Details.AppendLine("Sikuli testdata failure.");
				return testDataFail;
			}

			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var lowestIntervalBalances = ValidatorHelperMethods.GetDailyLowestIntraIntervalBalanceForPeriod(scheduleTestData.SchedulerState, scheduleTestData.TotalSkill.AggregateSkills[1], timeZoneGuard);
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
