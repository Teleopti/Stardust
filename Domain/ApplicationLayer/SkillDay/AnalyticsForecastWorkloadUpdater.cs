using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SkillDay
{
	[DisabledBy(Toggles.WFM_AnalyticsForecastUpdater_80798)]
	public class AnalyticsForecastWorkloadUpdater :
		IHandleEvent<SkillDayChangedEvent>,
		IRunOnHangfire
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsForecastWorkloadUpdater));

		private static readonly int minutesPerDay = (int)TimeSpan.FromDays(1).TotalMinutes;
		private readonly IAnalyticsWorkloadRepository _analyticsWorkloadRepository;
		private readonly IAnalyticsScenarioRepository _analyticsScenarioRepository;
		private readonly IAnalyticsForecastWorkloadRepository _analyticsForecastWorkloadRepository;
		private readonly IAnalyticsIntervalRepository _analyticsIntervalRepository;
		private readonly AnalyticsDateMapper _analyticsDateMapper;

		public AnalyticsForecastWorkloadUpdater(IAnalyticsWorkloadRepository analyticsWorkloadRepository, IAnalyticsScenarioRepository analyticsScenarioRepository, IAnalyticsForecastWorkloadRepository analyticsForecastWorkloadRepository, IAnalyticsIntervalRepository analyticsIntervalRepository, AnalyticsDateMapper analyticsDateMapper, ISkillDayRepository skillDayRepository)
		{
			_analyticsWorkloadRepository = analyticsWorkloadRepository;
			_analyticsScenarioRepository = analyticsScenarioRepository;
			_analyticsForecastWorkloadRepository = analyticsForecastWorkloadRepository;
			_analyticsIntervalRepository = analyticsIntervalRepository;
			_analyticsDateMapper = analyticsDateMapper;
			_skillDayRepository = skillDayRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[Attempts(5)]
		public virtual void Handle(SkillDayChangedEvent @event)
		{
			var skillDay = _skillDayRepository.Get(@event.SkillDayId);
			if (skillDay == null)
			{
				logger.Debug($"Aborting because {typeof(ISkillDay)} {@event.SkillDayId} was not found in Application database.");
				return;
			}
			if (!skillDay.Scenario.EnableReporting)
			{
				logger.Debug($"Aborting because {typeof(ISkillDay)} {@event.SkillDayId} is not for a reportable scenario.");
				return;
			}
			var intervalsPerDay = analyticsIntervalsPerDay();
			var minutesPerInterval = minutesPerDay / intervalsPerDay;
			var analyticsDateIds = mapDates(new List<ISkillDay> { skillDay }, minutesPerInterval);
			handleForecasts(skillDay, analyticsDateIds, intervalsPerDay, minutesPerInterval);
		}

		[AnalyticsUnitOfWork]
		protected virtual int analyticsIntervalsPerDay()
		{
			return _analyticsIntervalRepository.IntervalsPerDay();
		}
		private AnalyticsScenario getAnalyticsScenario(ISkillDay skillDay)
		{
			var analyticsScenario = _analyticsScenarioRepository.Get(skillDay.Scenario.Id.GetValueOrDefault());
			if (analyticsScenario == null)
				throw new ScenarioMissingInAnalyticsException();
			return analyticsScenario;
		}

		private AnalyticsWorkload getAnalyticsWorkload(IWorkloadDayBase workloadDay)
		{
			var analyticsWorkload = _analyticsWorkloadRepository.GetWorkload(workloadDay.Workload.Id.GetValueOrDefault());
			if (analyticsWorkload == null)
				throw new WorkloadMissingInAnalyticsException();
			return analyticsWorkload;
		}

		[AnalyticsUnitOfWork]
		protected virtual IDictionary<DateOnly, int> mapDates(IEnumerable<ISkillDay> skillDays, int minutesPerInterval)
		{
			var fromDate = skillDays.Min(s => s.CurrentDate).AddDays(-1);
			var toDate = skillDays.Max(s => s.CurrentDate).AddDays(1);
			var analyticsDates = new Dictionary<DateOnly, int>();

			for (DateOnly theDate = fromDate; theDate <= toDate; theDate = theDate.AddDays(1))
			{
				_analyticsDateMapper.MapDateId(theDate, out var dateId);
				if (!analyticsDates.ContainsKey(theDate))
				{
					analyticsDates.Add(theDate, dateId);
				}
			}

			return analyticsDates;
		}

		[AnalyticsUnitOfWork]
		protected virtual void handleForecasts(ISkillDay skillDay, IDictionary<DateOnly, int> analyticsDateIds, int intervalsPerDay, int minutesPerInterval)
		{
			var newForecastWorkloads = createForecastWorkloads(skillDay, analyticsDateIds, intervalsPerDay, minutesPerInterval).ToList();
			var currentForecastWorkloads = getCurrentForecastWorkloads(skillDay, analyticsDateIds, intervalsPerDay, minutesPerInterval).ToList();
			var workloadsToRemove = currentForecastWorkloads.Except(newForecastWorkloads).ToArray();

			addOrUpdateWorkloads(newForecastWorkloads);
			removeWorkloads(workloadsToRemove);
		}

		private void addOrUpdateWorkloads(IEnumerable<AnalyticsForcastWorkload> workloads)
		{
			foreach (var workload in workloads)
			{
				_analyticsForecastWorkloadRepository.AddOrUpdate(workload);
			}
		}

		private void removeWorkloads(IEnumerable<AnalyticsForcastWorkload> workloads)
		{
			foreach (var workload in workloads)
			{
				_analyticsForecastWorkloadRepository.Delete(workload);
			}
		}

		private IEnumerable<AnalyticsForcastWorkload> getCurrentForecastWorkloads(ISkillDay skillDay,
			IDictionary<DateOnly,
			int> analyticsDateIds,
			int intervalsPerDay,
			int minutesPerInterval)
		{
			var currentDate = skillDay.CurrentDate.Date;
			var timezone = skillDay.Skill.TimeZone;

			var startDateUtc = TimeZoneHelper.ConvertToUtc(currentDate, timezone);
			var endDateUtc = TimeZoneHelper.ConvertToUtc(currentDate.AddDays(1).AddMinutes(-minutesPerInterval), timezone);

			var startIntervalId = new IntervalBase(startDateUtc, intervalsPerDay).Id;
			var endIntervalId = new IntervalBase(endDateUtc, intervalsPerDay).Id;

			var startDateId = analyticsDateIds[new DateOnly(startDateUtc.Date)];
			var endDateId = analyticsDateIds[new DateOnly(endDateUtc.Date)];

			var analyticsScenario = getAnalyticsScenario(skillDay);
			var currentForecastWorkloads = new List<AnalyticsForcastWorkload>();

			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				var analyticsWorkload = getAnalyticsWorkload(workloadDay);
				var workloads = _analyticsForecastWorkloadRepository.GetForecastWorkloads(analyticsWorkload.WorkloadId,
					analyticsScenario.ScenarioId, startDateId, endDateId, startIntervalId, endIntervalId);
				currentForecastWorkloads.AddRange(workloads);
			}

			return currentForecastWorkloads;
		}

		private IEnumerable<AnalyticsForcastWorkload> createForecastWorkloads(ISkillDay skillDay,
			IDictionary<DateOnly, int> analyticsDateIds,
			int intervalsPerDay,
			int minutesPerInterval)
		{
			var analyticsScenario = getAnalyticsScenario(skillDay);

			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				var analyticsWorkload = getAnalyticsWorkload(workloadDay);
				var templateTaskPeriodList = workloadDay.TemplateTaskPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));

				foreach (var taskPeriod in templateTaskPeriodList)
				{
					var dateId = analyticsDateIds[new DateOnly(taskPeriod.Period.StartDateTime)];
					var forecastedTalkTimeSeconds = taskPeriod.TotalAverageTaskTime.TotalSeconds * taskPeriod.TotalTasks;
					var forecastedAfterCallWorkSeconds = taskPeriod.TotalAverageAfterTaskTime.TotalSeconds * taskPeriod.TotalTasks;
					var forecastedCampaignTalkTimeSeconds = taskPeriod.CampaignTaskTime.Value * forecastedTalkTimeSeconds;
					var forecastedCampaignAfterCallWorkSeconds = taskPeriod.CampaignAfterTaskTime.Value * forecastedAfterCallWorkSeconds;
					var forecastedTalkTimeExclCampaignSeconds = taskPeriod.AverageTaskTime.TotalSeconds * taskPeriod.Tasks;
					var forecastedAfterCallWorkExclCampaignSeconds = taskPeriod.AverageAfterTaskTime.TotalSeconds * taskPeriod.Tasks;
					yield return new AnalyticsForcastWorkload
					{
						BusinessUnitId = analyticsWorkload.BusinessUnitId,
						WorkloadId = analyticsWorkload.WorkloadId,
						SkillId = analyticsWorkload.SkillId,
						DateId = dateId,
						ScenarioId = analyticsScenario.ScenarioId,
						IntervalId = new IntervalBase(taskPeriod.Period.StartDateTime, intervalsPerDay).Id,
						StartTime = taskPeriod.Period.StartDateTime,
						EndTime = taskPeriod.Period.EndDateTime,
						PeriodLengthMinutes = taskPeriod.Period.ElapsedTime().TotalMinutes,
						DatasourceUpdateDate = skillDay.UpdatedOn ?? DateTime.UtcNow,
						ForecastedAfterCallWorkExclCampaignSeconds = forecastedAfterCallWorkExclCampaignSeconds,
						ForecastedAfterCallWorkSeconds = forecastedAfterCallWorkSeconds,
						ForecastedBackofficeTasks = skillDay.Skill.SkillType.ForecastSource == ForecastSource.Backoffice ? taskPeriod.Tasks : 0,
						ForecastedCalls = taskPeriod.TotalTasks,
						ForecastedCallsExclCampaign = taskPeriod.Tasks,
						ForecastedCampaignAfterCallWorkSeconds = forecastedCampaignAfterCallWorkSeconds,
						ForecastedCampaignCalls = taskPeriod.CampaignTasks,
						ForecastedCampaignHandlingTimeSeconds = forecastedCampaignTalkTimeSeconds + forecastedCampaignAfterCallWorkSeconds,
						ForecastedCampaignTalkTimeSeconds = forecastedCampaignTalkTimeSeconds,
						ForecastedEmails = skillDay.Skill.SkillType.ForecastSource == ForecastSource.Email ? taskPeriod.Tasks : 0,
						ForecastedHandlingTimeExclCampaignSeconds = forecastedTalkTimeExclCampaignSeconds + forecastedAfterCallWorkExclCampaignSeconds,
						ForecastedHandlingTimeSeconds = forecastedTalkTimeSeconds + forecastedAfterCallWorkSeconds,
						ForecastedTalkTimeExclCampaignSeconds = forecastedTalkTimeExclCampaignSeconds,
						ForecastedTalkTimeSeconds = forecastedTalkTimeSeconds
					};
				}
			}
		}
	}
}