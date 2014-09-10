using Microsoft.ReportingServices.Interfaces;
using Teleopti.Ccc.Win.Sikuli.Validators;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli
{
	public static class SikuliValidators
	{
		public static class Register
		{
			/// <summary>
			/// Identifier for validators
			/// </summary>
			public const string None = "None";
			public const string DeleteAll = "DeleteAll";
			public const string Schedule = "Schedule";
			public const string Optimize = "Optimize";
			public const string OptimizeDaysOff = "OptimizeDaysOff";
			public const string OptimizeBetweenDays = "OptimizeBetweenDays";
			public const string OptimizeWithinDays = "OptimizeWithinDays";

		}

		public static class Factory
		{
			public static class Scheduler
			{
				public static ISikuliValidator CreateValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
				{
					switch (SikuliHelper.CurrentValidator)
					{
						case Register.Schedule:
							return new SchedulerValidator(schedulerState, totalSkill);

						case Register.Optimize:
							return new OptimizerValidator(schedulerState, totalSkill);

						case Register.OptimizeDaysOff:
							return new OptimizeDaysOffValidator(schedulerState, totalSkill);

						case Register.OptimizeBetweenDays:
							return new OptimizeBetweenDaysValidator(schedulerState, totalSkill);

						case Register.OptimizeWithinDays:
							return new OptimizeWithinDaysValidator(schedulerState, totalSkill);

						case Register.DeleteAll:
							return new DeleteAllValidator(schedulerState, totalSkill);

						default:
							return null;
					}
				}
			}
		}
	}
}
