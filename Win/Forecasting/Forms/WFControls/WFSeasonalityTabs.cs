using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Win.Forecasting.Forms.SeasonPages;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WFControls
{
    /// <summary>
    /// presents the seasonality tabs
    /// </summary>
    /// <remarks>
    /// Created by: östenp
    /// Created date: 2/19/2008
    /// </remarks>
    public partial class WFSeasonalityTabs : WFBaseControl
    {
        private WorkflowTotalView _workflowTotalView;
        private WorkflowTrendView _workflowTrendView;

        private Percent _trendPercent = new Percent(0);
        private bool _useTrend;
        private bool _changes;

        public WFSeasonalityTabs()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public bool GoToNextTab()
        {
            int i = tabSeasonality.SelectedIndex;//old index
            tabSeasonality.SelectedTab = TabHandler.TabForward(tabSeasonality);
            return i != tabSeasonality.SelectedIndex;//if the index has changed, focus should stay here
        }

        protected override void SetColors()
        {
            BrushInfo myBrush = ColorHelper.ControlGradientPanelBrush();
            tabSeasonality.BackColor = myBrush.BackColor;
            tabPageDayOfWeek.BackColor = myBrush.BackColor;
            tabPageWeekOfMonth.BackColor = myBrush.BackColor;
            tabPageMonthOfYear.BackColor = myBrush.BackColor;
            tabPageTotal.BackColor = myBrush.BackColor;

            tabSeasonality.TabPanelBackColor = ColorHelper.TabBackColor();
        }

        public bool GoToPreviousTab()
        {//it is not pretty but it works 
            int i = tabSeasonality.SelectedIndex;//old index
            tabSeasonality.SelectedTab = TabHandler.TabBack(tabSeasonality);
            return i != tabSeasonality.SelectedIndex;//if the index has changed, focus should stay here
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Presenter.SetWorkPeriod(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));
            Presenter.InitializeOutliersByWorkDate();

            var dataSummary = new WorkflowDataSummaryView(this);
            dataSummary.Dock = DockStyle.Fill;
            tabPageDataSummary.Controls.Add(dataSummary);
            enableTabs(false);

            if (tabSeasonality.SelectedTab.Controls.Count > 0)
                tabSeasonality.SelectedTab.Controls[0].Select();
        }

        protected override void OnOutlierNeedRefresh(IOutlier outlier)
        {
            if (_workflowTotalView == null) return;
            _workflowTotalView.RefreshOutlier(outlier);
        }

        private void enableTabs(bool enabled)
        {
            tabPageDayOfWeek.TabEnabled = enabled;
            tabPageWeekOfMonth.TabEnabled = enabled;
            tabPageMonthOfYear.TabEnabled = enabled;
            tabPageTotal.TabEnabled = enabled;
            tabPageTrend.TabEnabled = enabled;
        }

        public void ReloadHistoricalDataDepth(VolumeYear volumeYear, IList<DateOnlyPeriod> selectedDates)
        {
            Presenter.ReloadHistoricalDataDepth(volumeYear, selectedDates);
            _changes = true;
        }

        public void ReloadHistoricalDataDepth(IList<DateOnlyPeriod> selectedDates)
        {
            Presenter.ReloadHistoricalDataDepth(selectedDates);

            if (Presenter.Model.WorkloadDaysWithStatistics.Count > 0)
            {
                enableTabs(true);
                createTabs(selectedDates);
            }
        }

        private void createTabs(IList<DateOnlyPeriod> selectedDates)
        {
            Presenter.InitializeTaskOwnerPeriod(DateOnly.Today, TaskOwnerPeriodType.Other);

            VolumeYear volumeMonthYear = new MonthOfYear(Presenter.Model.TaskOwnerPeriod, new MonthOfYearCreator()); 
            VolumeYear volumeWeekYear = new WeekOfMonth(Presenter.Model.TaskOwnerPeriod, new WeekOfMonthCreator());
            VolumeYear volumeDayYear = new DayOfWeeks(Presenter.Model.TaskOwnerPeriod, new DaysOfWeekCreator());

            WorkflowSeasonView monthOfYear = new WorkflowSeasonView(volumeMonthYear, selectedDates, this);
            WorkflowSeasonView weekOfMonth = new WorkflowSeasonView(volumeWeekYear, selectedDates, this);
            WorkflowSeasonView dayOfWeek = new WorkflowSeasonView(volumeDayYear, selectedDates, this);

            monthOfYear.Name = "MonthOfYear";
            weekOfMonth.Name = "WeekOfMonth";
            dayOfWeek.Name = "DayOfWeek";
            
            IList<IVolumeYear> volumeYears = new List<IVolumeYear>
            {
                volumeMonthYear,
                volumeWeekYear,
                volumeDayYear
            };

            _workflowTrendView = new WorkflowTrendView(volumeYears, Presenter.Model.WorkloadDaysWithoutOutliers, this);
            _workflowTotalView = new WorkflowTotalView(volumeYears, this); 

            monthOfYear.Dock = DockStyle.Fill;
            weekOfMonth.Dock = DockStyle.Fill;
            dayOfWeek.Dock = DockStyle.Fill;
            _workflowTrendView.Dock = DockStyle.Fill;
            _workflowTotalView.Dock = DockStyle.Fill;

			disposeAndClearChildControls(tabPageDayOfWeek);
			disposeAndClearChildControls(tabPageMonthOfYear);
			disposeAndClearChildControls(tabPageWeekOfMonth);
			disposeAndClearChildControls(tabPageTrend);
			disposeAndClearChildControls(tabPageTotal);

            tabPageDayOfWeek.Controls.Add(dayOfWeek);
            tabPageMonthOfYear.Controls.Add(monthOfYear);
            tabPageWeekOfMonth.Controls.Add(weekOfMonth);
            tabPageTrend.Controls.Add(_workflowTrendView);
            tabPageTotal.Controls.Add(_workflowTotalView);
        }

		private static void disposeAndClearChildControls(Control control)
		{
			foreach (IDisposable disposable in control.Controls)
			{
				disposable.Dispose();
			}
			control.Controls.Clear();
		}

        public void SetTrendValues(Percent trendPercent, bool useTrend)
        {
            _trendPercent = trendPercent;
            _useTrend = useTrend;
        }

        public Percent TrendPercent
        {
            get { return _trendPercent; }
        }

        public bool UseTrend
        {
            get { return _useTrend; }
        }

        //These methods selected index and ReportChanges keeps track of changes in seasonView 
        //Might need refactoring, might be better to have some kind of object that can keep track 
        //of changes in the tabs
        private void tabSeasonality_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((TabControlAdv)sender).SelectedTab == tabPageTotal && _changes)
            {
                _workflowTotalView.Reload();
                _changes = false;
            }
            if (tabSeasonality.SelectedTab.Controls.Count > 0)
                tabSeasonality.SelectedTab.Controls[0].Select();
        }

        public void ReportChanges(bool changes)
        {
            _changes = changes;
        }

        private void UnhookEvents()
        {
            tabSeasonality.SelectedIndexChanged -= tabSeasonality_SelectedIndexChanged;
        }

        private void ReleaseManagedResources()
        {
            if (_workflowTotalView != null)
            {
                _workflowTotalView.Dispose();
                _workflowTotalView = null;
            }
            if (_workflowTrendView != null)
            {
                _workflowTrendView.Dispose();
                _workflowTrendView = null;
            }
        }
    }
}