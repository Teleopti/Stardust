using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers
{
	internal static class ValidatorHelperMethods
	{
		

		public static IEnumerable<double?> GetDailyScheduledHoursForFullPeriod(ISchedulerStateHolder stateHolder, IAggregateSkill totalSkill, ITimeZoneGuard timeZoneGuard)
		{
			try
			{
				var skillStaffPeriodsOfFullPeriod = getDailySkillStaffPeriodsForFullPeriod(stateHolder, totalSkill, timeZoneGuard);
				var dailyScheduledHours =
					skillStaffPeriodsOfFullPeriod.Select(SkillStaffPeriodHelper.ScheduledHours).ToList();
				return dailyScheduledHours;
			}
			catch
			{
				return null;
			}
		}

		public static double? GetStandardDeviationForPeriod(ISchedulerStateHolder stateHolder, IAggregateSkill totalSkill, ITimeZoneGuard timeZoneGuard)
		{
			try
			{
				var skillStaffPeriodsOfFullPeriod = getDailySkillStaffPeriodsForFullPeriod(stateHolder, totalSkill, timeZoneGuard);
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

		
		private static IEnumerable<IList<ISkillStaffPeriod>> getDailySkillStaffPeriodsForFullPeriod(ISchedulerStateHolder stateHolder, IAggregateSkill totalSkill, ITimeZoneGuard timeZoneGuard)
		{
			var period = stateHolder.RequestedPeriod.DateOnlyPeriod.ToDateTimePeriod(timeZoneGuard.CurrentTimeZone());
			var skillStaffPeriodsTotal = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
					totalSkill, period);

			var dailySkillStaffPeriodsForFullPeriod = new List<IList<ISkillStaffPeriod>>();

			foreach (var day in stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection())
			{
				var dayUtcPeriod = new DateOnlyPeriod(day, day).ToDateTimePeriod(timeZoneGuard.CurrentTimeZone());
				var skillStaffPeriods = skillStaffPeriodsTotal.Where(x => dayUtcPeriod.Contains(x.Period)).ToList();
				dailySkillStaffPeriodsForFullPeriod.Add(skillStaffPeriods);
			}
			return dailySkillStaffPeriodsForFullPeriod;
		}

		
	}
}
