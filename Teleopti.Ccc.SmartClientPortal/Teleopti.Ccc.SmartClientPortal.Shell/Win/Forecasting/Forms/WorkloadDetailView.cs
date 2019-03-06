using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.Win.Forecasting.Forms;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class WorkloadDetailView : AbstractDetailView
    {
        private readonly IWorkload _workload;
        private TaskOwnerDayGridControl _taskOwnerDayGridControl;
	    private IStatisticHelper _statisticsHelper;

	    /// <summary>
        /// Initializes a new instance of the <see cref="WorkloadDetailView"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public WorkloadDetailView(IStatisticHelper statisticsHelper)
	    {
		    _statisticsHelper = statisticsHelper;
	    }

	    /// <summary>
        /// Initializes a new instance of the <see cref="WorkloadDetailView"/> class.
        /// </summary>
        /// <param name="skillDayCalculator">The skill day calculator.</param>
        /// <param name="workload">The workload.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="forecasterChartSetting">The forecaster chart setting.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        public WorkloadDetailView(SkillDayCalculator skillDayCalculator, IWorkload workload, ForecasterChartSetting forecasterChartSetting, IStatisticHelper statisticsHelper)
            : base(skillDayCalculator, forecasterChartSetting)
        {
            _workload = workload;
		    _statisticsHelper = statisticsHelper;

		    DetailViewLoad();
        }

        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>The type of the target.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        public override TemplateTarget TargetType
        {
            get
            {
                return TemplateTarget.Workload;
            }
        }

        /// <summary>
        /// Gets the workload.
        /// </summary>
        /// <value>The workload.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-03-28
        /// </remarks>
        public IWorkload Workload
        {
            get { return _workload; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void DetailViewLoad()
        {
			IList<IWorkloadDayBase> workloadDays = new WorkloadDayHelper().GetWorkloadDaysFromSkillDays(SkillDayCalculator.VisibleSkillDays, _workload);
            
            TaskOwnerHelper taskOwnerPeriodHelper = new TaskOwnerHelper(workloadDays);
            IList<TaskOwnerPeriod> taskOwnerWeeks = taskOwnerPeriodHelper.CreateWholeWeekTaskOwnerPeriods();
            IList<TaskOwnerPeriod> taskOwnerMonths = taskOwnerPeriodHelper.CreateWholeMonthTaskOwnerPeriods();
            IList<ITaskOwner> taskOwnerDays = taskOwnerPeriodHelper.TaskOwnerDays;

            string keyName = _workload.Id + ".Workload.";

            Control control = new TaskOwnerMonthGridControl(
                taskOwnerMonths.OfType<ITaskOwner>(),
                this, ForecasterChartSetting.GetChartSettings());
            control.Dock = DockStyle.Fill;
            TabControl.TabPages[0].Tag = WorkingInterval.Month;
            TabControl.TabPages[0].Controls.Add(control);
            GridCollection.Add(keyName + WorkingInterval.Month, (TeleoptiGridControl)control);

            control = new TaskOwnerWeekGridControl(
                taskOwnerWeeks.OfType<ITaskOwner>(),
                this, ForecasterChartSetting.GetChartSettings());
            control.Dock = DockStyle.Fill;
            TabControl.TabPages[1].Tag = WorkingInterval.Week;
            TabControl.TabPages[1].Controls.Add(control);
            GridCollection.Add(keyName + WorkingInterval.Week, (TeleoptiGridControl)control);

            _taskOwnerDayGridControl = new TaskOwnerDayGridControl(taskOwnerDays, taskOwnerPeriodHelper, this,
                                                                   ForecasterChartSetting.GetChartSettings(), _statisticsHelper);
            _taskOwnerDayGridControl.Create();
            _taskOwnerDayGridControl.Dock = DockStyle.Fill;
            _taskOwnerDayGridControl.TemplateSelected += taskOwnerDayGridControl_TemplateSelected;
            
            TabControl.TabPages[2].Tag = WorkingInterval.Day;
            TabControl.TabPages[2].Controls.Add(_taskOwnerDayGridControl);
            GridCollection.Add(keyName + WorkingInterval.Day, _taskOwnerDayGridControl);

            if (workloadDays.Count > 0)
            {
                CurrentDay = workloadDays[0].CurrentDate;
            }

            WorkloadIntradayGridControl workloadIntradayGridControl = new WorkloadIntradayGridControl(taskOwnerDays[0], 
                taskOwnerPeriodHelper, 
                _workload.Skill.TimeZone,
                _workload.Skill.DefaultResolution,
				this, ForecasterChartSetting.GetChartSettings(), _workload.Skill.SkillType, _statisticsHelper);
            workloadIntradayGridControl.Create(); //Inits the oject
            workloadIntradayGridControl.Dock = DockStyle.Fill;
            workloadIntradayGridControl.ModifyCells += workloadIntradayGridControl_ModifyCells;
            TabControl.TabPages[3].Tag = WorkingInterval.Intraday;
            TabControl.TabPages[3].Controls.Add(workloadIntradayGridControl);
            GridCollection.Add(keyName + WorkingInterval.Intraday, workloadIntradayGridControl);

            LoadOutliers();

            foreach (TabPageAdv tabPage in TabControl.TabPages)
            {
                foreach (ITaskOwnerGrid taskOwnerGrid in GetGridsInControl(tabPage))
                {
                    if (!taskOwnerGrid.HasColumns &&
                        ((WorkingInterval)tabPage.Tag) != WorkingInterval.Intraday)
                    {
                        TabControl.TabPages.Remove(tabPage);
                    }
                }
            }

            EntityEventAggregator.EntitiesNeedsRefresh += MainScreen_EntitiesNeedsRefresh;
            TabControl.ItemSize = new System.Drawing.Size(1, 0);
        }

        private void MainScreen_EntitiesNeedsRefresh(object sender, EntitiesUpdatedEventArgs e)
        {
            if (e.EntityType == typeof(Outlier))
            {
                LoadOutliers();
                _taskOwnerDayGridControl.RefreshGrid();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                EntityEventAggregator.EntitiesNeedsRefresh -= MainScreen_EntitiesNeedsRefresh;
            }
            base.Dispose(disposing);
        }

        private void LoadOutliers()
        {
            using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                OutlierRepository outlierRepository = OutlierRepository.DONT_USE_CTOR(unitOfWork);
                IList<IOutlier> outliers = outlierRepository.FindByWorkload(_workload);
                foreach (IOutlier outlier in outliers)
                {
                    unitOfWork.Refresh(outlier);
                }
                IDictionary<DateOnly, IOutlier> outliersByDate =
                    Outlier.GetOutliersByDates(
                        SkillDayCalculator.VisiblePeriod,
                        outliers);
                _taskOwnerDayGridControl.SetOutliers(outliersByDate);
            }
        }

        private void taskOwnerDayGridControl_TemplateSelected(object sender, TemplateEventArgs e)
        {
            TriggerTemplateSelected(e);
        }

        private void workloadIntradayGridControl_ModifyCells(object sender, ModifyCellEventArgs e)
        {
            IList<ITemplateTaskPeriod> dataPeriods = e.DataPeriods.OfType<ITemplateTaskPeriod>().ToList();
            if (dataPeriods.Count < 1) return;

            IWorkloadDayBase workloadDay = dataPeriods[0].Parent as IWorkloadDayBase;
            if (workloadDay == null) return;

            switch (e.ModifyCellOption)
            {
                case ModifyCellOption.Merge:
                    if (dataPeriods.Count == 1) return;
                    workloadDay.MergeTemplateTaskPeriods(dataPeriods);
                    break;
                case ModifyCellOption.Split:
                    workloadDay.SplitTemplateTaskPeriods(dataPeriods);
                    break;
            }

            ITaskOwnerGrid grid = sender as ITaskOwnerGrid;
            if (grid == null) return;
            grid.RefreshGrid();
        }
    }
}
