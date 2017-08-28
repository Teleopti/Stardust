using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class TaskPeriodsProvider
	{
		private readonly IUserTimeZone _timeZone;
		private readonly INow _now;

		public TaskPeriodsProvider(IUserTimeZone timeZone, INow now)
		{
			_timeZone = timeZone;
			_now = now;
		}

		public IEnumerable<ITemplateTaskPeriod> Load(ISkillDay skillDay,
			int minutesPerInterval,
			DateTime? latestStatisticsTime)
		{
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersNowStartOfDayUtc = TimeZoneHelper.ConvertToUtc(usersNow.Date, _timeZone.TimeZone());
			var latestStatisticsTimeUtc = getLatestStatisticsTimeUtc(latestStatisticsTime);

			var taskPeriods = new List<ITemplateTaskPeriod>();

			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				taskPeriods.AddRange(taskPeriodsUpUntilNow(workloadDay.OpenTaskPeriodList, minutesPerInterval, skillDay.Skill.DefaultResolution, latestStatisticsTimeUtc, usersNowStartOfDayUtc));
			}

			return taskPeriods;
		}

		public IEnumerable<ITemplateTaskPeriod> Load(ISkillDay skillDay,
			int minutesPerInterval,
			DateTime? latestStatisticsTime,
			DateTime? nullableCurrentDateTime)
		{
			if (nullableCurrentDateTime == null) return Load(skillDay, minutesPerInterval, latestStatisticsTime);

			var userTimeZone = _timeZone.TimeZone();
			var usersNow = TimeZoneHelper.ConvertFromUtc(DateTime.SpecifyKind(nullableCurrentDateTime.Value,DateTimeKind.Utc), userTimeZone);
			var usersNowStartOfDayUtc = TimeZoneHelper.ConvertToUtc(usersNow.Date, userTimeZone);
			var latestStatisticsTimeUtc = getLatestStatisticsTimeUtc(latestStatisticsTime);

			var taskPeriods = new List<ITemplateTaskPeriod>();

			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				taskPeriods.AddRange(taskPeriodsUpUntilNow(workloadDay.OpenTaskPeriodList, minutesPerInterval, skillDay.Skill.DefaultResolution, latestStatisticsTimeUtc, usersNowStartOfDayUtc));
			}

			return taskPeriods;
		}

		private IEnumerable<ITemplateTaskPeriod> taskPeriodsUpUntilNow(
			IList<ITemplateTaskPeriod> templateTaskPeriodCollection,
			int targetMinutesPerInterval,
			int skillMinutesPerInterval,
			DateTime? latestStatisticsTimeUtc,
			DateTime? usersNowStartOfDayUtc)
		{
			var periodLength = TimeSpan.FromMinutes(targetMinutesPerInterval);

			if (!latestStatisticsTimeUtc.HasValue)
				return Enumerable.Empty<ITemplateTaskPeriod>();

			if (!templateTaskPeriodCollection.Any())
				return Enumerable.Empty<ITemplateTaskPeriod>();

			if (targetMinutesPerInterval > skillMinutesPerInterval)
				return Enumerable.Empty<ITemplateTaskPeriod>();

			if (targetMinutesPerInterval < skillMinutesPerInterval || isTaskPeriodsMerged(templateTaskPeriodCollection, skillMinutesPerInterval))
				templateTaskPeriodCollection = splitTaskPeriods(templateTaskPeriodCollection, periodLength);


			return templateTaskPeriodCollection
			.Where(t =>
					t.Period.StartDateTime >= usersNowStartOfDayUtc.Value &&
					t.Period.EndDateTime <= latestStatisticsTimeUtc.Value.AddMinutes(targetMinutesPerInterval)
			)
			.ToList();
		}

		private bool isTaskPeriodsMerged(IList<ITemplateTaskPeriod> taskPeriodCollection, int skillResolution)
		{
			var periodStart = taskPeriodCollection.Min(x => x.Period.StartDateTime);
			var periodEnd = taskPeriodCollection.Max(x => x.Period.EndDateTime);
			var periodLength = (int)periodEnd.Subtract(periodStart).TotalMinutes;
			var expectedIntervalCount = periodLength / skillResolution;
			return (expectedIntervalCount != taskPeriodCollection.Count);
		}

		private static IList<ITemplateTaskPeriod> splitTaskPeriods(IList<ITemplateTaskPeriod> templateTaskPeriodCollection, TimeSpan periodLength)
		{
			List<ITemplateTaskPeriod> returnList = new List<ITemplateTaskPeriod>();
			foreach (var taskPeriod in templateTaskPeriodCollection)
			{
				var splittedTaskPeriods = taskPeriod.Split(periodLength);
				returnList.AddRange(splittedTaskPeriods.Select(p => new TemplateTaskPeriod(
					new Task(p.TotalTasks, p.TotalAverageTaskTime, p.TotalAverageAfterTaskTime), p.Period)));
			}
			return returnList;
		}


		private DateTime? getLatestStatisticsTimeUtc(DateTime? latestStatisticsTime)
		{
			return latestStatisticsTime.HasValue
				? TimeZoneHelper.ConvertToUtc(latestStatisticsTime.Value, _timeZone.TimeZone())
				: (DateTime?)null;
		}
	}
}