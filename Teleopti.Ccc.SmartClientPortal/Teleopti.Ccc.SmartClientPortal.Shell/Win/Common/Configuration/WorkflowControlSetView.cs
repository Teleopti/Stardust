using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateTimePeriodVisualizer;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class WorkflowControlSetView : BaseUserControl, ISettingPage, IWorkflowControlSetView
	{
		private readonly IToggleManager _toggleManager;
		private readonly WorkflowControlSetPresenter _presenter;
		private SFGridColumnGridHelper<AbsenceRequestPeriodModel> _gridHelper;
		private SFGridColumnGridHelper<OvertimeRequestPeriodModel> _overtimeRequestOpenPeriodGridHelper;

		private IDictionary<IAbsence, MonthlyProjectionVisualiser> _projectionCache =
			new Dictionary<IAbsence, MonthlyProjectionVisualiser>();

		private Point _gridPoint;

		public WorkflowControlSetView()
		{
			InitializeComponent();
		}

		public WorkflowControlSetView(IToggleManager toggleManager) : this()
		{
			_toggleManager = toggleManager;
			if (DesignMode) return;
			_presenter = new WorkflowControlSetPresenter(this, UnitOfWorkFactory.Current, new RepositoryFactory(), _toggleManager);
			GridStyleInfoStore.CellValueProperty.IsCloneable = false;
			dateTimePickerAdvPublishedTo.NullString = Resources.NotPublished;
			dateOnlyPeriodsVisualizer1.Culture = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture;
			timeSpanTextBox1.TimeSpanBoxWidth = timeSpanTextBox1.Width;
			dateTimePickerAdvViewpoint.SetCultureInfoSafe(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);
			SetMaxConsecutiveWorkingDaysVisibility(_toggleManager);
			checkOvertimeProbabilityLicense();
			checkOvertimeRequestsLicense();
			loadOvertimeRequestAutoGrantTypeAdapterCollection();
		}

		public WorkflowControlSetView(IToggleManager toggleManager, WorkflowControlSetPresenter presenter) : this(toggleManager)
		{
			_presenter = presenter;
			_presenter.SetParentView(this);
		}

		private void SetMaxConsecutiveWorkingDaysVisibility(IToggleManager toggleManager)
		{
			if (!toggleManager.IsEnabled(Toggles.MyTimeWeb_ShiftTradeRequest_MaximumWorkdayCheck_74889))
			{
				tableLayoutPanelShiftTrade.RowStyles[tableLayoutPanelShiftTrade.GetRow(this.tableLayoutPanel3)].Height = 0;
				tableLayoutPanelShiftTrade.RowStyles[tableLayoutPanelShiftTrade.GetRow(this.panel10)].Height = 0;
				this.tableLayoutPanel3.Visible = false;
				this.panel10.Visible = false;

			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void configureAbsenceRequestPeriodGrid()
		{
			var columnList = new List<SFGridColumnBase<AbsenceRequestPeriodModel>>
			{
				new SFGridRowHeaderColumn<AbsenceRequestPeriodModel>(string.Empty)
			};

			// Add cellmodels
			gridControlAbsenceRequestOpenPeriods.CellModels.Add("IgnoreCell", new IgnoreCellModel(gridControlAbsenceRequestOpenPeriods.Model));
			var cellModel = new GridDropDownMonthCalendarAdvCellModel(gridControlAbsenceRequestOpenPeriods.Model);
			cellModel.HideNoneButton();
			cellModel.HideTodayButton();
			gridControlAbsenceRequestOpenPeriods.CellModels.Add(GridCellModelConstants.CellTypeDatePickerCell, cellModel);
			var cell = new NullableIntegerCellModel(gridControlAbsenceRequestOpenPeriods.Model)
			{
				MinValue = 0,
				MaxValue = 999
			};
			gridControlAbsenceRequestOpenPeriods.CellModels.Add("IntegerCellModel", cell);

			var periodTypeDropDownColumn = new SFGridDropDownColumn
				<AbsenceRequestPeriodModel, AbsenceRequestPeriodTypeModel>(
				"PeriodType", Resources.Type, " ", WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters, "DisplayText", null);

			columnList.Add(periodTypeDropDownColumn);

			var absenceDropdownColumn = new SFGridDropDownColumn
				<AbsenceRequestPeriodModel, IAbsence>(
				"Absence", Resources.Absence, " ", _presenter.RequestableAbsenceCollection, "Name", typeof(IAbsence));

			columnList.Add(absenceDropdownColumn);

			var checkPersonAccountColumn =
				new SFGridDynamicDropDownColumn<AbsenceRequestPeriodModel, IAbsenceRequestValidator>(
					"PersonAccountValidator", Resources.CheckPersonAccount, " ", "PersonAccountValidatorList", "DisplayText",
					typeof(IAbsenceRequestValidator));

			columnList.Add(checkPersonAccountColumn);
			var checkStaffingColumn =
				new SFGridDynamicDropDownColumn<AbsenceRequestPeriodModel, IAbsenceRequestValidator>(
					"StaffingThresholdValidator", Resources.CheckStaffing, " ", "StaffingThresholdValidatorList", "DisplayText",
					typeof(IAbsenceRequestValidator));

			columnList.Add(checkStaffingColumn);

			var autoGrantColumn =
				new SFGridDynamicDropDownColumn<AbsenceRequestPeriodModel, IProcessAbsenceRequest>(
					"AbsenceRequestProcess", Resources.AutoGrant, " ", "AbsenceRequestProcessList", "DisplayText",
					typeof(IProcessAbsenceRequest));

			columnList.Add(autoGrantColumn);

			columnList.Add(new DateOnlyColumn<AbsenceRequestPeriodModel>("PeriodStartDate", Resources.Start, Resources.Period));
			columnList.Add(new DateOnlyColumn<AbsenceRequestPeriodModel>("PeriodEndDate", Resources.End, Resources.Period));
			columnList.Add(new SFGridIntegerCellWithIgnoreColumn<AbsenceRequestPeriodModel>("RollingStart", Resources.Start, Resources.Rolling));
			columnList.Add(new SFGridIntegerCellWithIgnoreColumn<AbsenceRequestPeriodModel>("RollingEnd", Resources.End, Resources.Rolling));
			columnList.Add(new DateOnlyColumn<AbsenceRequestPeriodModel>("OpenStartDate", Resources.Start, Resources.Open));
			columnList.Add(new DateOnlyColumn<AbsenceRequestPeriodModel>("OpenEndDate", Resources.End, Resources.Open));

			gridControlAbsenceRequestOpenPeriods.Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow;
			gridControlAbsenceRequestOpenPeriods.Rows.HeaderCount = 1;

			// Adds column list.
			var gridColumns = new ReadOnlyCollection<SFGridColumnBase<AbsenceRequestPeriodModel>>(columnList);
			_gridHelper = new SFGridColumnGridHelper<AbsenceRequestPeriodModel>(gridControlAbsenceRequestOpenPeriods,
																						gridColumns, new List<AbsenceRequestPeriodModel>());
			//_gridHelper.UnbindClipboardPasteEvent();

			gridControlAbsenceRequestOpenPeriods.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
			gridControlAbsenceRequestOpenPeriods.Model.Options.ExcelLikeCurrentCell = true;

			gridControlAbsenceRequestOpenPeriods.CurrentCellCloseDropDown += gridControlAbsenceRequestOpenPeriods_CurrentCellCloseDropDown;
			gridControlAbsenceRequestOpenPeriods.SaveCellInfo += gridControlAbsenceRequestOpenPeriods_SaveCellInfo;
			gridControlAbsenceRequestOpenPeriods.KeyDown += gridControlAbsenceRequestOpenPeriods_KeyDown;

			twoListSelectorDayOffs.SelectedAdded += twoListSelectorDayOffs_SelectedAdded;
			twoListSelectorDayOffs.SelectedRemoved += twoListSelectorDayOffs_SelectedRemoved;

			twoListSelectorCategories.SelectedAdded += twoListSelectorCategories_SelectedAdded;
			twoListSelectorCategories.SelectedRemoved += twoListSelectorCategories_SelectedRemoved;

			twoListSelectorAbsences.SelectedAdded += twoListSelectorAbsencesSelectedAdded;
			twoListSelectorAbsences.SelectedRemoved += twoListSelectorAbsencesSelectedRemoved;

			twoListSelectorAbsencesForReport.SelectedAdded += twoListSelectorAbsencesForReportSelectedAdded;
			twoListSelectorAbsencesForReport.SelectedRemoved += twoListSelectorAbsencesForReportSelectedRemoved;

			twoListSelectorMatchingSkills.SelectedAdded += twoListSelectorMatchingSkills_SelectedAdded;
			twoListSelectorMatchingSkills.SelectedRemoved += twoListSelectorMatchingSkills_SelectedRemoved;
		}

		private void twoListSelectorAbsencesSelectedRemoved(object sender, Controls.SelectedChangedEventArgs e)
		{
			var item = e.MovedItem as IAbsence;
			if (item != null)
				_presenter.RemoveAllowedPreferenceAbsence(item);
		}

		private void twoListSelectorAbsencesSelectedAdded(object sender, Controls.SelectedChangedEventArgs e)
		{
			var item = e.MovedItem as IAbsence;
			if (item != null)
				_presenter.AddAllowedPreferenceAbsence(item);
		}

		private void twoListSelectorAbsencesForReportSelectedRemoved(object sender, Controls.SelectedChangedEventArgs e)
		{
			var item = e.MovedItem as IAbsence;
			if (item != null)
				_presenter.RemoveAllowedAbsenceForReport(item);
		}

		private void twoListSelectorAbsencesForReportSelectedAdded(object sender, Controls.SelectedChangedEventArgs e)
		{
			var item = e.MovedItem as IAbsence;
			if (item != null)
				_presenter.AddAllowedAbsenceForReport(item);
		}

		private void releaseMangedResources()
		{
			twoListSelectorDayOffs.SelectedAdded -= twoListSelectorDayOffs_SelectedAdded;
			twoListSelectorDayOffs.SelectedRemoved -= twoListSelectorDayOffs_SelectedRemoved;

			twoListSelectorCategories.SelectedAdded -= twoListSelectorCategories_SelectedAdded;
			twoListSelectorCategories.SelectedRemoved -= twoListSelectorCategories_SelectedRemoved;

			twoListSelectorAbsences.SelectedAdded -= twoListSelectorAbsencesSelectedAdded;
			twoListSelectorAbsences.SelectedRemoved -= twoListSelectorAbsencesSelectedRemoved;

			twoListSelectorAbsencesForReport.SelectedAdded -= twoListSelectorAbsencesForReportSelectedAdded;
			twoListSelectorAbsencesForReport.SelectedRemoved -= twoListSelectorAbsencesForReportSelectedRemoved;

			twoListSelectorMatchingSkills.SelectedAdded -= twoListSelectorMatchingSkills_SelectedAdded;
			twoListSelectorMatchingSkills.SelectedRemoved -= twoListSelectorMatchingSkills_SelectedRemoved;

			gridControlAbsenceRequestOpenPeriods.CurrentCellCloseDropDown -= gridControlAbsenceRequestOpenPeriods_CurrentCellCloseDropDown;
			gridControlAbsenceRequestOpenPeriods.SaveCellInfo -= gridControlAbsenceRequestOpenPeriods_SaveCellInfo;
			gridControlAbsenceRequestOpenPeriods.KeyDown -= gridControlAbsenceRequestOpenPeriods_KeyDown;

			_gridHelper?.Dispose();
			_gridHelper = null;
			_projectionCache = null;

			_overtimeRequestOpenPeriodGridHelper?.Dispose();
			_overtimeRequestOpenPeriodGridHelper = null;
		}

		private void twoListSelectorCategories_SelectedRemoved(object sender, Controls.SelectedChangedEventArgs e)
		{
			var item = e.MovedItem as IShiftCategory;
			if (item != null)
				_presenter.RemoveAllowedPreferenceShiftCategory(item);
		}

		private void twoListSelectorCategories_SelectedAdded(object sender, Controls.SelectedChangedEventArgs e)
		{
			var item = e.MovedItem as IShiftCategory;
			if (item != null)
				_presenter.AddAllowedPreferenceShiftCategory(item);
		}

		private void twoListSelectorDayOffs_SelectedRemoved(object sender, Controls.SelectedChangedEventArgs e)
		{
			var item = e.MovedItem as IDayOffTemplate;
			if (item != null)
				_presenter.RemoveAllowedPreferenceDayOff(item);
		}

		private void twoListSelectorDayOffs_SelectedAdded(object sender, Controls.SelectedChangedEventArgs e)
		{
			var item = e.MovedItem as IDayOffTemplate;
			if (item != null)
				_presenter.AddAllowedPreferenceDayOff(item);
		}

		private void twoListSelectorMatchingSkills_SelectedRemoved(object sender, Controls.SelectedChangedEventArgs e)
		{
			var item = e.MovedItem as ISkill;
			if (item != null)
				_presenter.RemoveSkillFromMatchList(item);
		}

		private void twoListSelectorMatchingSkills_SelectedAdded(object sender, Controls.SelectedChangedEventArgs e)
		{
			var item = e.MovedItem as ISkill;
			if (item != null)
				_presenter.AddSkillToMatchList(item);
		}

		private void gridControlAbsenceRequestOpenPeriods_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Delete) return;
			deleteSelected();
			e.Handled = true;
		}

		private void gridControlAbsenceRequestOpenPeriods_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			refreshProjectionGrid();
		}

		private void gridControlAbsenceRequestOpenPeriods_CurrentCellCloseDropDown(object sender, PopupClosedEventArgs e)
		{
			var gridBase = (GridControl)sender;
			if (gridBase == null) return;
			if (gridBase.CurrentCell.RowIndex <= gridBase.CurrentCell.Model.Grid.Rows.HeaderCount ||
				gridBase.CurrentCell.ColIndex != 1) return;
			var chosenAbsenceRequestPeriodTypeModel = (AbsenceRequestPeriodTypeModel)gridBase.CurrentCellInfo.CellView.ControlValue;

			// Gets the current item.
			var absenceRequestPeriodModel = _gridHelper.FindSelectedItem();
			if (absenceRequestPeriodModel.PeriodType.Equals(chosenAbsenceRequestPeriodTypeModel)) return;
			// Replace selected item with a new definition of different type
			_presenter.SetPeriodType(absenceRequestPeriodModel, chosenAbsenceRequestPeriodTypeModel);
			gridControlAbsenceRequestOpenPeriods.Invalidate();
		}

		private void comboBoxAdvWorkflowControlSet_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxAdvWorkflowControlSet.SelectedItem == null) return;
			var selectedItem = (WorkflowControlSetModel)comboBoxAdvWorkflowControlSet.SelectedItem;
			gridControlAbsenceRequestOpenPeriods.BeginUpdate();
			_presenter.SetSelectedWorkflowControlSetModel(selectedItem);
			_gridHelper.SetSourceList(selectedItem.AbsenceRequestPeriodModels);
			_overtimeRequestOpenPeriodGridHelper.SetSourceList(selectedItem.OvertimeRequestPeriodModels);
			gridControlAbsenceRequestOpenPeriods.EndUpdate();
			refreshProjectionGrid();

			comboBoxAdvAllowedPreferenceActivity.SelectedIndexChanged -= comboBoxAdvAllowedPreferenceActivity_SelectedIndexChanged;
			dateTimePickerAdvPublishedTo.ValueChanged -= dateTimePickerAdvPublishedTo_ValueChanged;

			comboBoxAdvAllowedPreferenceActivity.SelectedItem = selectedItem.AllowedPreferenceActivity;
			if (selectedItem.SchedulePublishedToDate.HasValue)
				dateTimePickerAdvPublishedTo.Value = selectedItem.SchedulePublishedToDate.Value;
			else
			{
				dateTimePickerAdvPublishedTo.Value = DateTime.Today;
				dateTimePickerAdvPublishedTo.IsNullDate = true;
			}

			dateTimePickerAdvPublishedTo.ValueChanged += dateTimePickerAdvPublishedTo_ValueChanged;
			comboBoxAdvAllowedPreferenceActivity.SelectedIndexChanged += comboBoxAdvAllowedPreferenceActivity_SelectedIndexChanged;

			SetPreferencePeriods(selectedItem.PreferenceInputPeriod, selectedItem.PreferencePeriod);
			SetStudentAvailabilityPeriods(selectedItem.StudentAvailabilityInputPeriod, selectedItem.StudentAvailabilityPeriod);
			SetShiftTradePeriodDays(selectedItem.ShiftTradeOpenPeriodDays);
			SetAllowedDayOffs(selectedItem);
			SetAllowedShiftCategories(selectedItem);
			SetMatchingSkills(selectedItem);
		}

		private void buttonNew_Click(object sender, EventArgs e)
		{
			_presenter.AddWorkflowControlSet();
			_presenter.DefaultPreferencePeriods(_presenter.SelectedModel, DateTime.Today);
			_presenter.DefaultStudentAvailabilityPeriods(_presenter.SelectedModel, DateTime.Today);
			_presenter.DefaultShiftTradePeriodDays(new MinMax<int>(2, 17));
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
			autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
			autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panelBasic.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panel1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panel2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panel3.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panel4.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panel5.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panel6.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panel7.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panel8.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panel9.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panelMatchingSkills.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panelOpenForShiftTrade.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panelOpenPreference.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			tableLayoutPanelOpenForAbsenceRequests.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panelTolerance.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			tableLayoutPanelAbsenceRequestMiscellaneous.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panel10.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();

			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelBasic.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelOpenStudentAvailability.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelOpenPreference.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelDaysOffAvailableForExtendedPreferences.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelShiftCategoriesAvailableForExtendedPreference.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelAbsencesAvailableForExtendedPreference.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelAllowedPreferenceActivity.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			label4.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelAbsenceRequestsVisualisation.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelOpenForAbsenceRequests.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelOpenForShiftTrade.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelTolerance.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			label1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelMatchingSkills.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			label3.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelAllowedAbsencesForReport.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelAbsenceRequestMiscellaneous.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelOvertimeRequestBasic.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelOpenForOvertimeRequests.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			toolTip1.SetToolTip(buttonDelete, Resources.Delete);
			toolTip1.SetToolTip(buttonNew, Resources.New);
			toolTip1.SetToolTip(dateTimePickerAdvViewpoint, Resources.Viewpoint);
			dateTimePickerAdvViewpoint.RightToLeft = RightToLeft.No; //This is to avoid having drop down button hiding the date
			toolTip1.SetToolTip(buttonAdvNextProjectionPeriod, Resources.NextPeriod);
			toolTip1.SetToolTip(buttonAdvPreviousProjectionPeriod, Resources.PreviousPeriod);
		}

		public void LoadControl()
		{
			_presenter.Initialize();
			dateTimePickerAdvViewpoint.Value = DateTime.Today;
			comboBoxAdvWorkflowControlSet.Focus();
			// Hide Absence Request tab if no permissions.
			var authorization = PrincipalAuthorization.Current();
			tabControlAdvArea.TabPages[1].TabVisible = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AbsenceRequests);
			tabControlAdvArea.TabPages[2].TabVisible = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequests);
		}

		public void SaveChanges()
		{
			_presenter.SaveChanges();
			// in some cases the list don't get updated
			updateSourceList();
		}

		private void updateSourceList()
		{
			if (comboBoxAdvWorkflowControlSet.SelectedItem == null)
				return;

			var selectedItem = (WorkflowControlSetModel)comboBoxAdvWorkflowControlSet.SelectedItem;
			_gridHelper.SetSourceList(selectedItem.AbsenceRequestPeriodModels);
			_overtimeRequestOpenPeriodGridHelper.SetSourceList(selectedItem.OvertimeRequestPeriodModels);
		}

		public void Unload()
		{
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
		}

		public void Persist()
		{
			SaveChanges();
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.Scheduling);
		}

		public string TreeNode()
		{
			return Resources.WorkflowControlSetHeader;
		}

		public void OnShow()
		{
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
		}

		public ViewType ViewType
		{
			get { return ViewType.WorkflowControlSets; }
		}

		public void FillWorkloadControlSetCombo(IEnumerable<IWorkflowControlSetModel> workflowControlSetModelCollection, string displayMember)
		{
			comboBoxAdvWorkflowControlSet.DataSource = null;
			comboBoxAdvWorkflowControlSet.DataSource = new List<IWorkflowControlSetModel>(workflowControlSetModelCollection);
			comboBoxAdvWorkflowControlSet.DisplayMember = displayMember;
		}

		public void FillAllowedPreferenceActivityCombo(IEnumerable<IActivity> activityCollection, string displayMember)
		{
			comboBoxAdvAllowedPreferenceActivity.DataSource = null;
			comboBoxAdvAllowedPreferenceActivity.DataSource = new List<IActivity>(activityCollection);
			comboBoxAdvAllowedPreferenceActivity.DisplayMember = displayMember;
		}

		public void SetName(string name)
		{
			textBoxDescription.Text = name;
		}

		public void SetUpdatedInfo(string text)
		{
			autoLabelInfoAboutChanges.Text = text;
		}

		public void SelectWorkflowControlSet(IWorkflowControlSetModel model)
		{
			comboBoxAdvWorkflowControlSet.SelectedItem = model;
		}

		public void InitializeView()
		{
			configureAbsenceRequestPeriodGrid();
			configureOvertimeRequestPeriodGrid();
			configureProjectionGrid();
			initOvertimeRequestMaximumTimeHandleType();
			initOvertimeRequestStaffingCheckMethod();
			initOvertimeRequestMaximumContinuousWorkTimeHandleType();
			initOvertimeRequestUsePrimarySkill();
		}

		private void configureProjectionGrid()
		{
			gridControlVisualisation.CellModels.Add("ProjectionHeader",
				new MonthlyProjectionColumnHeaderCellModel(gridControlVisualisation.Model));

			gridControlVisualisation.Cols.HeaderCount = 1;
			gridControlVisualisation.QueryCellInfo += gridControlVisualisation_QueryCellInfo;
			gridControlVisualisation.QueryColCount += gridControlVisualisation_QueryColCount;
			gridControlVisualisation.QueryRowCount += gridControlVisualisation_QueryRowCount;
			gridControlVisualisation.QueryColWidth += gridControlVisualisation_QueryColWidth;
			GridHelper.GridStyle(gridControlVisualisation);
		}

		private void gridControlVisualisation_QueryColWidth(object sender, GridRowColSizeEventArgs e)
		{
			if (e.Index == gridControlVisualisation.ColCount)
			{
				e.Size = gridControlVisualisation.ClientSize.Width -
						 gridControlVisualisation.ColWidths.GetTotal(0, gridControlVisualisation.ColCount - 1);
				e.Handled = true;
			}
			else if (e.Index == 1)
			{
				var size = new SizeF();
				if (_presenter.DoRequestableAbsencesExist)
				{
					int longestAbsenceNameLength = _presenter.RequestableAbsenceCollection.Max(a => a.Name.Length);
					size =
						gridControlVisualisation.CreateGraphics().MeasureString(
							"".PadRight(longestAbsenceNameLength, 'm'), gridControlVisualisation.Font);
				}

				e.Size = (int)(size.Width + 4);
				e.Handled = true;
			}
		}

		private void gridControlVisualisation_QueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			int rowCount = 0;
			if (_presenter.DoRequestableAbsencesExist)
			{
				rowCount = _presenter.RequestableAbsenceCollection.Count;
			}

			e.Count = rowCount;
			e.Handled = true;
		}

		private static void gridControlVisualisation_QueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = 2;
			e.Handled = true;
		}

		private void gridControlVisualisation_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (e.RowIndex == 0 && e.ColIndex == 2)
			{
				e.Style.CellType = "ProjectionHeader";
				e.Style.Tag = _presenter.ProjectionPeriod;
				e.Style.ShowButtons = GridShowButtons.ShowCurrentCell;
			}

			if (e.RowIndex > 0 && e.ColIndex == 1)
			{
				e.Style.CellValue = _presenter.RequestableAbsenceCollection[e.RowIndex - 1].Name;
			}

			if (e.RowIndex <= 0 || e.ColIndex != 2) return;
			var absence = _presenter.RequestableAbsenceCollection[e.RowIndex - 1];
			var extractor = _presenter.SelectedModel.DomainEntity.GetExtractorForAbsence(absence);
			extractor.ViewpointDate = new DateOnly(dateTimePickerAdvViewpoint.Value);
			e.Style.CellType = "Control";

			var layers = extractor.Projection.GetProjectedPeriods(_presenter.ProjectionPeriod, CultureInfo.InvariantCulture,
				CultureInfo.InvariantCulture).OfType<AbsenceRequestOpenDatePeriod>();
			MonthlyProjectionVisualiser visualiser;
			if (!_projectionCache.TryGetValue(absence, out visualiser))
			{
				visualiser = new MonthlyProjectionVisualiser { Dock = DockStyle.Fill };
				_projectionCache.Add(absence, visualiser);
			}

			visualiser.SetControlDatePeriod(_presenter.ProjectionPeriod);
			visualiser.SetLayerCollection(layers.Select(p => new DateOnlyProjectionItem
			{
				DisplayColor = p.Absence == null ? Color.Empty : p.Absence.DisplayColor,
				Period = new DateOnlyPeriod(p.Period.StartDate, p.Period.EndDate.AddDays(1)),
				ToolTipText = string.Format(CultureInfo.CurrentUICulture,
					Resources.WorkflowControlSetToolTip,
					p.PersonAccountValidator.DisplayText,
					p.StaffingThresholdValidator.DisplayText,
					p.AbsenceRequestProcess.DisplayText,
					p.Period.DateString)
			}).ToList());
			e.Style.Control = visualiser;
		}

		public void RefreshOpenPeriodsGrid()
		{
			gridControlAbsenceRequestOpenPeriods.Invalidate();
			refreshProjectionGrid();
		}

		public void SetOpenPeriodsGridRowCount(int rowCount)
		{
			gridControlAbsenceRequestOpenPeriods.RowCount = 0;
			gridControlAbsenceRequestOpenPeriods.RowCount = rowCount + gridControlAbsenceRequestOpenPeriods.Rows.HeaderCount;
		}

		public bool ConfirmDeleteOfRequestPeriod()
		{
			string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);
			DialogResult dialogResult = ViewBase.ShowConfirmationMessage(Resources.AreYouSureYouWantToDelete, caption);
			return dialogResult == DialogResult.Yes;
		}

		public IList<AbsenceRequestPeriodModel> AbsenceRequestPeriodSelected
		{
			get { return _gridHelper.FindSelectedItems(gridControlAbsenceRequestOpenPeriods.Rows.HeaderCount + 1); }
		}

		public void EnableHandlingOfAbsenceRequestPeriods(bool enable)
		{
			buttonAddAbsenceRequestPeriod.Enabled = enable;
			buttonDeleteAbsenceRequestPeriod.Enabled = enable;

			toolStripMenuItemRollingPeriod.Enabled = enable;
			toolStripMenuItemFromToPeriod.Enabled = enable;
			toolStripMenuItemMoveDown.Enabled = enable;
			toolStripMenuItemMoveUp.Enabled = enable;
			toolStripMenuItemDelete.Enabled = enable;
		}

		public void SetWriteProtectedDays(int? writeProtection)
		{
			integerTextBoxWriteProtect.Text = writeProtection.ToString();
		}

		public void SetCalendarCultureInfo(CultureInfo cultureInfo)
		{
			dateTimePickerAdvPublishedTo.SetCultureInfoSafe(cultureInfo);

			var minPeriod = new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime),
											   new DateOnly(DateHelper.MaxSmallDateTime));
			dateTimePickerAdvPublishedTo.SetAvailableTimeSpan(minPeriod);

			dateSelectionFromToIsOpen.SetCulture(cultureInfo);
			dateSelectionFromToPreferencePeriod.SetCulture(cultureInfo);
			dateSelectionFromToIsOpenStudentAvailability.SetCulture(cultureInfo);
			dateSelectionFromToStudentAvailability.SetCulture(cultureInfo);
		}

		public void SetPreferencePeriods(DateOnlyPeriod insertPeriod, DateOnlyPeriod preferencePeriod)
		{
			dateSelectionFromToIsOpen.WorkPeriodStart = insertPeriod.StartDate;
			dateSelectionFromToIsOpen.WorkPeriodEnd = insertPeriod.EndDate;
			dateSelectionFromToPreferencePeriod.WorkPeriodStart = preferencePeriod.StartDate;
			dateSelectionFromToPreferencePeriod.WorkPeriodEnd = preferencePeriod.EndDate;
		}

		public void SetShiftTradePeriodDays(MinMax<int> periodDays)
		{
			minMaxIntegerTextBoxControl1.MinTextBoxValue = periodDays.Minimum;
			minMaxIntegerTextBoxControl1.MaxTextBoxValue = periodDays.Maximum;
		}

		public void SetAllowedDayOffs(IWorkflowControlSetModel selectedModel)
		{
			twoListSelectorDayOffs.Initiate(_presenter.DayOffCollection(), selectedModel.AllowedPreferenceDayOffs, "Description",
				Resources.NotAvailable, Resources.Available);
		}

		public void SetAllowedShiftCategories(IWorkflowControlSetModel selectedModel)
		{
			twoListSelectorCategories.Initiate(_presenter.ShiftCategoriesCollection(),
				selectedModel.AllowedPreferenceShiftCategories, "Description", Resources.NotAvailable, Resources.Available);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "0")]
		public void SetAllowedAbsences(IWorkflowControlSetModel selectedModel)
		{
			twoListSelectorAbsences.Initiate(_presenter.AbsencesCollection(), selectedModel.AllowedPreferenceAbsences,
				"Description", Resources.NotAvailable, Resources.Available);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "0")]
		public void SetAllowedAbsencesForReport(IWorkflowControlSetModel selectedModel)
		{
			twoListSelectorAbsencesForReport.Initiate(_presenter.AbsencesCollection(), selectedModel.AllowedAbsencesForReport,
				"Description", Resources.NotAvailable, Resources.Available);
		}

		public void SetMatchingSkills(IWorkflowControlSetModel selectedModel)
		{
			twoListSelectorMatchingSkills.Initiate(_presenter.SkillCollection(), selectedModel.MustMatchSkills, "Name",
				Resources.Skills, Resources.MatchingSkills);
		}

		public void SetShiftTradeTargetTimeFlexibility(TimeSpan flexibility)
		{
			timeSpanTextBox1.SetInitialResolution(flexibility);
		}

		public void SetAutoGrant(bool autoGrant)
		{
			checkBoxAdvAutoGrant.CheckStateChanged -= checkBoxAdvAutoGrant_CheckStateChanged;
			checkBoxAdvAutoGrant.Checked = autoGrant;
			checkBoxAdvAutoGrant.CheckStateChanged += checkBoxAdvAutoGrant_CheckStateChanged;
		}

		public void SetAnonymousTrading(bool anonymousTrading)
		{
			checkBoxAdvAnonymousTrading.CheckStateChanged -= checkBoxAdvAnonymousTrading_CheckStateChanged;
			checkBoxAdvAnonymousTrading.Checked = anonymousTrading;
			checkBoxAdvAnonymousTrading.CheckStateChanged += checkBoxAdvAnonymousTrading_CheckStateChanged;
		}

		public void SetAbsenceRequestWaitlisting(bool absenceRequestWaitlistingEnabled, WaitlistProcessOrder processOrder)
		{
			checkBoxEnableAbsenceRequestWaitlisting.CheckStateChanged -= checkBoxEnableAbsenceRequestWaitlisting_CheckStateChanged;
			checkBoxEnableAbsenceRequestWaitlisting.Checked = absenceRequestWaitlistingEnabled;
			checkBoxEnableAbsenceRequestWaitlisting.CheckStateChanged += checkBoxEnableAbsenceRequestWaitlisting_CheckStateChanged;

			updateWaitlistControlsStatus(absenceRequestWaitlistingEnabled, processOrder);
		}

		private void updateWaitlistControlsStatus(bool absenceRequestWaitlistingEnabled, WaitlistProcessOrder processOrder)
		{
			radioButtonWaitlistBySeniority.Enabled = absenceRequestWaitlistingEnabled;
			radioButtonWaitlistFirstComeFirstServed.Enabled = absenceRequestWaitlistingEnabled;

			radioButtonWaitlistBySeniority.Checked = processOrder == WaitlistProcessOrder.BySeniority;
			radioButtonWaitlistFirstComeFirstServed.Checked = processOrder == WaitlistProcessOrder.FirstComeFirstServed;
		}

		private void setWaitlistOptions()
		{
			var waitlistEnabled = checkBoxEnableAbsenceRequestWaitlisting.Checked;
			var waitlistingProcessOrder = radioButtonWaitlistBySeniority.Checked
				? WaitlistProcessOrder.BySeniority
				: WaitlistProcessOrder.FirstComeFirstServed;

			_presenter.SetAbsenceRequestWaitlisting(waitlistEnabled, waitlistingProcessOrder);
		}

		public void SetAbsenceRequestCancellation(IWorkflowControlSetModel selectedModel)
		{
			txtAbsenceRequestCancellationThreshold.Leave -= txtAbsenceRequestCancellationThreshold_Leave;
			txtAbsenceRequestCancellationThreshold.Text = selectedModel.AbsenceRequestCancellationThreshold.ToString();
			txtAbsenceRequestCancellationThreshold.Leave += txtAbsenceRequestCancellationThreshold_Leave;
		}

		public void SetAbsenceRequestExpiration(IWorkflowControlSetModel selectedModel)
		{
			txtAbsenceRequestExpiredThreshold.Leave -= txtAbsenceRequestExpiredThreshold_Leave;
			txtAbsenceRequestExpiredThreshold.Text = selectedModel.AbsenceRequestExpiredThreshold.ToString();
			txtAbsenceRequestExpiredThreshold.Leave += txtAbsenceRequestExpiredThreshold_Leave;
		}

		public void SetAbsenceProbability(bool absenceProbabilityEnabled)
		{
			checkBoxEnableAbsenceProbability.CheckStateChanged -= checkBoxEnableAbsenceProbability_CheckStateChanged;
			checkBoxEnableAbsenceProbability.Checked = absenceProbabilityEnabled;
			checkBoxEnableAbsenceProbability.CheckStateChanged += checkBoxEnableAbsenceProbability_CheckStateChanged;
		}

		public void SetLockTrading(bool lockTrading)
		{
			checkBoxAdvLockTrading.CheckStateChanged -= checkBoxAdvLockTrading_CheckStateChanged;
			checkBoxAdvLockTrading.Checked = lockTrading;
			checkBoxAdvLockTrading.CheckStateChanged += checkBoxAdvLockTrading_CheckStateChanged;
		}

		public void DisableAllButAdd()
		{
			tabControlAdvArea.Enabled = false;
			textBoxDescription.Text = string.Empty;
			textBoxDescription.Enabled = false;
			buttonDelete.Enabled = false;
			comboBoxAdvWorkflowControlSet.Text = string.Empty;
			comboBoxAdvWorkflowControlSet.Enabled = false;
			autoLabelInfoAboutChanges.Text = string.Empty;
		}

		public void EnableAllAuthorized()
		{
			tabControlAdvArea.Enabled = true;
			textBoxDescription.Enabled = true;
			buttonDelete.Enabled = true;
			comboBoxAdvWorkflowControlSet.Enabled = true;
		}

		private bool _loading;

		public void LoadDateOnlyVisualizer()
		{
			if (_loading) return;
			_loading = true;
			dateOnlyPeriodsVisualizer1.SuspendLayout();
			dateOnlyPeriodsVisualizer1.Rows.Clear();

			IList<DateOnlyPeriod> list = _presenter.BasicVisualizerWriteProtectionPeriods(DateOnly.Today);
			var row = new DateOnlyPeriodVisualizerRow(Resources.WriteProtectionColon, list, Color.LightGreen);
			dateOnlyPeriodsVisualizer1.Rows.Add(row);

			list = _presenter.BasicVisualizerPublishedPeriods();
			row = new DateOnlyPeriodVisualizerRow(Resources.PublishedColon, list, Color.YellowGreen);
			dateOnlyPeriodsVisualizer1.Rows.Add(row);

			list = _presenter.BasicVisualizerStudentAvailabilityPeriods();
			row = new DateOnlyPeriodVisualizerRow(Resources.StudentAvailability + Resources.Colon, list, Color.OliveDrab);
			dateOnlyPeriodsVisualizer1.Rows.Add(row);

			list = _presenter.BasicVisualizerPreferencePeriods();
			row = new DateOnlyPeriodVisualizerRow(Resources.PreferencesColon, list, Color.DeepSkyBlue);
			dateOnlyPeriodsVisualizer1.Rows.Add(row);

			dateOnlyPeriodsVisualizer1.Draw();
			dateOnlyPeriodsVisualizer1.ResumeLayout(true);
			_loading = false;
		}

		private void refreshProjectionGrid()
		{
			foreach (var monthlyProjectionVisualizer in _projectionCache)
			{
				monthlyProjectionVisualizer.Value.Dispose();
			}

			_projectionCache.Clear();
			gridControlVisualisation.RowCount = 0;
			gridControlVisualisation.RowCount = _presenter.DoRequestableAbsencesExist
				? _presenter.RequestableAbsenceCollection.Count
				: 0;

			gridControlVisualisation.Invalidate();
		}

		private void buttonDelete_Click(object sender, EventArgs e)
		{
			if (_presenter.SelectedModel == null) return;

			var culture = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
			string text = string.Format(
				culture,
				Resources.AreYouSureYouWantToDeleteItem,
				_presenter.SelectedModel.Name
				);

			string caption = string.Format(culture, Resources.ConfirmDelete);
			DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
			if (response != DialogResult.Yes) return;
			Cursor.Current = Cursors.WaitCursor;

			_presenter.DeleteWorkflowControlSet();

			Cursor.Current = Cursors.Default;
		}

		private void textBoxDescription_Leave(object sender, EventArgs e)
		{
			if (_presenter == null || _presenter.SelectedModel == null || Disposing) return;
			_presenter.SelectedModel.Name = textBoxDescription.Text;
			var selectedModel = _presenter.SelectedModel;
			FillWorkloadControlSetCombo(_presenter.WorkflowControlSetModelCollection, "Name");
			SelectWorkflowControlSet(selectedModel);
		}

		private void daysTbx_Leave(object sender, EventArgs e)
		{
			if (_presenter == null || _presenter.SelectedModel == null || Disposing) return;
			int days = (int)daysTbx.Value;
			_presenter.SetMaxConsecutiveWorkingDays(days);
		}

		private void buttonAddAbsenceRequestPeriod_Click(object sender, EventArgs e)
		{
			_presenter.AddOpenDatePeriod();
		}

		private void buttonAdvDeleteAbsenceRequestPeriod_Click(object sender, EventArgs e)
		{
			gridControlAbsenceRequestOpenPeriods.BeginUpdate();
			deleteSelected();
			gridControlAbsenceRequestOpenPeriods.EndUpdate();
		}

		private void deleteSelected()
		{
			ReadOnlyCollection<AbsenceRequestPeriodModel> selectedPeriodModels =
				_gridHelper.FindSelectedItems(gridControlAbsenceRequestOpenPeriods.Rows.HeaderCount + 1);

			_presenter.DeleteAbsenceRequestPeriod(new List<AbsenceRequestPeriodModel>(selectedPeriodModels));
		}

		private void dateTimePickerAdvViewpoint_ValueChanged(object sender, EventArgs e)
		{
			refreshProjectionGrid();
		}

		private void buttonAdvPreviousProjectionPeriod_Click(object sender, EventArgs e)
		{
			_presenter.PreviousProjectionPeriod();
		}

		private void buttonAdvNextProjectionPeriod_Click(object sender, EventArgs e)
		{
			_presenter.NextProjectionPeriod();
		}

		private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
		{
			buttonAdvDeleteAbsenceRequestPeriod_Click(sender, e);
		}

		private void toolStripMenuItemMoveUp_Click(object sender, EventArgs e)
		{
			
			GridRangeInfo activeRange = gridControlAbsenceRequestOpenPeriods.Selections.Ranges.ActiveRange;

			if (activeRange.Top <= gridControlAbsenceRequestOpenPeriods.Rows.HeaderCount + 1) return;
			ReadOnlyCollection<AbsenceRequestPeriodModel> selectedItems = _gridHelper.FindItemsBySelectionOrPoint(_gridPoint);
			gridControlAbsenceRequestOpenPeriods.Selections.Clear();
			foreach (AbsenceRequestPeriodModel absenceRequestPeriodModel in selectedItems)
			{
				_presenter.MoveUp(absenceRequestPeriodModel);
			}

			gridControlAbsenceRequestOpenPeriods.Selections.ChangeSelection(activeRange, activeRange.OffsetRange(-1, 0));
		}

		private void toolStripMenuItemMoveDown_Click(object sender, EventArgs e)
		{
			GridRangeInfo activeRange = gridControlAbsenceRequestOpenPeriods.Selections.Ranges.ActiveRange;
			if (activeRange.Bottom >= gridControlAbsenceRequestOpenPeriods.RowCount) return;
			ReadOnlyCollection<AbsenceRequestPeriodModel> selectedItems = _gridHelper.FindItemsBySelectionOrPoint(_gridPoint);
			gridControlAbsenceRequestOpenPeriods.Selections.Clear();
			foreach (AbsenceRequestPeriodModel absenceRequestPeriodModel in selectedItems.Reverse())
			{
				_presenter.MoveDown(absenceRequestPeriodModel);
			}

			gridControlAbsenceRequestOpenPeriods.Selections.ChangeSelection(activeRange, activeRange.OffsetRange(1, 0));
		}

		private void gridControlAbsenceRequestOpenPeriods_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right) return;
			_gridPoint = e.Location;
			enableContextMenu(false);
			if (_gridHelper == null)
				return;

			AbsenceRequestPeriodModel rightClickedItem = _gridHelper.FindItemByPoint(_gridPoint);
			if (rightClickedItem == null) return;
			enableContextMenu(true);
			var selectedItems = _gridHelper.FindSelectedItems(gridControlAbsenceRequestOpenPeriods.Rows.HeaderCount + 1);

			if (selectedItems.Contains(rightClickedItem)) return;
			gridControlAbsenceRequestOpenPeriods.Selections.Clear();
			gridControlAbsenceRequestOpenPeriods.CurrentCell.Deactivate(false);
			gridControlAbsenceRequestOpenPeriods.Selections.SelectRange(
				gridControlAbsenceRequestOpenPeriods.PointToRangeInfo(_gridPoint), true);
		}

		private void enableContextMenu(bool value)
		{
			foreach (ToolStripItem item in contextMenuStripOpenPeriodsGrid.Items)
			{
				item.Enabled = value;
			}
		}

		private void toolStripMenuItemFromToPeriod_Click(object sender, EventArgs e)
		{
			_presenter.AddOpenDatePeriod();
		}

		private void toolStripMenuItemRollingPeriod_Click(object sender, EventArgs e)
		{
			_presenter.AddOpenRollingPeriod();
		}

		private void comboBoxAdvAllowedPreferenceActivity_SelectedIndexChanged(object sender, EventArgs e)
		{
			var activity = (IActivity)comboBoxAdvAllowedPreferenceActivity.SelectedItem;
			_presenter.SetSelectedAllowedPreferenceActivity(activity);
		}

		private void integerTextBoxWriteProtect_Leave(object sender, EventArgs e)
		{
			if (_presenter == null || _presenter.SelectedModel == null) return;
			_presenter.SetWriteProtectedDays(integerTextBoxWriteProtect.IntegerValue());
		}

		private void dateTimePickerAdvPublishedTo_ValueChanged(object sender, EventArgs e)
		{
			if (dateTimePickerAdvPublishedTo.IsNullDate)
				_presenter.SetPublishedToDate(null);
			else
				_presenter.SetPublishedToDate(dateTimePickerAdvPublishedTo.Value);
		}

		private void dateSelectionFromToPreferencePeriod_Validating(object sender, CancelEventArgs e)
		{
			validateFromToPeriods(e, sender);
		}

		private void dateSelectionFromToIsOpen_Validating(object sender, CancelEventArgs e)
		{
			validateFromToPeriods(e, sender);
		}

		private void dateSelectionFromToPreferencePeriod_Validated(object sender, EventArgs e)
		{
			var preferencePeriod = new DateOnlyPeriod(dateSelectionFromToPreferencePeriod.WorkPeriodStart,
				dateSelectionFromToPreferencePeriod.WorkPeriodEnd);
			_presenter.SetPreferencePeriod(preferencePeriod);
		}

		private void dateSelectionFromToIsOpen_Validated(object sender, EventArgs e)
		{
			var preferenceInputPeriod = new DateOnlyPeriod(dateSelectionFromToIsOpen.WorkPeriodStart,
				dateSelectionFromToIsOpen.WorkPeriodEnd);
			_presenter.SetPreferenceInputPeriod(preferenceInputPeriod);
		}

		private void minMaxIntegerTextBoxControl1_Validating(object sender, CancelEventArgs e)
		{
			validateShiftTradePeriodDays(e, sender);
		}

		private void minMaxIntegerTextBoxControl1_Validated(object sender, EventArgs e)
		{
			_presenter.SetOpenShiftTradePeriod(new MinMax<int>(minMaxIntegerTextBoxControl1.MinTextBoxValue,
				minMaxIntegerTextBoxControl1.MaxTextBoxValue));
		}

		private void validateFromToPeriods(CancelEventArgs e, object sender)
		{
			string error = null;
			var dateSelection = sender as DateSelectionFromTo;

			if (dateSelection != null && !dateSelection.IsWorkPeriodValid)
			{
				error = Resources.StartDateMustBeSmallerThanEndDate;
				e.Cancel = true;
			}

			errorProvider1.SetError((Control)sender, error);
		}

		private void validateShiftTradePeriodDays(CancelEventArgs e, object sender)
		{
			string error = null;

			if (!minMaxIntegerTextBoxControl1.IsValid)
			{
				error = Resources.FromDaysMustBeLessThanToDays;
				e.Cancel = true;
			}

			errorProvider1.SetError((Control)sender, error);
		}

		private void buttonPanRight_Click(object sender, EventArgs e)
		{
			dateOnlyPeriodsVisualizer1.Next();
		}

		private void buttonPanLeft_Click(object sender, EventArgs e)
		{
			dateOnlyPeriodsVisualizer1.Previous();
		}

		private void buttonZoomIn_Click(object sender, EventArgs e)
		{
			if (dateOnlyPeriodsVisualizer1.MonthsOnEachSide <= 0)
				return;
			dateOnlyPeriodsVisualizer1.MonthsOnEachSide--;
		}

		private void buttonZoomOut_Click(object sender, EventArgs e)
		{
			dateOnlyPeriodsVisualizer1.MonthsOnEachSide++;
		}

		private void timeSpanTextBox1_Leave(object sender, EventArgs e)
		{
			if (_presenter == null || _presenter.SelectedModel == null) return;
			_presenter.SetShiftTradeTargetTimeFlexibility(timeSpanTextBox1.Value);
		}

		private void checkBoxAdvAutoGrant_CheckStateChanged(object sender, EventArgs e)
		{
			_presenter.SetAutoGrant(checkBoxAdvAutoGrant.Checked);
		}

		private void radioButtonAdvFairnessEqualCheckChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonAdvFairnessEqualCheckChanged(radioButtonAdvFairnessEqual.Checked);
		}

		private void radioButtonAdvSeniorityCheckChanged(object sender, EventArgs e)
		{
			_presenter.OnRadioButtonAdvSeniorityCheckedChanged(radioButtonAdvSeniority.Checked);
		}

		public void SetFairnessType(FairnessType value)
		{
			radioButtonAdvFairnessEqual.CheckChanged -= radioButtonAdvFairnessEqualCheckChanged;
			radioButtonAdvSeniority.CheckChanged -= radioButtonAdvSeniorityCheckChanged;

			radioButtonAdvFairnessEqual.Checked = false;

			if (value == FairnessType.EqualNumberOfShiftCategory)
				radioButtonAdvFairnessEqual.Checked = true;
			else if (value == FairnessType.Seniority)
				radioButtonAdvSeniority.Checked = true;

			radioButtonAdvFairnessEqual.CheckChanged += radioButtonAdvFairnessEqualCheckChanged;
			radioButtonAdvSeniority.CheckChanged += radioButtonAdvSeniorityCheckChanged;
		}

		public void SetStudentAvailabilityPeriods(DateOnlyPeriod insertPeriod, DateOnlyPeriod studentAvailabilityPeriod)
		{
			dateSelectionFromToIsOpenStudentAvailability.WorkPeriodStart = insertPeriod.StartDate;
			dateSelectionFromToIsOpenStudentAvailability.WorkPeriodEnd = insertPeriod.EndDate;
			dateSelectionFromToStudentAvailability.WorkPeriodStart = studentAvailabilityPeriod.StartDate;
			dateSelectionFromToStudentAvailability.WorkPeriodEnd = studentAvailabilityPeriod.EndDate;
		}

		private void dateSelectionFromToStudentAvailability_Validating(object sender, CancelEventArgs e)
		{
			validateFromToPeriods(e, sender);
		}

		private void dateSelectionFromToStudentAvailability_Validated(object sender, EventArgs e)
		{
			var studentAvailabilityPeriod = new DateOnlyPeriod(dateSelectionFromToStudentAvailability.WorkPeriodStart,
				dateSelectionFromToStudentAvailability.WorkPeriodEnd);
			_presenter.SetStudentAvailabilityPeriod(studentAvailabilityPeriod);
		}

		private void dateSelectionFromToIsOpenStudentAvailability_Validating(object sender, CancelEventArgs e)
		{
			validateFromToPeriods(e, sender);
		}

		private void dateSelectionFromToIsOpenStudentAvailability_Validated(object sender, EventArgs e)
		{
			var studentAvailabilityInputPeriod = new DateOnlyPeriod(dateSelectionFromToIsOpenStudentAvailability.WorkPeriodStart,
				dateSelectionFromToIsOpenStudentAvailability.WorkPeriodEnd);
			_presenter.SetStudentAvailabilityInputPeriod(studentAvailabilityInputPeriod);
		}

		private void checkBoxAdvAnonymousTrading_CheckStateChanged(object sender, EventArgs e)
		{
			_presenter.SetAnonymousTrading(checkBoxAdvAnonymousTrading.Checked);
		}

		private void checkBoxAdvLockTrading_CheckStateChanged(object sender, EventArgs e)
		{
			_presenter.SetLockTrading(checkBoxAdvLockTrading.Checked);
		}

		private void checkBoxEnableAbsenceRequestWaitlisting_CheckStateChanged(object sender, EventArgs e)
		{
			var waitlistEnabled = checkBoxEnableAbsenceRequestWaitlisting.Checked;
			radioButtonWaitlistFirstComeFirstServed.Enabled = waitlistEnabled;
			radioButtonWaitlistBySeniority.Enabled = waitlistEnabled;

			setWaitlistOptions();
		}

		private void radioButtonWaitlistFirstComeFirstServed_Click(object sender, EventArgs e)
		{
			setWaitlistOptions();
		}

		private void radioButtonWaitlistBySeniority_Click(object sender, EventArgs e)
		{
			setWaitlistOptions();
		}

		private void txtAbsenceRequestCancellationThreshold_Leave(object sender, EventArgs e)
		{
			_presenter.SetAbsenceRequestCancellationThreshold(txtAbsenceRequestCancellationThreshold.IntegerValue());
		}

		private void txtAbsenceRequestExpiredThreshold_Leave(object sender, EventArgs e)
		{
			_presenter.SetAbsenceRequestExpiredThreshold(txtAbsenceRequestExpiredThreshold.IntegerValue());
		}

		private void checkBoxEnableAbsenceProbability_CheckStateChanged(object sender, EventArgs e)
		{
			_presenter.SetAbsenceProbability(checkBoxEnableAbsenceProbability.Checked);
		}

		public void SetMaxConsecutiveWorkingDays(int maxConsecutiveWorkingDays)
		{
			daysTbx.Value = maxConsecutiveWorkingDays;
		}
	}
}