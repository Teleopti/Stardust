using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators
{
	internal class DurationValidator : IAtomicValidator
	{
		private readonly TimeSpan _durationLimit;
		private readonly ITestDuration _testDurationuration;

		public DurationValidator(TimeSpan durationLimit, ITestDuration testDurationuration)
		{
			_durationLimit = durationLimit;
			_testDurationuration = testDurationuration;
		}

		public string Description
		{
			get { return "Duration must be under limit."; }
		}

		public SikuliValidationResult Validate()
		{
			var result = new SikuliValidationResult();

			if (_testDurationuration.GetDuration() > _durationLimit)
				result.Result = SikuliValidationResult.ResultValue.Warn;

			result.AppendResultLine("Duration", _durationLimit.ToString(@"mm\:ss"),  _testDurationuration.GetDurationString(), result.Result);
			
			return result;
		}
	}
}
