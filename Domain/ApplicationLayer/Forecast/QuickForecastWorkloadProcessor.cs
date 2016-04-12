using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.DayInMonthIndex;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	public interface IQuickForecastWorkloadProcessor
	{
		void Handle(QuickForecastWorkloadEvent @event);
	}
	//NEED TO RE-WRITE THIS TO GET RID OF UNIT OF CRAP 
	public class QuickForecastWorkloadProcessor : IQuickForecastWorkloadProcessor
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IMultisiteDayRepository _multisiteDayRepository;
		private readonly IOutlierRepository _outlierRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IRepository<IMultisiteSkill> _skillRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IWorkloadDayHelper _workloadDayHelper;
		private readonly IForecastClassesCreator _forecastClassesCreator;
		private readonly IStatisticHelper _statisticHelper;
		private readonly IValidatedVolumeDayRepository _rep;

		public QuickForecastWorkloadProcessor(ISkillDayRepository skillDayRepository,
													IMultisiteDayRepository multisiteDayRepository,
													IOutlierRepository outlierRepository,
													IWorkloadRepository workloadRepository,
													IRepository<IMultisiteSkill> skillRepository,
													IScenarioRepository scenarioRepository,
													IJobResultRepository jobResultRepository,
													IJobResultFeedback feedback,
													IMessageBrokerComposite messageBroker,
													IWorkloadDayHelper workloadDayHelper,
													IForecastClassesCreator forecastClassesCreator, IStatisticHelper statisticHelper, IValidatedVolumeDayRepository rep, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_skillDayRepository = skillDayRepository;
			_multisiteDayRepository = multisiteDayRepository;
			_outlierRepository = outlierRepository;
			_workloadRepository = workloadRepository;
			_skillRepository = skillRepository;
			_scenarioRepository = scenarioRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_workloadDayHelper = workloadDayHelper;
			_forecastClassesCreator = forecastClassesCreator;
			_statisticHelper = statisticHelper;
			_rep = rep;
		}
		
		public void Handle(QuickForecastWorkloadEvent @event)
		{
            using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var jobResult = _jobResultRepository.Get(@event.JobId);
				if (jobResult == null) return;
				// more work not finished yet
				jobResult.FinishedOk = false;
				unitOfWork.PersistAll();
			}

			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var jobResult = _jobResultRepository.Get(@event.JobId);
				_feedback.SetJobResult(jobResult, _messageBroker);
				var remainingProgress = @event.IncreaseWith*3;
				try
				{
					var workload = _workloadRepository.Get(@event.WorkloadId);
					if (workload == null) return;
					
					jobResult.AddDetail(new JobResultDetail(DetailLevel.Info, "Loaded workload " + workload.Name, DateTime.UtcNow, null));

					_feedback.ReportProgress(@event.IncreaseWith, "Loaded workload " + workload.Name);
					remainingProgress -= @event.IncreaseWith;

					var skill = workload.Skill;

					var scenario = _scenarioRepository.Get(@event.ScenarioId);
					//Load statistic data
					var stat = _statisticHelper.LoadStatisticData(@event.StatisticPeriod, workload);
					
					var validated = new List<IValidatedVolumeDay>(0);

					var validatedVolumeDays = _rep.FindRange(@event.StatisticPeriod, workload);
					if (validatedVolumeDays != null && stat != null)
					{
						_feedback.ReportProgress(@event.IncreaseWith, "Found " + validatedVolumeDays.Count() + " validated days.");
						var daysResult = _rep.MatchDays(workload, stat, validatedVolumeDays);

						if (daysResult != null)
							validated = daysResult.OfType<IValidatedVolumeDay>().ToList();
					}

					var daysWithValidatedStatistics =
						_statisticHelper.GetWorkloadDaysWithValidatedStatistics(@event.StatisticPeriod,
																			   workload,
																			   validated);
					if (!daysWithValidatedStatistics.Any())
					{
						// this never happens because we always get empty days back if we don't have statistcs, how should we check that?
						jobResult.AddDetail(new JobResultDetail(DetailLevel.Info,
															"No statistics found for workload on " + @event.StatisticPeriod,
															DateTime.UtcNow, null));

						_feedback.ReportProgress(remainingProgress,
												 "No statistics found for workload on " +
												 @event.StatisticPeriod.ToShortDateString(CultureInfo.CurrentCulture));
						return;
					}
					jobResult.AddDetail(new JobResultDetail(DetailLevel.Info,
															daysWithValidatedStatistics.Count + " days with statistics loaded for workload on " + @event.StatisticPeriod,
															DateTime.UtcNow, null));

					_feedback.ReportProgress(@event.IncreaseWith,
											 daysWithValidatedStatistics.Count + " days with statistics loaded for workload on " +
											 @event.StatisticPeriod.ToShortDateString(CultureInfo.CurrentCulture));
					remainingProgress -= @event.IncreaseWith;

					var outlierWorkloadDayFilter = new OutlierWorkloadDayFilter<ITaskOwner>(workload, _outlierRepository);
					var taskOwnerDaysWithoutOutliers =
						outlierWorkloadDayFilter.FilterStatistics(daysWithValidatedStatistics,
																  new[] {@event.StatisticPeriod});

					var taskOwnerPeriod = _forecastClassesCreator.GetNewTaskOwnerPeriod(taskOwnerDaysWithoutOutliers);

					//Load (or create) workload days
					// Here we could split it in smaller chunks so we don't lock the tables too long
					ISkillDayCalculator calculator;
					var multisiteSkill = _skillRepository.Get(skill.Id.GetValueOrDefault());
					if (multisiteSkill!=null)
					{
						var skillDays = _skillDayRepository.FindRange(@event.TargetPeriod, skill, scenario);
						skillDays = _skillDayRepository.GetAllSkillDays(@event.TargetPeriod, skillDays, skill, scenario,
																		_skillDayRepository.AddRange);

						var allChildSkillDays = new Dictionary<IChildSkill, ICollection<ISkillDay>>();
						foreach (var childSkill in multisiteSkill.ChildSkills)
						{
							var childSkillDays = _skillDayRepository.FindRange(@event.TargetPeriod, childSkill, scenario);
							childSkillDays = _skillDayRepository.GetAllSkillDays(@event.TargetPeriod, childSkillDays, childSkill, scenario,
																			_skillDayRepository.AddRange);
							allChildSkillDays.Add(childSkill,childSkillDays);
						}

						var multisiteDays = _multisiteDayRepository.FindRange(@event.TargetPeriod, multisiteSkill, scenario);
						multisiteDays = _multisiteDayRepository.GetAllMultisiteDays(@event.TargetPeriod, multisiteDays, multisiteSkill,
																					scenario, true);

						calculator = _forecastClassesCreator.CreateSkillDayCalculator(multisiteSkill, skillDays.ToList(), multisiteDays.ToList(), allChildSkillDays, @event.TargetPeriod);
					}
					else
					{
						var skillDays = _skillDayRepository.FindRange(@event.TargetPeriod, skill, scenario);
						skillDays = _skillDayRepository.GetAllSkillDays(@event.TargetPeriod, skillDays, skill, scenario,
																		_skillDayRepository.AddRange);

						calculator = _forecastClassesCreator.CreateSkillDayCalculator(skill, skillDays.ToList(), @event.TargetPeriod);
					}
					
					var workloadDays =
						_workloadDayHelper.GetWorkloadDaysFromSkillDays(calculator.SkillDays, workload).OfType<ITaskOwner>().ToList();
					jobResult.AddDetail(new JobResultDetail(DetailLevel.Info, "Loaded skill days on " + @event.TargetPeriod,
															DateTime.UtcNow, null));
					applyVolumes(workload, taskOwnerPeriod, workloadDays, @event.UseDayOfMonth);

					//(Update templates for workload)
					updateStandardTemplates(workload, _statisticHelper, @event.TemplatePeriod, @event.SmoothingStyle);

					//Create budget forecast (apply standard templates for all days in target)
					var helper = new TaskOwnerHelper(workloadDays);
					helper.BeginUpdate();
					workload.SetDefaultTemplates(workloadDays);
					helper.EndUpdate();
					jobResult.AddDetail(new JobResultDetail(DetailLevel.Info, "Updated forecast for " + workload.Name, DateTime.UtcNow,
															null));
					_feedback.ReportProgress(@event.IncreaseWith, "Updated forecast for " + workload.Name);
					if (!jobResult.HasError())
						jobResult.FinishedOk = true;
				}
				catch (Exception exception)
				{
					jobResult.AddDetail(new JobResultDetail(DetailLevel.Error, "Error occurred!", DateTime.UtcNow, exception));
					_feedback.ReportProgress(remainingProgress, "Error occurred! " + exception.Message);
				}
				finally
				{
				//	//Save!
					unitOfWork.PersistAll();
				}
				
			}
		}

		private void applyVolumes(IWorkload workload, ITaskOwnerPeriod taskOwnerPeriod, IList<ITaskOwner> workloadDays, bool useDayOfMonth)
		{
			//Calculate indexes
			VolumeYear volumeMonthYear = new MonthOfYear(taskOwnerPeriod, new MonthOfYearCreator());
			VolumeYear volumeWeekYear = new WeekOfMonth(taskOwnerPeriod, new WeekOfMonthCreator());
			VolumeYear volumeDayYear = new DayOfWeeks(taskOwnerPeriod, new DaysOfWeekCreator());
			var indexes = new List<IVolumeYear> {volumeMonthYear, volumeWeekYear, volumeDayYear};
			if(useDayOfMonth)
				indexes.Add(new DayInMonth(taskOwnerPeriod,new DayInMonthCreator()));

			//Apply new volumes to workload days
			var outliers = _outlierRepository.FindByWorkload(workload);
			var totalVolume = _forecastClassesCreator.CreateTotalVolume();
			totalVolume.Create(taskOwnerPeriod, workloadDays, indexes, outliers,
							   0, 0, false, workload);
		}

		private void updateStandardTemplates(IWorkload workload, IStatisticHelper statisticsHelper, DateOnlyPeriod templatePeriod, int smoothing)
		{
			var workloadDayTemplateCalculator = _forecastClassesCreator.CreateWorkloadDayTemplateCalculator(statisticsHelper, _outlierRepository);
			workloadDayTemplateCalculator.LoadWorkloadDayTemplates(new[] { templatePeriod }, workload);

			if (smoothing > 1) //mer än None
			{
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

	public interface IForecastClassesCreator
	{
		IWorkloadDayTemplateCalculator CreateWorkloadDayTemplateCalculator(IStatisticHelper statisticsHelper,
																						  IOutlierRepository outlierRepository);

		ITotalVolume CreateTotalVolume();

		ISkillDayCalculator CreateSkillDayCalculator(ISkill skill, IList<ISkillDay> skillDays,
																	DateOnlyPeriod visiblePeriod);

		ISkillDayCalculator CreateSkillDayCalculator(IMultisiteSkill skill, IList<ISkillDay> skillDays, IList<IMultisiteDay> multisiteDays , IDictionary<IChildSkill,ICollection<ISkillDay>> childSkillDays,
																	DateOnlyPeriod visiblePeriod);


		//IStatisticHelper CreateStatisticHelper(IUnitOfWork unitOfWork);


		ITaskOwnerPeriod GetNewTaskOwnerPeriod(IList<ITaskOwner> taskOwnerDaysWithoutOutliers);
	}

	public class ForecastClassesCreator : IForecastClassesCreator
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public ForecastClassesCreator(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

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

		public ISkillDayCalculator CreateSkillDayCalculator(IMultisiteSkill skill, IList<ISkillDay> skillDays, IList<IMultisiteDay> multisiteDays, IDictionary<IChildSkill, ICollection<ISkillDay>> childSkillDays,
															DateOnlyPeriod visiblePeriod)
		{
			var multisiteSkillDayCalculator = new MultisiteSkillDayCalculator(skill, skillDays, multisiteDays, visiblePeriod);
			foreach (var pair in childSkillDays)
			{
				multisiteSkillDayCalculator.SetChildSkillDays(pair.Key,pair.Value.ToList());
			}
			multisiteSkillDayCalculator.InitializeChildSkills();
			return multisiteSkillDayCalculator;
		}

		//public IStatisticHelper CreateStatisticHelper(IUnitOfWork unitOfWork)
		//{
		//	return new StatisticHelper(_repositoryFactory,unitOfWork);
		//}

		public ITaskOwnerPeriod GetNewTaskOwnerPeriod(IList<ITaskOwner> taskOwnerDaysWithoutOutliers)
		{
			return new TaskOwnerPeriod(DateOnly.Today, taskOwnerDaysWithoutOutliers, TaskOwnerPeriodType.Other);
		}
	}
}