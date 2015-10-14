using System;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.Win.Sikuli
{
	internal static class SikuliValidatorFactory
	{
		internal static class Scheduler
		{

			private static object createInstanceByReflection(string className)
			{
				var assembly = Assembly.GetExecutingAssembly();

				var type = assembly.GetTypes()
					.First(t => t.Name == className);

				return Activator.CreateInstance(type);
			}

			public static IRootValidator CreateValidator(string validatorName)
			{
				var validatorClassName = validatorName + "Validator";

				return createInstanceByReflection(validatorClassName) as IRootValidator;

				//switch (validatorName)
				//{
				//	case SikuliValidatorRegister.DeleteAll:
				//		return new DeleteAllValidator();

				//	case SikuliValidatorRegister.OptimizeDaysOff:
				//		return new OptimizeDaysOffValidator();

				//	case SikuliValidatorRegister.OptimizeBetweenDays:
				//		return new OptimizeBetweenDaysValidator();

				//	case SikuliValidatorRegister.OptimizeWithinDays:
				//		return new OptimizeWithinDaysValidator();

				//	case SikuliValidatorRegister.OptimizeIntervalBalanceBefore:
				//		return new OptimizeIntervalBalanceBeforeValidator();

				//	case SikuliValidatorRegister.OptimizeIntervalBalanceAfter:
				//		return new OptimizeIntervalBalanceAfterValidator();

				//	case SikuliValidatorRegister.ScheduleAllOff:
				//		return new ScheduleAllOptionsOffValidator();

				//	case SikuliValidatorRegister.ScheduleOvertimePeriod:
				//		return new ScheduleOvertimePeriodValidator();

				//	case SikuliValidatorRegister.ScheduleBlockSameShiftCategory :
				//		return new ScheduleBlockSameShiftCategoryValidator();

				//	case SikuliValidatorRegister.OptimizeBlockSameShiftCategory:
				//		return new OptimizeBlockSameShiftCategoryValidator();

				//	case SikuliValidatorRegister.ScheduleBlockSameStartTime:
				//		return new ScheduleBlockSameStartTimeValidator();

				//	case SikuliValidatorRegister.OptimizeBlockSameStartTime:
				//		return new OptimizeBlockSameStartTimeValidator();

				//	case SikuliValidatorRegister.ScheduleBlockSameShift:
				//		return new ScheduleBlockSameShiftValidator();

				//	case SikuliValidatorRegister.OptimizeBlockSameShift:
				//		return new OptimizeBlockSameShiftValidator();

				//	case SikuliValidatorRegister.ScheduleTeamSameShiftCategory:
				//		return new ScheduleTeamSameShiftCategoryValidator();

				//	case SikuliValidatorRegister.OptimizeTeamSameShiftCategory:
				//		return new OptimizeTeamSameShiftCategoryValidator();

				//	case SikuliValidatorRegister.ScheduleTeamSameStartTime:
				//		return new ScheduleTeamSameStartTimeValidator();

				//	case SikuliValidatorRegister.OptimizeTeamSameStartTime:
				//		return new OptimizeTeamSameStartTimeValidator();

				//	case SikuliValidatorRegister.ScheduleTeamSameEndTime:
				//		return new ScheduleTeamSameEndTimeValidator();

				//	case SikuliValidatorRegister.OptimizeTeamSameEndTime:
				//		return new OptimizeTeamSameEndTimeValidator();

				//	case SikuliValidatorRegister.ScheduleBlockTeamSameShiftCategory:
				//		return new ScheduleBlockTeamSameShiftCategoryValidator();

				//	case SikuliValidatorRegister.OptimizeBlockTeamSameShiftCategoryDaysOff:
				//		return new OptimizeBlockTeamSameShiftCategoryDaysOffValidator();

				//	case SikuliValidatorRegister.OptimizeBlockTeamSameShiftCategoryBetweenDays:
				//		return new OptimizeBlockTeamSameShiftCategoryBetweenDaysValidator();

				//	case SikuliValidatorRegister.OptimizeBlockTeamSameShiftCategoryWithinDays:
				//		return new OptimizeBlockTeamSameShiftCategoryWithinDaysValidator();

				//	case SikuliValidatorRegister.ScheduleBlockTeamSameStartTime:
				//		return new ScheduleBlockTeamSameStartTimeValidator();

				//	case SikuliValidatorRegister.OptimizeBlockTeamSameStartTime:
				//		return new OptimizeBlockTeamSameStartTimeValidator();

				//	case SikuliValidatorRegister.MemoryUsage:
				//		return new MemoryUsageValidator();

				//	case SikuliValidatorRegister.TestPass:
				//		return new SelftestPassValidator();

				//	case SikuliValidatorRegister.TestWarn:
				//		return new SelftestWarnValidator();

				//	case SikuliValidatorRegister.TestFail:
				//		return new SelftestFailValidator();

				//	case SikuliValidatorRegister.TestRoot:
				//		return new SelftestRootValidator();

				//	default:
				//		return null;
			
			}
		}
	}
}