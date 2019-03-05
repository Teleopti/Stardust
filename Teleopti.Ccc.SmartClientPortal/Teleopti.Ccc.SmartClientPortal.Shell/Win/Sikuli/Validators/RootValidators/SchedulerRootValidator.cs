using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal abstract class SchedulerRootValidator : RootValidator
	{
		private readonly ITestDuration _timer;

		protected SchedulerRootValidator()
		{
			_timer = new TestDuration();
			_timer.SetStart();
		}

		protected ITestDuration EndTimer()
		{
			_timer.SetEnd();
			return _timer; 
		}

		public override SikuliValidationResult Validate(object data, ITimeZoneGuard timeZoneGuard)
		{
			var scheduleTestData = data as SchedulerTestData;
			if (scheduleTestData == null)
			{
				var testDataFail = new SikuliValidationResult(SikuliValidationResult.ResultValue.Fail);
				testDataFail.Details.AppendLine("Sikuli scheduler testdata failure.");
				return testDataFail;
			}
			return Validate(scheduleTestData, timeZoneGuard);
		}

		protected abstract SikuliValidationResult Validate(SchedulerTestData data, ITimeZoneGuard timeZoneGuard);
	}
}
