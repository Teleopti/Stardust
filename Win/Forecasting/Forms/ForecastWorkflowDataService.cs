using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public class ForecastWorkflowDataService : IForecastWorkflowDataService
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory = new RepositoryFactory();
		private readonly WorkloadDayHelper _workloadDayHelper = new WorkloadDayHelper();

        public ForecastWorkflowDataService(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public IList<IOutlier> InitializeOutliers(IWorkload workload)
        {
            IList<IOutlier> outliers;
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var outlierRepository = new OutlierRepository(uow);
                outliers = outlierRepository.FindByWorkload(workload);
            }
            
            return outliers;
        }

        public IScenario InitializeDefaultScenario()
        {
            IScenario defaultScenario;
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                defaultScenario = _repositoryFactory.CreateScenarioRepository(uow).LoadDefaultScenario();
            }
            return defaultScenario;
        }

        public void LoadWorkloadTemplates(IList<DateOnlyPeriod> dates, IWorkload workload)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var statHelper = new StatisticHelper(_repositoryFactory, uow);
                var wr = new WorkloadDayTemplateCalculator(statHelper, new OutlierRepository(uow));
                wr.LoadWorkloadDayTemplates(dates, workload);
            }
        }

        public DateOnlyPeriod InitializeWorkPeriod(IScenario scenario, IWorkload workload)
        {
            DateOnlyPeriod dateOnlyPeriod;
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var skillDayRepository = _repositoryFactory.CreateSkillDayRepository(uow);
                var lastSkillDayDate = skillDayRepository.FindLastSkillDayDate(workload, scenario);
                dateOnlyPeriod = new DateOnlyPeriod(lastSkillDayDate, new DateOnly(CultureInfo.CurrentCulture.Calendar.AddMonths(lastSkillDayDate.Date, 6)));
            }
            return dateOnlyPeriod;
        }


        public IList<ITaskOwner> GetWorkloadDays(IScenario scenario, IWorkload workload, DateOnlyPeriod period)
        {
            List<ITaskOwner> workloadDays;
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var skillDayRepository = _repositoryFactory.CreateSkillDayRepository(uow);
                var skillRepository = _repositoryFactory.CreateSkillRepository(uow);
				uow.Reassociate(workload.Skill);
                var skillDays = skillDayRepository.FindRange(period, skillRepository.LoadSkill(workload.Skill), scenario);
                skillDays = skillDayRepository.GetAllSkillDays(period, skillDays, workload.Skill, scenario, _ => {});

                //todo: Hmm, InitializeWorkPeriod, 
                var calculator = new SkillDayCalculator(workload.Skill, skillDays.ToList(), InitializeWorkPeriod(scenario, workload));
            	workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(calculator.SkillDays, workload).OfType<ITaskOwner>().ToList();
            }
            return workloadDays;
        }

        public void ReloadFilteredWorkloadTemplates(IList<DateOnlyPeriod> selectedDates, IList<DateOnly> filteredDates, IWorkload workload, int templateIndex)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var statHelper = new StatisticHelper(_repositoryFactory, uow);
                var wr = new WorkloadDayTemplateCalculator(statHelper, new OutlierRepository(uow));
				wr.LoadFilteredWorkloadDayTemplates(selectedDates, workload, filteredDates, templateIndex);
            }
        }

        public IList<ITaskOwner> GetWorkloadDaysWithStatistics(DateOnlyPeriod period, IWorkload workload, IScenario scenario, IList<IValidatedVolumeDay> validatedVolumeDays)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var statisticsHelper = new StatisticHelper(_repositoryFactory, uow);
                return statisticsHelper.GetWorkloadDaysWithValidatedStatistics(period, workload, validatedVolumeDays);
            }
        }

        public DateOnly FindLatestValidateDay(IWorkload workload)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var rep = _repositoryFactory.CreateValidatedVolumeDayRepository(uow);
                return rep.FindLastValidatedDay(workload);
            }
        }

        public IList<IWorkloadDayBase> LoadStatisticData(DateOnlyPeriod period, IWorkload workload)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var helper = new StatisticHelper(_repositoryFactory,uow);
                return helper.LoadStatisticData(period, workload);
            }
        }

        public IList<IValidatedVolumeDay> FindRange(DateOnlyPeriod dateTimePeriod, IWorkload workload, IList<IWorkloadDayBase> workloadDaysToValidate)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var rep = _repositoryFactory.CreateValidatedVolumeDayRepository(uow);
                var validatedVolumeDays = rep.FindRange(dateTimePeriod, workload);
                if (validatedVolumeDays == null || workloadDaysToValidate == null)
                    return new List<IValidatedVolumeDay>(0);

                var daysResult = rep.MatchDays(workload, workloadDaysToValidate.OfType<ITaskOwner>(), validatedVolumeDays);
                if (daysResult == null) return new List<IValidatedVolumeDay>(0);

                return daysResult.OfType<IValidatedVolumeDay>().ToList();
            }
        }

        public void SaveWorkflow(IWorkload workload, IList<ITaskOwner> workloadDays, IList<IValidatedVolumeDay> validatedVolumeDays)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                uow.Merge(workload);

                if (validatedVolumeDays != null)
                    SaveValidatedVolumeDays(validatedVolumeDays, uow);

                if (workloadDays != null)
                    SaveSkillDays(workloadDays, uow);

                uow.PersistAll();
            }
        }

        public void AddOutlier(IOutlier outlier)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var outlierRepository = new OutlierRepository(uow);
                outlierRepository.Add(outlier);
                uow.PersistAll();
            }
        }

        public void RemoveOutlier(IOutlier outlier)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var outlierRepository = new OutlierRepository(uow);
                
                outlierRepository.Remove(outlier);
                uow.PersistAll();
            }
        }

        public void EditOutlier(IOutlier outlier)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                uow.Merge(outlier);
                uow.PersistAll();
            }
        }

        private void SaveValidatedVolumeDays(IEnumerable<IValidatedVolumeDay> validatedVolumeDays, IUnitOfWork uow)
        {
            var rep = _repositoryFactory.CreateValidatedVolumeDayRepository(uow);
            foreach (var validatedVolumeDay in validatedVolumeDays)
            {
                rep.Add(validatedVolumeDay);
            }
        }

        private void SaveSkillDays(IEnumerable<ITaskOwner> workloadDays, IUnitOfWork uow)
        {
            var rep = new SkillDayRepository(uow);
            foreach (var taskOwner in workloadDays)
            {
                rep.Add((ISkillDay)((IWorkloadDay)taskOwner).Parent); //todo: why dont we use skilldays instead?
            }
        }
    }
}