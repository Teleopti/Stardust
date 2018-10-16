using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{ 
	public class ForecastedCallsProvider : IForecastedCallsProvider
	{
		private readonly IUserTimeZone _timeZone;
		private readonly ITaskPeriodsProvider _taskPeriodsProvider;

		public ForecastedCallsProvider(IUserTimeZone timeZone, ITaskPeriodsProvider taskPeriodsProvider)
		{
			_timeZone = timeZone;
			_taskPeriodsProvider = taskPeriodsProvider;
		}
		
		public ForecastedCallsModel Load(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateTime? latestStatisticsTime, int minutesPerInterval, DateTime? currentDateTime = null)
		{
			var callsPerSkill= new Dictionary<Guid, List<SkillIntervalStatistics>>();
			var skillStatsRange = new List<SkillDayStatsRange>();

			foreach (var skill in skillDays.Keys)
			{
				var mergedTaskPeriodList = new List<ISkillStaffPeriodView>();
				var skillDaysForSkill = skillDays[skill].ToList();
				if (!skillDaysForSkill.Any())
					continue;
								
				foreach (var skillDay in skillDaysForSkill)
				{
				    var templateTaskPeriods = _taskPeriodsProvider.Load(skillDay, minutesPerInterval, latestStatisticsTime, currentDateTime).ToList();
				    mergedTaskPeriodList.AddRange(templateTaskPeriods);

					skillStatsRange.Add(getSkillStatsRange(skill, skillDay));
				}

				callsPerSkill.Add(skill.Id.Value, getForecastedCalls(mergedTaskPeriodList, skill));
			}
			
			return new ForecastedCallsModel
			{
				CallsPerSkill = callsPerSkill,
				SkillDayStatsRange = skillStatsRange
			};
		}

		private List<SkillIntervalStatistics> getForecastedCalls(IEnumerable<ISkillStaffPeriodView> mergedTaskPeriodList, ISkill skill)
		{
			var timeZone = _timeZone.TimeZone();
			var mergedTaskPeriodPerSkill = mergedTaskPeriodList
				.Select(x => new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					StartTime = x.Period.StartDateTimeLocal(timeZone),
					Calls = x.ForecastedTasks,
					AverageHandleTime = x.AverageHandlingTaskTime.TotalSeconds
				})
				.ToList();

			var forecastedCalls =  mergedTaskPeriodPerSkill
				.GroupBy(h => h.StartTime)
				.Select(s => new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					StartTime = s.First().StartTime,
					Calls = s.Sum(c => c.Calls),
					AverageHandleTime = s.Sum(a => a.AverageHandleTime)
				})
				.OrderBy(g => g.StartTime)
				.ToList();

			return forecastedCalls;
		}

		private static SkillDayStatsRange getSkillStatsRange(ISkill skill, ISkillDay skillDay)
		{
			var timePeriods = skillDay.OpenHours();
			if (timePeriods.IsEmpty()) return new SkillDayStatsRange();

			var start = timePeriods.Min(t => t.StartTime);
			var end = timePeriods.Max(t => t.EndTime);

			return new SkillDayStatsRange
			{
				SkillId = skill.Id.Value,
				SkillDayDate = skillDay.CurrentDate,
				RangePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(skillDay.CurrentDate.Date.Add(start),
					skillDay.CurrentDate.Date.Add(end), skill.TimeZone)
			};
		}

	}
}
