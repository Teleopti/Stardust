using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Validators;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;
using Teleopti.Ccc.Win.Sikuli.Validators.SelftestValidators;
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

					case SikuliValidatorRegister.OptimizeDaysOff:
						return new OptimizeAllOptionsOffDaysOffValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBetweenDays:
						return new OptimizeAllOptionsOffTimeBetweenDaysValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeWithinDays:
						return new OptimizeAllOptionsOffShiftsWithinDayValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeIntervalBalanceBefore:
						return new OptimizeAllOptionsOffIntradayBalanceBeforeValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeIntervalBalanceAfter:
						return new OptimizeAllOptionsOffIntradayBalanceAfterValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleAllOff:
						return new ScheduleAllOptionsOffValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleOvertimePeriod:
						return new ScheduleOvertimePeriodValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleBlockSameShiftCategory :
						return new ScheduleBlockSameShiftCategoryValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBlockSameShiftCategory:
						return new OptimizeBlockSameShiftCategoryValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleBlockSameStartTime:
						return new ScheduleBlockSameStartTimeValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBlockSameStartTime:
						return new OptimizeBlockSameStartTimeValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleBlockSameShift:
						return new ScheduleBlockSameShiftValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBlockSameShift:
						return new OptimizeBlockSameShiftValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleTeamSameShiftCategory:
						return new ScheduleTeamSameShiftCategoryValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeTeamSameShiftCategory:
						return new OptimizeTeamSameShiftCategoryValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleTeamSameStartTime:
						return new ScheduleTeamSameStartTimeValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeTeamSameStartTime:
						return new OptimizeTeamSameStartTimeValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleTeamSameEndTime:
						return new ScheduleTeamSameEndTimeValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeTeamSameEndTime:
						return new OptimizeTeamSameEndTimeValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleBlockTeamSameShiftCategory:
						return new ScheduleBlockTeamSameShiftCategoryValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBlockTeamSameShiftCategoryDaysOff:
						return new OptimizeBlockTeamSameShiftCategoryDaysOffValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBlockTeamSameShiftCategoryBetweenDays:
						return new OptimizeBlockTeamSameShiftCategoryBetweenDaysValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBlockTeamSameShiftCategoryWithinDays:
						return new OptimizeBlockTeamSameShiftCategoryWithinDaysValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.ScheduleBlockTeamSameStartTime:
						return new ScheduleBlockTeamSameStartTimeValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBlockTeamSameStartTime:
						return new OptimizeBlockTeamSameStartTimeValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.TestPass:
						return new SelftestPassValidator();

					case SikuliValidatorRegister.TestWarn:
						return new SelftestWarnValidator();

					case SikuliValidatorRegister.TestFail:
						return new SelftestFailValidator();

					case SikuliValidatorRegister.TestRoot:
						return new SelftestRootValidator();

					default:
						return null;
				}
			}
		}
	}
}