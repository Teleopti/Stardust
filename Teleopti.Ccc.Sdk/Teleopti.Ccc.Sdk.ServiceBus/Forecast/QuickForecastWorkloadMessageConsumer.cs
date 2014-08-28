using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.DayInMonthIndex;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
	public class QuickForecastWorkloadMessageConsumer : ConsumerOf<QuickForecastWorkloadMessage>
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IMultisiteDayRepository _multisiteDayRepository;
		private readonly IOutlierRepository _outlierRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IRepository<IMultisiteSkill> _skillRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBroker _messageBroker;
		private readonly IWorkloadDayHelper _workloadDayHelper;
		private readonly IForecastClassesCreator _forecastClassesCreator;

		public QuickForecastWorkloadMessageConsumer(ISkillDayRepository skillDayRepository,
		                                            IMultisiteDayRepository multisiteDayRepository,
		                                            IOutlierRepository outlierRepository,
		                                            IWorkloadRepository workloadRepository,
		                                            IRepository<IMultisiteSkill> skillRepository,
		                                            IScenarioRepository scenarioRepository,
		                                            IRepositoryFactory repositoryFactory,
		                                            ICurrentUnitOfWorkFactory unitOfWorkFactory,
		                                            IJobResultRepository jobResultRepository,
		                                            IJobResultFeedback feedback,
		                                            IMessageBroker messageBroker,
		                                            IWorkloadDayHelper workloadDayHelper,
		                                            IForecastClassesCreator forecastClassesCreator)
		{
			_skillDayRepository = skillDayRepository;
			_multisiteDayRepository = multisiteDayRepository;
			_outlierRepository = outlierRepository;
			_workloadRepository = workloadRepository;
			_skillRepository = skillRepository;
			_scenarioRepository = scenarioRepository;
			_repositoryFactory = repositoryFactory;
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_workloadDayHelper = workloadDayHelper;
			_forecastClassesCreator = forecastClassesCreator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Common.JobResultDetail.#ctor(Teleopti.Interfaces.Domain.DetailLevel,System.String,System.DateTime,System.Exception)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Consume(QuickForecastWorkloadMessage message)
		{
			using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var jobResult = _jobResultRepository.Get(message.JobId);
				if (jobResult == null) return;
				// more work not finished yet
				jobResult.FinishedOk = false;
				unitOfWork.PersistAll();
			}

			using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var jobResult = _jobResultRepository.Get(message.JobId);
				_feedback.SetJobResult(jobResult, _messageBroker);
				var remainingProgress = message.IncreaseWith*3;
				try
				{
					var workload = _workloadRepository.Get(message.WorkloadId);
					if (workload == null) return;
					
					jobResult.AddDetail(new JobResultDetail(DetailLevel.Info, "Loaded workload " + workload.Name, DateTime.UtcNow, null));

					_feedback.ReportProgress(message.IncreaseWith, "Loaded workload " + workload.Name);
					remainingProgress -= message.IncreaseWith;

					var skill = workload.Skill;

					var scenario = _scenarioRepository.Get(message.ScenarioId);
					//Load statistic data
					var statisticHelper = _forecastClassesCreator.CreateStatisticHelper(unitOfWork);
					var stat = statisticHelper.LoadStatisticData(message.StatisticPeriod, workload);
					
					var validated = new List<IValidatedVolumeDay>(0);

					var rep = _repositoryFactory.CreateValidatedVolumeDayRepository(unitOfWork);
					var validatedVolumeDays = rep.FindRange(message.StatisticPeriod, workload);
					if (validatedVolumeDays != null && stat != null)
					{
						_feedback.ReportProgress(message.IncreaseWith, "Found " + validatedVolumeDays.Count() + " validated days.");
						var daysResult = rep.MatchDays(workload, stat, validatedVolumeDays, false);

						if (daysResult != null)
							validated = daysResult.OfType<IValidatedVolumeDay>().ToList();
					}

					var daysWithValidatedStatistics =
						statisticHelper.GetWorkloadDaysWithValidatedStatistics(message.StatisticPeriod,
						                                                       workload, scenario,
						                                                       validated);
					if (!daysWithValidatedStatistics.Any())
					{
						// this never happens because we always get empty days back if we don't have statistcs, how should we check that?
						jobResult.AddDetail(new JobResultDetail(DetailLevel.Info,
															"No statistics found for workload on " + message.StatisticPeriod,
															DateTime.UtcNow, null));

						_feedback.ReportProgress(remainingProgress,
						                         "No statistics found for workload on " +
						                         message.StatisticPeriod.ToShortDateString(CultureInfo.CurrentCulture));
						return;
					}
					jobResult.AddDetail(new JobResultDetail(DetailLevel.Info,
															daysWithValidatedStatistics.Count + " days with statistics loaded for workload on " + message.StatisticPeriod,
					                                        DateTime.UtcNow, null));

					_feedback.ReportProgress(message.IncreaseWith,
					                         daysWithValidatedStatistics.Count + " days with statistics loaded for workload on " +
					                         message.StatisticPeriod.ToShortDateString(CultureInfo.CurrentCulture));
					remainingProgress -= message.IncreaseWith;

					var outlierWorkloadDayFilter = new OutlierWorkloadDayFilter<ITaskOwner>(workload, _outlierRepository);
					var taskOwnerDaysWithoutOutliers =
						outlierWorkloadDayFilter.FilterStatistics(daysWithValidatedStatistics,
						                                          new[] {message.StatisticPeriod});

					var taskOwnerPeriod = _forecastClassesCreator.GetNewTaskOwnerPeriod(taskOwnerDaysWithoutOutliers);

					//Load (or create) workload days
					// Here we could split it in smaller chunks so we don't lock the tables too long
					ISkillDayCalculator calculator;
					var multisiteSkill = _skillRepository.Get(skill.Id.GetValueOrDefault());
					if (multisiteSkill!=null)
					{
						var skillDays = _skillDayRepository.FindRange(message.TargetPeriod, skill, scenario);
						skillDays = _skillDayRepository.GetAllSkillDays(message.TargetPeriod, skillDays, skill, scenario,
																		_skillDayRepository.AddRange);

						var allChildSkillDays = new Dictionary<IChildSkill, ICollection<ISkillDay>>();
						foreach (var childSkill in multisiteSkill.ChildSkills)
						{
							var childSkillDays = _skillDayRepository.FindRange(message.TargetPeriod, childSkill, scenario);
							childSkillDays = _skillDayRepository.GetAllSkillDays(message.TargetPeriod, childSkillDays, childSkill, scenario,
																			_skillDayRepository.AddRange);
							allChildSkillDays.Add(childSkill,childSkillDays);
						}

						var multisiteDays = _multisiteDayRepository.FindRange(message.TargetPeriod, multisiteSkill, scenario);
						multisiteDays = _multisiteDayRepository.GetAllMultisiteDays(message.TargetPeriod, multisiteDays, multisiteSkill,
						                                                            scenario, true);

						calculator = _forecastClassesCreator.CreateSkillDayCalculator(multisiteSkill, skillDays.ToList(), multisiteDays.ToList(), allChildSkillDays, message.TargetPeriod);
					}
					else
					{
						var skillDays = _skillDayRepository.FindRange(message.TargetPeriod, skill, scenario);
						skillDays = _skillDayRepository.GetAllSkillDays(message.TargetPeriod, skillDays, skill, scenario,
																		_skillDayRepository.AddRange);

						calculator = _forecastClassesCreator.CreateSkillDayCalculator(skill, skillDays.ToList(), message.TargetPeriod);
					}
					
					var workloadDays =
						_workloadDayHelper.GetWorkloadDaysFromSkillDays(calculator.SkillDays, workload).OfType<ITaskOwner>().ToList();
					jobResult.AddDetail(new JobResultDetail(DetailLevel.Info, "Loaded skill days on " + message.TargetPeriod,
					                                        DateTime.UtcNow, null));
                    applyVolumes(workload, taskOwnerPeriod, workloadDays, message.UseDayOfMonth);

					//(Update templates for workload)
					updateStandardTemplates(workload, statisticHelper, message.TemplatePeriod, message.SmoothingStyle);

					//Create budget forecast (apply standard templates for all days in target)
				    var helper = new TaskOwnerHelper(workloadDays);
                    helper.BeginUpdate();
					workload.SetDefaultTemplates(workloadDays);
                    helper.EndUpdate();
					jobResult.AddDetail(new JobResultDetail(DetailLevel.Info, "Updated forecast for " + workload.Name, DateTime.UtcNow,
					                                        null));
					_feedback.ReportProgress(message.IncreaseWith, "Updated forecast for " + workload.Name);
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
					//Save!
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


		IStatisticHelper CreateStatisticHelper(IUnitOfWork unitOfWork);


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

		public IStatisticHelper CreateStatisticHelper(IUnitOfWork unitOfWork)
		{
			return new StatisticHelper(_repositoryFactory,unitOfWork);
		}

		public ITaskOwnerPeriod GetNewTaskOwnerPeriod(IList<ITaskOwner> taskOwnerDaysWithoutOutliers)
		{
			return new TaskOwnerPeriod(DateOnly.Today, taskOwnerDaysWithoutOutliers, TaskOwnerPeriodType.Other);
		}
	}
}