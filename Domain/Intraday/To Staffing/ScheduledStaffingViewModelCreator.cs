using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class ScheduledStaffingViewModelCreator
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IScheduledStaffingToDataSeries _scheduledStaffingToDataSeries;
		private readonly IForecastedStaffingToDataSeries _forecastedStaffingToDataSeries;
		
		private readonly IIntradayStaffingApplicationService _intradayStaffingApplicationService;

		public ScheduledStaffingViewModelCreator(
			INow now,
			IUserTimeZone timeZone,
			IIntervalLengthFetcher intervalLengthFetcher,
			IScheduledStaffingToDataSeries scheduledStaffingToDataSeries,
			IForecastedStaffingToDataSeries forecastedStaffingToDataSeries,
			IIntradayStaffingApplicationService intradayStaffingApplicationService
		)
		{
			_now = now;
			_timeZone = timeZone;
			_intervalLengthFetcher = intervalLengthFetcher;
			_scheduledStaffingToDataSeries = scheduledStaffingToDataSeries;
			_forecastedStaffingToDataSeries = forecastedStaffingToDataSeries;
			
			_intradayStaffingApplicationService = intradayStaffingApplicationService ?? throw new ArgumentNullException(nameof(intradayStaffingApplicationService));
		}

		public ScheduledStaffingViewModel Load(Guid[] skillIdList, DateOnly? dateInLocalTime = null, bool useShrinkage = false)
		{
			var startOfDayLocal = dateInLocalTime?.Date ?? TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).Date;

			var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayLocal.Date, _timeZone.TimeZone());

			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			if (minutesPerInterval <= 0) throw new Exception($"IntervalLength is cannot be {minutesPerInterval}!");

			var forecastedStaffing = _intradayStaffingApplicationService.GetForecastedStaffing(
				skillIdList, 
				startOfDayUtc, 
				startOfDayUtc.AddDays(1).AddHours(1),
				TimeSpan.FromMinutes(minutesPerInterval), 
				useShrinkage)
				.Select(x => new StaffingIntervalModel
				{
					StartTime = TimeZoneInfo.ConvertTimeFromUtc(x.StartTimeUtc, _timeZone.TimeZone()),
					SkillId = x.SkillId,
					Agents = x.Agents
				})
				.ToList();

			var scheduledStaffingPerSkill = _intradayStaffingApplicationService
				.GetScheduledStaffing(
					skillIdList, 
					startOfDayUtc, 
					startOfDayUtc.AddDays(1).AddHours(1), 
					TimeSpan.FromMinutes(minutesPerInterval), 
					useShrinkage)
				.Select(x => new SkillStaffingIntervalLightModel
				{
					Id = x.SkillId,
					StartDateTime = TimeZoneInfo.ConvertTimeFromUtc(x.StartDateTimeUtc, _timeZone.TimeZone()),
					EndDateTime = TimeZoneInfo.ConvertTimeFromUtc(x.EndDateTimeUtc, _timeZone.TimeZone()),
					StaffingLevel = x.StaffingLevel
				})
				.ToList();				

			var timeSeries = TimeSeriesProvider.DataSeries(forecastedStaffing, scheduledStaffingPerSkill, minutesPerInterval, _timeZone.TimeZone()).Where(x => x.Date == startOfDayLocal.Date).ToArray();

			var dataSeries = new StaffingDataSeries
			{
				Date = new DateOnly(startOfDayLocal),
				Time = timeSeries,
				ForecastedStaffing = _forecastedStaffingToDataSeries.DataSeries(forecastedStaffing, timeSeries),
				ScheduledStaffing = _scheduledStaffingToDataSeries.DataSeries(scheduledStaffingPerSkill, timeSeries)
			};
			calculateAbsoluteDifference(dataSeries);
			return new ScheduledStaffingViewModel
			{
				DataSeries = dataSeries,
				StaffingHasData = forecastedStaffing.Any()
			};
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
					dataSeries.AbsoluteDifference[index] = Math.Round((double) dataSeries.AbsoluteDifference[index],1);
					dataSeries.ScheduledStaffing[index] = Math.Round((double)dataSeries.ScheduledStaffing[index], 1);
				}
				dataSeries.ForecastedStaffing[index] = Math.Round((double) dataSeries.ForecastedStaffing[index], 1);
			}
		}
	}
}