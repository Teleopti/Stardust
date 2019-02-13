using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class StaffingViewModelCreator : IStaffingViewModelCreator
	{
		private readonly ResourceCalculationUsingReadModels _resourceCalculationUsingReadModels;
		private readonly IUserTimeZone _timeZone;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly INow _now;

		public StaffingViewModelCreator(IUserTimeZone timeZone,IIntervalLengthFetcher intervalLengthFetcher, 
			ResourceCalculationUsingReadModels resourceCalculationUsingReadModels, INow now)
		{
			_resourceCalculationUsingReadModels = resourceCalculationUsingReadModels;
			_timeZone = timeZone;
			_intervalLengthFetcher = intervalLengthFetcher;
			_now = now;
		}

		public ScheduledStaffingViewModel Load(Guid[] skillIds, DateOnly? dateInLocalTime = null,
			bool useShrinkage = false)
		{
			var timeZone = _timeZone.TimeZone();
			var startOfDayLocal = dateInLocalTime?.Date ?? TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), timeZone).Date;

			var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayLocal.Date, timeZone);
		 	var endOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayLocal.AddDays(1).Date, timeZone);
			var minutesPerInterval = _intervalLengthFetcher.GetIntervalLength();
			if (minutesPerInterval <= 0) throw new Exception($"IntervalLength is cannot be {minutesPerInterval}!");

			var skillStaffingIntervals = _resourceCalculationUsingReadModels.LoadAndResourceCalculate(skillIds, startOfDayUtc, endOfDayUtc, useShrinkage, _timeZone);
			//var relevantPeriod = FindStartAndEndDateFromIntervals(skillStaffingIntervals);
			var relevantStartTime = skillStaffingIntervals.Any() ? skillStaffingIntervals.Min(s => s.StartDateTime) : startOfDayUtc;
			var relevantEndTime = skillStaffingIntervals.Any()
				? skillStaffingIntervals.Max(s => s.StartDateTime)
				: endOfDayUtc;
			var timeSeries = DataSeries(
				relevantStartTime,
				relevantEndTime,
				minutesPerInterval, _timeZone.TimeZone()).Where(x => x.Date == startOfDayLocal.Date).ToArray();
			//var timeSeries = DataSeries(startOfDayLocal,  minutesPerInterval, _timeZone.TimeZone()).Where(x => x.Date == startOfDayLocal.Date).ToArray();
			
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

		//private DateTimePeriod FindStartAndEndDateFromIntervals(IList<SkillStaffingInterval> skillStaffingIntervals)
		//{
		//	var period = new DateTimePeriod(skillStaffingIntervals.Min(s => s.StartDateTime), skillStaffingIntervals.Max(s => s.EndDateTime));
		//	return period;
		//}

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
						StartTime = TimeZoneHelper.ConvertFromUtc(staffingIntervalModel.StartDateTime, _timeZone.TimeZone()),
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
					StartTime = TimeZoneHelper.ConvertFromUtc(s.Key, _timeZone.TimeZone()),
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

		private  DateTime[] DataSeries(DateTime startDateTimeUtc, DateTime endDateTimeUtc, int minutesPerInterval, TimeZoneInfo timeZoneInfo)
		{
			var theMinTime = TimeZoneHelper.ConvertFromUtc(startDateTimeUtc, timeZoneInfo);
			var theMaxTime = TimeZoneHelper.ConvertFromUtc(endDateTimeUtc, timeZoneInfo);

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
