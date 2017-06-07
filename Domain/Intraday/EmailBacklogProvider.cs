using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class EmailBacklogProvider
	{
		private readonly IIntradayQueueStatisticsLoader _intradayQueueStatisticsLoader;

		public EmailBacklogProvider(IIntradayQueueStatisticsLoader intradayQueueStatisticsLoader)
		{
			_intradayQueueStatisticsLoader = intradayQueueStatisticsLoader;
		}

		public Dictionary<Guid, int> GetStatisticsBacklogByWorkload(
			ICollection<ISkillDay> skillDays, 
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

		private static Dictionary<Guid, ClosedPeriodWorkload> GetWorkloadsClosedPeriod(ICollection<ISkillDay> skillDays, DateOnly userDateOnly)
		{
			var skillDaysBySkill = skillDays
				.Where(y => y.CurrentDate <= userDateOnly)
				.ToLookup(x => x.Skill);
			var workloadClosedHoursDictionary = new Dictionary<Guid, ClosedPeriodWorkload>();
			foreach (var skillDayList in skillDaysBySkill)
			{
				if (skillDayList.Key.SkillType.Description.Name != "SkillTypeEmail")
					continue;

				var reversedSkillDayList = skillDayList.OrderByDescending(x => x.CurrentDate);
				DateTime skillClosedStart = DateTime.MinValue;
				DateTime skillClosedEnd = DateTime.MinValue;

				foreach (var skillDay in reversedSkillDayList)
				{
					foreach (var workloadDay in skillDay.WorkloadDayCollection)
					{
						if (!workloadDay.OpenTaskPeriodList.Any())
							continue;
						var openingHour = workloadDay.OpenTaskPeriodList.First().Period.StartDateTime;
						if (!workloadClosedHoursDictionary.ContainsKey(workloadDay.Workload.Id.Value))
						{
							workloadClosedHoursDictionary.Add(workloadDay.Workload.Id.Value,
								new ClosedPeriodWorkload(openingHour.Date, openingHour, false));
						}
						else
						{
							if (workloadClosedHoursDictionary[workloadDay.Workload.Id.Value].hasBacklogStart)
								continue;
							var todayStartOpeningHour = workloadClosedHoursDictionary[workloadDay.Workload.Id.Value].Period.EndDateTime;
							workloadClosedHoursDictionary[workloadDay.Workload.Id.Value] =
								new ClosedPeriodWorkload(workloadDay.OpenTaskPeriodList.Last().Period.EndDateTime, todayStartOpeningHour, true);
						}
						
						if (skillClosedEnd < workloadClosedHoursDictionary[workloadDay.Workload.Id.Value].Period.EndDateTime)
						{
							skillClosedEnd = workloadClosedHoursDictionary[workloadDay.Workload.Id.Value].Period.EndDateTime;
						}
						if (workloadClosedHoursDictionary[workloadDay.Workload.Id.Value].Period.StartDateTime < skillClosedStart)
						{
							skillClosedStart = workloadClosedHoursDictionary[workloadDay.Workload.Id.Value].Period.StartDateTime;
						}

					}
				}

			}

			return workloadClosedHoursDictionary;
		}

		public Dictionary<Guid, int> GetForecastedBacklogBySkill(ICollection<ISkillDay> skillDays, IList<ISkill> skills, DateOnly userDateOnly)
		{
			var forecastedBacklog = new Dictionary<Guid, int>();


			return forecastedBacklog;
		}
	}
}