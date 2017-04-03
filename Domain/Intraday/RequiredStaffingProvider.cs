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

			foreach (var skill in skills)
			{
				var actualStatsPerInterval = actualStatistics.Where(s => s.SkillId == skill.Id).ToList();
				var skillDaysForSkill = skillDays.Where(x => x.Skill.Id.Value == skill.Id.Value).ToList();
				if (!skillDaysForSkill.Any())
					continue;

				foreach (var skillDay in skillDaysForSkill)
				{
					var skillDayStats = GetSkillDayStatistics(skillDayStatsRange, skill, skillDay,actualStatsPerInterval);

					actualStaffingIntervals.AddRange(GetRequiredStaffing(resolution, skillDayStats, skillDay));
				}
			}

			return actualStaffingIntervals;
		}

		private IList<StaffingIntervalModel> GetRequiredStaffing(TimeSpan resolution, IList<SkillIntervalStatistics> skillDayStats, ISkillDay skillDay)
		{
			var staffingCalculatorService = new StaffingCalculatorServiceFacade();
			var actualStaffingIntervals = new List<StaffingIntervalModel>();

			foreach (var actualInterval in skillDayStats)
			{
				var actualStatsStartTimeUtc = TimeZoneHelper.ConvertToUtc(actualInterval.StartTime, _timeZone.TimeZone());

				var skillData =
					skillDay.SkillDataPeriodCollection
					.FirstOrDefault(skillDataPeriod => skillDataPeriod.Period.StartDateTime <= actualStatsStartTimeUtc &&
										   skillDataPeriod.Period.EndDateTime > actualStatsStartTimeUtc);

				if (skillData == null)
					continue;

				var efficencyPerSkillInterval = skillDay.SkillStaffPeriodCollection
					.Where(x => x.Period.StartDateTime <= actualStatsStartTimeUtc && x.Period.EndDateTime > actualStatsStartTimeUtc)
					.Select(s => s.Payload.Efficiency)
					.First();
				var efficiencyFactor = (1/efficencyPerSkillInterval.Value);

				var agents = staffingCalculatorService.AgentsUseOccupancy(
								 skillData.ServiceAgreement.ServiceLevel.Percent.Value,
								 (int) skillData.ServiceAgreement.ServiceLevel.Seconds,
								 actualInterval.Calls,
								 actualInterval.AverageHandleTime,
								 resolution,
								 skillData.ServiceAgreement.MinOccupancy.Value,
								 skillData.ServiceAgreement.MaxOccupancy.Value,
								 skillDay.Skill.MaxParallelTasks)*efficiencyFactor;

				actualStaffingIntervals.Add(new StaffingIntervalModel()
				{
					SkillId = skillDay.Skill.Id.Value,
					StartTime = actualInterval.StartTime,
					Agents = agents
				});
			}

			return actualStaffingIntervals;
		}

		private IList<SkillIntervalStatistics> GetSkillDayStatistics(IList<SkillDayStatsRange> skillDayStatsRange, ISkill skill, ISkillDay skillDay,
			List<SkillIntervalStatistics> actualStatsPerInterval)
		{
			var rangeStartUtc = skillDayStatsRange
				.FirstOrDefault(x => x.skillId == skill.Id.Value && x.skillDayId == skillDay.Id.Value)?
				.RangePeriod.StartDateTime;
			if (!rangeStartUtc.HasValue)
				return new List<SkillIntervalStatistics>();
			var rangeEndUtc = skillDayStatsRange
				.FirstOrDefault(x => x.skillId == skill.Id.Value && x.skillDayId == skillDay.Id.Value)?
				.RangePeriod.EndDateTime;
			var rangeStartLocal = TimeZoneHelper.ConvertFromUtc(rangeStartUtc.Value, _timeZone.TimeZone());
			var rangeEndLocal = TimeZoneHelper.ConvertFromUtc(rangeEndUtc.Value, _timeZone.TimeZone());

			return actualStatsPerInterval
				.Where(x => x.StartTime >= rangeStartLocal && x.StartTime < rangeEndLocal)
				.ToList();
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