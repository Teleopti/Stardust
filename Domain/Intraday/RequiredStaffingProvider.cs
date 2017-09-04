using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class RequiredStaffingProvider
	{
		private readonly IUserTimeZone _timeZone;

		public RequiredStaffingProvider(IUserTimeZone timeZone)
		{
			_timeZone = timeZone;
		}

		public IList<StaffingIntervalModel> Load(IList<SkillIntervalStatistics> actualStatistics, 
			IList<ISkill> skills, 
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, 
			IList<StaffingIntervalModel> forecastedStaffingIntervals,
			TimeSpan resolution, IList<SkillDayStatsRange> skillDayStatsRange,
			Dictionary<Guid, int> workloadBacklog)
		{

			var actualStaffingIntervals = new List<StaffingIntervalModel>();

			var skillDaysBySkill = skillDays
				.SelectMany(x => x.Value)
				.Select(s => new { Original = s, Clone = s.NoneEntityClone() })
				.ToLookup(s => s.Original.Skill.Id.Value);

			var actualStatisticsBySkill = actualStatistics.ToLookup(s => s.SkillId);
			foreach (var skill in skills)
			{
				var actualStatsPerSkillInterval = actualStatisticsBySkill[skill.Id.Value].ToList();
				var skillDaysForSkill = skillDaysBySkill[skill.Id.Value].ToArray();
				new SkillDayCalculator(skill, skillDaysForSkill.Select(s => s.Clone), new DateOnlyPeriod());

				foreach (var skillDay in skillDaysForSkill)
				{
					var skillDayStats = GetSkillDayStatistics(skillDayStatsRange, skill, skillDay.Original.CurrentDate, actualStatsPerSkillInterval, resolution);

					mapWorkloadIds(skillDay.Clone, skillDay.Original);
					assignActualWorkloadToClonedSkillDay(skillDay.Clone, skillDayStats, workloadBacklog);
					actualStaffingIntervals.AddRange(GetRequiredStaffing(resolution, skillDayStats, skillDay.Clone));
				}
				foreach (var skillDayToReset in skillDaysForSkill)
				{
					reset(skillDayToReset.Clone, skillDayToReset.Original);
				}
			}

			return actualStaffingIntervals;
		}


		private void mapWorkloadIds(ISkillDay clonedSkillDay, ISkillDay skillDay)
		{
			var index = 0;
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				clonedSkillDay.WorkloadDayCollection[index].Workload.SetId(workloadDay.Workload.Id.Value);
				index++;
			}
		}

		private void assignActualWorkloadToClonedSkillDay(ISkillDay clonedSkillDay, IList<SkillIntervalStatistics> skillDayStats, Dictionary<Guid, int> workloadBacklog)
		{
			clonedSkillDay.Lock();
			var timeZone = _timeZone.TimeZone();
			var skillDayStatsPerWorkload = skillDayStats.ToLookup(w => w.WorkloadId);
			foreach (var workloadDay in clonedSkillDay.WorkloadDayCollection)
			{
				if (workloadDay.OpenTaskPeriodList.Any())
				{
					var openHourStartLocal = TimeZoneHelper.ConvertFromUtc(workloadDay.OpenTaskPeriodList.First().Period.StartDateTime, timeZone);
					var firstOpenInterval = skillDayStatsPerWorkload[workloadDay.Workload.Id.Value].FirstOrDefault(x => x.StartTime == openHourStartLocal);
					int calls = 0;
					if (firstOpenInterval != null && workloadBacklog.TryGetValue(workloadDay.Workload.Id.Value, out calls))
					{
						firstOpenInterval.Calls = calls + firstOpenInterval.Calls;
					}
				}
				else
					continue;
				foreach (var taskPeriod in workloadDay.TaskPeriodList)
				{
					var statInterval = skillDayStatsPerWorkload[workloadDay.Workload.Id.Value]
						.FirstOrDefault(x => TimeZoneHelper.ConvertToUtc(x.StartTime, timeZone) == taskPeriod.Period.StartDateTime);

                    taskPeriod.CampaignTasks = new Percent(0);
                    taskPeriod.CampaignTaskTime = new Percent(0);
                    taskPeriod.CampaignAfterTaskTime = new Percent(0);

                    if (statInterval == null || double.IsNaN(statInterval.AverageHandleTime))
					{
						taskPeriod.SetTasks(0);
						taskPeriod.AverageTaskTime = TimeSpan.Zero;
						continue;
					}

					taskPeriod.SetTasks(statInterval.Calls);
					taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(statInterval.AverageHandleTime);
					taskPeriod.AverageAfterTaskTime = TimeSpan.Zero;
				}
			}
			clonedSkillDay.Release();
		}

		private void reset(ISkillDay clonedSkillDay, ISkillDay skillday)
		{
			clonedSkillDay.Lock();
			foreach (var workloadDay in clonedSkillDay.WorkloadDayCollection)
			{
				var innerWorkloadDay = skillday.WorkloadDayCollection.FirstOrDefault(x => x.Workload.Id.Value == workloadDay.Workload.Id.Value);
				foreach (var taskPeriod in workloadDay.TaskPeriodList)
				{
					var statInterval = innerWorkloadDay
						.TaskPeriodList.First(x => x.Period.StartDateTime == taskPeriod.Period.StartDateTime);

					taskPeriod.SetTasks(statInterval.Tasks);
					taskPeriod.AverageTaskTime = statInterval.Task.AverageTaskTime;
					taskPeriod.AverageAfterTaskTime = statInterval.Task.AverageAfterTaskTime;
				}
			}
			clonedSkillDay.Release();
		}

		private IList<StaffingIntervalModel> GetRequiredStaffing(TimeSpan resolution, IList<SkillIntervalStatistics> skillDayStats, 
			ISkillDay skillDay)
		{
			var actualStaffingIntervals = new List<StaffingIntervalModel>();
			var staffing = skillDay.SkillStaffPeriodViewCollection(resolution, false);
			var skillIntervals = skillDayStats.GroupBy(x => x.StartTime);
			var timeZone = _timeZone.TimeZone();
			foreach (var interval in skillIntervals)
			{
				var utcTime = TimeZoneHelper.ConvertToUtc(interval.Key, timeZone);
				var staffingInterval = staffing.FirstOrDefault(x => x.Period.StartDateTime == utcTime);

				if(staffingInterval == null)
					continue;

				actualStaffingIntervals.Add(new StaffingIntervalModel
				{
					SkillId = skillDay.Skill.Id.Value,
					StartTime = interval.Key,
					Agents = staffingInterval.FStaff
				});
			}

			return actualStaffingIntervals;
		}

		private IList<SkillIntervalStatistics> GetSkillDayStatistics(IList<SkillDayStatsRange> skillDayStatsRange, ISkill skill, DateOnly skillDayDate, List<SkillIntervalStatistics> actualStatsPerInterval, TimeSpan resolution)
		{			
			var rangeStartUtc = skillDayStatsRange
				.FirstOrDefault(x => x.SkillId == skill.Id.Value && x.SkillDayDate == skillDayDate)?
				.RangePeriod.StartDateTime;
			if (!rangeStartUtc.HasValue)
				return new List<SkillIntervalStatistics>();
			var rangeEndUtc = skillDayStatsRange
				.FirstOrDefault(x => x.SkillId == skill.Id.Value && x.SkillDayDate == skillDayDate)?
				.RangePeriod.EndDateTime;
			var timeZone = _timeZone.TimeZone();
			var rangeStartLocal = TimeZoneHelper.ConvertFromUtc(rangeStartUtc.Value, timeZone);
			var rangeEndLocal = TimeZoneHelper.ConvertFromUtc(rangeEndUtc.Value, timeZone);

			var retList = actualStatsPerInterval
				.Where(x => x.StartTime >= rangeStartLocal && x.StartTime < rangeEndLocal)
				.ToList();

			if (skill.DefaultResolution == resolution.TotalMinutes)
				return retList;

			var intervalsInResolution = skill.DefaultResolution / resolution.TotalMinutes;
			var mergedStats = new List<SkillIntervalStatistics>();
			for (var interval = rangeStartLocal; interval < rangeEndLocal; interval = interval.AddMinutes(skill.DefaultResolution))
			{
				var statsPerWorkload = retList
					.Where(x => x.StartTime >= interval && x.StartTime < interval.AddMinutes(skill.DefaultResolution))
					.GroupBy(x => x.WorkloadId);
				foreach (var item in statsPerWorkload)
				{
					var itemCount = item.Count();
					var sumAnsweredCalls = item.Sum(x => x.AnsweredCalls);

					var averageCalls = (itemCount > 0 ? item.Sum(x => x.Calls) / itemCount : 0);
					var averageAht = (sumAnsweredCalls > 0 ? item.Sum(x => x.HandleTime) / sumAnsweredCalls : 0);

					foreach (var statInterval in item)
					{
						mergedStats.Add(new SkillIntervalStatistics
						{
							Calls = averageCalls * intervalsInResolution,
							AverageHandleTime = averageAht,
							SkillId = skill.Id.Value,
							StartTime = statInterval.StartTime,
							WorkloadId = item.Key
						});
					}
				}

			}
			return mergedStats;
		}

		public double?[] DataSeries(IList<StaffingIntervalModel> requiredStaffingPerSkill, DateTime? latestStatsTime, int minutesPerInterval, DateTime[] timeSeries)
		{
			var returnValue = new List<double?>();

			if (!latestStatsTime.HasValue || !requiredStaffingPerSkill.Any())
				return new double?[] {};

			foreach (var interval in timeSeries)
			{
				var requiredStaffingInterval = requiredStaffingPerSkill.Where(x => x.StartTime == interval).ToList();
				if (requiredStaffingInterval.Any())
					returnValue.Add(requiredStaffingInterval.Sum(a => a.Agents));
				else
					returnValue.Add(null);
			}

			return returnValue.ToArray();
		}

	}
}