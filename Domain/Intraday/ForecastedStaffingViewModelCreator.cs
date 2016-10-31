using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingViewModelCreator
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly IIntradayQueueStatisticsLoader _intradayQueueStatisticsLoader;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly ISkillRepository _skillRepository;

		public ForecastedStaffingViewModelCreator(
			INow now,
			IUserTimeZone timeZone,
			ForecastedStaffingProvider forecastedStaffingProvider,
			IIntradayQueueStatisticsLoader intradayQueueStatisticsLoader,
			IIntervalLengthFetcher intervalLengthFetcher,
			ISkillRepository skillRepository
			)
		{
			_now = now;
			_timeZone = timeZone;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_intradayQueueStatisticsLoader = intradayQueueStatisticsLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
			_skillRepository = skillRepository;
		}

		public IntradayStaffingViewModel Load(Guid[] skillIdList)
		{

			var skills = _skillRepository.LoadSkills(skillIdList);
			var supportedSkillIdList = skillIdList;

			foreach (var skill in skills)
			{
				if (!checkSupportedSkill(skill))
				{
					var skillToRemove = skill.Id.Value;
					supportedSkillIdList = supportedSkillIdList.Where(val => val != skillToRemove).ToArray();
				}
			}

			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);
			var actualCallsPerSkillInterval = _intradayQueueStatisticsLoader.LoadActualCallPerSkillInterval(supportedSkillIdList, _timeZone.TimeZone(), usersToday);
			DateTime? latestStatsTime = null;

			if (actualCallsPerSkillInterval.Count > 0)
				latestStatsTime = actualCallsPerSkillInterval.Max(d => d.StartTime);

			var forecastedStaffingModel = _forecastedStaffingProvider.Load(supportedSkillIdList, latestStatsTime, minutesPerInterval, actualCallsPerSkillInterval);

			var staffingForUsersToday = forecastedStaffingModel.StaffingIntervals
												.Where(t => t.StartTime >= usersToday.Date && t.StartTime < usersToday.Date.AddDays(1))
												.ToList();

			var updatedForecastedSeries = getUpdatedForecastedStaffing(
				staffingForUsersToday,
				actualCallsPerSkillInterval,
				forecastedStaffingModel.CallsPerSkill,
				latestStatsTime,
				usersNow,
				minutesPerInterval
			);

			staffingForUsersToday = staffingForUsersToday
				.GroupBy(g => g.StartTime)
				.Select(s =>
				{
					var staffingIntervalModel = s.First();
					return new StaffingIntervalModel
					{
						SkillId = staffingIntervalModel.SkillId,
						StartTime = staffingIntervalModel.StartTime,
						Agents = s.Sum(a => a.Agents)
					};
				})
				.OrderBy(o => o.StartTime)
				.ToList();

			return new IntradayStaffingViewModel
			{
				DataSeries = new StaffingDataSeries
				{
					Time = staffingForUsersToday
								.Select(t => t.StartTime)
								.ToArray(),
					ForecastedStaffing = staffingForUsersToday
								.Select(t => t.Agents)
								.ToArray(),
					UpdatedForecastedStaffing = updatedForecastedSeries
								.ToArray(),
					ActualStaffing = getActualStaffingSeries(forecastedStaffingModel.ActualStaffingPerSkill, latestStatsTime, minutesPerInterval, staffingForUsersToday)
								.ToArray()
				}
			};
		}

		private bool checkSupportedSkill(ISkill skill)
		{
			var isMultisiteSkill = skill.GetType() == typeof(MultisiteSkill);

			return !isMultisiteSkill &&
				   skill.SkillType.Description.Name.Equals("SkillTypeInboundTelephony", StringComparison.InvariantCulture);
		}

		private List<double?> getActualStaffingSeries(List<StaffingIntervalModel> actualStaffingPerSkill, DateTime? latestStatsTime, int minutesPerInterval, List<StaffingIntervalModel> staffingForUsersToday)
		{
			var returnValue = new List<double?>();

			if (!latestStatsTime.HasValue || !actualStaffingPerSkill.Any())
				return new List<double?>();

			returnValue.AddRange(actualStaffingPerSkill
				.OrderBy(x => x.StartTime)
				.GroupBy(y => y.StartTime)
				.Select(s => (double?)s.Sum(a => a.Agents))
				.ToList());

			var actualStartTime = actualStaffingPerSkill.Min(x => x.StartTime);
			var actualEndTime = actualStaffingPerSkill.Max(x => x.StartTime).AddMinutes(minutesPerInterval);

			var forecastStartTime = staffingForUsersToday.Min(x => x.StartTime);
			var forecastEndTime = staffingForUsersToday.Max(x => x.StartTime).AddMinutes(minutesPerInterval);

			for (DateTime i = forecastStartTime; i < actualStartTime; i = i.AddMinutes(minutesPerInterval))
				returnValue.Insert(0, null);

			for (DateTime i = actualEndTime; i < forecastEndTime; i = i.AddMinutes(minutesPerInterval))
				returnValue.Add(null);


			return returnValue;
		}

		private List<double?> getUpdatedForecastedStaffing(
			List<StaffingIntervalModel> forecastedStaffingList,
			IList<SkillIntervalStatistics> actualCallsPerSkillList,
			Dictionary<Guid, List<SkillIntervalStatistics>> forecastedCallsPerSkillDictionary,
			DateTime? latestStatsTime,
			DateTime usersNow,
			int minutesPerInterval)
		{
			if (!latestStatsTime.HasValue)
				return new List<double?>();

			if (forecastedCallsPerSkillDictionary.Count(x => x.Value.Any()) == 0)
			{
				return new List<double?>();
			}

			if (latestStatsTime > usersNow) // This case only for dev, test and demo
				usersNow = latestStatsTime.Value.AddMinutes(minutesPerInterval);

			var updatedForecastedSeries = new List<StaffingIntervalModel>();

			foreach (var skillId in forecastedCallsPerSkillDictionary.Keys)
			{
				List<SkillIntervalStatistics> actualStats = actualCallsPerSkillList.Where(x => x.SkillId == skillId).ToList();

				double averageDeviation = 1;
				if (actualStats.Count > 0)
				{
					double callsDeviationFactor = 0;
					int intervalCounter = 0;
					foreach (var forecastedIntervalCalls in forecastedCallsPerSkillDictionary[skillId])
					{
						var actualIntervalCalls =
							actualStats.SingleOrDefault(x => x.StartTime == forecastedIntervalCalls.StartTime);
						if (actualIntervalCalls == null)
							continue;
						if (Math.Abs(forecastedIntervalCalls.Calls) < 0.1)
							continue;

						intervalCounter++;
						callsDeviationFactor += actualIntervalCalls.Calls / forecastedIntervalCalls.Calls;
					}

					averageDeviation = callsDeviationFactor / intervalCounter;
				}


				var updatedForecastedSeriesPerSkill = forecastedStaffingList
					.Where(x => x.SkillId == skillId && x.StartTime >= usersNow)
					.Select(t => new StaffingIntervalModel
					{
						SkillId = skillId,
						Agents = t.Agents * averageDeviation,
						StartTime = t.StartTime
					})
					.OrderBy(y => y.StartTime)
					.ToList();

				updatedForecastedSeries.AddRange(updatedForecastedSeriesPerSkill);
			}

			if (!updatedForecastedSeries.Any())
			{
				return new List<double?>();
			}

			var returnValue = updatedForecastedSeries
				.OrderBy(g => g.StartTime)
				.GroupBy(h => h.StartTime)
				.Select(s => (double?)s.Sum(a => a.Agents))
				.ToList();


			var nullStartTime = forecastedStaffingList
				.Min(y => y.StartTime);

			var nullEndTime = updatedForecastedSeries
				.Min(m => m.StartTime);

			for (DateTime i = nullStartTime; i < nullEndTime; i = i.AddMinutes(minutesPerInterval))
			{
				returnValue.Insert(0, null);
			}

			return returnValue;
		}
	}
}