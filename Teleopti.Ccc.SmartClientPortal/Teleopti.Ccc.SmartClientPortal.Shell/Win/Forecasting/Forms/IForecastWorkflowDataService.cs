using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public interface IForecastWorkflowDataService
    {
        IList<IOutlier> InitializeOutliers(IWorkload workload);
        IScenario InitializeDefaultScenario();
        void LoadWorkloadTemplates(IList<DateOnlyPeriod> dates, IWorkload workload);
        DateOnlyPeriod InitializeWorkPeriod(IScenario scenario, IWorkload workload);
        IList<ITaskOwner> GetWorkloadDays(IScenario scenario, IWorkload workload, DateOnlyPeriod period);
        void ReloadFilteredWorkloadTemplates(IList<DateOnlyPeriod> selectedDates, IList<DateOnly> filteredDates, IWorkload workload, int templateIndex);
        IList<ITaskOwner> GetWorkloadDaysWithStatistics(DateOnlyPeriod period, IWorkload workload, IScenario scenario, IList<IValidatedVolumeDay> validatedVolumeDays);
        DateOnly FindLatestValidateDay(IWorkload workload);
        IList<IWorkloadDayBase> LoadStatisticData(DateOnlyPeriod period, IWorkload workload);
        IList<IValidatedVolumeDay> FindRange(DateOnlyPeriod dateTimePeriod, IWorkload workload, IList<IWorkloadDayBase> workloadDaysToValidate);
        void SaveWorkflow(IWorkload workload, IList<ITaskOwner> workloadDays, IList<IValidatedVolumeDay> validatedVolumeDays);
        void AddOutlier(IOutlier outlier);
        void RemoveOutlier(IOutlier outlier);
        void EditOutlier(IOutlier outlier);
    }
}