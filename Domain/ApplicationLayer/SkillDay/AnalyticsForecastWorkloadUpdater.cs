using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SkillDay
{
	public class AnalyticsForecastWorkloadUpdater :
		IHandleEvent<SkillDayChangedEvent>,
		IRunOnHangfire
	{
		private static readonly int minutesPerDay = (int)TimeSpan.FromDays(1).TotalMinutes;
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsForecastWorkloadUpdater));
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IAnalyticsWorkloadRepository _analyticsWorkloadRepository;
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IAnalyticsScenarioRepository _analyticsScenarioRepository;
		private readonly IAnalyticsForecastWorkloadRepository _analyticsForecastWorkloadRepository;
		private readonly IAnalyticsIntervalRepository _analyticsIntervalRepository;

		public AnalyticsForecastWorkloadUpdater(ISkillDayRepository skillDayRepository, IAnalyticsWorkloadRepository analyticsWorkloadRepository, IAnalyticsDateRepository analyticsDateRepository, IAnalyticsScenarioRepository analyticsScenarioRepository, IAnalyticsForecastWorkloadRepository analyticsForecastWorkloadRepository, IAnalyticsIntervalRepository analyticsIntervalRepository)
		{
			_skillDayRepository = skillDayRepository;
			_analyticsWorkloadRepository = analyticsWorkloadRepository;
			_analyticsDateRepository = analyticsDateRepository;
			_analyticsScenarioRepository = analyticsScenarioRepository;
			_analyticsForecastWorkloadRepository = analyticsForecastWorkloadRepository;
			_analyticsIntervalRepository = analyticsIntervalRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(SkillDayChangedEvent @event)
		{
			var skillDay = _skillDayRepository.Get(@event.SkillDayId);
			if (skillDay == null)
			{
				logger.Warn($"Aborting because {typeof(ISkillDay)} {@event.SkillDayId} was not found in Application database.");
				return;
			}
			if (!skillDay.Scenario.EnableReporting)
			{
				logger.Debug($"Aborting because {typeof(ISkillDay)} {@event.SkillDayId} is not for a reportable scenario.");
				return;
			}

			var newForecastWorkloads = createForecastWorkloads(skillDay).ToList();
			var currentForecastWorkloads = getCurrentForecastWorkloads(skillDay).ToList();
			var workloadsToRemove = currentForecastWorkloads.Where(x => !newForecastWorkloads.Contains(x)).ToList();

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

		private IEnumerable<AnalyticsForcastWorkload> getCurrentForecastWorkloads(ISkillDay skillDay)
		{
			var dateId = getAnalyticsDate(skillDay.CurrentDate.Date).DateId;
			var analyticsScenario = getAnalyticsScenario(skillDay);
			var currentForecastWorkloads = new List<AnalyticsForcastWorkload>();

			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				var analyticsWorkload = getAnalyticsWorkload(workloadDay);
				var workloads = _analyticsForecastWorkloadRepository.GetForecastWorkloads(analyticsWorkload.WorkloadId, dateId, analyticsScenario.ScenarioId);
				currentForecastWorkloads.AddRange(workloads);
			}

			return currentForecastWorkloads;
		}

		private IEnumerable<AnalyticsForcastWorkload> createForecastWorkloads(ISkillDay skillDay)
		{
			var analyticsScenario = getAnalyticsScenario(skillDay);
			var intervals = _analyticsIntervalRepository.IntervalsPerDay();
			var minutesPerInterval = minutesPerDay / intervals;

			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				var analyticsWorkload = getAnalyticsWorkload(workloadDay);
				var templateTaskPeriodList = workloadDay.TemplateTaskPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
				
				foreach (var taskPeriod in templateTaskPeriodList)
				{
					var analyticsDate = getAnalyticsDate(taskPeriod.Period.StartDateTime.Date);
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
						DateId = analyticsDate.DateId,
						ScenarioId = analyticsScenario.ScenarioId,
						IntervalId = new IntervalBase(taskPeriod.Period.StartDateTime, intervals).Id,
						StartTime = taskPeriod.Period.StartDateTime,
						EndTime = taskPeriod.Period.EndDateTime,
						PeriodLengthMinutes = taskPeriod.Period.EndDateTime.Subtract(taskPeriod.Period.StartDateTime).TotalMinutes,
						DatasourceUpdateDate = skillDay.UpdatedOn ?? DateTime.UtcNow,
						ForecastedAfterCallWorkExclCampaignSeconds = forecastedAfterCallWorkExclCampaignSeconds,
						ForecastedAfterCallWorkSeconds = forecastedAfterCallWorkSeconds,
						ForecastedBackofficeTasks = skillDay.Skill.SkillType.ForecastSource == ForecastSource.Backoffice ? taskPeriod.Tasks: 0,
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

		private AnalyticsScenario getAnalyticsScenario(ISkillDay skillDay)
		{
			var analyticsScenario = _analyticsScenarioRepository.Get(skillDay.Scenario.Id.GetValueOrDefault());
			if (analyticsScenario == null)
				throw new ScenarioMissingInAnalyticsException();
			return analyticsScenario;
		}

		private IAnalyticsDate getAnalyticsDate(DateTime date)
		{
			var analyticsDate = _analyticsDateRepository.Date(date);
			if (analyticsDate == null)
				throw new DateMissingInAnalyticsException(date);
			return analyticsDate;
		}

		private AnalyticsWorkload getAnalyticsWorkload(IWorkloadDayBase workloadDay)
		{
			var analyticsWorkload = _analyticsWorkloadRepository.GetWorkload(workloadDay.Workload.Id.GetValueOrDefault());
			if (analyticsWorkload == null)
				throw new WorkloadMissingInAnalyticsException();
			return analyticsWorkload;
		}
	}
}