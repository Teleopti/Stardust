﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using log4net;
using Syncfusion.Drawing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Obfuscated.Forecasting;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Chart;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Forecasting.Forms.WFControls;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SeasonPages
{
    public partial class WorkflowTotalView : BaseUserControl
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(WorkflowTotalView));
        private IWorkload _workload;
        private readonly IList<IVolumeYear> _volumes;
        private TaskOwnerPeriod _currentHistoricPeriod;
        private TotalVolumeGridControl _totalVolumeGridControl;
        private WFSeasonalityTabs _owner;
        private TotalVolume _totalVolume;

        private WorkflowTotalView()
        {
            InitializeComponent();
            SetColors();
            if (!DesignMode) SetTexts();
			dateSelectionFromToTarget.SetCulture(CultureInfo.CurrentCulture);
        }

        public WorkflowTotalView(IList<IVolumeYear> volumes, WFSeasonalityTabs owner)
            : this()
        {
            _volumes = volumes;
            _owner = owner;
            _workload = _owner.Presenter.Model.Workload;
        }

        private void SetColors()
        {
            BrushInfo myBrush = ColorHelper.ControlGradientPanelBrush();
            gradientPanel1.BackgroundColor = myBrush;
            gradientPanel2.BackgroundColor = myBrush;
            splitContainerAdv1.Panel1.BackgroundColor = myBrush;
            splitContainerAdv1.Panel2.BackgroundColor = myBrush;
            xpTaskBarTotal.BackColor = ColorHelper.XPTaskbarBackgroundColor();
            xpTaskBarTotal.ThemesEnabled = false;
            xpTaskBarTotal.BackColor = ColorHelper.XPTaskbarBackgroundColor();
            xpTaskBarBoxPeriod.HeaderBackColor = ColorHelper.XPTaskbarHeaderColor();
            xpTaskBarBoxSpecialEvents.HeaderBackColor = ColorHelper.XPTaskbarHeaderColor();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            scenarioSelectorControl.SelectedItem = _owner.Presenter.Model.Scenario;

            try
            {
                if (_owner.Presenter.Locked)
                {
                    _owner.Presenter.InitializeWorkPeriod(scenarioSelectorControl.SelectedItem);
                    _owner.Presenter.InitializeWorkloadDaysWhenLocked();
                    dateSelectionFromToTarget.Enabled = false;
                    scenarioSelectorControl.Enabled = false;
                }

                _currentHistoricPeriod =
                    new TaskOwnerPeriod(_owner.Presenter.Model.WorkloadDaysWithStatistics[0].CurrentDate,
                                        _owner.Presenter.Model.WorkloadDaysWithStatistics, TaskOwnerPeriodType.Other);

                if (dateSelectionFromToTarget.Enabled)
                {
                    _owner.Presenter.InitializeWorkPeriod(scenarioSelectorControl.SelectedItem);
                }

                dateSelectionFromToTarget.WorkPeriodStart = _owner.Presenter.Model.WorkPeriod.StartDate;
                dateSelectionFromToTarget.WorkPeriodEnd = _owner.Presenter.Model.WorkPeriod.EndDate;
                _owner.Presenter.InitializeWorkloadDays(scenarioSelectorControl.SelectedItem);

                reloadData();
            }
            catch (DataSourceException dataSourceException)
            {
                datasourceExceptionOccurred(dataSourceException);
                return;
            }
            _totalVolumeGridControl = new TotalVolumeGridControl(_totalVolume, _workload.Skill.SkillType);
            _totalVolumeGridControl.UpdateTotalVolumeDayItems();
            _totalVolumeGridControl.SetOutliers(_owner.Presenter.Model.OutliersByWorkDate);

            reloadGridToChart();

            Name = "TotalView";

            _owner.ReportChanges(false);
        }

        private void datasourceExceptionOccurred(Exception exception)
        {
            if (exception != null)
            {
                _logger.Error("An error occurred in the workflow total view.",exception);

                var dataSourceException = exception as DataSourceException;
                if (dataSourceException == null) return;

                using (var view = new SimpleExceptionHandlerView(dataSourceException, UserTexts.Resources.OpenTeleoptiCCC, UserTexts.Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }
            }
        }

        public void RefreshOutlier(IOutlier outlier)
        {
            if (_owner.Presenter.Model.WorkPeriod.StartDate.Date == DateTime.MinValue) return;
			if (_totalVolume == null) return;

            _owner.Presenter.InitializeOutliersByWorkDate();
            var outlierToUpdate = _owner.Presenter.Model.Outliers.FirstOrDefault(o => o == outlier);
            if (outlierToUpdate == null)
            {
                _totalVolume.RemoveOutlier(outlier);
            }
            else
            {
                _totalVolume.RecalculateOutlier(outlierToUpdate);
            }

            reloadGridToChart();
            outlierBox.SetOutliers(_owner.Presenter.Model.Outliers);
            _totalVolumeGridControl.SetOutliers(_owner.Presenter.Model.OutliersByWorkDate);
            _totalVolumeGridControl.Refresh();
        }

        private void dualPeriodHandler1_DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            if (e.SelectedDates.Count == 0) return;

            _owner.Presenter.SetWorkPeriod(e.SelectedDates[0]);
            var scenario = scenarioSelectorControl.SelectedItem;
            _owner.Presenter.InitializeWorkloadDays(scenario);
            Reload();
            _owner.Presenter.InitializeOutliersByWorkDate();
        }

        public void Reload()
        {
            reloadData();
            _totalVolumeGridControl.UpdateTotalVolume(_totalVolume);
            _totalVolumeGridControl.SetOutliers(_owner.Presenter.Model.OutliersByWorkDate);
            reloadGridToChart();
        }

        private void reloadData()
        {
            Cursor = Cursors.WaitCursor;
            _totalVolume = new TotalVolume();
            _owner.Presenter.InitializeOutliersByWorkDate();
            if (_owner.UseTrend)
            {
                DateTime historicalStartDate = _currentHistoricPeriod.StartDate;

                double trendStartDayFactor =
                    VolumeTrend.CalculateStartDayFactor(historicalStartDate,
                                                        dateSelectionFromToTarget.WorkPeriodStart, _owner.TrendPercent);

                double trendDayFactor = VolumeTrend.DayChangeFactor;
                _totalVolume.Create(_currentHistoricPeriod, _owner.Presenter.Model.WorkloadDays, _volumes, _owner.Presenter.Model.Outliers, trendStartDayFactor, trendDayFactor, true, _owner.Presenter.Model.Workload);
            }
            else
            {
                _totalVolume.Create(_currentHistoricPeriod, _owner.Presenter.Model.WorkloadDays, _volumes, _owner.Presenter.Model.Outliers, 0, 0, false, _owner.Presenter.Model.Workload);
            }
            outlierBox.SetOutliers(_owner.Presenter.Model.Outliers);

            Cursor = Cursors.Default;
        }

        private void reloadGridToChart()
        {
            GridToChart gridToChart = new GridToChart(_totalVolumeGridControl);
            gridToChart.Dock = DockStyle.Fill;
            gridToChart.Name = "Total";
            if (splitContainerAdv1.Panel1.Controls.Count > 0)
            {
                foreach (Control control in splitContainerAdv1.Panel1.Controls)
                {
                    control.Dispose();
                    splitContainerAdv1.Panel1.Controls.Remove(control);
                }
            }
            splitContainerAdv1.Panel1.Controls.Add(gridToChart);
        }

        private void outlierBox_AddOutlier(object sender, CustomEventArgs<DateOnly> e)
        {
            _owner.CreateNewOutlier(new [] { e.Value });
        }

        private void outlierBox_DeleteOutlier(object sender, CustomEventArgs<IOutlier> e)
        {
            _owner.DeleteOutlier(e.Value);
        }

        private void outlierBox_UpdateOutlier(object sender, CustomEventArgs<IOutlier> e)
        {
            _owner.EditOutlier(e.Value);
        }

        private void splitContainerAdv1_DoubleClick(object sender, EventArgs e)
        {
            splitContainerAdv1.Panel2Collapsed = !splitContainerAdv1.Panel2Collapsed;
        }

        private void UnhookEvents()
        {
            dateSelectionFromToTarget.DateRangeChanged -= dualPeriodHandler1_DateRangeChanged;
            outlierBox.AddOutlier -= outlierBox_AddOutlier;
            outlierBox.DeleteOutlier -= outlierBox_DeleteOutlier;
            outlierBox.UpdateOutlier -= outlierBox_UpdateOutlier;
            splitContainerAdv1.DoubleClick -= splitContainerAdv1_DoubleClick;
        }

        private void ReleaseManagedResources()
        {
            _owner = null;
            _workload = null;
            _currentHistoricPeriod = null;
            _totalVolume = null;
            if (_volumes != null)
                _volumes.Clear();
            if (_totalVolumeGridControl != null)
            {
                _totalVolumeGridControl.Dispose();
                _totalVolumeGridControl = null;
            }
            //Kill presenter?!?!
        }
    }
}
