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
			ICollection<ISkillDay> skillDays,
			IList<StaffingIntervalModel> forecastedStaffingIntervals,
			TimeSpan resolution,
			IList<SkillDayStatsRange> skillDayStatsRange)
		{

			var actualStaffingIntervals = new List<StaffingIntervalModel>();

			var skillDaysBySkill = skillDays.Select(s => new {Original = s, Clone = s.NoneEntityClone()})
				.ToLookup(s => s.Original.Skill.Id.Value);
			var actualStatisticsBySkill = actualStatistics.ToLookup(s => s.SkillId);
			foreach (var skill in skills)
			{
				var actualStatsPerSkillInterval = actualStatisticsBySkill[skill.Id.Value].ToList();
				var skillDaysForSkill = skillDaysBySkill[skill.Id.Value].ToArray();
				new SkillDayCalculator(skill, skillDaysForSkill.Select(s => s.Clone), new DateOnlyPeriod());

				foreach (var skillDay in skillDaysForSkill)
				{
					var skillDayStats = GetSkillDayStatistics(skillDayStatsRange, skill, skillDay.Original.CurrentDate,
						actualStatsPerSkillInterval, resolution);

					if (!skillDayStats.Any())
						continue;
					mapWorkloadIds(skillDay.Clone, skillDay.Original);
					assignActualWorkloadToClonedSkillDay(skillDay.Clone, skillDayStats);
					skillDay.Clone.RecalculateDailyTasks();
					actualStaffingIntervals.AddRange(GetRequiredStaffing(resolution, skillDayStats, skillDay.Clone));
					reset(skillDay.Clone, skillDay.Original);
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

		private void assignActualWorkloadToClonedSkillDay(ISkillDay clonedSkillDay, IList<SkillIntervalStatistics> skillDayStats)
		{
			foreach (var workloadDay in clonedSkillDay.WorkloadDayCollection)
			{
				foreach (var taskPeriod in workloadDay.TaskPeriodList)
				{
					var statInterval = skillDayStats
						.FirstOrDefault(x => TimeZoneHelper.ConvertToUtc(x.StartTime, _timeZone.TimeZone()) == taskPeriod.Period.StartDateTime &&
																		  x.WorkloadId == workloadDay.Workload.Id.Value);

                    taskPeriod.CampaignTasks = new Percent(0);
                    taskPeriod.CampaignTaskTime = new Percent(0);
                    taskPeriod.CampaignAfterTaskTime = new Percent(0);

                    if (statInterval == null)
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
		}

		private void reset(ISkillDay clonedSkillDay, ISkillDay skillday)
		{
			foreach (var workloadDay in clonedSkillDay.WorkloadDayCollection)
			{
				foreach (var taskPeriod in workloadDay.TaskPeriodList)
				{
					//var statInterval = skillday.WorkloadDayCollection.First().TaskPeriodList.Where(x => x.Period.StartDateTime == taskPeriod.Period.StartDateTime);
					var statInterval = skillday.WorkloadDayCollection.FirstOrDefault(x => x.Workload.Id.Value == workloadDay.Workload.Id.Value)
						.TaskPeriodList.First(x => x.Period.StartDateTime == taskPeriod.Period.StartDateTime);

					taskPeriod.SetTasks(statInterval.Tasks);
					taskPeriod.AverageTaskTime = statInterval.Task.AverageTaskTime;
					taskPeriod.AverageAfterTaskTime = statInterval.Task.AverageAfterTaskTime;
				}
			}
		}

		private IList<StaffingIntervalModel> GetRequiredStaffing(TimeSpan resolution, IList<SkillIntervalStatistics> skillDayStats, 
			ISkillDay skillDay)
		{
			var actualStaffingIntervals = new List<StaffingIntervalModel>();
			var staffing = skillDay.SkillStaffPeriodViewCollection(resolution, false);
			var skillIntervals = skillDayStats.GroupBy(x => x.StartTime);
			foreach (var interval in skillIntervals)
			{
				var staffingInterval = staffing
					.FirstOrDefault(x => x.Period.StartDateTime == TimeZoneHelper.ConvertToUtc(interval.Key, _timeZone.TimeZone()));

				if(staffingInterval == null)
					continue;

				actualStaffingIntervals.Add(new StaffingIntervalModel()
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
			var rangeStartLocal = TimeZoneHelper.ConvertFromUtc(rangeStartUtc.Value, _timeZone.TimeZone());
			var rangeEndLocal = TimeZoneHelper.ConvertFromUtc(rangeEndUtc.Value, _timeZone.TimeZone());

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
					var averageCalls = item.Sum(x => x.Calls) / item.Count();
					var averageAht = item.Sum(x => x.HandleTime) / item.Sum(x => x.AnsweredCalls);

					foreach (var statInterval in item)
					{
						mergedStats.Add(new SkillIntervalStatistics()
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