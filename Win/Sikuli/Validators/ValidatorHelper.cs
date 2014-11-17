using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.XPS;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	public static class ValidatorHelper
	{

		public static IEnumerable<double?> GetDailyLowestIntraIntervalBalanceForPeriod(ISchedulerStateHolder stateHolder, IAggregateSkill totalSkill)
		{
			try
			{
				var result = new List<double?>();
				var skillStaffPeriodsOfFullPeriod = getDailySkillStaffPeriodsForFullPeriod(stateHolder, totalSkill);
				foreach (var dailySkillStaffPeriodList in skillStaffPeriodsOfFullPeriod)
				{
					result.Add(dailySkillStaffPeriodList.Min(c => c.IntraIntervalValue));
				}
				return result;
			}
			catch
			{
				return null;
			}
		}

		public static IEnumerable<double?> GetDailyScheduledHoursForFullPeriod(ISchedulerStateHolder stateHolder, IAggregateSkill totalSkill)
		{
			try
			{
				var skillStaffPeriodsOfFullPeriod = getDailySkillStaffPeriodsForFullPeriod(stateHolder, totalSkill);
				var dailyScheduledHours =
					skillStaffPeriodsOfFullPeriod.Select(SkillStaffPeriodHelper.ScheduledHours).ToList();
				return dailyScheduledHours;
			}
			catch
			{
				return null;
			}
		}

		public static double? GetStandardDeviationForPeriod(ISchedulerStateHolder stateHolder, IAggregateSkill totalSkill)
		{
			try
			{
				var skillStaffPeriodsOfFullPeriod = getDailySkillStaffPeriodsForFullPeriod(stateHolder, totalSkill);
				double? result = SkillStaffPeriodHelper.SkillPeriodGridSmoothness(skillStaffPeriodsOfFullPeriod);
				if (result.HasValue)
					return Math.Round(result.Value, 3);
				return null;
			}
			catch
			{
				return null;
			}
		}

		public static double GetDailySumOfStandardDeviationsFullPeriod(ISchedulerStateHolder stateHolder, IAggregateSkill totalSkill)
		{
			double result = 0d;
				var skillStaffPeriodsOfFullPeriod = getDailySkillStaffPeriodsForFullPeriod(stateHolder, totalSkill);
				foreach (var dailySkillStaffPeriodList in skillStaffPeriodsOfFullPeriod)
				{
					var dailyValue = SkillStaffPeriodHelper.SkillDayGridSmoothness(dailySkillStaffPeriodList);
					if (dailyValue.HasValue)
						result += dailyValue.Value;
				}
			return Math.Round(result, 3);
		}

		private static IEnumerable<IList<ISkillStaffPeriod>> getDailySkillStaffPeriodsForFullPeriod(ISchedulerStateHolder stateHolder, IAggregateSkill totalSkill)
		{
			var period = stateHolder.RequestedPeriod.DateOnlyPeriod.ToDateTimePeriod(stateHolder.TimeZoneInfo);
			var skillStaffPeriodsTotal = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
					totalSkill, period);

			var dailySkillStaffPeriodsForFullPeriod = new List<IList<ISkillStaffPeriod>>();

			foreach (var day in stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection())
			{
				var dayUtcPeriod = new DateOnlyPeriod(day, day).ToDateTimePeriod(stateHolder.TimeZoneInfo);
				var skillStaffPeriods = skillStaffPeriodsTotal.Where(x => dayUtcPeriod.Contains(x.Period)).ToList();
				dailySkillStaffPeriodsForFullPeriod.Add(skillStaffPeriods);
			}
			return dailySkillStaffPeriodsForFullPeriod;
		}
	}
}
