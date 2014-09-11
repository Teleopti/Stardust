using Teleopti.Ccc.Win.Sikuli.Validators;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli
{
	public static class SikuliValidatorFactory
	{
		public static class Scheduler
		{
			public static ISikuliValidator CreateValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
			{
				switch (SikuliHelper.CurrentValidator)
				{
					case SikuliValidatorRegister.Schedule:
						return new SchedulerValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.Optimize:
						return new OptimizerValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeDaysOff:
						return new OptimizeDaysOffValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBetweenDays:
						return new OptimizeBetweenDaysValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeWithinDays:
						return new OptimizeWithinDaysValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.OptimizeBlockTeam:
						return new OptimizeBlockTeamValidator(schedulerState, totalSkill);

					case SikuliValidatorRegister.DeleteAll:
						return new DeleteAllValidator(schedulerState, totalSkill);

					default:
						return null;
				}
			}
		}
	}
}