using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal class DeleteAllValidator : RootValidator
	{
		public override string Description
		{
			get { return "All scheduled hours must be 0."; }
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
			var schedulerState = scheduleTestData.SchedulerState;
			var totalSkill = scheduleTestData.TotalSkill;
			var scheduledHours = ValidatorHelperMethods.GetDailyScheduledHoursForFullPeriod(schedulerState, totalSkill, timeZoneGuard);
			if (scheduledHours.Any(d => d.HasValue && d.Value > 0))
			{
				result.Details.AppendLine("Scheduled hours = 0 : Fail");
				result.Result = SikuliValidationResult.ResultValue.Fail;
				return result;
			}
			result.Details.AppendLine("Scheduled hours = 0 : Pass");
			return result;
		}
	}
}
