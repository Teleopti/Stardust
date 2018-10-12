using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class StaffingViewModelCreator : IStaffingViewModelCreator
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IForecastedCallsProvider _forecastedCallsProvider;
		private readonly IRequiredStaffingProvider _requiredStaffingProvider;
		private readonly IIntradayQueueStatisticsLoader _intradayQueueStatisticsLoader;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly IScheduledStaffingToDataSeries _scheduledStaffingToDataSeries;
		private readonly IForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly IForecastedStaffingToDataSeries _forecastedStaffingToDataSeries;
		private readonly IReforecastedStaffingProvider _reforecastedStaffingProvider;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly IEmailBacklogProvider _emailBacklogProvider;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly ISkillTypeInfoProvider _skillTypeInfoProvider;

		public StaffingViewModelCreator(
			INow now,
			IUserTimeZone timeZone,
			IForecastedCallsProvider forecastedCallsProvider,
			IRequiredStaffingProvider requiredStaffingProvider,
			IIntradayQueueStatisticsLoader intradayQueueStatisticsLoader,
			IIntervalLengthFetcher intervalLengthFetcher,
			IScenarioRepository scenarioRepository,
			IScheduledStaffingProvider scheduledStaffingProvider,
			IScheduledStaffingToDataSeries scheduledStaffingToDataSeries,
			IForecastedStaffingProvider forecastedStaffingProvider,
			IForecastedStaffingToDataSeries forecastedStaffingToDataSeries,
			IReforecastedStaffingProvider reforecastedStaffingProvider,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			IEmailBacklogProvider emailBacklogProvider,
			ISkillDayLoadHelper skillDayLoadHelper,
			ISkillTypeInfoProvider skillTypeInfoProvider
			)
		{
			_now = now;
			_timeZone = timeZone;
			_forecastedCallsProvider = forecastedCallsProvider;
			_requiredStaffingProvider = requiredStaffingProvider;
			_intradayQueueStatisticsLoader = intradayQueueStatisticsLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
			_scenarioRepository = scenarioRepository;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_scheduledStaffingToDataSeries = scheduledStaffingToDataSeries;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_forecastedStaffingToDataSeries = forecastedStaffingToDataSeries;
			_reforecastedStaffingProvider = reforecastedStaffingProvider;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_emailBacklogProvider = emailBacklogProvider;
			_skillDayLoadHelper = skillDayLoadHelper;
			_skillTypeInfoProvider = skillTypeInfoProvider;
		}

		public IntradayStaffingViewModel Load_old(Guid[] skillIdList, int dayOffset)
		{
			var userDate = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).AddDays(dayOffset);
			return Load_old(skillIdList, new DateOnly(userDate));
		}
		public IntradayStaffingViewModel Load_old(Guid[] skillIdList, DateOnly? dateOnly = null,  bool useShrinkage = false)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			if (minutesPerInterval <= 0) throw new Exception($"IntervalLength is cannot be {minutesPerInterval}!");

			var timeZone = _timeZone.TimeZone();
			var userDateOnly = dateOnly ?? new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timeZone));

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skills = _supportedSkillsInIntradayProvider.GetSupportedSkills(skillIdList);
			if (!skills.Any())
				return new IntradayStaffingViewModel();
			var supportReforecastedAgents = skills.All(x => _skillTypeInfoProvider.GetSkillTypeInfo(x).SupportsReforecastedAgents);

			var skillDaysBySkills = _skillDayLoadHelper.LoadSchedulerSkillDays(new DateOnlyPeriod(userDateOnly, userDateOnly.AddDays(1)), skills, scenario);

			calculateForecastedAgentsForEmailSkills(useShrinkage, skillDaysBySkills);

			var forecastedStaffingModel = _forecastedStaffingProvider.StaffingPerSkill(skillDaysBySkills, minutesPerInterval, dateOnly, useShrinkage);

			var statisticsPerWorkloadInterval = _intradayQueueStatisticsLoader.LoadActualCallPerSkillInterval(skills, timeZone, userDateOnly);

			var latestStatsTime = getLastestStatsTime(statisticsPerWorkloadInterval);
			
			var forecastedCallsModel = _forecastedCallsProvider.Load(skillDaysBySkills, latestStatsTime, minutesPerInterval, dateOnly?.Date);

			var scheduledStaffingPerSkill = _scheduledStaffingProvider.StaffingPerSkill(skills.Any(x => x is MultisiteSkill) ? skillDaysBySkills.Keys.ToList() : skills, minutesPerInterval, dateOnly, useShrinkage);

			var timeSeries = TimeSeriesProvider.DataSeries(forecastedStaffingModel, scheduledStaffingPerSkill, minutesPerInterval, _timeZone.TimeZone());

			var workloadBacklog = _emailBacklogProvider.GetStatisticsBacklogByWorkload(skillDaysBySkills, statisticsPerWorkloadInterval, userDateOnly, minutesPerInterval, forecastedCallsModel.SkillDayStatsRange);

			var updatedForecastedSeries = supportReforecastedAgents
				? _reforecastedStaffingProvider.DataSeries(
					forecastedStaffingModel,
					statisticsPerWorkloadInterval,
					forecastedCallsModel.CallsPerSkill,
					latestStatsTime,
					minutesPerInterval,
					timeSeries
				)
				: new double?[0];

			var requiredStaffingPerSkill = _requiredStaffingProvider.Load(statisticsPerWorkloadInterval, 
				skills,
				skillDaysBySkills,
				forecastedStaffingModel, 
				TimeSpan.FromMinutes(minutesPerInterval),
				forecastedCallsModel.SkillDayStatsRange,
				workloadBacklog
			);


			var dataSeries = new  ApplicationLayer.ViewModels.StaffingDataSeries
			{
				Date = userDateOnly,
				Time = timeSeries,
				ForecastedStaffing = _forecastedStaffingToDataSeries.DataSeries(forecastedStaffingModel, timeSeries),
				UpdatedForecastedStaffing = updatedForecastedSeries,
				ActualStaffing = _requiredStaffingProvider.DataSeries(requiredStaffingPerSkill, latestStatsTime, minutesPerInterval, timeSeries),
				ScheduledStaffing = _scheduledStaffingToDataSeries.DataSeries(scheduledStaffingPerSkill, timeSeries)
			};
			return new IntradayStaffingViewModel
			{
				DataSeries = dataSeries,
				StaffingHasData = forecastedStaffingModel.Any()
			};
		}

		private void calculateForecastedAgentsForEmailSkills(bool useShrinkage, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays)
		{
			var scheduledStaffingPerSkill = new List<SkillStaffingIntervalLightModel>();
			var skillGroupsByResuolution = skillDays.Keys
				.Where(SkillTypesWithBacklog.IsBacklogSkillType)
				.GroupBy(x => x.DefaultResolution);
			foreach (var group in skillGroupsByResuolution)
			{
				var emailSkillsForOneResoultion = group.ToList();

				foreach (var skill in emailSkillsForOneResoultion)
				{
					var skillDaysEmail = skillDays[skill];
					foreach (var skillDay in skillDaysEmail)
					{
						var skillDayDate = skillDay.CurrentDate;
						scheduledStaffingPerSkill.AddRange(_scheduledStaffingProvider.StaffingPerSkill(emailSkillsForOneResoultion, group.Key, skillDayDate, useShrinkage));
						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							var intervalStart = skillStaffPeriod.Period.StartDateTime;
							var scheduledStaff =
								scheduledStaffingPerSkill.FirstOrDefault(
									x => x.Id == skill.Id.Value && x.StartDateTime == intervalStart);
							skillStaffPeriod.SetCalculatedResource65(0);
							if (scheduledStaff.StaffingLevel > 0)
								skillStaffPeriod.SetCalculatedResource65(scheduledStaff.StaffingLevel);
						}
					}
				}
			}
		}
		
		private static DateTime? getLastestStatsTime(IList<SkillIntervalStatistics> actualCallsPerSkillInterval)
		{
			return actualCallsPerSkillInterval.Any() ? (DateTime?)actualCallsPerSkillInterval.Max(d => d.StartTime) : null;
		}
	}
}