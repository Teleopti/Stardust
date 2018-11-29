using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer.DTOs;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Intraday.Extensions;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer
{
	public interface IIntradayStaffingApplicationService
	{
		IntradayStaffingViewModel GenerateStaffingViewModel(Guid[] skillIdList, int dayOffset);
		IEnumerable<IntradayStaffingViewModel> GenerateStaffingViewModel(Guid[] skillIdList, DateOnlyPeriod dateOnlyPeriod, bool useShrinkage = false);
		IntradayStaffingViewModel GenerateStaffingViewModel(Guid[] skillIdList, DateOnly? dateInLocalTime = null, bool useShrinkage = false);

		IEnumerable<IntradayForcastedStaffingIntervalDTO> GetForecastedStaffing(IList<Guid> skillIdList, DateTime fromTime, DateTime toTime, TimeSpan resolution, bool useShrinkage);

		IList<IntradayScheduleStaffingIntervalDTO> GetScheduledStaffing(Guid[] skillIdList, DateTime fromUtc, DateTime toUtc, TimeSpan resolution, bool useShrinkage);
	}

	public class IntradayStaffingApplicationService : IIntradayStaffingApplicationService
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly ISkillTypeInfoProvider _skillTypeInfoProvider;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IIntradayQueueStatisticsLoader _intradayQueueStatisticsLoader;
		private readonly IEmailBacklogProvider _emailBacklogProvider;

		private readonly IIntradayReforecastingService _reforecastingService;
		private readonly IIntradayForecastingService _forecastingService;
		private readonly IIntradayStaffingService _staffingService;

		public IntradayStaffingApplicationService(
			INow now,
			IUserTimeZone timeZone,
			IIntervalLengthFetcher intervalLengthFetcher,
			IScenarioRepository scenarioRepository,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			ISkillTypeInfoProvider skillTypeInfoProvider,
			ISkillDayLoadHelper skillDayLoadHelper,
			IIntradayQueueStatisticsLoader intradayQueueStatisticsLoader,
			IEmailBacklogProvider emailBacklogProvider,

			IIntradayForecastingService forecastingService,
			IIntradayReforecastingService reforecastingService,
			IIntradayStaffingService staffingService
			)
		{
			_now = now;
			_timeZone = timeZone;
			_intervalLengthFetcher = intervalLengthFetcher;
			_scenarioRepository = scenarioRepository;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_skillTypeInfoProvider = skillTypeInfoProvider;
			_skillDayLoadHelper = skillDayLoadHelper;
			_intradayQueueStatisticsLoader = intradayQueueStatisticsLoader;
			_emailBacklogProvider = emailBacklogProvider;
			
			_forecastingService = forecastingService ?? throw new ArgumentNullException(nameof(forecastingService));
			_reforecastingService = reforecastingService ?? throw new ArgumentNullException(nameof(reforecastingService));
			_staffingService = staffingService ?? throw new ArgumentNullException(nameof(staffingService));
		}

		public IntradayStaffingViewModel GenerateStaffingViewModel(Guid[] skillIdList, int dayOffset)
		{
			var localTimeWithOffset = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).AddDays(dayOffset);
			return GenerateStaffingViewModel(skillIdList, new DateOnly(localTimeWithOffset));
		}
		public IEnumerable<IntradayStaffingViewModel> GenerateStaffingViewModel(Guid[] skillIdList, DateOnlyPeriod dateOnlyPeriod, bool useShrinkage = false)
		{
			return dateOnlyPeriod.DayCollection().Select(day => GenerateStaffingViewModel(skillIdList, day, useShrinkage)).ToList();
		}

		public IntradayStaffingViewModel GenerateStaffingViewModel(
			Guid[] skillIdList, 
			DateOnly? dateInLocalTime = null,
			bool useShrinkage = false)
		{
			var startOfDayLocal = dateInLocalTime?.Date ?? TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).Date;

			var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayLocal.Date, _timeZone.TimeZone());

			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			if (minutesPerInterval <= 0) throw new Exception($"IntervalLength is cannot be {minutesPerInterval}!");

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			if (!skills.Any())
				return new IntradayStaffingViewModel();

			var skillDaysBySkills = 
				_skillDayLoadHelper.LoadSchedulerSkillDays(new DateOnlyPeriod(new DateOnly(startOfDayUtc), new DateOnly(startOfDayUtc.AddDays(1))), skills, scenario);
			var skillDays = skillDaysBySkills.SelectMany(x => x.Value);
			
			var forecast = _forecastingService.GenerateForecast(skillDays, startOfDayUtc, startOfDayUtc.AddDays(1), TimeSpan.FromMinutes(minutesPerInterval), useShrinkage);
			if (!forecast.Any())
				return new IntradayStaffingViewModel();

			var statisticsPerWorkloadInterval = _intradayQueueStatisticsLoader.LoadSkillVolumeStatistics(skills, startOfDayUtc);

			var skillDayStatsRanges = skillDays.GetStatsRanges(startOfDayUtc, startOfDayUtc.AddDays(1), minutesPerInterval);
			 
			var skillsToGetSchemaFor = skillDaysBySkills.Keys.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToArray();

			var scheduledStaffingPerSkill = _staffingService
				.GetScheduledStaffing(skillsToGetSchemaFor, startOfDayUtc, startOfDayUtc.AddDays(1), TimeSpan.FromMinutes(minutesPerInterval), useShrinkage)
				.ToList();

			var scheduleStaffingMin = scheduledStaffingPerSkill.Any() ? scheduledStaffingPerSkill.Min(x => x.StartDateTime) : forecast.Min(x => x.StartTime);
			var scheduleStaffingMax = scheduledStaffingPerSkill.Any() ? scheduledStaffingPerSkill.Max(x => x.StartDateTime).Ticks : 0;
			var opensAtUtc = new DateTime(Math.Min(forecast.Min(x => x.StartTime).Ticks, scheduleStaffingMin.Ticks));
			var closeAtUtc = new DateTime(Math.Max(forecast.Max(x => x.StartTime).AddMinutes(minutesPerInterval).Ticks, scheduleStaffingMax));

			var timesUtc = this.GenerateTimeSeries(opensAtUtc, closeAtUtc, minutesPerInterval);

			var workloadBacklog = _emailBacklogProvider.GetStatisticsBacklogByWorkload(
				skillDaysBySkills,
				statisticsPerWorkloadInterval,
				new DateOnly(startOfDayUtc),
				minutesPerInterval,
				skillDayStatsRanges);

			var forecastedStaffing = forecast
				.Select(x => new StaffingInterval
				{
					SkillId = x.SkillId,
					StartTime = x.StartTime,
					Agents = x.Agents
				}).ToList();

			var requiredStaffingPerSkill = _staffingService.GetRequiredStaffing(
				statisticsPerWorkloadInterval,
				skills,
				skillDaysBySkills,
				forecastedStaffing,
				TimeSpan.FromMinutes(minutesPerInterval),
				skillDayStatsRanges,
				workloadBacklog
			);

			double?[] updatedForecastedSeries = new double?[] { };
			if (statisticsPerWorkloadInterval != null && statisticsPerWorkloadInterval.Any())
			{
				var latestStatisticsTimeUtc = statisticsPerWorkloadInterval
					.Max(x => x.StartTime)
					.AddMinutes(minutesPerInterval);

				var allSkillsReforecasted = new List<StaffingInterval>();
				var supportReforecastedAgents = skills.All(x => _skillTypeInfoProvider.GetSkillTypeInfo(x).SupportsReforecastedAgents);
				if (supportReforecastedAgents)
				{
					var forecastedVolume = forecast
						.Select(x => new SkillIntervalStatistics
						{
							SkillId = x.SkillId,
							StartTime = x.StartTime,
							AverageHandleTime = x.AverageHandleTime,
							Calls = x.Calls
						}).ToList();

					allSkillsReforecasted = _reforecastingService.ReforecastAllSkills(
						forecastedStaffing,
						forecastedVolume,
						statisticsPerWorkloadInterval,
						timesUtc,
						//_now.UtcDateTime())
						(_now.UtcDateTime() < latestStatisticsTimeUtc ? latestStatisticsTimeUtc : _now.UtcDateTime()))
						.ToList();
				}

				if (allSkillsReforecasted.Any())
				{
					var allSkillsReforecastedGroupedByTime = allSkillsReforecasted
						.GroupBy(h => h.StartTime);

					updatedForecastedSeries = timesUtc
						.OrderBy(x => x)
						.Select(x => allSkillsReforecastedGroupedByTime
							.Where(r => r.Key == x)
							.Select(v => (double?)v.Sum(a => a.Agents))
							.SingleOrDefault())
						.ToArray();
				}
			}


			var opensAtLocal = TimeZoneHelper.ConvertFromUtc(opensAtUtc, _timeZone.TimeZone());
			var closeAtLocal = TimeZoneHelper.ConvertFromUtc(closeAtUtc, _timeZone.TimeZone());

			var dataSeries = new StaffingDataSeries
			{
				Date = new DateOnly(startOfDayLocal),
				Time = this.GenerateTimeSeries(opensAtLocal, closeAtLocal, minutesPerInterval),
				ForecastedStaffing = timesUtc
					.Select(x => forecastedStaffing.Any(f => f.StartTime.Equals(x)) 
						? forecastedStaffing.Where(f => f.StartTime.Equals(x)).Sum(f => (double?)f.Agents)
						: null)
					.ToArray(),
				UpdatedForecastedStaffing = updatedForecastedSeries,
				ActualStaffing = statisticsPerWorkloadInterval.Any()
					? (timesUtc.Select(x => 
						requiredStaffingPerSkill.Any(s => s.StartTime.Equals(x))
						? requiredStaffingPerSkill.Where(a => a.StartTime.Equals(x)).Sum(a =>  (double ?) a.Agents)
						: null).ToArray())
					: new double?[]{}
				,
				ScheduledStaffing = scheduledStaffingPerSkill.Any()
					? (timesUtc.Select(x => 
						scheduledStaffingPerSkill.Any(s => s.StartDateTime.Equals(x))
						? scheduledStaffingPerSkill.Where(s => s.StartDateTime.Equals(x)).Sum(s => (double?) s.StaffingLevel)
						: null).ToArray())
					: new double?[]{}
			};
			return new IntradayStaffingViewModel
			{
				DataSeries = dataSeries,
				StaffingHasData = forecastedStaffing.Any()
			};

		}

		public DateTime[] GenerateTimeSeries(DateTime start, DateTime stop, int resolution)
		{
			var times = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(stop - start).TotalMinutes / resolution))
				.Select(offset => start.AddMinutes(offset * resolution))
				.ToArray();
			return times;
		}

		public IEnumerable<IntradayForcastedStaffingIntervalDTO> GetForecastedStaffing(
			IList<Guid> skillIdList,
			DateTime fromTimeUtc,
			DateTime toTimeUtc,
			TimeSpan resolution,
			bool useShrinkage)
		{
			if (resolution.TotalMinutes <= 0) throw new Exception($"IntervalLength is cannot be {resolution.TotalMinutes}!");

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList.ToArray());
			if (!skills.Any())
				return new List<IntradayForcastedStaffingIntervalDTO>();

			var skillDays =
				_skillDayLoadHelper.LoadSchedulerSkillDays(new DateOnlyPeriod(new DateOnly(fromTimeUtc), new DateOnly(toTimeUtc)), skills, scenario)
					.SelectMany(x => x.Value);

			var forecast = skillDays
				.SelectMany(x => x.SkillStaffPeriodViewCollection(resolution,useShrinkage).Select(i => new {SkillDay = x, StaffPeriod = i}))
				.Where(x => x.StaffPeriod.Period.StartDateTime >= fromTimeUtc && x.StaffPeriod.Period.EndDateTime <= toTimeUtc);

			if (!forecast.Any())
				return new List<IntradayForcastedStaffingIntervalDTO>();
		
			return forecast.Select(x =>
				new IntradayForcastedStaffingIntervalDTO
				{
					SkillId = x.SkillDay.Skill.Id.Value,
					StartTimeUtc = x.StaffPeriod.Period.StartDateTime,
					Agents = x.StaffPeriod.FStaff
				});
		}

		public IList<IntradayScheduleStaffingIntervalDTO> GetScheduledStaffing(
			Guid[] skillIdList, 
			DateTime fromTimeUtc, 
			DateTime toTimeUtc, 
			TimeSpan resolution, 
			bool useShrinkage)
		{
			//var fromTimeUtc = TimeZoneInfo.ConvertTimeToUtc(fromTimeLocal, _timeZone.TimeZone());
			//var toTimeUtc = TimeZoneInfo.ConvertTimeToUtc(toTimeLocal, _timeZone.TimeZone());

			var scheduledStaffing = _staffingService.GetScheduledStaffing(skillIdList, fromTimeUtc, toTimeUtc, resolution, useShrinkage);
			return scheduledStaffing.Select(x => new IntradayScheduleStaffingIntervalDTO
				{
					SkillId = x.Id,
					StartDateTimeUtc = x.StartDateTime,
					EndDateTimeUtc = x.EndDateTime,
					StaffingLevel = x.StaffingLevel
				})
				.ToList();
		}
	}
}
