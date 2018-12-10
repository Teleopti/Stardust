using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class ForecastAndScheduleSumForDay
	{
		private readonly IUserTimeZone _userTimeZone;
		private const double boostingForecastValueIfMinimumAgentsBreaks = 1_000_000;
		
		public ForecastAndScheduleSumForDay(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}
			
		public (double ForecastSum, double ScheduledSum, double BrokenMinimumAgentsIntervals) Execute(IOptimizationPreferences optimizationPreferences, ISchedulingResultStateHolder stateHolder, IEnumerable<ISkill> skills, DateOnly date)
		{
			double dailyForecastSum = 0;
			double dailyScheduledSum = 0;
			double brokenMinimumAgentsIntervals = 0;

			foreach (var skillStaffPeriod in stateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(skills, date.ToDateTimePeriod(_userTimeZone.TimeZone())))
			{
				if (optimizationPreferences.Advanced.UseMinimumStaffing)
				{
					var minimumPersons = skillStaffPeriod.Payload.SkillPersonData.MinimumPersons;
					var lackingPersons = minimumPersons - skillStaffPeriod.CalculatedLoggedOn;
					if (lackingPersons > 0)
					{
						brokenMinimumAgentsIntervals += lackingPersons;						
					}
				}
	
				dailyForecastSum += skillStaffPeriod.FStaffTime().TotalMinutes;
				dailyScheduledSum += TimeSpan.FromMinutes(skillStaffPeriod.CalculatedResource * skillStaffPeriod.Period.ElapsedTime().TotalMinutes).TotalMinutes;
			}

			return brokenMinimumAgentsIntervals > 0 ? 
				(boostingForecastValueIfMinimumAgentsBreaks + brokenMinimumAgentsIntervals, 1, brokenMinimumAgentsIntervals) : 
				(dailyForecastSum, dailyScheduledSum, brokenMinimumAgentsIntervals);
		}
	}
}