using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls
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
			backgroundWorkerValidationPeriod.DoWork += backgroundWorkerValidationPeriodDoWork;
			backgroundWorkerValidationPeriod.RunWorkerCompleted += backgroundWorkerValidationPeriodRunWorkerCompleted;
			backgroundWorkerStatistics.DoWork += backgroundWorkerStatisticsDoWork;
			backgroundWorkerStatistics.RunWorkerCompleted += backgroundWorkerStatisticsRunWorkerCompleted;
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

			setControlPeriod(_owner.Presenter.Model.WorkPeriod, dateSelectionFromToTarget);
			setControlPeriod(_owner.Presenter.Model.CompareHistoricPeriod, dateSelectionFromToHistorical);

			initializeGrid();
			setGridValidatedVolumeDays(true);

			percentTextBoxTasks.DoubleValue = _gridControl.DeviationTasks.Value;
			percentTextBoxDeviationTaskTime.DoubleValue = _gridControl.DeviationTaskTime.Value;
			percentTextBoxDeviationAfterTaskTime.DoubleValue = _gridControl.DeviationAfterTaskTime.Value;
			_workPeriodChanged = false;
			_historicPeriodChanged = false;
		}

		private void initializeGrid()
		{
			_gridControl = new ValidatedVolumeDayGridControl(_owner.Presenter.Model.Workload.Skill.SkillType);
			_gridControl.AddOutlier += gridControlAddOutlier;
		}

		private static void setControlPeriod(DateOnlyPeriod period, DateSelectionFromTo dateControl)
		{
			dateControl.WorkPeriodEnd = period.EndDate;
			dateControl.WorkPeriodStart = period.StartDate;
		}

		private void initializeOutliers()
		{
			_owner.Presenter.InitializeOutliersByWorkDate();
			outlierBoxControl.SetOutliers(_owner.Presenter.Model.Outliers);
			_gridControl.SetOutliers(_owner.Presenter.Model.OutliersByWorkDate);
		}

		private void gridControlAddOutlier(object sender, CustomEventArgs<DateOnly> e)
		{
			_owner.CreateNewOutlier(new List<DateOnly> { e.Value });
		}

		private void createGridToChart()
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
		private void updateWorkPeriod(bool reloadStatistics)
		{
			_owner.Presenter.SetWorkPeriod(new DateOnlyPeriod(dateSelectionFromToTarget.WorkPeriodStart, dateSelectionFromToTarget.WorkPeriodEnd));
			setGridValidatedVolumeDays(reloadStatistics);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		void backgroundWorkerValidationPeriodRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			if (IsDisposed) return;
			if (e.Cancelled)
			{
				enableWindow();
				return;
			}
			if (e.Error != null)
			{
				var ex = new Exception("Background thread exception", e.Error);
				throw ex;
			}

			_gridControl.UpdateValidatedVolumeDays(_owner.Presenter.Model.ValidatedVolumeDays);
			initializeOutliers();
			createGridToChart();

			if (e.Result as bool? == true)
			{
				updateStatistics();
			}
			else
			{
				enableWindow();
			}
		}

		private void backgroundWorkerValidationPeriodDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			if (backgroundWorkerValidationPeriod.CancellationPending || IsDisposed)
			{
				e.Cancel = true;
				return;
			}
			_owner.Presenter.InitializeValidatedVolumeDays();
			e.Result = e.Argument;
		}

		private void setGridValidatedVolumeDays(bool reloadStatistics)
		{
			Cursor = Cursors.WaitCursor;
			ParentForm.ControlBox = false;//is there really a risk of a nullrefexception?
			dateSelectionFromToHistorical.Enabled = false;
			buttonAdvCancelLoad.Visible = true;
			backgroundWorkerValidationPeriod.RunWorkerAsync(reloadStatistics);
		}

		#endregion

		#region statistics

		private void updateStatistics()
		{
			Cursor = Cursors.WaitCursor;
			ParentForm.ControlBox = false;

			_owner.Presenter.InitializeCompareHistoricPeriod(new DateOnlyPeriod(dateSelectionFromToHistorical.WorkPeriodStart, dateSelectionFromToHistorical.WorkPeriodEnd));
			dateSelectionFromToHistorical.Enabled = false;
			buttonAdvCancelLoad.Visible = true;
			backgroundWorkerStatistics.RunWorkerAsync();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		private void backgroundWorkerStatisticsRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			if (IsDisposed) return;
			if (e.Cancelled)
			{
				enableWindow();
				return;
			}
			if (e.Error != null)
			{
				var ex = new Exception("Background thread exception", e.Error);
				throw ex;
			}
			_gridControl.UpdateHistoricStatistics(_owner.Presenter.Model.DayOfWeeks);
			enableWindow();
		}

		private void enableWindow()
		{
			ParentForm.ControlBox = true;
			dateSelectionFromToHistorical.Enabled = true;
			buttonAdvCancelLoad.Visible = false;
			Cursor = Cursors.Default;
		}

		private void backgroundWorkerStatisticsDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			if (backgroundWorkerStatistics.CancellationPending ||IsDisposed || _owner.Presenter.Model==null)  { e.Cancel = true; return; }
			_owner.Presenter.InitializeDayOfWeeks();
		}

		#endregion

		#region cancelLoadButton events

		private void buttonAdvCancelLoadClick(object sender, EventArgs e)
		{
			PerformCancel();
		}

		public void PerformCancel()
		{
			if (!backgroundWorkerValidationPeriod.CancellationPending)
			{
				if (backgroundWorkerValidationPeriod.IsBusy) backgroundWorkerValidationPeriod.CancelAsync();
				if (backgroundWorkerStatistics.IsBusy) backgroundWorkerStatistics.CancelAsync();
				buttonAdvCancelLoad.Text = UserTexts.Resources.CancellingThreeDots;
			   
				//Seems that users wants to be able to click Apply again with the same workperiod after cancelation?!?
				_workPeriodChanged = true;
			}
		}

		private void buttonAdvCancelLoadMouseEnter(object sender, EventArgs e)
		{
			if (((ButtonAdv)sender).Visible) Cursor = Cursors.Default;

		}

		private void buttonAdvCancelLoadMouseLeave(object sender, EventArgs e)
		{
			if (((ButtonAdv)sender).Visible) Cursor = Cursors.WaitCursor;
		}

		#endregion

		#endregion

		private void percentTextBoxTasksValidated(object sender, EventArgs e)
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

		private void percentTextBoxDeviationAfterTaskTimeValidated(object sender, EventArgs e)
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

		private void percentTextBoxDeviationTaskTimeValidated(object sender, EventArgs e)
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

		private void outlierBoxControlAddOutlier(object sender, CustomEventArgs<DateOnly> e)
		{
			_owner.CreateNewOutlier(new List<DateOnly> { e.Value });
		}

		private void outlierBoxControlDeleteOutlier(object sender, CustomEventArgs<IOutlier> e)
		{
			_owner.DeleteOutlier(e.Value);
		}

		private void outlierBoxControlUpdateOutlier(object sender, CustomEventArgs<IOutlier> e)
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

		private void dateSelectionFromToTargetValueChanged(object sender, EventArgs e)
		{
			_workPeriodChanged = true;
		}

		private void dateSelectionFromToHistoricalValueChanged(object sender, EventArgs e)
		{
			_historicPeriodChanged = true;
		}

		private void dateSelectionFromToHistoricalButtonClickedNovalidation(object sender, EventArgs e)
		{
			buttonAdvCancelLoad.Text = UserTexts.Resources.CancelLoading;
			if (dateSelectionFromToHistorical.IsWorkPeriodValid && dateSelectionFromToTarget.IsWorkPeriodValid)
			{
				if (_workPeriodChanged)
					updateWorkPeriod(_historicPeriodChanged);
				else if (_historicPeriodChanged && !_workPeriodChanged)
					updateStatistics();

				_workPeriodChanged = false;
				_historicPeriodChanged = false;
			}
			else
			{
				dateSelectionFromToTarget.ShowWarning();
				dateSelectionFromToHistorical.ShowWarning();
			}
		}

		private void splitContainerAdv2DoubleClick(object sender, EventArgs e)
		{
			splitContainerAdv2.Panel2Collapsed = !splitContainerAdv2.Panel2Collapsed;
		}

		public override bool HasHelp => false;

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			buttonAdvCancelLoad.Text = UserTexts.Resources.CancelLoading;
		}

		private void unhookEvents()
		{
			outlierBoxControl.AddOutlier -= outlierBoxControlAddOutlier;
			outlierBoxControl.DeleteOutlier -= outlierBoxControlDeleteOutlier;
			outlierBoxControl.UpdateOutlier -= outlierBoxControlUpdateOutlier;
			splitContainerAdv2.DoubleClick -= splitContainerAdv2DoubleClick;
			dateSelectionFromToTarget.ValueChanged -= dateSelectionFromToTargetValueChanged;
			dateSelectionFromToHistorical.ValueChanged -= dateSelectionFromToHistoricalValueChanged;
			dateSelectionFromToHistorical.ButtonClickedNoValidation -= dateSelectionFromToHistoricalButtonClickedNovalidation;
			percentTextBoxDeviationAfterTaskTime.Validated -= percentTextBoxDeviationAfterTaskTimeValidated;
			percentTextBoxDeviationTaskTime.Validated -= percentTextBoxDeviationTaskTimeValidated;
			percentTextBoxTasks.Validated -= percentTextBoxTasksValidated;
			backgroundWorkerValidationPeriod.DoWork -= backgroundWorkerValidationPeriodDoWork;
			backgroundWorkerValidationPeriod.RunWorkerCompleted -= backgroundWorkerValidationPeriodRunWorkerCompleted;
			backgroundWorkerStatistics.DoWork -= backgroundWorkerStatisticsDoWork;
			backgroundWorkerStatistics.RunWorkerCompleted -= backgroundWorkerStatisticsRunWorkerCompleted;
		}

		private void releaseManagedResources()
		{
			if (_gridControl != null)
			{
				_gridControl.AddOutlier -= gridControlAddOutlier;
				_gridControl.Dispose();
				_gridControl = null;
			}
			if (buttonAdvCancelLoad != null)
			{
				buttonAdvCancelLoad.Click -= buttonAdvCancelLoadClick;
				buttonAdvCancelLoad.MouseEnter -= buttonAdvCancelLoadMouseEnter;
				buttonAdvCancelLoad.MouseLeave -= buttonAdvCancelLoadMouseLeave;
				buttonAdvCancelLoad.Dispose();
			}

			if (_owner != null)
			{
				_owner.Presenter.Model.ValidatedVolumeDays?.Clear();
				_owner = null;
			}
		}
	}
}
