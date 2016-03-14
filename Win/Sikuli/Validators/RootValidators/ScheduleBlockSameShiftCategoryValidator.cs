using System;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal class ScheduleBlockSameShiftCategoryValidator : SchedulerRootValidator
	{

		public ScheduleBlockSameShiftCategoryValidator()
		{
			AtomicValidators.Add(new DurationValidator(TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(10))));
		}

		protected override SikuliValidationResult Validate(SchedulerTestData data)
		{
			AtomicValidators.Add(new SchedulerHoursWeeklyPatternValidator(data.SchedulerState, data.TotalSkill));
			return ValidateAtomicValidators(AtomicValidators);
		}
	}
}
