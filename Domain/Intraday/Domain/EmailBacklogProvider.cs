using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public class EmailBacklogProvider : IEmailBacklogProvider
	{
		private readonly IIntradayQueueStatisticsLoader _intradayQueueStatisticsLoader;

		public EmailBacklogProvider(IIntradayQueueStatisticsLoader intradayQueueStatisticsLoader)
		{
			_intradayQueueStatisticsLoader = intradayQueueStatisticsLoader;
		}

		public Dictionary<Guid, int> GetStatisticsBacklogByWorkload(
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, 
			IList<SkillIntervalStatistics> actualVolumePerWorkloadInterval, 
			DateOnly userDateOnly, 
			int minutesPerInterval, 
			IList<SkillDayStatsRange> skillDayStatsRange)
		{
			var workloadClosedHoursWithSl = GetWorkloadsClosedPeriod(skillDays, userDateOnly);
			var workloadBacklogs = GetEmailBacklogs(workloadClosedHoursWithSl);
			return workloadBacklogs;
		}

		private Dictionary<Guid, int> GetEmailBacklogs(Dictionary<Guid, ClosedPeriodWorkload> workloadsClosedHours)
		{
			var workloadBacklogDictionary = new Dictionary<Guid, int>();
			foreach (var workloadId in workloadsClosedHours.Keys)
			{
				var closedPeriod = new DateTimePeriod(workloadsClosedHours[workloadId].Period.StartDateTime,
					workloadsClosedHours[workloadId].Period.EndDateTime);
				var backlog = _intradayQueueStatisticsLoader.LoadActualEmailBacklogForWorkload(workloadId, closedPeriod);
				workloadBacklogDictionary.Add(workloadId, backlog);
			}

			return workloadBacklogDictionary;
		}

		private static Dictionary<Guid, ClosedPeriodWorkload> GetWorkloadsClosedPeriod(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateOnly userDateOnly)
		{
			var workloadClosedHoursDictionary = new Dictionary<Guid, ClosedPeriodWorkload>();
			foreach (var skillDayList in skillDays)
			{
				if (!SkillTypesWithBacklog.IsBacklogSkillType(skillDayList.Key))
					continue;

				var reversedSkillDayList = skillDayList.Value
					.Where(y => y.CurrentDate <= userDateOnly)
					.OrderByDescending(x => x.CurrentDate);

				foreach (var skillDay in reversedSkillDayList)
				{
					foreach (var workloadDay in skillDay.WorkloadDayCollection)
					{
						if (!workloadDay.OpenTaskPeriodList.Any())
							continue;
						var openingHour = workloadDay.OpenTaskPeriodList.First().Period.StartDateTime;
						if (!workloadClosedHoursDictionary.TryGetValue(workloadDay.Workload.Id.Value, out var closePeriod))
						{
							workloadClosedHoursDictionary.Add(workloadDay.Workload.Id.Value,
								new ClosedPeriodWorkload(openingHour.Date, openingHour, false));
						}
						else
						{
							if (closePeriod.hasBacklogStart)
								continue;
							var todayStartOpeningHour = closePeriod.Period.EndDateTime;
							workloadClosedHoursDictionary[workloadDay.Workload.Id.Value] =
								new ClosedPeriodWorkload(workloadDay.OpenTaskPeriodList.Last().Period.EndDateTime, todayStartOpeningHour, true);
						}
					}
				}

			}

			return workloadClosedHoursDictionary;
		}
	}
}