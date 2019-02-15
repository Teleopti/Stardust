using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Intraday.Extensions;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer
{
	public class IntradayStaffingApplicationService : IIntradayApplicationStaffingService
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly ISkillTypeInfoProvider _skillTypeInfoProvider;
		//private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IIntradayQueueStatisticsLoader _intradayQueueStatisticsLoader;
		private readonly IEmailBacklogProvider _emailBacklogProvider;

		private readonly IntradayReforecastingService _reforecastingService;
		private readonly IntradayForecastingService _forecastingService;
		private readonly IIntradayStaffingService _staffingService;
		private readonly ILoadSkillDaysWithPeriodFlexibility _loadSkillDaysWithPeriodFlexibility;

		public IntradayStaffingApplicationService(
			INow now,
			IUserTimeZone timeZone,
			IIntervalLengthFetcher intervalLengthFetcher,
			IScenarioRepository scenarioRepository,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			ISkillTypeInfoProvider skillTypeInfoProvider,
			//ISkillDayLoadHelper skillDayLoadHelper,
			IIntradayQueueStatisticsLoader intradayQueueStatisticsLoader,
			IEmailBacklogProvider emailBacklogProvider,
			IntradayForecastingService forecastingService,
			IntradayReforecastingService reforecastingService,
			IIntradayStaffingService staffingService, ILoadSkillDaysWithPeriodFlexibility loadSkillDaysWithPeriodFlexibility)
		{
			_now = now;
			_timeZone = timeZone;
			_intervalLengthFetcher = intervalLengthFetcher;
			_scenarioRepository = scenarioRepository;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_skillTypeInfoProvider = skillTypeInfoProvider;
			//_skillDayLoadHelper = skillDayLoadHelper;
			_intradayQueueStatisticsLoader = intradayQueueStatisticsLoader;
			_emailBacklogProvider = emailBacklogProvider;
			
			_forecastingService = forecastingService ?? throw new ArgumentNullException(nameof(forecastingService));
			_reforecastingService = reforecastingService ?? throw new ArgumentNullException(nameof(reforecastingService));
			_staffingService = staffingService ?? throw new ArgumentNullException(nameof(staffingService));
			_loadSkillDaysWithPeriodFlexibility = loadSkillDaysWithPeriodFlexibility;
		}


		public ScheduledStaffingViewModel GenerateStaffingViewModel(Guid[] skillIdList, int dayOffset)
		{
			var localTimeWithOffset = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).AddDays(dayOffset);
			return GenerateStaffingViewModel(skillIdList, new DateOnly(localTimeWithOffset));
		}

		public ScheduledStaffingViewModel GenerateStaffingViewModel(
			Guid[] skillIdList, 
			DateOnly? dateInLocalTime = null,
			bool useShrinkage = false)
		{
			var timeZone = _timeZone.TimeZone();
			var startOfDayLocal = dateInLocalTime?.Date ?? TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), timeZone).Date;

			var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayLocal.Date, timeZone);

			var minutesPerInterval = _intervalLengthFetcher.GetIntervalLength();
			if (minutesPerInterval <= 0) throw new Exception($"IntervalLength is cannot be {minutesPerInterval}!");

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			if (!skills.Any())
				return new ScheduledStaffingViewModel();

			//var skillDaysBySkills = 
			//	_skillDayLoadHelper.LoadSchedulerSkillDays(new DateOnlyPeriod(new DateOnly(startOfDayUtc), new DateOnly(startOfDayUtc.AddDays(1))), skills, scenario);
			var skillDaysBySkills =
				_loadSkillDaysWithPeriodFlexibility.Load(new DateOnlyPeriod(new DateOnly(startOfDayUtc), new DateOnly(startOfDayUtc.AddDays(1))), skills, scenario);
			var skillDays = skillDaysBySkills.SelectMany(x => x.Value);
			
			var forecast = _forecastingService.GenerateForecast(skillDays, startOfDayUtc, startOfDayUtc.AddDays(1), TimeSpan.FromMinutes(minutesPerInterval), useShrinkage);
			if (!forecast.Any())
				return new ScheduledStaffingViewModel();

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

			var timesUtc = generateTimeSeries(opensAtUtc, closeAtUtc, minutesPerInterval);

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
						}).ToArray();

					allSkillsReforecasted.Clear();
					allSkillsReforecasted.AddRange(_reforecastingService.ReforecastAllSkills(
						forecastedStaffing,
						forecastedVolume,
						statisticsPerWorkloadInterval,
						timesUtc,
						(_now.UtcDateTime() < latestStatisticsTimeUtc ? latestStatisticsTimeUtc : _now.UtcDateTime())));
				}

				if (allSkillsReforecasted.Any())
				{
					var allSkillsReforecastedGroupedByTime = allSkillsReforecasted
						.ToLookup(h => h.StartTime);

					updatedForecastedSeries = timesUtc
						.OrderBy(x => x)
						.Select(x => allSkillsReforecastedGroupedByTime[x].Any()
							? allSkillsReforecastedGroupedByTime[x]
								.Sum(v => (double?) v.Agents)
							: null)
						.ToArray();
				}
			}


			var opensAtLocal = TimeZoneHelper.ConvertFromUtc(opensAtUtc, timeZone);
			var closeAtLocal = TimeZoneHelper.ConvertFromUtc(closeAtUtc, timeZone);

			var dataSeries = new StaffingDataSeries
			{
				Date = new DateOnly(startOfDayLocal),
				Time = generateTimeSeries(opensAtLocal, closeAtLocal, minutesPerInterval),
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
			return new ScheduledStaffingViewModel
			{
				DataSeries = dataSeries,
				StaffingHasData = forecastedStaffing.Any()
			};

		}

		private DateTime[] generateTimeSeries(DateTime start, DateTime stop, int resolution)
		{
			var times = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(stop - start).TotalMinutes / resolution))
				.Select(offset => start.AddMinutes(offset * resolution))
				.ToArray();
			return times;
		}

		public IList<StaffingIntervalModel> GetForecastedStaffing(
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
				return new List<StaffingIntervalModel>();

			//var skillDays =
			//	_skillDayLoadHelper.LoadSchedulerSkillDays(new DateOnlyPeriod(new DateOnly(fromTimeUtc), new DateOnly(toTimeUtc)), skills, scenario)
			var skillDays =
				_loadSkillDaysWithPeriodFlexibility.Load(new DateOnlyPeriod(new DateOnly(fromTimeUtc), new DateOnly(toTimeUtc)), skills, scenario)
			.SelectMany(x => x.Value);

			var forecast = skillDays
				.SelectMany(x => x.SkillStaffPeriodViewCollection(resolution,useShrinkage).Select(i => new {SkillDay = x, StaffPeriod = i}))
				.Where(x => x.StaffPeriod.Period.StartDateTime >= fromTimeUtc && x.StaffPeriod.Period.EndDateTime <= toTimeUtc);

			if (!forecast.Any())
				return new List<StaffingIntervalModel>();

			return forecast.Select(x => new StaffingIntervalModel
				{
					StartTime = TimeZoneInfo.ConvertTimeFromUtc(x.StaffPeriod.Period.StartDateTime, _timeZone.TimeZone()),
					SkillId = x.SkillDay.Skill.Id.Value,
					Agents = x.StaffPeriod.FStaff
				}).ToList();
		}

		public IList<SkillStaffingIntervalLightModel> GetScheduledStaffing(
			Guid[] skillIdList, 
			DateTime fromTimeUtc, 
			DateTime toTimeUtc, 
			TimeSpan resolution, 
			bool useShrinkage)
		{
			var scheduledStaffing = _staffingService.GetScheduledStaffing(skillIdList, fromTimeUtc, toTimeUtc, resolution, useShrinkage);

			return scheduledStaffing.Select(x => new SkillStaffingIntervalLightModel
				{
					Id = x.Id,
					StartDateTime = TimeZoneInfo.ConvertTimeFromUtc(x.StartDateTime, _timeZone.TimeZone()),
					EndDateTime = TimeZoneInfo.ConvertTimeFromUtc(x.EndDateTime, _timeZone.TimeZone()),
					StaffingLevel = x.StaffingLevel
				})
				.ToList();
		}
	}

	
}
