using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Intraday.To_Staffing
{
	public class StaffingViewModelCreator
	{
		private readonly IResourceCalculation _resourceCalculation;
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillForecastReadModelRepository _skillForecastReadModelRepository;
		private readonly IUserTimeZone _timeZone;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly INow _now;

		public StaffingViewModelCreator(IResourceCalculation resourceCalculation, ISkillRepository skillRepository, ISkillCombinationResourceRepository skillCombinationResourceRepository,
			ISkillForecastReadModelRepository skillForecastReadModelRepository, IUserTimeZone timeZone,
			IIntervalLengthFetcher intervalLengthFetcher, INow now)
		{
			_resourceCalculation = resourceCalculation;
			_skillRepository = skillRepository;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_skillForecastReadModelRepository = skillForecastReadModelRepository;
			_timeZone = timeZone;
			_intervalLengthFetcher = intervalLengthFetcher;
			_now = now;
		}

		public ScheduledStaffingViewModel Load(Guid[] skillIdList, DateOnly? dateInLocalTime = null,
			bool useShrinkage = false)
		{
			var timeZone = _timeZone.TimeZone();
			var startOfDayLocal = dateInLocalTime?.Date ?? TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), timeZone).Date;

			var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayLocal.Date, timeZone);

			var minutesPerInterval = _intervalLengthFetcher.GetIntervalLength();
			if (minutesPerInterval <= 0) throw new Exception($"IntervalLength is cannot be {minutesPerInterval}!");

			var skillStaffingIntervals = loadAndResorceCalculate(startOfDayUtc);
			var timeSeries = DataSeries(startOfDayLocal, minutesPerInterval, _timeZone.TimeZone()).Where(x => x.Date == startOfDayLocal.Date).ToArray();


			var dataSeries = new StaffingDataSeries
			{
				Date = new DateOnly(startOfDayLocal),
				Time = timeSeries,
				ForecastedStaffing = ForecastDataSeries(skillStaffingIntervals.ToList(), timeSeries),
				ScheduledStaffing = ScheduledDataSeries(skillStaffingIntervals.ToList(), timeSeries)
			};
			calculateAbsoluteDifference(dataSeries);
			return new ScheduledStaffingViewModel
			{
				DataSeries = dataSeries,
				StaffingHasData = skillStaffingIntervals.Any()
			};
		}

		private IList<SkillStaffingInterval> loadAndResorceCalculate(DateTime startOfDayUtc)
		{
			var skills = _skillRepository.LoadAll().ToList();
			var firstPeriodDateInSkillTimeZone = new DateOnly(skills.Select(x => TimeZoneInfo.ConvertTimeFromUtc(startOfDayUtc, x.TimeZone)).Min());
			var lastPeriodDateInSkillTimeZone = new DateOnly(skills.Select(x => TimeZoneInfo.ConvertTimeFromUtc(startOfDayUtc.AddDays(1), x.TimeZone)).Max());
			var dateOnlyPeriod = new DateOnlyPeriod(firstPeriodDateInSkillTimeZone, lastPeriodDateInSkillTimeZone);

			var period = new DateTimePeriod(startOfDayUtc, startOfDayUtc.AddDays(1));
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			var skillForecastList =
				_skillForecastReadModelRepository.LoadSkillForecast(skills.Select(x => x.Id.GetValueOrDefault()).ToArray(), startOfDayUtc, startOfDayUtc.AddDays(1));

			var intervals = skillForecastList.Select(skillForecast => new SkillStaffingInterval
			{
				SkillId = skillForecast.SkillId,
				StartDateTime = skillForecast.StartDateTime,
				EndDateTime = skillForecast.EndDateTime,
				Forecast = skillForecast.Agents,
				StaffingLevel = 0,
			});
			var returnList = new HashSet<SkillStaffingInterval>();
			intervals.ForEach(i => returnList.Add(i));

			var skillStaffingIntervals = returnList
				.Where(x => period.Contains(x.StartDateTime) || x.DateTimePeriod.Contains(period.StartDateTime)).ToList();
			skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

			var relevantSkillStaffPeriods = skillStaffingIntervals
				.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
				.ToDictionary(k => k.Key,
					v =>
						(IResourceCalculationPeriodDictionary)
						new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
							s => (IResourceCalculationPeriod)s)));

			var resCalcData = new ResourceCalculationData(skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));

			using (new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, false))))
			{
				_resourceCalculation
					.ResourceCalculate(dateOnlyPeriod, resCalcData, () => new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, true))));
			}

			return skillStaffingIntervals;
		}
		private double?[] ForecastDataSeries(IList<SkillStaffingInterval> forecastedStaffingPerSkill, DateTime[] timeSeries)
		{
			var forecastedStaffing = forecastedStaffingPerSkill
				.GroupBy(g => g.StartDateTime)
				.Select(s =>
				{
					var staffingIntervalModel = s.First();
					return new StaffingInterval
					{
						SkillId = staffingIntervalModel.SkillId,
						StartTime = staffingIntervalModel.StartDateTime,
						Agents = s.Sum(a => a.FStaff)
					};
				})
				.OrderBy(o => o.StartTime)
				.ToArray();

			if (timeSeries.Length == forecastedStaffing.Length)
				return forecastedStaffing.Select(x => (double?)x.Agents).ToArray();

			var forecastedStaffings = forecastedStaffing.ToLookup(x => x.StartTime);
			return timeSeries.Select(intervalStart => forecastedStaffings[intervalStart].FirstOrDefault()?.Agents).ToArray();
		}

		public double?[] ScheduledDataSeries(IList<SkillStaffingInterval> scheduledStaffing, DateTime[] timeSeries)
		{
			if (!scheduledStaffing.Any())
				return new double?[] { };

			var staffingIntervals = scheduledStaffing
				.GroupBy(x => x.StartDateTime)
				.Select(s => new staffingStartInterval
				{
					StartTime = s.Key,
					StaffingLevel = s.Sum(a => a.StaffingLevel)
				})
				.ToList();

			if (timeSeries.Length == staffingIntervals.Count)
				return staffingIntervals.Select(x => (double?)x.StaffingLevel).ToArray();

			List<double?> scheduledStaffingList = new List<double?>();
			foreach (var intervalStart in timeSeries)
			{
				var scheduledStaffingInterval = staffingIntervals.FirstOrDefault(x => x.StartTime == intervalStart);
				scheduledStaffingList.Add(scheduledStaffingInterval?.StaffingLevel);
			}
			return scheduledStaffingList.ToArray();
		}

		private class staffingStartInterval
		{
			public DateTime StartTime { get; set; }
			public double StaffingLevel { get; set; }
		}

		private static void calculateAbsoluteDifference(StaffingDataSeries dataSeries)
		{
			dataSeries.AbsoluteDifference = new double?[dataSeries.ForecastedStaffing.Length];
			for (var index = 0; index < dataSeries.ForecastedStaffing.Length; index++)
			{
				if (!dataSeries.ForecastedStaffing[index].HasValue) continue;

				if (dataSeries.ScheduledStaffing.Length == 0)
				{
					dataSeries.AbsoluteDifference[index] = -dataSeries.ForecastedStaffing[index];
					continue;
				}

				if (dataSeries.ScheduledStaffing[index].HasValue)
				{
					dataSeries.AbsoluteDifference[index] = Math.Round((double)dataSeries.ScheduledStaffing[index], 1) -
														   Math.Round((double)dataSeries.ForecastedStaffing[index], 1);
					dataSeries.AbsoluteDifference[index] = Math.Round((double)dataSeries.AbsoluteDifference[index], 1);
					dataSeries.ScheduledStaffing[index] = Math.Round((double)dataSeries.ScheduledStaffing[index], 1);
				}
				dataSeries.ForecastedStaffing[index] = Math.Round((double)dataSeries.ForecastedStaffing[index], 1);
			}
		}

		private  DateTime[] DataSeries(DateTime startDateTime, int minutesPerInterval, TimeZoneInfo timeZoneInfo)
		{
			var theMinTime = startDateTime;
			var theMaxTime = startDateTime.AddDays(1);

			var timeSeries = new List<DateTime>();

			var ambiguousTimes = new List<DateTime>();
			for (var time = theMinTime; time <= theMaxTime; time = time.AddMinutes(minutesPerInterval))
			{
				if (timeZoneInfo != null && timeZoneInfo.IsAmbiguousTime(time))
					ambiguousTimes.Add(time);

				if (!ambiguousTimes.IsEmpty() && timeZoneInfo != null && !timeZoneInfo.IsAmbiguousTime(time))
				{
					timeSeries.AddRange(ambiguousTimes);
					ambiguousTimes.Clear();
				}

				if (timeZoneInfo == null || !timeZoneInfo.IsInvalidTime(time))
					timeSeries.Add(time);
			}

			foreach (var timeSeriesTime in timeSeries)
			{
				if (timeZoneInfo != null && timeZoneInfo.IsAmbiguousTime(timeSeriesTime))
					ambiguousTimes.Add(timeSeriesTime);
			}

			return timeSeries.ToArray();
		}
	}
}
