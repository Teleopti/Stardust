using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class ForecastWorkflowPresenter
    {
        private readonly IForecastWorkflowView _view;
        private readonly ForecastWorkflowModel _model;
        private bool _locked;

        public ForecastWorkflowPresenter(IForecastWorkflowView view, ForecastWorkflowModel model)
        {
            _view = view;
            _model = model;
        }

        //Hmm, i don't like this Property, will try to refactor away, but one step at the time
        //The presenter should  be responsible for sending back to view
        public ForecastWorkflowModel Model
        {
            get { return _model; }
        }

        public bool Locked
        {
            get { return _locked; }
            set { _locked = value; }
        }

        public void Initialize()
        {
            _model.InitializeOutliers();   
            _model.InitializeDefaultScenario();
            _view.SetWorkloadName(_model.Workload.Name);
        }

        public void RemoveOutlier(IOutlier outlier)
        {
            _model.RemoveOutlier(outlier);
            _view.OutlierChanged(outlier);
        }

        public void AddOutlier(IOutlier outlier)
        {
            _model.AddOutlier(outlier);
            _view.OutlierChanged(outlier);
        }

        public void InitializeOutliersByWorkDate()
        {
            _model.InitializeOutliersByWorkDate();
        }

        public void LoadWorkloadDayTemplates(IList<DateOnlyPeriod> dates)
        {
            _model.LoadWorkloadDayTemplates(dates);
        }

        public void SaveWorkflow()
        {
            _model.SaveWorkflow();
        }

        public void InitializeWorkloadDaysWhenLocked()
        {
            _model.InitializeWorkloadDaysWhenLocked();
        }

        public void InitializeWorkPeriod(IScenario scenario)
        {
            _model.InitializeWorkPeriod(scenario);
        }

        public void InitializeWorkloadDays(IScenario scenario)
        {
            _model.InitializeWorkloadDays(scenario);
        }

        public void SetWorkPeriod(DateOnlyPeriod selectedDate)
        {
            _model.SetWorkPeriod(selectedDate);
        }

        public void ReloadFilteredWorkloadDayTemplates(IFilteredData filteredDates, int templateIndex)
        {
			_model.ReloadFilteredWorkloadDayTemplates(filteredDates, templateIndex);
        }

		public void ReloadHistoricalDataDepth(VolumeYear volumeYear, IList<DateOnlyPeriod> selectedDates)
		{
			var taskOwnerDays = new List<ITaskOwner>();
			InitializeWorkloadDaysWithStatistics(selectedDates);
			InitializeWorkloadDaysWithoutOutliers();

			taskOwnerDays.AddRange(_model.WorkloadDaysWithoutOutliers);

			var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.Today, taskOwnerDays.Distinct(), TaskOwnerPeriodType.Other);
			volumeYear.ReloadHistoricalDataDepth(taskOwnerPeriod);
		}

    	public void InitializeWorkloadDaysWithStatistics(IList<DateOnlyPeriod> period)
        {
            _model.InitializeWorkloadDaysWithStatistics(period);
        }

        public void InitializeWorkloadDaysWithoutOutliers()
        {
            _model.InitializeWorkloadDaysWithoutOutliers();
        }

        public void InitializeTaskOwnerPeriod(DateOnly dateTime, TaskOwnerPeriodType taskOwnerPeriodType)
        {
            _model.InitializeTaskOwnerPeriod(dateTime, taskOwnerPeriodType);
        }

        public IList<TaskOwnerPeriod> CreateYearTaskOwnerPeriods()
        {
            return _model.CreateYearTaskOwnerPeriods();
        }

        public void InitializeHistoricPeriod()
        {
            _model.InitializeLatestValidateDay(_locked);
        }

        public void InitializeValidatedVolumeDays()
        {
            _model.InitializeValidatedVolumeDays(); 
            
        }
        public void InitializeCompareHistoricPeriod(DateOnlyPeriod period)
        {
            _model.InitializeCompareHistoricPeriod(period);
        }

        public void InitializeDayOfWeeks()
        {
            _model.InitializeDayOfWeeks();
        }

        public void Initialize(IScenario scenario, DateOnlyPeriod period, IList<ISkillDay> skillDays)
        {
            _model.Initialize(scenario, period, skillDays);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IList<IWorkloadDayBase> GetWorkloadDaysForTemplatesWithStatistics()
        {
            return _model.GetWorkloadDaysForTemplatesWithStatistics();
        }

        public void SetSelectedHistoricTemplatePeriod(IList<DateOnlyPeriod> selectedDates)
        {
            _model.SetSelectedHistoricTemplatePeriod(selectedDates);
        }

        public void EditOutlier(IOutlier outlier)
        {
            _model.EditOutlier(outlier);
            _view.OutlierChanged(outlier);
        }
    }
}
