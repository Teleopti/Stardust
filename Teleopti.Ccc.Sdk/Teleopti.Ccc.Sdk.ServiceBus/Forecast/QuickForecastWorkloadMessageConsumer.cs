using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
	public class QuickForecastWorkloadMessageConsumer : ConsumerOf<QuickForecastWorkloadMessage>
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IOutlierRepository _outlierRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBroker _messageBroker;
		private readonly IWorkloadDayHelper _workloadDayHelper;
		private readonly IForecastClassesCreator _forecastClassesCreator;

		public QuickForecastWorkloadMessageConsumer(ISkillDayRepository skillDayRepository,
													IOutlierRepository outlierRepository,
													IWorkloadRepository workloadRepository,
													IScenarioRepository scenarioRepository,
													IRepositoryFactory repositoryFactory,
													IUnitOfWorkFactory unitOfWorkFactory,
													IJobResultRepository jobResultRepository,
													IJobResultFeedback feedback,
													IMessageBroker messageBroker,
													IWorkloadDayHelper workloadDayHelper,
													IForecastClassesCreator forecastClassesCreator)
		{
			_skillDayRepository = skillDayRepository;
			_outlierRepository = outlierRepository;
			_workloadRepository = workloadRepository;
			_scenarioRepository = scenarioRepository;
			_repositoryFactory = repositoryFactory;
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_workloadDayHelper = workloadDayHelper;
			_forecastClassesCreator = forecastClassesCreator;
		}

		public void Consume(QuickForecastWorkloadMessage message)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var workload = _workloadRepository.Get(message.WorkloadId);
				if (workload == null) return;

				var jobResult = _jobResultRepository.Get(message.JobId);

				_feedback.SetJobResult(jobResult, _messageBroker);

				//TriggerWorkloadStart();

				var skill = workload.Skill;
				var scenario = _scenarioRepository.Get(message.ScenarioId);
				//Load statistic data
				var statisticHelper = _forecastClassesCreator.CreateStatisticHelper(_repositoryFactory, unitOfWork);
				var daysWithValidatedStatistics =
					statisticHelper.GetWorkloadDaysWithValidatedStatistics(message.StatisticPeriod,
																			workload, scenario,
																			new List<IValidatedVolumeDay>());

				var outlierWorkloadDayFilter = new OutlierWorkloadDayFilter<ITaskOwner>(workload, _outlierRepository);
				var taskOwnerDaysWithoutOutliers =
					outlierWorkloadDayFilter.FilterStatistics(daysWithValidatedStatistics,
																new[] { message.StatisticPeriod });

				var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.Today, taskOwnerDaysWithoutOutliers, TaskOwnerPeriodType.Other);

				//Load (or create) workload days
				var skillDays = _skillDayRepository.FindRange(message.StatisticPeriod, skill, scenario);
				skillDays = _skillDayRepository.GetAllSkillDays(message.StatisticPeriod, skillDays, skill, scenario, false);
				var calculator = _forecastClassesCreator.CreateSkillDayCalculator(skill, skillDays.ToList(), message.StatisticPeriod);

				var workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(calculator.SkillDays, workload).OfType<ITaskOwner>().ToList();

				applyVolumes(workload, taskOwnerPeriod, workloadDays);

				//(Update templates for workload)
				if (message.UpdateStandardTemplates)
					updateStandardTemplates(workload, statisticHelper, message.StatisticPeriod, message.SmoothingStyle);

				//Create budget forecast (apply standard templates for all days in target)
				workload.SetDefaultTemplates(workloadDays);

				//Save!
				unitOfWork.PersistAll();

				//TriggerWorkloadEnd();
			}
		}

		private void applyVolumes(IWorkload workload, TaskOwnerPeriod taskOwnerPeriod, IList<ITaskOwner> workloadDays)
		{
			//Calculate indexes
			VolumeYear volumeMonthYear = new MonthOfYear(taskOwnerPeriod, new MonthOfYearCreator());
			VolumeYear volumeWeekYear = new WeekOfMonth(taskOwnerPeriod, new WeekOfMonthCreator());
			VolumeYear volumeDayYear = new DayOfWeeks(taskOwnerPeriod, new DaysOfWeekCreator());

			//Apply new volumes to workload days
			var outliers = _outlierRepository.FindByWorkload(workload);
			var totalVolume = _forecastClassesCreator.CreateTotalVolume();
			totalVolume.Create(taskOwnerPeriod, workloadDays,
							   new List<IVolumeYear> { volumeMonthYear, volumeWeekYear, volumeDayYear }, outliers,
							   0, 0, false, workload);
		}

		private void updateStandardTemplates(IWorkload workload, IStatisticHelper statisticsHelper, DateOnlyPeriod statisticPeriod, int smoothing)
		{
			var workloadDayTemplateCalculator = _forecastClassesCreator.CreateWorkloadDayTemplateCalculator(statisticsHelper, _outlierRepository);
			workloadDayTemplateCalculator.LoadWorkloadDayTemplates(new[] { statisticPeriod }, workload);

			if (smoothing > 1) //mer än None
			{
				for (var i = 0; i < 7; i++)
				{
					var template = (WorkloadDayTemplate)workload.GetTemplateAt(TemplateTarget.Workload, i);
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

	public interface IForecastClassesCreator
	{
		IWorkloadDayTemplateCalculator CreateWorkloadDayTemplateCalculator(IStatisticHelper statisticsHelper,
		                                                                                  IOutlierRepository outlierRepository);

		ITotalVolume CreateTotalVolume();

		ISkillDayCalculator CreateSkillDayCalculator(ISkill skill, IList<ISkillDay> skillDays,
		                                                            DateOnlyPeriod visiblePeriod);

		IStatisticHelper CreateStatisticHelper(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork);

	}

	public class ForecastClassesCreator : IForecastClassesCreator
	{
		public IWorkloadDayTemplateCalculator CreateWorkloadDayTemplateCalculator(IStatisticHelper statisticsHelper,
		                                                                         IOutlierRepository outlierRepository)
		{
			return  new WorkloadDayTemplateCalculator(statisticsHelper,outlierRepository);
		}

		public ITotalVolume CreateTotalVolume()
		{
			return new TotalVolume();
		}

		public ISkillDayCalculator CreateSkillDayCalculator(ISkill skill, IList<ISkillDay> skillDays,
		                                                   DateOnlyPeriod visiblePeriod)
		{
			return new SkillDayCalculator(skill,skillDays,visiblePeriod);
		}

		public IStatisticHelper CreateStatisticHelper(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork)
		{
			return new StatisticHelper(repositoryFactory,unitOfWork);
		}
	}
}