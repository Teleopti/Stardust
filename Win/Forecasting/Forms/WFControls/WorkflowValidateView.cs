using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Chart;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WFControls
{
    public partial class WorkflowValidateView : BaseUserControl
    {
        private ValidatedVolumeDayGridControl _gridControl;
        private WFValidate _owner;
        private bool _workPeriodChanged;
        private bool _historicPeriodChanged;

        public WorkflowValidateView()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            dateSelectionFromToHistorical.SetCulture(CultureInfo.CurrentCulture);
            dateSelectionFromToTarget.SetCulture(CultureInfo.CurrentCulture);
            backgroundWorkerValidationPeriod.DoWork += backgroundWorkerValidationPeriod_DoWork;
            backgroundWorkerValidationPeriod.RunWorkerCompleted += backgroundWorkerValidationPeriod_RunWorkerCompleted;
            backgroundWorkerStatistics.DoWork += backgroundWorkerStatistics_DoWork;
            backgroundWorkerStatistics.RunWorkerCompleted += backgroundWorkerStatistics_RunWorkerCompleted;
        }

        public void Initialize(WFValidate owner)
        {
            _owner = owner;

            var manager = new TextManager(_owner.Presenter.Model.Workload.Skill.SkillType);
            labelDeviationTasks.Text = manager.WordDictionary["DeviationCallsColon"];
            labelDeviationTaskTime.Text = manager.WordDictionary["DeviationTalkTimeColon"];
            labelDeviationAfterTaskTime.Text = manager.WordDictionary["DeviationACWColon"];

            _owner.Presenter.InitializeHistoricPeriod();

            if (_owner.Presenter.Locked)
                dateSelectionFromToTarget.Enabled = false;

            SetControlPeriod(_owner.Presenter.Model.WorkPeriod, dateSelectionFromToTarget);
            SetControlPeriod(_owner.Presenter.Model.CompareHistoricPeriod, dateSelectionFromToHistorical);

            InitializeGrid();
            SetGridValidatedVolumeDays(true);

            percentTextBoxTasks.DoubleValue = _gridControl.DeviationTasks.Value;
            percentTextBoxDeviationTaskTime.DoubleValue = _gridControl.DeviationTaskTime.Value;
            percentTextBoxDeviationAfterTaskTime.DoubleValue = _gridControl.DeviationAfterTaskTime.Value;
            _workPeriodChanged = false;
            _historicPeriodChanged = false;
        }

        private void InitializeGrid()
        {
            _gridControl = new ValidatedVolumeDayGridControl(_owner.Presenter.Model.Workload.Skill.SkillType);
            _gridControl.AddOutlier += gridControl_AddOutlier;
        }

        private static void SetControlPeriod(DateOnlyPeriod period, DateSelectionFromTo dateControl)
        {
            dateControl.WorkPeriodEnd = period.EndDate;
            dateControl.WorkPeriodStart = period.StartDate;
        }

        private void InitializeOutliers()
        {
            _owner.Presenter.InitializeOutliersByWorkDate();
            outlierBoxControl.SetOutliers(_owner.Presenter.Model.Outliers);
            _gridControl.SetOutliers(_owner.Presenter.Model.OutliersByWorkDate);
        }

        private void gridControl_AddOutlier(object sender, CustomEventArgs<DateOnly> e)
        {
            _owner.CreateNewOutlier(new List<DateOnly> { e.Value });
        }

        private void CreateGridToChart()
        {
            var gridToChart = new GridToChart(_gridControl);
            gridToChart.Dock = DockStyle.Fill;
            if (splitContainerAdv2.Panel1.Controls.Count > 0)
            {
                foreach (Control control in splitContainerAdv2.Panel1.Controls)
                {
                    control.Dispose();
                    splitContainerAdv2.Panel1.Controls.Remove(control);
                }
            }
            splitContainerAdv2.Panel1.Controls.Add(gridToChart);
        }

        #region threaded part

        #region validation period
        private void UpdateWorkPeriod(bool reloadStatistics)
        {
            _owner.Presenter.SetWorkPeriod(new DateOnlyPeriod(dateSelectionFromToTarget.WorkPeriodStart, dateSelectionFromToTarget.WorkPeriodEnd));
            SetGridValidatedVolumeDays(reloadStatistics);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        void backgroundWorkerValidationPeriod_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed) return;
            if (e.Cancelled)
            {
                EnableWindow();
                return;
            }
            if (e.Error != null)
            {
                Exception ex = new Exception("Background thread exception", e.Error);
                throw ex;
            }

            _gridControl.UpdateValidatedVolumeDays(_owner.Presenter.Model.ValidatedVolumeDays);
            InitializeOutliers();
            CreateGridToChart();

            if (e.Result as bool? == true)
            {
                UpdateStatistics();
            }
            else
            {
                EnableWindow();
            }
        }

        private void backgroundWorkerValidationPeriod_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (backgroundWorkerValidationPeriod.CancellationPending || IsDisposed)
            {
                e.Cancel = true;
                return;
            }
            _owner.Presenter.InitializeValidatedVolumeDays();
            e.Result = e.Argument;
        }

        private void SetGridValidatedVolumeDays(bool reloadStatistics)
        {
            Cursor = Cursors.WaitCursor;
            ParentForm.ControlBox = false;//is there really a risk of a nullrefexception?
            buttonAdvCancelLoad.Visible = true;
            backgroundWorkerValidationPeriod.RunWorkerAsync(reloadStatistics);
        }

        #endregion

        #region statistics

        private void UpdateStatistics()
        {
            Cursor = Cursors.WaitCursor;
            ParentForm.ControlBox = false;

            _owner.Presenter.InitializeCompareHistoricPeriod(new DateOnlyPeriod(dateSelectionFromToHistorical.WorkPeriodStart, dateSelectionFromToHistorical.WorkPeriodEnd));
            buttonAdvCancelLoad.Visible = true;
            backgroundWorkerStatistics.RunWorkerAsync();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private void backgroundWorkerStatistics_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed) return;
            if (e.Cancelled)
            {
                EnableWindow();
                return;
            }
            if (e.Error != null)
            {
                Exception ex = new Exception("Background thread exception", e.Error);
                throw ex;
            }
            _gridControl.UpdateHistoricStatistics(_owner.Presenter.Model.DayOfWeeks);
            EnableWindow();
        }

        private void EnableWindow()
        {
            ParentForm.ControlBox = true;
            buttonAdvCancelLoad.Visible = false;
            Cursor = Cursors.Default;
        }

        private void backgroundWorkerStatistics_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (backgroundWorkerStatistics.CancellationPending ||IsDisposed || _owner.Presenter.Model==null)  { e.Cancel = true; return; }
            _owner.Presenter.InitializeDayOfWeeks();
        }

        #endregion

        #region cancelLoadButton events

        private void buttonAdvCancelLoad_Click(object sender, EventArgs e)
        {
            PerformCancel();
        }

        public void PerformCancel()
        {
            if (!backgroundWorkerValidationPeriod.CancellationPending)
            {
                _owner.Presenter.CancelGetDaysToValidate();
                if (backgroundWorkerValidationPeriod.IsBusy) backgroundWorkerValidationPeriod.CancelAsync();
                if (backgroundWorkerStatistics.IsBusy) backgroundWorkerStatistics.CancelAsync();
                buttonAdvCancelLoad.Text = UserTexts.Resources.CancellingThreeDots;
               
                //Seems that users wants to be able to click Apply again with the same workperiod after cancelation?!?
                _workPeriodChanged = true;
            }
        }

        private void buttonAdvCancelLoad_MouseEnter(object sender, EventArgs e)
        {
            if (((ButtonAdv)sender).Visible) Cursor = Cursors.Default;

        }

        private void buttonAdvCancelLoad_MouseLeave(object sender, EventArgs e)
        {
            if (((ButtonAdv)sender).Visible) Cursor = Cursors.WaitCursor;
        }

        #endregion

        #endregion

        private void percentTextBoxTasks_Validated(object sender, EventArgs e)
        {
            _gridControl.DeviationTasks = new Percent(percentTextBoxTasks.DoubleValue);
            _gridControl.RefreshGrid();
        }

	    private void PercentTextBoxTasks_OnKeyUp(object sender, KeyEventArgs keyEventArgs)
	    {
		    if (keyEventArgs.KeyCode != Keys.Enter) 
				return;
		    ProcessTabKey(true);
			keyEventArgs.Handled = true;
	    }

	    private void percentTextBoxDeviationAfterTaskTime_Validated(object sender, EventArgs e)
        {
            _gridControl.DeviationAfterTaskTime = new Percent(percentTextBoxDeviationAfterTaskTime.DoubleValue);
            _gridControl.RefreshGrid();
        }

		private void PercentTextBoxDeviationAfterTaskTime_OnKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			if (keyEventArgs.KeyCode != Keys.Enter)
				return;
			ProcessTabKey(true);
			keyEventArgs.Handled = true;
		}

        private void percentTextBoxDeviationTaskTime_Validated(object sender, EventArgs e)
        {
            _gridControl.DeviationTaskTime = new Percent(percentTextBoxDeviationTaskTime.DoubleValue);
            _gridControl.RefreshGrid();
        }

		private void PercentTextBoxDeviationTaskTime_OnKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			if (keyEventArgs.KeyCode != Keys.Enter)
				return;
			ProcessTabKey(true);
			keyEventArgs.Handled = true;			
		}

        private void outlierBoxControl_AddOutlier(object sender, CustomEventArgs<DateOnly> e)
        {
            _owner.CreateNewOutlier(new List<DateOnly> { e.Value });
        }

        private void outlierBoxControl_DeleteOutlier(object sender, CustomEventArgs<IOutlier> e)
        {
            _owner.DeleteOutlier(e.Value);
        }

        private void outlierBoxControl_UpdateOutlier(object sender, CustomEventArgs<IOutlier> e)
        {
            _owner.EditOutlier(e.Value);
        }

        public void RefreshOutliers()
        {
            _owner.Presenter.SetWorkPeriod(new DateOnlyPeriod(dateSelectionFromToTarget.WorkPeriodStart, dateSelectionFromToTarget.WorkPeriodEnd));
            _owner.Presenter.InitializeOutliersByWorkDate();

            outlierBoxControl.SetOutliers(_owner.Presenter.Model.Outliers);
            _gridControl.SetOutliers(_owner.Presenter.Model.OutliersByWorkDate);
            _gridControl.Refresh();
        }

        private void dateSelectionFromToTarget_ValueChanged(object sender, EventArgs e)
        {
            _workPeriodChanged = true;
        }

        private void dateSelectionFromToHistorical_ValueChanged(object sender, EventArgs e)
        {
            _historicPeriodChanged = true;
        }

        private void dateSelectionFromToHistorical_ButtonClicked_novalidation(object sender, EventArgs e)
        {
            buttonAdvCancelLoad.Text = UserTexts.Resources.CancelLoading;
            if (dateSelectionFromToHistorical.IsWorkPeriodValid && dateSelectionFromToTarget.IsWorkPeriodValid)
            {
                if (_workPeriodChanged)
                    UpdateWorkPeriod(_historicPeriodChanged);
                else if (_historicPeriodChanged && !_workPeriodChanged)
                    UpdateStatistics();

                _workPeriodChanged = false;
                _historicPeriodChanged = false;
            }
            else
            {
                dateSelectionFromToTarget.ShowWarning();
                dateSelectionFromToHistorical.ShowWarning();
            }
        }

        private void splitContainerAdv2_DoubleClick(object sender, EventArgs e)
        {
            splitContainerAdv2.Panel2Collapsed = !splitContainerAdv2.Panel2Collapsed;
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            buttonAdvCancelLoad.Text = UserTexts.Resources.CancelLoading;
        }

        private void UnhookEvents()
        {
            outlierBoxControl.AddOutlier -= outlierBoxControl_AddOutlier;
            outlierBoxControl.DeleteOutlier -= outlierBoxControl_DeleteOutlier;
            outlierBoxControl.UpdateOutlier -= outlierBoxControl_UpdateOutlier;
            splitContainerAdv2.DoubleClick -= splitContainerAdv2_DoubleClick;
            dateSelectionFromToTarget.ValueChanged -= dateSelectionFromToTarget_ValueChanged;
            dateSelectionFromToHistorical.ValueChanged -= dateSelectionFromToHistorical_ValueChanged;
            dateSelectionFromToHistorical.ButtonClickedNoValidation -= dateSelectionFromToHistorical_ButtonClicked_novalidation;
            percentTextBoxDeviationAfterTaskTime.Validated -= percentTextBoxDeviationAfterTaskTime_Validated;
            percentTextBoxDeviationTaskTime.Validated -= percentTextBoxDeviationTaskTime_Validated;
            percentTextBoxTasks.Validated -= percentTextBoxTasks_Validated;
            backgroundWorkerValidationPeriod.DoWork -= backgroundWorkerValidationPeriod_DoWork;
            backgroundWorkerValidationPeriod.RunWorkerCompleted -= backgroundWorkerValidationPeriod_RunWorkerCompleted;
            backgroundWorkerStatistics.DoWork -= backgroundWorkerStatistics_DoWork;
            backgroundWorkerStatistics.RunWorkerCompleted -= backgroundWorkerStatistics_RunWorkerCompleted;
        }

        private void ReleaseManagedResources()
        {
            if (_gridControl != null)
            {
                _gridControl.AddOutlier -= gridControl_AddOutlier;
                _gridControl.Dispose();
                _gridControl = null;
            }
            if (buttonAdvCancelLoad != null)
            {
                buttonAdvCancelLoad.Click -= buttonAdvCancelLoad_Click;
                buttonAdvCancelLoad.MouseEnter -= buttonAdvCancelLoad_MouseEnter;
                buttonAdvCancelLoad.MouseLeave -= buttonAdvCancelLoad_MouseLeave;
                buttonAdvCancelLoad.Dispose();
            }

            if (_owner != null)
            {
                if (_owner.Presenter.Model.ValidatedVolumeDays != null)
                {
                    _owner.Presenter.Model.ValidatedVolumeDays.Clear();
                }
                _owner = null;
            }
        }
    }
}
