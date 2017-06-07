using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ReforecastedStaffingProvider
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;

		public ReforecastedStaffingProvider(INow now, IUserTimeZone timeZone)
		{
			_now = now;
			_timeZone = timeZone;
		}

		public double?[] DataSeries(IList<StaffingIntervalModel> forecastedStaffingList,
			IList<SkillIntervalStatistics> actualCallsPerSkillList,
			Dictionary<Guid, List<SkillIntervalStatistics>> forecastedCallsPerSkillDictionary,
			DateTime? latestStatsTime,
			int minutesPerInterval,
			DateTime[] timeSeries,
			Dictionary<Guid, int> workloadBacklog)
		{
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			if (!latestStatsTime.HasValue)
				return new double?[] {};

			if (forecastedCallsPerSkillDictionary.Count(x => x.Value.Any()) == 0)
				return new double?[] {};

			if (latestStatsTime > usersNow) // This case only for dev, test and demo
				usersNow = latestStatsTime.Value.AddMinutes(minutesPerInterval);

			var reforecastedStaffingPerSkill = new List<StaffingIntervalModel>();

			foreach (var skillId in forecastedCallsPerSkillDictionary.Keys)
			{
				var averageDeviation = calculateMovingAverage(actualCallsPerSkillList, forecastedCallsPerSkillDictionary, skillId, workloadBacklog);

				var reforecastedStaffing = forecastedStaffingList
					.Where(x => x.SkillId == skillId && x.StartTime >= usersNow)
					.Select(t => new StaffingIntervalModel
					{
						SkillId = skillId,
						Agents = t.Agents*averageDeviation,
						StartTime = t.StartTime
					})
					.OrderBy(y => y.StartTime)
					.ToList();

				reforecastedStaffingPerSkill.AddRange(reforecastedStaffing);
			}

			return !reforecastedStaffingPerSkill.Any() 
				? new double?[] {} 
				: getDataSeries(minutesPerInterval, timeSeries, reforecastedStaffingPerSkill);
		}

		private static double?[] getDataSeries(int minutesPerInterval, DateTime[] timeSeries, List<StaffingIntervalModel> reforecastedStaffingPerSkill)
		{
			var returnValue = reforecastedStaffingPerSkill
				.OrderBy(g => g.StartTime)
				.GroupBy(h => h.StartTime)
				.Select(s => (double?) s.Sum(a => a.Agents))
				.ToList();


			var timeSeriesStart = timeSeries.Min();
			var reforecastStart = reforecastedStaffingPerSkill
				.Min(m => m.StartTime);

			for (DateTime i = timeSeriesStart; i < reforecastStart; i = i.AddMinutes(minutesPerInterval))
			{
				returnValue.Insert(0, null);
			}

			var reforecastEnd = reforecastedStaffingPerSkill
				.Max(m => m.StartTime);
			var timeSeriesEnd = timeSeries.Max().AddMinutes(minutesPerInterval);

			for (DateTime j = reforecastEnd.AddMinutes(minutesPerInterval);
				j < timeSeriesEnd;
				j = j.AddMinutes(minutesPerInterval))
			{
				returnValue.Add(null);
			}

			return returnValue.ToArray();
		}

		private static double calculateMovingAverage(IList<SkillIntervalStatistics> actualCallsPerSkillList, 
			Dictionary<Guid, List<SkillIntervalStatistics>> forecastedCallsPerSkillDictionary, 
			Guid skillId, 
			Dictionary<Guid, int> workloadBacklog)
		{
			List<SkillIntervalStatistics> actualStats = actualCallsPerSkillList.Where(x => x.SkillId == skillId).ToList();
			List<double> listDeviationFactorPerInterval = new List<double>();
			double averageDeviation = 1;
			var alpha = 0.2d;

			if (!actualStats.Any())
				return averageDeviation;

			var workloadsInSkill = actualStats.Select(x => x.WorkloadId)
				.Distinct()
				.ToArray();
			var statisticsBacklog = workloadBacklog
				.Where(x => workloadsInSkill.Contains(x.Key))
				.Sum(b => b.Value);

			foreach (var forecastedIntervalCalls in forecastedCallsPerSkillDictionary[skillId])
			{
				var actualIntervalCalls = actualStats.Where(x => x.StartTime == forecastedIntervalCalls.StartTime);
				if (actualIntervalCalls == null)
					continue;
				if (Math.Abs(forecastedIntervalCalls.Calls) < 0.1)
					continue;
				listDeviationFactorPerInterval.Add((actualIntervalCalls.Sum(x => x.Calls) + statisticsBacklog)/forecastedIntervalCalls.Calls);
				statisticsBacklog = 0;
			}
			
			if (listDeviationFactorPerInterval.Any())
				averageDeviation = listDeviationFactorPerInterval.Aggregate((current, next) => alpha*next + (1 - alpha)*current);

			return averageDeviation;
		}	
	}
}