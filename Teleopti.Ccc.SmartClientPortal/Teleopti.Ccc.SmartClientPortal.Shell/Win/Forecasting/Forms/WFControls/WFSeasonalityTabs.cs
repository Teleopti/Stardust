using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SeasonPages;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls
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

			var dataSummary = new WorkflowDataSummaryView(this) {Dock = DockStyle.Fill};
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
			Presenter.InitializeWorkloadDaysWithStatistics(selectedDates);

			if (Presenter.Model.WorkloadDaysWithStatistics.Count > 0)
			{
				createTabs(selectedDates);
			}
		}

		private void createTabs(IList<DateOnlyPeriod> selectedDates)
		{
			Presenter.InitializeTaskOwnerPeriod(DateOnly.Today, TaskOwnerPeriodType.Other);

			clearAllTabs();

			IList<IVolumeYear> volumeYears;
			if (Presenter.Model.HasOnlyOutliersInStatisticsSelection())
			{
				enableOnlyTotalTab();

				volumeYears = new List<IVolumeYear>();
			}
			else
			{
				enableTabs(true);

				VolumeYear volumeMonthYear = new MonthOfYear(Presenter.Model.TaskOwnerPeriod, new MonthOfYearCreator());
				VolumeYear volumeWeekYear = new WeekOfMonth(Presenter.Model.TaskOwnerPeriod, new WeekOfMonthCreator());
				VolumeYear volumeDayYear = new DayOfWeeks(Presenter.Model.TaskOwnerPeriod, new DaysOfWeekCreator());

				volumeYears = new List<IVolumeYear>
				{
					volumeMonthYear,
					volumeWeekYear,
					volumeDayYear
				};

				setupTrendView(volumeYears);
				setupMonthOfYearView(selectedDates, volumeMonthYear);
				setupWeekOfMonthView(selectedDates, volumeWeekYear);
				setupDayOfWeekView(selectedDates, volumeDayYear);
			}
			
			setupTotalView(volumeYears);
		}

		private void setupTrendView(IList<IVolumeYear> volumeYears)
		{
			//_workflowTrendView = new WorkflowTrendView(volumeYears, Presenter.Model.WorkloadDaysWithoutOutliers, this)
			_workflowTrendView = new WorkflowTrendView(){Dock = DockStyle.Fill};
			_workflowTrendView.LoadAllComponents(volumeYears, Presenter.Model.WorkloadDaysWithoutOutliers, this);
			tabPageTrend.Controls.Add(_workflowTrendView);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void setupMonthOfYearView(IList<DateOnlyPeriod> selectedDates, VolumeYear volumeMonthYear)
		{
			var monthOfYear = new WorkflowSeasonView(volumeMonthYear, selectedDates, this)
			{
				Name = "MonthOfYear",
				Dock = DockStyle.Fill
			};
			tabPageMonthOfYear.Controls.Add(monthOfYear);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void setupWeekOfMonthView(IList<DateOnlyPeriod> selectedDates, VolumeYear volumeWeekYear)
		{
			var weekOfMonth = new WorkflowSeasonView(volumeWeekYear, selectedDates, this)
			{
				Name = "WeekOfMonth",
				Dock = DockStyle.Fill
			};
			tabPageWeekOfMonth.Controls.Add(weekOfMonth);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void setupDayOfWeekView(IList<DateOnlyPeriod> selectedDates, VolumeYear volumeDayYear)
		{
			var dayOfWeek = new WorkflowSeasonView(volumeDayYear, selectedDates, this)
			{
				Name = "DayOfWeek",
				Dock = DockStyle.Fill
			};
			tabPageDayOfWeek.Controls.Add(dayOfWeek);
		}

		private void setupTotalView(IList<IVolumeYear> volumeYears)
		{
			_workflowTotalView = new WorkflowTotalView(volumeYears, this) {Dock = DockStyle.Fill};

			tabPageTotal.Controls.Add(_workflowTotalView);
		}

		private void enableOnlyTotalTab()
		{
			enableTabs(false);
			tabPageTotal.TabEnabled = true;
		}

		private void clearAllTabs()
		{
			disposeAndClearChildControls(tabPageDayOfWeek);
			disposeAndClearChildControls(tabPageMonthOfYear);
			disposeAndClearChildControls(tabPageWeekOfMonth);
			disposeAndClearChildControls(tabPageTotal);
			disposeAndClearChildControls(tabPageTrend);
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
		private void tabSeasonalitySelectedIndexChanged(object sender, EventArgs e)
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

		private void unhookEvents()
		{
			tabSeasonality.SelectedIndexChanged -= tabSeasonalitySelectedIndexChanged;
		}

		private void releaseManagedResources()
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