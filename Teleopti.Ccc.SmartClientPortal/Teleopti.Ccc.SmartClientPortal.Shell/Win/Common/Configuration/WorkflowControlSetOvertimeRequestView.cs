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
using Teleopti.Ccc.Domain.Security.AuthorizationData;
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
		private ILicenseAvailability _licenseAvailability = new LicenseAvailability(CurrentDataSource.Make());

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

		public void SetOvertimeRequestUsePrimarySkill(bool usePrimarySkill)
		{
			checkBoxUsePrimarySkill.CheckStateChanged -= checkBoxUsePrimarySkill_CheckStateChanged;
			checkBoxUsePrimarySkill.Checked = usePrimarySkill;
			checkBoxUsePrimarySkill.CheckStateChanged += checkBoxUsePrimarySkill_CheckStateChanged;
		}

		public void SetOvertimeRequestMaximumTimeEnabled(bool overtimeRequestMaximumTimeEnabled)
		{
			checkBoxAdvOvertimeMaximumEnabled.CheckStateChanged -= CheckBoxAdvOvertimeMaximumEnabled_CheckStateChanged;
			checkBoxAdvOvertimeMaximumEnabled.Checked = overtimeRequestMaximumTimeEnabled;
			checkBoxAdvOvertimeMaximumEnabled.CheckStateChanged += CheckBoxAdvOvertimeMaximumEnabled_CheckStateChanged;
			CheckBoxAdvOvertimeMaximumEnabled_CheckStateChanged(this, EventArgs.Empty);
		}

		public void SetOverTimeRequestMaximumContinuousWorkTimeHandleType(
			OvertimeRequestValidationHandleOptionView
				selectedModelOvertimeRequestMaximumContinuousWorkTimeValidationHandleOptionView)
		{
			comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.SelectedItem = selectedModelOvertimeRequestMaximumContinuousWorkTimeValidationHandleOptionView;
			if (comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.SelectedItem == null)
				comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.SelectedIndex = 0;
		}

		public void SetOverTimeRequestMaximumContinuousWorkTime(TimeSpan? selectedModelOvertimeRequestMaximumContinuousWorkTime)
		{
			if (selectedModelOvertimeRequestMaximumContinuousWorkTime.HasValue && selectedModelOvertimeRequestMaximumContinuousWorkTime.Value != TimeSpan.Zero)
			{
				timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.SetInitialResolution(selectedModelOvertimeRequestMaximumContinuousWorkTime.Value);
			}
		}

		public void SetOverTimeRequestMinimumRestTimeThreshold(TimeSpan? selectedModelOvertimeRequestMinimumRestTimeThreshold)
		{
			if (selectedModelOvertimeRequestMinimumRestTimeThreshold.HasValue && selectedModelOvertimeRequestMinimumRestTimeThreshold.Value != TimeSpan.Zero)
			{
				timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.SetInitialResolution(selectedModelOvertimeRequestMinimumRestTimeThreshold.Value);
			}
		}

		public void SetOvertimeRequestMaximumContinuousWorkTimeEnabled(
			bool selectedModelOvertimeRequestMaximumContinuousWorkTimeEnabled)
		{
			checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.CheckStateChanged -=
				checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled_CheckStateChanged;
			checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.Checked =
				selectedModelOvertimeRequestMaximumContinuousWorkTimeEnabled;
			checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.CheckStateChanged +=
				checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled_CheckStateChanged;
			checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled_CheckStateChanged(this, EventArgs.Empty);
		}

		public void SetOverTimeRequestMaximumTimeHandleType(
			OvertimeRequestValidationHandleOptionView overtimeRequestValidationHandleOptionView)
		{
			comboBoxOvertimeRequestMaximumTimeHandleType.SelectedItem = overtimeRequestValidationHandleOptionView;
			if (comboBoxOvertimeRequestMaximumTimeHandleType.SelectedItem == null)
				comboBoxOvertimeRequestMaximumTimeHandleType.SelectedIndex = 0;
		}

		public void SetOverTimeRequestStaffingCheckMethod(
			OvertimeRequestStaffingCheckMethodOptionView overtimeRequestStaffingCheckMethodOptionView)
		{
			comboBoxOvertimeRequestStaffingCheckMethod.SelectedItem = overtimeRequestStaffingCheckMethodOptionView;
			if (comboBoxOvertimeRequestStaffingCheckMethod.SelectedItem == null)
				comboBoxOvertimeRequestStaffingCheckMethod.SelectedIndex = 1;
		}

		public void SetOverTimeRequestMaximumTime(TimeSpan? selectedModelOvertimeRequestMaximumTime)
		{
			if (selectedModelOvertimeRequestMaximumTime.HasValue && selectedModelOvertimeRequestMaximumTime.Value != TimeSpan.Zero)
			{
				timeSpanTextBoxOvertimeRequestMaximumTime.SetInitialResolution(selectedModelOvertimeRequestMaximumTime.Value);
				comboBoxOvertimeRequestMaximumTimeHandleType.Enabled = true;
			}
		}

		private void checkOvertimeProbabilityLicense()
		{
			if (!_licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiCccOvertimeAvailability) &&
				!_licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiWfmOvertimeRequests))
			{
				tabPageAdvETOTRequest.Hide();
			}
		}

		private void checkOvertimeRequestsLicense()
		{
			var hasLicense = _licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiWfmOvertimeRequests);

			if (!hasLicense)
			{
				tableLayoutPanelOvertimeMaximumSetting.Visible = false;
				tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.Visible = false;
				tableLayoutPanelOpenForOvertimeRequests.Visible = false;
				gridControlOvertimeRequestOpenPeriods.Visible = false;
			}
		}

		private void checkOvertimeStaffingCheckMethodToggle()
		{
			if (!_toggleManager.IsEnabled(Toggles.OvertimeRequestStaffingCheckMethod_74949))
			{
				tableLayoutPanelOvertimeStaffingCheckMethodSetting.Visible = false;
				var rowIndex = tableLayoutPanelETOTRequest.Controls.GetChildIndex(tableLayoutPanelOvertimeStaffingCheckMethodSetting);
				tableLayoutPanelETOTRequest.RowStyles[rowIndex].Height = 0;
			}
		}

		private void initOvertimeRequestMaximumTimeHandleType()
		{
			comboBoxOvertimeRequestMaximumTimeHandleType.DataSource =
				WorkflowControlSetModel.OvertimeRequestWorkRuleValidationHandleOptionViews.Select(s => s.Value).ToList();
			comboBoxOvertimeRequestMaximumTimeHandleType.DisplayMember = "Description";
		}

		private void initOvertimeRequestStaffingCheckMethod()
		{
			comboBoxOvertimeRequestStaffingCheckMethod.DataSource =
				WorkflowControlSetModel.OvertimeRequestStaffingCheckMethodOptionViews.Select(s => s.Value).ToList();
			comboBoxOvertimeRequestStaffingCheckMethod.DisplayMember = "Description";
		}

		private void initOvertimeRequestMaximumContinuousWorkTimeHandleType()
		{
			comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.DataSource =
				WorkflowControlSetModel.OvertimeRequestWorkRuleValidationHandleOptionViews.Select(s => s.Value).ToList();
			comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.DisplayMember = "Description";
		}

		private void initOvertimeRequestUsePrimarySkill()
		{
			if (!_toggleManager.IsEnabled(Toggles.OvertimeRequestUsePrimarySkillOption_75573) || !_presenter.IsUsingPrimarySkill())
			{
				checkBoxUsePrimarySkill.Visible = false;
			}
		}
		private void timeSpanTextBoxOvertimeRequestMaximumTime_Leave(object sender, EventArgs e)
		{
			_presenter.SetOvertimeRequestMaximumTime(timeSpanTextBoxOvertimeRequestMaximumTime.Value);
		}

		private void ComboBoxOvertimeRequestMaximumTimeHandleType_SelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.SetOvertimeRequestMaximumTimeHandleType(
				(OvertimeRequestValidationHandleOptionView)comboBoxOvertimeRequestMaximumTimeHandleType.SelectedItem);
		}

		private void ComboBoxOvertimeRequestStaffingCheckMethod_SelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.SetOvertimeRequestIntradayStaffingCheckMethod(
				(OvertimeRequestStaffingCheckMethodOptionView)comboBoxOvertimeRequestStaffingCheckMethod.SelectedItem);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void configureOvertimeRequestPeriodGrid()
		{
			var columnList = new List<SFGridColumnBase<OvertimeRequestPeriodModel>>
			{
				new SFGridRowHeaderColumn<OvertimeRequestPeriodModel>(string.Empty)
			};

			gridControlOvertimeRequestOpenPeriods.CellModels.Add("IgnoreCell",
				new IgnoreCellModel(gridControlOvertimeRequestOpenPeriods.Model));
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

			WorkflowControlSetModel.SetSupportedSkillTypes(_presenter.SupportedSkillTypes());

			var periodTypeDropDownColumn = new SFGridDropDownColumn
				<OvertimeRequestPeriodModel, OvertimeRequestPeriodTypeModel>(
					"PeriodType", Resources.Type, " ", WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters, "DisplayText",
					null);

			columnList.Add(periodTypeDropDownColumn);

			var autoGrantColumn =
				new SFGridDropDownEnumColumn<OvertimeRequestPeriodModel, OvertimeRequestAutoGrantTypeAdapter,
					OvertimeRequestAutoGrantType>(
					"AutoGrantType", Resources.AutoGrant, " ", _overtimeRequestAutoGrantTypeAdapterCollection, "DisplayName",
					"AutoGrantType");

			columnList.Add(autoGrantColumn);

			columnList.Add(new SFGridCheckBoxColumn<OvertimeRequestPeriodModel>("EnableWorkRuleValidation", Resources.Enabled,
				Resources.ContractWorkRuleValidation));
			columnList.Add(new SFGridDropDownColumn<OvertimeRequestPeriodModel, OvertimeRequestValidationHandleOptionView>(
				"WorkRuleValidationHandleType",
				Resources.WhenValidationFails, Resources.ContractWorkRuleValidation,
				OvertimeRequestPeriodModel.OvertimeRequestWorkRuleValidationHandleOptionViews.Values.ToList(), "Description",
				typeof(OvertimeRequestValidationHandleOptionView)));

			var enabledMultiSelect = _toggleManager.IsEnabled(Toggles.OvertimeRequestSupportMultiSelectionSkillTypes_74945);
			var comboBoxCellModel = new GridDropDownAdvComboBoxCellModel(gridControlOvertimeRequestOpenPeriods.Model, enabledMultiSelect);

			gridControlOvertimeRequestOpenPeriods.CellModels.Add("MultiSelectCellModel", comboBoxCellModel);

			var skillTypeColumn =
				new SFGridMultiSelectDropDownColumn
					<OvertimeRequestPeriodModel, OvertimeRequestPeriodSkillTypeModel>(
						"SkillTypes", Resources.SkillType, " ", WorkflowControlSetModel.DefaultOvertimeRequestSkillTypeAdapters,
						"DisplayText", null);

			columnList.Add(skillTypeColumn);

			columnList.Add(new DateOnlyColumn<OvertimeRequestPeriodModel>("PeriodStartDate", Resources.Start, Resources.Period)
			{
				CellValidator = new OvertimeRequestDatePeriodStartCellValidator(_toggleManager)
			});
			columnList.Add(new DateOnlyColumn<OvertimeRequestPeriodModel>("PeriodEndDate", Resources.End, Resources.Period)
			{
				CellValidator = new OvertimeRequestDatePeriodEndCellValidator(_toggleManager)
			});
			columnList.Add(
				new SFGridIntegerCellWithIgnoreColumn<OvertimeRequestPeriodModel>("RollingStart", Resources.Start,
					Resources.Rolling)
				{
					CellValidator = new OvertimeRequestRollingPeriodStartCellValidator(_toggleManager)
				});
			columnList.Add(
				new SFGridIntegerCellWithIgnoreColumn<OvertimeRequestPeriodModel>("RollingEnd", Resources.End, Resources.Rolling)
				{
					CellValidator = new OvertimeRequestRollingPeriodEndCellValidator(_toggleManager)
				});

			gridControlOvertimeRequestOpenPeriods.Model.Options.MergeCellsMode =
				GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow;
			gridControlOvertimeRequestOpenPeriods.Rows.HeaderCount = 1;

			var gridColumns = new ReadOnlyCollection<SFGridColumnBase<OvertimeRequestPeriodModel>>(columnList);
			_overtimeRequestOpenPeriodGridHelper = new SFGridColumnGridHelper<OvertimeRequestPeriodModel>(
				gridControlOvertimeRequestOpenPeriods, gridColumns, new List<OvertimeRequestPeriodModel>());

			gridControlOvertimeRequestOpenPeriods.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
			gridControlOvertimeRequestOpenPeriods.Model.Options.ExcelLikeCurrentCell = true;

			gridControlOvertimeRequestOpenPeriods.CheckBoxClick += gridControlOvertimeRequestOpenPeriods_CheckBoxClick;
			gridControlOvertimeRequestOpenPeriods.QueryCellInfo += gridControlOvertimeRequestOpenPeriods_QueryCellInfo;

			gridControlOvertimeRequestOpenPeriods.CurrentCellCloseDropDown +=
				gridControlOvertimeRequestOpenPeriods_CurrentCellCloseDropDown;
			gridControlOvertimeRequestOpenPeriods.KeyDown += gridControlOvertimeRequestOpenPeriods_KeyDown;
			gridControlOvertimeRequestOpenPeriods.ActivateToolTip += gridControlOvertimeRequestOpenPeriods_ActivateToolTip;
		}

		private void checkBoxAdvOvertimeProbability_CheckStateChanged(object sender, EventArgs e)
		{
			_presenter.SetOvertimeProbability(checkBoxAdvOvertimeProbability.Checked);
		}
		private void checkBoxUsePrimarySkill_CheckStateChanged(object sender, EventArgs e)
		{
			_presenter.SetOvertimeRequestUsePrimarySkill(checkBoxUsePrimarySkill.Checked);
		}

		private void CheckBoxAdvOvertimeMaximumEnabled_CheckStateChanged(object sender, EventArgs e)
		{
			timeSpanTextBoxOvertimeRequestMaximumTime.Enabled = checkBoxAdvOvertimeMaximumEnabled.Checked;
			comboBoxOvertimeRequestMaximumTimeHandleType.Enabled = checkBoxAdvOvertimeMaximumEnabled.Checked;
			_presenter.SetOvertimeRequestMaximumEnabled(checkBoxAdvOvertimeMaximumEnabled.Checked);
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

			if (workflowControlSetModel.OvertimeRequestPeriodModels == null || !workflowControlSetModel.OvertimeRequestPeriodModels.Any())
			{
				return;
			}

			var overtimeRequestOpenPeriod = workflowControlSetModel.OvertimeRequestPeriodModels.ElementAt(e.RowIndex - _overtimeRequestOpenPeriodDataStartRowIndex);
			e.Style.Enabled = overtimeRequestOpenPeriod.EnableWorkRuleValidation;
		}

		private void gridControlOvertimeRequestOpenPeriods_ActivateToolTip(object sender, GridActivateToolTipEventArgs e)
		{
			if (e.RowIndex == 0 && (e.ColIndex == 3 || e.ColIndex == 4))
			{
				e.Style.CellTipText = " - " + Resources.NewMaxWeekWorkTimeRuleName + "\n" +
									  " - " + Resources.NewNightlyRestRuleName + "\n" +
									  " - " + Resources.MinWeeklyRestRuleName;
			}
		}

		private void gridControlOvertimeRequestOpenPeriods_CurrentCellCloseDropDown(object sender, PopupClosedEventArgs e)
		{
			var gridBase = (GridControl)sender;
			if (gridBase == null) return;

			var chosenOvertimeRequestPeriodSkillTypeModel =
				gridBase.CurrentCellInfo.CellView.ControlValue as OvertimeRequestPeriodSkillTypeModel;
			var chosenOvertimeRequestPeriodTypeModel = gridBase.CurrentCellInfo.CellView.ControlValue as OvertimeRequestPeriodTypeModel;

			if (gridBase.CurrentCell.RowIndex <= gridBase.CurrentCell.Model.Grid.Rows.HeaderCount ||
				chosenOvertimeRequestPeriodTypeModel == null && chosenOvertimeRequestPeriodSkillTypeModel == null) return;

			var overtimeRequestPeriodModel = _overtimeRequestOpenPeriodGridHelper.FindSelectedItem();
			if (chosenOvertimeRequestPeriodTypeModel != null)
			{
				if (overtimeRequestPeriodModel.PeriodType.Equals(chosenOvertimeRequestPeriodTypeModel)) return;
				_presenter.SetOvertimeRequestPeriodType(overtimeRequestPeriodModel, chosenOvertimeRequestPeriodTypeModel);
			}

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

		private void checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled_CheckStateChanged(object sender, EventArgs e)
		{
			timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.Enabled = checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.Checked;
			comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.Enabled = checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.Checked;
			timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.Enabled = checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.Checked;
			_presenter.SetOvertimeRequestMaximumContinuousWorkTimeEnabled(checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.Checked);
		}

		private void timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold_Leave(object sender, EventArgs e)
		{
			_presenter.SetOvertimeRequestMinimumRestTimeThreshold(timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.Value);
		}

		private void timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime_Leave(object sender, EventArgs e)
		{
			_presenter.SetOvertimeRequestMaximumContinuousWorkTime(timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.Value);
		}

		private void ComboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType_SelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.SetOvertimeRequestMaximumContinuousWorkTimeHandleType(
				(OvertimeRequestValidationHandleOptionView)comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.SelectedItem);
		}
	}
}
