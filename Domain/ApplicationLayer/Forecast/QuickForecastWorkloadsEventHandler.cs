using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.DayInMonthIndex;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	public class QuickForecastWorkloadsEventHandlerHangfire : IHandleEvent<QuickForecastWorkloadsEvent>, IRunOnHangfire
	{
		private readonly IJobResultFeedback _feedback;
		private readonly IForecastClassesCreator _forecastClassesCreator;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IMultisiteDayRepository _multisiteDayRepository;
		private readonly IOutlierRepository _outlierRepository;
		private readonly IValidatedVolumeDayRepository _rep;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IRepository<IMultisiteSkill> _skillRepository;
		private readonly IStatisticHelper _statisticHelper;
		private readonly IWorkloadDayHelper _workloadDayHelper;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public QuickForecastWorkloadsEventHandlerHangfire(IWorkloadRepository workloadRepository, IMultisiteDayRepository multisiteDayRepository, IOutlierRepository outlierRepository, ISkillDayRepository skillDayRepository,
														  IScenarioRepository scenarioRepository, IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IWorkloadDayHelper workloadDayHelper, IForecastClassesCreator forecastClassesCreator,
														  IStatisticHelper statisticHelper, IValidatedVolumeDayRepository rep, IMessageBrokerComposite messageBroker, IRepository<IMultisiteSkill> skillRepository, IEventPublisher eventPublisher, ICurrentUnitOfWork currentUnitOfWork)
		{
			_workloadRepository = workloadRepository;
			_multisiteDayRepository = multisiteDayRepository;
			_outlierRepository = outlierRepository;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_workloadDayHelper = workloadDayHelper;
			_forecastClassesCreator = forecastClassesCreator;
			_statisticHelper = statisticHelper;
			_rep = rep;
			_messageBroker = messageBroker;
			_skillRepository = skillRepository;
			_eventPublisher = eventPublisher;
			_currentUnitOfWork = currentUnitOfWork;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(QuickForecastWorkloadsEvent @event)
		{
			var targetPeriod = new DateOnlyPeriod(new DateOnly(@event.TargetPeriodStart), new DateOnly(@event.TargetPeriodEnd));
			var staticticPeriod = new DateOnlyPeriod(new DateOnly(@event.StatisticPeriodStart), new DateOnly(@event.StatisticPeriodEnd));
			var templatePeriod = new DateOnlyPeriod(new DateOnly(@event.TemplatePeriodStart), new DateOnly(@event.TemplatePeriodEnd));
			var jobResult = _jobResultRepository.Get(@event.JobId);
			if (jobResult == null || !@event.WorkloadIds.Any())
			{
				return;
			}
			if (!@event.CurrentWorkloadId.HasValue)
				@event.CurrentWorkloadId = @event.WorkloadIds.First();

			_feedback.SetJobResult(jobResult, _messageBroker);
			var remainingProgress = @event.IncreaseWith * 3;
			try
			{
				var workload = _workloadRepository.Get(@event.CurrentWorkloadId.Value);

				var calculator = getSkillDayCalculator(workload.Skill, targetPeriod, @event.ScenarioId);

				var stat = _statisticHelper.LoadStatisticData(staticticPeriod, workload);

				jobResult.AddDetail(new JobResultDetail(DetailLevel.Info, $"Loaded workload {workload.Name}", DateTime.UtcNow, null));
				_feedback.ReportProgress(@event.IncreaseWith, $"Loaded workload {workload.Name}");

				var validated = new List<IValidatedVolumeDay>();

				var validatedVolumeDays = _rep.FindRange(staticticPeriod, workload);
				if (validatedVolumeDays != null && stat != null)
				{
					_feedback.ReportProgress(@event.IncreaseWith, $"Found {validatedVolumeDays.Count} validated days.");
					var daysResult = _rep.MatchDays(workload, stat, validatedVolumeDays);

					if (daysResult != null)
					{
						validated = daysResult.OfType<IValidatedVolumeDay>().ToList();
					}
				}

				var daysWithValidatedStatistics =
					_statisticHelper.GetWorkloadDaysWithValidatedStatistics(staticticPeriod, workload, validated);
				if (!daysWithValidatedStatistics.Any())
				{
					// this never happens because we always get empty days back if we don't have statistcs, how should we check that?
					jobResult.AddDetail(new JobResultDetail(DetailLevel.Info,
						$"No statistics found for workload on {staticticPeriod}", DateTime.UtcNow, null));

					_feedback.ReportProgress(remainingProgress,
						$"No statistics found for workload on {staticticPeriod.ToShortDateString(CultureInfo.CurrentCulture)}");
					return;
				}
				jobResult.AddDetail(new JobResultDetail(DetailLevel.Info,
					$"{daysWithValidatedStatistics.Count} days with statistics loaded for workload on {staticticPeriod}",
					DateTime.UtcNow, null));

				_feedback.ReportProgress(@event.IncreaseWith,
					$"{daysWithValidatedStatistics.Count} days with statistics loaded for workload on {staticticPeriod.ToShortDateString(CultureInfo.CurrentCulture)}");
				remainingProgress -= @event.IncreaseWith;

				var outlierWorkloadDayFilter = new OutlierWorkloadDayFilter<ITaskOwner>(workload, _outlierRepository);
				var taskOwnerDaysWithoutOutliers =
					outlierWorkloadDayFilter.FilterStatistics(daysWithValidatedStatistics,
						new[] { staticticPeriod });

				var taskOwnerPeriod = _forecastClassesCreator.GetNewTaskOwnerPeriod(taskOwnerDaysWithoutOutliers);

				var workloadDays =
					_workloadDayHelper.GetWorkloadDaysFromSkillDays(calculator.SkillDays, workload).OfType<ITaskOwner>().ToList();
				jobResult.AddDetail(new JobResultDetail(DetailLevel.Info, $"Loaded skill days on {targetPeriod}", DateTime.UtcNow, null));
				applyVolumes(workload, taskOwnerPeriod, workloadDays, @event.UseDayOfMonth);

				//(Update templates for workload)
				updateStandardTemplates(workload, _statisticHelper, templatePeriod, @event.SmoothingStyle);

				//Create budget forecast (apply standard templates for all days in target)
				var helper = new TaskOwnerHelper(workloadDays);
				helper.BeginUpdate();
				workload.SetDefaultTemplates(workloadDays);
				helper.EndUpdate();

				jobResult.AddDetail(new JobResultDetail(DetailLevel.Info, $"Updated forecast for {workload.Name}", DateTime.UtcNow, null));
				_feedback.ReportProgress(@event.IncreaseWith, $"Updated forecast for {workload.Name}");
			}
			catch (Exception exception)
			{
				jobResult.AddDetail(new JobResultDetail(DetailLevel.Error, "Error occurred!", DateTime.UtcNow, exception));
				_feedback.ReportProgress(remainingProgress, $"Error occurred! {exception.Message}");
				return;
			}

			if (@event.CurrentWorkloadId != @event.WorkloadIds.Last())
			{
				var next = @event.WorkloadIds.SkipWhile(item => item != @event.CurrentWorkloadId) // Should never be duplicates
					.Skip(1)
					.First();
				@event.CurrentWorkloadId = next;
				_currentUnitOfWork.Current().AfterSuccessfulTx(() =>
				{
					_eventPublisher.Publish(@event);
				});
			}
			else
			{
				if (!jobResult.HasError())
				{
					jobResult.FinishedOk = true;
				}
			}
		}

		private ISkillDayCalculator getSkillDayCalculator(ISkill skill, DateOnlyPeriod targetPeriod, Guid scenarioId)
		{
			var scenario = _scenarioRepository.Get(scenarioId);
			var multisiteSkill = _skillRepository.Get(skill.Id.GetValueOrDefault());
			if (multisiteSkill != null)
			{
				var skillDays = _skillDayRepository.FindRange(targetPeriod, skill, scenario);
				skillDays = _skillDayRepository.GetAllSkillDays(targetPeriod, skillDays, skill, scenario, _skillDayRepository.AddRange);

				var allChildSkillDays = new Dictionary<IChildSkill, ICollection<ISkillDay>>();
				foreach (var childSkill in multisiteSkill.ChildSkills)
				{
					var childSkillDays = _skillDayRepository.FindRange(targetPeriod, childSkill, scenario);
					childSkillDays = _skillDayRepository.GetAllSkillDays(targetPeriod, childSkillDays, childSkill, scenario, _skillDayRepository.AddRange);
					allChildSkillDays.Add(childSkill, childSkillDays);
				}

				var multisiteDays = _multisiteDayRepository.FindRange(targetPeriod, multisiteSkill, scenario);
				multisiteDays = _multisiteDayRepository.GetAllMultisiteDays(targetPeriod, multisiteDays, multisiteSkill, scenario);

				return _forecastClassesCreator.CreateSkillDayCalculator(multisiteSkill, skillDays.ToList(),
					multisiteDays.ToList(), allChildSkillDays, targetPeriod);
			}
			else
			{
				var skillDays = _skillDayRepository.FindRange(targetPeriod, skill, scenario);
				skillDays = _skillDayRepository.GetAllSkillDays(targetPeriod, skillDays, skill, scenario,
					_skillDayRepository.AddRange);

				return _forecastClassesCreator.CreateSkillDayCalculator(skill, skillDays.ToList(), targetPeriod);
			}
		}

		private void applyVolumes(IWorkload workload, ITaskOwnerPeriod taskOwnerPeriod, IEnumerable<ITaskOwner> workloadDays, bool useDayOfMonth)
		{
			//Calculate indexes
			VolumeYear volumeMonthYear = new MonthOfYear(taskOwnerPeriod, new MonthOfYearCreator());
			VolumeYear volumeWeekYear = new WeekOfMonth(taskOwnerPeriod, new WeekOfMonthCreator());
			VolumeYear volumeDayYear = new DayOfWeeks(taskOwnerPeriod, new DaysOfWeekCreator());
			var indexes = new List<IVolumeYear> { volumeMonthYear, volumeWeekYear, volumeDayYear };
			if (useDayOfMonth)
			{
				indexes.Add(new DayInMonth(taskOwnerPeriod, new DayInMonthCreator()));
			}

			//Apply new volumes to workload days
			var outliers = _outlierRepository.FindByWorkload(workload);
			var totalVolume = _forecastClassesCreator.CreateTotalVolume();
			totalVolume.Create(taskOwnerPeriod, workloadDays, indexes, outliers, 0, 0, false, workload);
		}

		private void updateStandardTemplates(IWorkload workload, IStatisticHelper statisticsHelper, DateOnlyPeriod templatePeriod, int smoothing)
		{
			var workloadDayTemplateCalculator = _forecastClassesCreator.CreateWorkloadDayTemplateCalculator(statisticsHelper, _outlierRepository);
			workloadDayTemplateCalculator.LoadWorkloadDayTemplates(new[] { templatePeriod }, workload);

			if (smoothing <= 1) return;
			//mer än None
			for (var i = 0; i < 7; i++)
			{
				var template = (IWorkloadDayTemplate)workload.GetTemplateAt(TemplateTarget.Workload, i);
				template.SnapshotTemplateTaskPeriodList(TaskPeriodType.Tasks);
				template.DoRunningSmoothing(smoothing, TaskPeriodType.Tasks);
				template.SnapshotTemplateTaskPeriodList(TaskPeriodType.AverageTaskTime);
				template.DoRunningSmoothing(smoothing, TaskPeriodType.AverageTaskTime);
				template.SnapshotTemplateTaskPeriodList(TaskPeriodType.AverageAfterTaskTime);
				template.DoRunningSmoothing(smoothing, TaskPeriodType.AverageAfterTaskTime);
			}
		}
	}
}