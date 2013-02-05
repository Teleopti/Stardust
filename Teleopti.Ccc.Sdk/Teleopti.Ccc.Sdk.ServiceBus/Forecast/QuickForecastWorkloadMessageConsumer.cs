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
		private readonly WorkloadDayHelper _workloadDayHelper;

		public QuickForecastWorkloadMessageConsumer(ISkillDayRepository skillDayRepository,
													IOutlierRepository outlierRepository,
													IWorkloadRepository workloadRepository,
													IScenarioRepository scenarioRepository,
													IRepositoryFactory repositoryFactory,
													IUnitOfWorkFactory unitOfWorkFactory,
													IJobResultRepository jobResultRepository,
													IJobResultFeedback feedback,
													IMessageBroker messageBroker,
													WorkloadDayHelper workloadDayHelper)
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
			// måste reggas i autofac???
			_workloadDayHelper = workloadDayHelper;
		}

		public void Consume(QuickForecastWorkloadMessage message)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var jobResult = _jobResultRepository.Get(message.JobId);

				_feedback.SetJobResult(jobResult, _messageBroker);

				var workload = _workloadRepository.Get(message.WorkloadId);
				if (workload == null) return;

				//TriggerWorkloadStart();

				var skill = workload.Skill;
				var scenario = _scenarioRepository.Get(message.ScenarioId);
				//Load statistic data
				var statisticsHelper = new StatisticHelper(_repositoryFactory, unitOfWork);
				var daysWithValidatedStatistics =
					statisticsHelper.GetWorkloadDaysWithValidatedStatistics(message.StatisticPeriod,
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
				var calculator = new SkillDayCalculator(skill, skillDays.ToList(), message.StatisticPeriod);

				var workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(calculator.SkillDays, workload).OfType<ITaskOwner>().ToList();

				applyVolumes(workload, taskOwnerPeriod, workloadDays);

				//(Update templates for workload)
				if (message.UpdateStandardTemplates)
					updateStandardTemplates(workload, statisticsHelper, message.StatisticPeriod);

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
			var totalVolume = new TotalVolume();
			totalVolume.Create(taskOwnerPeriod, workloadDays,
							   new List<IVolumeYear> { volumeMonthYear, volumeWeekYear, volumeDayYear }, outliers,
							   0, 0, false, workload);
		}

		private void updateStandardTemplates(IWorkload workload, StatisticHelper statisticsHelper, DateOnlyPeriod statisticPeriod)
		{
			var workloadDayTemplateCalculator = new WorkloadDayTemplateCalculator(statisticsHelper, _outlierRepository);
			workloadDayTemplateCalculator.LoadWorkloadDayTemplates(new[] { statisticPeriod }, workload);
		}
	}
}