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
			get { return "Duration limit is: " + _durationLimit.ToString(@"mm\:ss"); }
		}

		public SikuliValidationResult Validate()
		{
			var result = new SikuliValidationResult();

			if (_testDurationuration.GetDuration() > _durationLimit)
			{
				result.Result = SikuliValidationResult.ResultValue.Warn;
				result.Details.AppendLine("Duration is over limit!");
			}

			result.Details.AppendLine(string.Format("Duration is {0}: {1}", _testDurationuration.GetDurationString(), result.Result));
			
			return result;
		}
	}
}
