using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public class QuickForecastCommand : IExecutableCommand
    {
        private readonly QuickForecastModel _quickForecastModel;
        private readonly ISkillDayRepository _skillDayRepository;
        private readonly IOutlierRepository _outlierRepository;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly WorkloadDayHelper _workloadDayHelper = new WorkloadDayHelper();

        public event EventHandler<CustomEventArgs<WorkloadModel>> WorkloadForecasting;
        public event EventHandler<CustomEventArgs<WorkloadModel>> WorkloadForecasted;

        public QuickForecastCommand(QuickForecastModel quickForecastModel, ISkillDayRepository skillDayRepository, IOutlierRepository outlierRepository, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _quickForecastModel = quickForecastModel;
            _skillDayRepository = skillDayRepository;
            _outlierRepository = outlierRepository;
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void Execute()
        {
            foreach (var selectedWorkload in _quickForecastModel.SelectedWorkloads)
            {
                using (IUnitOfWork unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    TriggerWorkloadStart(selectedWorkload);

                    var workload = selectedWorkload.Workload;
                    var skill = workload.Skill;

                    unitOfWork.Reassociate(skill);
                    unitOfWork.Reassociate(workload);

                    //Load statistic data
                    StatisticHelper statisticsHelper = new StatisticHelper(_repositoryFactory, unitOfWork);
                    IList<ITaskOwner> daysWithValidatedStatistics =
                        statisticsHelper.GetWorkloadDaysWithValidatedStatistics(_quickForecastModel.StatisticPeriod,
                                                                                workload, _quickForecastModel.Scenario,
                                                                                new List<IValidatedVolumeDay>());

                    var outlierWorkloadDayFilter = new OutlierWorkloadDayFilter<ITaskOwner>(workload,_outlierRepository);
                    IList<ITaskOwner> taskOwnerDaysWithoutOutliers =
                        outlierWorkloadDayFilter.FilterStatistics(daysWithValidatedStatistics,
                                                                  new[] {_quickForecastModel.StatisticPeriod});

                    TaskOwnerPeriod taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.Today, taskOwnerDaysWithoutOutliers, TaskOwnerPeriodType.Other);

                    //Load (or create) workload days
                    ICollection<ISkillDay> skillDays = _skillDayRepository.FindRange(_quickForecastModel.StatisticPeriod, skill, _quickForecastModel.Scenario);
                    skillDays = _skillDayRepository.GetAllSkillDays(_quickForecastModel.StatisticPeriod, skillDays, skill, _quickForecastModel.Scenario, false);
                    SkillDayCalculator calculator = new SkillDayCalculator(skill, skillDays.ToList(), _quickForecastModel.StatisticPeriod);

					var workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(calculator.SkillDays, workload).OfType<ITaskOwner>().ToList();

                    ApplyVolumes(workload, taskOwnerPeriod, workloadDays);

                    //(Update templates for workload)
                    UpdateStandardTemplates(workload, statisticsHelper);

                    //Create budget forecast (apply standard templates for all days in target)
                    selectedWorkload.Workload.SetDefaultTemplates(workloadDays);

                    //Save!
                    unitOfWork.PersistAll();

                    TriggerWorkloadEnd(selectedWorkload);
                }
            }
        }

        private void ApplyVolumes(IWorkload workload, TaskOwnerPeriod taskOwnerPeriod, IList<ITaskOwner> workloadDays)
        {
            //Calculate indexes
            VolumeYear volumeMonthYear = new MonthOfYear(taskOwnerPeriod, new MonthOfYearCreator());
            VolumeYear volumeWeekYear = new WeekOfMonth(taskOwnerPeriod, new WeekOfMonthCreator());
            VolumeYear volumeDayYear = new DayOfWeeks(taskOwnerPeriod, new DaysOfWeekCreator());

            //Apply new volumes to workload days
            var outliers = _outlierRepository.FindByWorkload(workload);
            TotalVolume totalVolume = new TotalVolume();
            totalVolume.Create(taskOwnerPeriod, workloadDays,
                               new List<IVolumeYear> {volumeMonthYear, volumeWeekYear, volumeDayYear}, outliers,
                               0, 0, false, workload);
        }

        private void TriggerWorkloadEnd(WorkloadModel selectedWorkload)
        {
            var workloadForecasted = WorkloadForecasted;
            if (workloadForecasted != null)
            {
                workloadForecasted.Invoke(this, new CustomEventArgs<WorkloadModel>(selectedWorkload));
            }
        }

        private void TriggerWorkloadStart(WorkloadModel selectedWorkload)
        {
            var workloadForecasting = WorkloadForecasting;
            if (workloadForecasting != null)
            {
                workloadForecasting.Invoke(this, new CustomEventArgs<WorkloadModel>(selectedWorkload));
            }
        }

        private void UpdateStandardTemplates(IWorkload workload, StatisticHelper statisticsHelper)
        {
            if (_quickForecastModel.UpdateStandardTemplates)
            {
                WorkloadDayTemplateCalculator workloadDayTemplateCalculator =
                    new WorkloadDayTemplateCalculator(statisticsHelper, _outlierRepository);
                workloadDayTemplateCalculator.LoadWorkloadDayTemplates(
                    new[] {_quickForecastModel.StatisticPeriod}, workload);
            }
        }
    }
}