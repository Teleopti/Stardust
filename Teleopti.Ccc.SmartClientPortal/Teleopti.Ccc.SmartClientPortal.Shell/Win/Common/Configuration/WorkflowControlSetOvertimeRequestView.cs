using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class WorkflowControlSetView
	{
		private List<OvertimeRequestAutoGrantTypeAdapter> _overtimeRequestAutoGrantTypeAdapterCollection;
		private static readonly int _enableWorkRuleColumnIndex = 3;
		private static readonly int _overtimeRequestOpenPeriodDataStartRowIndex = 2;

		public void SetOvertimeOpenPeriodsGridRowCount(int rowCount)
		{
			gridControlOvertimeRequestOpenPeriods.RowCount = 0;
			gridControlOvertimeRequestOpenPeriods.RowCount = rowCount + gridControlOvertimeRequestOpenPeriods.Rows.HeaderCount;
		}

		public void RefreshOvertimeOpenPeriodsGrid()
		{
			gridControlOvertimeRequestOpenPeriods.Invalidate();
		}

		public void SetOvertimeProbability(bool overtimeProbability)
		{
			checkBoxAdvOvertimeProbability.CheckStateChanged -= checkBoxAdvOvertimeProbability_CheckStateChanged;
			checkBoxAdvOvertimeProbability.Checked = overtimeProbability;
			checkBoxAdvOvertimeProbability.CheckStateChanged += checkBoxAdvOvertimeProbability_CheckStateChanged;
		}

		public void SetAutoGrantOvertimeRequest(bool autoGrantOvertimeRequest)
		{
			checkBoxAdvAutoGrantOvertimeRequest.CheckStateChanged -= checkBoxAdvAutoGrantOvertimeRequest_CheckStateChanged;
			checkBoxAdvAutoGrantOvertimeRequest.Checked = autoGrantOvertimeRequest;
			checkBoxAdvAutoGrantOvertimeRequest.CheckStateChanged += checkBoxAdvAutoGrantOvertimeRequest_CheckStateChanged;
		}

		public void SetOverTimeRequestMaximumTimeHandleType(OvertimeRequestValidationHandleOptionView overtimeRequestValidationHandleOptionView)
		{
			comboBoxOvertimeRequestMaximumTimeHandleType.SelectedItem = overtimeRequestValidationHandleOptionView;
		}

		public void SetOverTimeRequestMaximumTime(TimeSpan? selectedModelOvertimeRequestMaximumTime)
		{
			if (selectedModelOvertimeRequestMaximumTime.HasValue && selectedModelOvertimeRequestMaximumTime.Value != TimeSpan.Zero) { 
				timeSpanTextBoxOvertimeRequestMaximumTime.SetInitialResolution(selectedModelOvertimeRequestMaximumTime.Value);
				comboBoxOvertimeRequestMaximumTimeHandleType.Enabled = true;
			}
		}

		private void checkOvertimeRequestsLicense()
		{
			var toggleEnabled = _toggleManager.IsEnabled(Toggles.Staffing_Info_Configuration_44687);
			var hasLicense = new OvertimeRequestAvailability(CurrentDataSource.Make(), PrincipalAuthorization.Current()).IsLicenseEnabled();

			var visible = hasLicense && toggleEnabled;
			if (visible)
			{
				setOvertimeRequestVisibility();
			}
			else
			{
				tabPageAdvETOTRequest.Hide();
			}
		}

		private void setOvertimeRequestVisibility()
		{
			if (!_toggleManager.IsEnabled(Toggles.Wfm_Requests_OvertimeRequestHandling_45177) 
				|| _toggleManager.IsEnabled(Toggles.OvertimeRequestPeriodSetting_46417))
			{
				checkBoxAdvAutoGrantOvertimeRequest.Visible = false;
				tableLayoutPanelETOTRequest.RowStyles[tableLayoutPanelETOTRequest.Controls.IndexOf(checkBoxAdvAutoGrantOvertimeRequest)].Height = 0;
			}

			if (!_toggleManager.IsEnabled(Toggles.OvertimeRequestPeriodSetting_46417))
			{
				tableLayoutPanelOpenForOvertimeRequests.Visible = false;
				gridControlOvertimeRequestOpenPeriods.Visible = false;
			}

			if (!_toggleManager.IsEnabled(Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024))
			{
				tableLayoutPanelOvertimeMaximumSetting.Visible = false;
				tableLayoutPanelETOTRequest.RowStyles[tableLayoutPanelETOTRequest.Controls.IndexOf(tableLayoutPanelOvertimeMaximumSetting)].Height = 0;
			}
		}

		private void initOvertimeRequestMaximumTimeHandleType()
		{
			comboBoxOvertimeRequestMaximumTimeHandleType.DataSource =
				WorkflowControlSetModel.OvertimeRequestWorkRuleValidationHandleOptionViews.Select(s => s.Value).ToList();
			comboBoxOvertimeRequestMaximumTimeHandleType.DisplayMember = "Description";
		}

		private void timeSpanTextBoxOvertimeRequestMaximumTime_Leave(object sender, System.EventArgs e)
		{
			comboBoxOvertimeRequestMaximumTimeHandleType.Enabled = timeSpanTextBoxOvertimeRequestMaximumTime.Value != TimeSpan.Zero;
			if (!comboBoxOvertimeRequestMaximumTimeHandleType.Enabled)
				comboBoxOvertimeRequestMaximumTimeHandleType.SelectedItem = null;
			else
			{
				if (comboBoxOvertimeRequestMaximumTimeHandleType.SelectedItem == null)
					comboBoxOvertimeRequestMaximumTimeHandleType.SelectedIndex = 0;
			}
			_presenter.SetOvertimeRequestMaximumTime(timeSpanTextBoxOvertimeRequestMaximumTime.Value);
		}

		private void ComboBoxOvertimeRequestMaximumTimeHandleType_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			_presenter.SetOvertimeRequestMaximumTimeHandleType(
				(OvertimeRequestValidationHandleOptionView) comboBoxOvertimeRequestMaximumTimeHandleType.SelectedItem);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void configureOvertimeRequestPeriodGrid()
		{
			var columnList = new List<SFGridColumnBase<OvertimeRequestPeriodModel>>
			{
				new SFGridRowHeaderColumn<OvertimeRequestPeriodModel>(string.Empty)
			};

			gridControlOvertimeRequestOpenPeriods.CellModels.Add("IgnoreCell", new IgnoreCellModel(gridControlOvertimeRequestOpenPeriods.Model));
			var cellModel = new GridDropDownMonthCalendarAdvCellModel(gridControlOvertimeRequestOpenPeriods.Model);
			cellModel.HideNoneButton();
			cellModel.HideTodayButton();
			gridControlOvertimeRequestOpenPeriods.CellModels.Add(GridCellModelConstants.CellTypeDatePickerCell, cellModel);
			var cell = new NullableIntegerCellModel(gridControlOvertimeRequestOpenPeriods.Model)
			{
				MinValue = 0,
				MaxValue = 999
			};
			gridControlOvertimeRequestOpenPeriods.CellModels.Add("IntegerCellModel", cell);

			var periodTypeDropDownColumn = new SFGridDropDownColumn
				<OvertimeRequestPeriodModel, OvertimeRequestPeriodTypeModel>(
				"PeriodType", Resources.Type, " ", WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters, "DisplayText", null);

			columnList.Add(periodTypeDropDownColumn);

			var autoGrantColumn =
				new SFGridDropDownEnumColumn<OvertimeRequestPeriodModel, OvertimeRequestAutoGrantTypeAdapter, OvertimeRequestAutoGrantType>(
					"AutoGrantType", Resources.AutoGrant, " ", _overtimeRequestAutoGrantTypeAdapterCollection, "DisplayName", "AutoGrantType");

			columnList.Add(autoGrantColumn);

			if (_toggleManager.IsEnabled(Toggles.OvertimeRequestPeriodWorkRuleSetting_46638))
			{
				columnList.Add(new SFGridCheckBoxColumn<OvertimeRequestPeriodModel>("EnableWorkRuleValidation", Resources.Enabled, Resources.ContractWorkRuleValidation));
				columnList.Add(new SFGridDropDownColumn<OvertimeRequestPeriodModel, OvertimeRequestValidationHandleOptionView>("WorkRuleValidationHandleType",
					Resources.WhenValidationFails, Resources.ContractWorkRuleValidation, OvertimeRequestPeriodModel.OvertimeRequestWorkRuleValidationHandleOptionViews.Values.ToList(), "Description", typeof(OvertimeRequestValidationHandleOptionView)));
			}

			

			columnList.Add(new DateOnlyColumn<OvertimeRequestPeriodModel>("PeriodStartDate", Resources.Start, Resources.Period)
			{
				CellValidator = new OvertimeRequestDatePeriodStartCellValidator(_toggleManager)
			});
			columnList.Add(new DateOnlyColumn<OvertimeRequestPeriodModel>("PeriodEndDate", Resources.End, Resources.Period)
			{
				CellValidator = new OvertimeRequestDatePeriodEndCellValidator(_toggleManager)
			});
			columnList.Add(new SFGridIntegerCellWithIgnoreColumn<OvertimeRequestPeriodModel>("RollingStart", Resources.Start, Resources.Rolling)
			{
				CellValidator = new OvertimeRequestRollingPeriodStartCellValidator(_toggleManager)
			});
			columnList.Add(new SFGridIntegerCellWithIgnoreColumn<OvertimeRequestPeriodModel>("RollingEnd", Resources.End, Resources.Rolling)
			{
				CellValidator = new OvertimeRequestRollingPeriodEndCellValidator(_toggleManager)
			});

			gridControlOvertimeRequestOpenPeriods.Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow;
			gridControlOvertimeRequestOpenPeriods.Rows.HeaderCount = 1;

			var gridColumns = new ReadOnlyCollection<SFGridColumnBase<OvertimeRequestPeriodModel>>(columnList);
			_overtimeRequestOpenPeriodGridHelper = new SFGridColumnGridHelper<OvertimeRequestPeriodModel>(
				gridControlOvertimeRequestOpenPeriods, gridColumns, new List<OvertimeRequestPeriodModel>());

			gridControlOvertimeRequestOpenPeriods.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
			gridControlOvertimeRequestOpenPeriods.Model.Options.ExcelLikeCurrentCell = true;

			if (_toggleManager.IsEnabled(Toggles.OvertimeRequestPeriodWorkRuleSetting_46638))
			{
				gridControlOvertimeRequestOpenPeriods.CheckBoxClick += gridControlOvertimeRequestOpenPeriods_CheckBoxClick;
				gridControlOvertimeRequestOpenPeriods.QueryCellInfo += gridControlOvertimeRequestOpenPeriods_QueryCellInfo;
			}
			gridControlOvertimeRequestOpenPeriods.CurrentCellCloseDropDown += gridControlOvertimeRequestOpenPeriods_CurrentCellCloseDropDown;
			gridControlOvertimeRequestOpenPeriods.KeyDown += gridControlOvertimeRequestOpenPeriods_KeyDown;
			gridControlOvertimeRequestOpenPeriods.ActivateToolTip += gridControlOvertimeRequestOpenPeriods_ActivateToolTip;
		}

		private void checkBoxAdvOvertimeProbability_CheckStateChanged(object sender, EventArgs e)
		{
			_presenter.SetOvertimeProbability(checkBoxAdvOvertimeProbability.Checked);
		}

		private void checkBoxAdvAutoGrantOvertimeRequest_CheckStateChanged(object sender, EventArgs e)
		{
			_presenter.SetAutoGrantOvertimeRequest(checkBoxAdvAutoGrantOvertimeRequest.Checked);
		}

		private void gridControlOvertimeRequestOpenPeriods_CheckBoxClick(object sender, GridCellClickEventArgs e)
		{
			if (e.ColIndex != _enableWorkRuleColumnIndex)
				return;

			if (!bool.TryParse(gridControlOvertimeRequestOpenPeriods[e.RowIndex, e.ColIndex].CellValue.ToString(), out bool oldCheckBoxValue))
				return;

			var workflowControlSetModel = (WorkflowControlSetModel)comboBoxAdvWorkflowControlSet.SelectedItem;
			if (workflowControlSetModel == null)
				return;

			var overtimeRequestPeriodModel = workflowControlSetModel.OvertimeRequestPeriodModels.ElementAt(e.RowIndex - _overtimeRequestOpenPeriodDataStartRowIndex);

			overtimeRequestPeriodModel.WorkRuleValidationHandleType = oldCheckBoxValue
				? null
				: OvertimeRequestPeriodModel.OvertimeRequestWorkRuleValidationHandleOptionViews[
					OvertimeValidationHandleType.Pending];

			gridControlOvertimeRequestOpenPeriods.RefreshRange(GridRangeInfo.Cell(e.RowIndex, e.ColIndex + 1));
		}

		private void gridControlOvertimeRequestOpenPeriods_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (e.RowIndex < _overtimeRequestOpenPeriodDataStartRowIndex || e.ColIndex != _enableWorkRuleColumnIndex + 1)
				return;

			var workflowControlSetModel = (WorkflowControlSetModel)comboBoxAdvWorkflowControlSet.SelectedItem;
			var overtimeRequestOpenPeriod = workflowControlSetModel.OvertimeRequestPeriodModels.ElementAt(e.RowIndex - _overtimeRequestOpenPeriodDataStartRowIndex);
			e.Style.Enabled = overtimeRequestOpenPeriod.EnableWorkRuleValidation;
		}

		private void gridControlOvertimeRequestOpenPeriods_ActivateToolTip(object sender, GridActivateToolTipEventArgs e)
		{
			if (e.RowIndex == 0 && (e.ColIndex == 3 || e.ColIndex == 4))
			{
				e.Style.CellTipText = " - " + Resources.NewMaxWeekWorkTimeRuleName + "\n" +
									  " - "	+ Resources.NewNightlyRestRuleName + "\n" +
									  " - " + Resources.MinWeeklyRestRuleName;
			}
		}

		private void gridControlOvertimeRequestOpenPeriods_CurrentCellCloseDropDown(object sender, PopupClosedEventArgs e)
		{
			var gridBase = (GridControl)sender;
			if (gridBase == null) return;
			if (gridBase.CurrentCell.RowIndex <= gridBase.CurrentCell.Model.Grid.Rows.HeaderCount ||
				gridBase.CurrentCell.ColIndex != 1) return;
			var chosenOvertimeRequestPeriodTypeModel = (OvertimeRequestPeriodTypeModel)gridBase.CurrentCellInfo.CellView.ControlValue;

			var overtimeRequestPeriodModel = _overtimeRequestOpenPeriodGridHelper.FindSelectedItem();
			if (overtimeRequestPeriodModel.PeriodType.Equals(chosenOvertimeRequestPeriodTypeModel)) return;
			_presenter.SetOvertimeRequestPeriodType(overtimeRequestPeriodModel, chosenOvertimeRequestPeriodTypeModel);
			gridControlOvertimeRequestOpenPeriods.Invalidate();
		}

		private void loadOvertimeRequestAutoGrantTypeAdapterCollection()
		{
			_overtimeRequestAutoGrantTypeAdapterCollection = new List<OvertimeRequestAutoGrantTypeAdapter>();

			foreach (var value in Enum.GetValues(typeof(OvertimeRequestAutoGrantType)))
			{
				var autoGrantTypeView = new OvertimeRequestAutoGrantTypeAdapter((OvertimeRequestAutoGrantType)value);
				_overtimeRequestAutoGrantTypeAdapterCollection.Add(autoGrantTypeView);
			}
		}

		private void toolStripMenuItemOvertimeRequestDelete_Click(object sender, EventArgs e)
		{
			buttonAdvDeleteOvertimeRequestPeriod_Click(sender, e);
		}

		private void toolStripMenuItemOvertimeRequestMoveUp_Click(object sender, EventArgs e)
		{

			GridRangeInfo activeRange = gridControlOvertimeRequestOpenPeriods.Selections.Ranges.ActiveRange;

			if (activeRange.Top <= gridControlOvertimeRequestOpenPeriods.Rows.HeaderCount + 1) return;
			var selectedItems = _overtimeRequestOpenPeriodGridHelper.FindItemsBySelectionOrPoint(_gridPoint);
			gridControlOvertimeRequestOpenPeriods.Selections.Clear();
			foreach (var overtimeRequestPeriodModel in selectedItems)
			{
				_presenter.MoveUp(overtimeRequestPeriodModel);
			}

			gridControlOvertimeRequestOpenPeriods.Selections.ChangeSelection(activeRange, activeRange.OffsetRange(-1, 0));
		}

		private void toolStripMenuItemOvertimeRequestMoveDown_Click(object sender, EventArgs e)
		{
			var activeRange = gridControlOvertimeRequestOpenPeriods.Selections.Ranges.ActiveRange;
			if (activeRange.Bottom >= gridControlOvertimeRequestOpenPeriods.RowCount) return;
			var selectedItems = _overtimeRequestOpenPeriodGridHelper.FindItemsBySelectionOrPoint(_gridPoint);
			gridControlOvertimeRequestOpenPeriods.Selections.Clear();
			foreach (var overtimeRequestPeriodModel in selectedItems.Reverse())
			{
				_presenter.MoveDown(overtimeRequestPeriodModel);
			}

			gridControlOvertimeRequestOpenPeriods.Selections.ChangeSelection(activeRange, activeRange.OffsetRange(1, 0));
		}

		private void enableOvertimeRequestContextMenu(bool value)
		{
			foreach (ToolStripItem item in contextMenuStripOvertimeRequestOpenPeriodsGrid.Items)
			{
				item.Enabled = value;
			}
		}

		private void toolStripMenuItemOvertimeRequestFromToPeriod_Click(object sender, EventArgs e)
		{
			_presenter.AddOvertimeRequestOpenDatePeriod();
		}

		private void toolStripMenuItemOvertimeRequestRollingPeriod_Click(object sender, EventArgs e)
		{
			_presenter.AddOvertimeRequestOpenRollingPeriod();
		}

		private void buttonAdvDeleteOvertimeRequestPeriod_Click(object sender, EventArgs e)
		{
			ReadOnlyCollection<OvertimeRequestPeriodModel> selectedOvertimeRequestPeriodModels =
				_overtimeRequestOpenPeriodGridHelper.FindSelectedItems(gridControlOvertimeRequestOpenPeriods.Rows.HeaderCount + 1);

			_presenter.DeleteOvertimeRequestPeriod(new List<OvertimeRequestPeriodModel>(selectedOvertimeRequestPeriodModels));
		}

		private void buttonAddOvertimeRequestPeriod_Click(object sender, EventArgs e)
		{
			_presenter.AddOvertimeRequestOpenDatePeriod();
		}

		private void gridControlOvertimeRequestOpenPeriods_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Delete) return;
			deleteSelectedOvertimeRequestOpenPeriod();
			e.Handled = true;
		}

		private void deleteSelectedOvertimeRequestOpenPeriod()
		{
			ReadOnlyCollection<OvertimeRequestPeriodModel> selectedPeriodModels =
				_overtimeRequestOpenPeriodGridHelper.FindSelectedItems(gridControlOvertimeRequestOpenPeriods.Rows.HeaderCount + 1);

			_presenter.DeleteOvertimeRequestPeriod(new List<OvertimeRequestPeriodModel>(selectedPeriodModels));
		}

		private void gridControlOvertimeRequestOpenPeriods_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right) return;
			_gridPoint = e.Location;
			enableOvertimeRequestContextMenu(false);
			if (_overtimeRequestOpenPeriodGridHelper == null)
				return;

			OvertimeRequestPeriodModel rightClickedItem = _overtimeRequestOpenPeriodGridHelper.FindItemByPoint(_gridPoint);
			if (rightClickedItem == null) return;
			enableOvertimeRequestContextMenu(true);

			var selectedItems = _overtimeRequestOpenPeriodGridHelper.FindSelectedItems(gridControlOvertimeRequestOpenPeriods.Rows.HeaderCount + 1);
			if (selectedItems.Contains(rightClickedItem)) return;
			gridControlOvertimeRequestOpenPeriods.Selections.Clear();
			gridControlOvertimeRequestOpenPeriods.CurrentCell.Deactivate(false);
			gridControlOvertimeRequestOpenPeriods.Selections.SelectRange(
				gridControlOvertimeRequestOpenPeriods.PointToRangeInfo(_gridPoint), true);
		}
	}
}
