using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;
using Teleopti.Ccc.Win.Sikuli.Validators.SelftestValidators;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli
{
	internal static class SikuliValidatorFactory
	{
		internal static class Scheduler
		{
			public static IRootValidator CreateValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
			{
				switch (SikuliHelper.CurrentValidator)
				{
					case SikuliValidatorRegister.DeleteAll:
						return new DeleteAllValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.Optimize:
						return new OptimizerValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeDaysOff:
						return new OptimizeAllOptionsOffDaysOffValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBetweenDays:
						return new OptimizeAllOptionsOffTimeBetweenDaysValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeWithinDays:
						return new OptimizeAllOptionsOffShiftsWithinDayValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBlockTeam:
						return new OptimizeBlockTeamValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeIntervalBalanceBefore:
						return new IntervalBalanceBeforeValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeIntervalBalanceAfter:
						return new IntervalBalanceAfterValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.Schedule:
						return new SchedulerValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleAllOptionsOff:
						return new ScheduleAllOptionsOffValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleOvertimePeriod:
						return new ScheduleOvertimePeriodValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.TestPass:
						return new PassValidator();

					case SikuliValidatorRegister.TestWarn:
						return new WarnValidator();

					case SikuliValidatorRegister.TestFail:
						return new FailValidator();

					case SikuliValidatorRegister.TestRoot:
						return new TestRootValidator();

					default:
						return null;
				}
			}
		}
	}
}