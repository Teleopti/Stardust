using System;
using System.Collections.Generic;
using Microsoft.FSharp.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class ForecastAndScheduleSumForDay
	{
		private readonly IUserTimeZone _userTimeZone;
		public const double MinimumAgentsNotFulfilledValue = 100_000;
		
		public ForecastAndScheduleSumForDay(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}
			
		public (double ForecastSum, double ScheduledSum) Execute(IOptimizationPreferences optimizationPreferences, ISchedulingResultStateHolder stateHolder, IEnumerable<ISkill> skills, DateOnly date)
		{
			double dailyForecastSum = 0;
			double dailyScheduledSum = 0;

			foreach (var skillStaffPeriod in stateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(skills, date.ToDateTimePeriod(_userTimeZone.TimeZone())))
			{
				if (optimizationPreferences.Advanced.UseMinimumStaffing)
				{
					var minimumPersons = skillStaffPeriod.Payload.SkillPersonData.MinimumPersons;
					var lackingPersons = minimumPersons - skillStaffPeriod.CalculatedLoggedOn;
					if (lackingPersons > 0)
					{
						return (MinimumAgentsNotFulfilledValue + lackingPersons, 1);						
					}
				}
	
				dailyForecastSum += skillStaffPeriod.FStaffTime().TotalMinutes;
				dailyScheduledSum += TimeSpan.FromMinutes(skillStaffPeriod.CalculatedResource * skillStaffPeriod.Period.ElapsedTime().TotalMinutes).TotalMinutes;
			}

			return (dailyForecastSum, dailyScheduledSum);
		}
	}
}