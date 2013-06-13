using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortal.Main;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Common.Clipboard;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using ShiftCategory=Teleopti.Ccc.AgentPortalCode.Common.ShiftCategory;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public partial class PreferenceView : BaseUserControl, IPreferenceView
    {
        private const int RowHeight = 80;
        private const int ColHeaderHeight = 30;
        private const int ColumnWidth = 135;
        private const int RowHeaderWidth = 140;
        private const float EditViewMinHeight = 57F;
        private const float EditViewMaxHeight = 114F;

        private readonly PreferencePresenter _presenter;
        private readonly PreferenceModel _model;
        private static readonly NotValidatedSpecification NotValidatedSpecification = new NotValidatedSpecification();

        public PreferenceView(IToggleButtonState parent)
        {
            InitializeComponent();
            IScheduleHelper scheduleHelper = new ScheduleHelper();
            _model = new PreferenceModel(StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson, scheduleHelper);
            _presenter = new PreferencePresenter(_model, this, new ClipHandler<IPreferenceCellData>(), parent, AgentScheduleStateHolder.Instance());
            gridControl1.Model.CellModels.Add("RestrictionViewCellModel", new RestrictionViewCellModel(gridControl1.Model));
            gridControl1.Model.CellModels.Add("RestrictionWeekHeaderViewCellModel", new RestrictionWeekHeaderViewCellModel(gridControl1.Model));
            gridControl1.ColWidths.SetRange(1, gridControl1.ColCount, ColumnWidth);
            gridControl1.ColWidthEntries[0].Width = RowHeaderWidth;
            gridControl1.ClipboardCopy += gridControl1_ClipboardCopy;
            gridControl1.ClipboardPaste += gridControl1_ClipboardPaste;
            gridControl1.ClipboardCut += gridControl1_ClipboardCut;
            gridControl1.KeyDown += gridControl1_KeyDown;
            gridControl1.SelectionChanged += gridControl1_SelectionChanged;
            contextMenuStripPreference.Opening += OnShowContextMenu;
            labelInfo.Text = _presenter.PeriodInfo();
            gridControl1.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
            Presenter.GetNextPeriod();

            editExtendedPreferenceView1.Presenter.SavePreferenceCellData += editExtendedPreferenceView1_SavePreferenceCellData;
            editExtendedPreferenceView1.Presenter.VisibleChanged += editExtendedPreferenceView1Presenter_VisibleChanged;
            SetPanelHeight();
        }

        private void SetPanelHeight()
        {
            if (tableLayoutPanel1.RowStyles.Count < 3)
                return;
            tableLayoutPanel1.RowStyles[2].Height = editExtendedPreferenceView1.ActivityViewVisible ? EditViewMaxHeight : EditViewMinHeight;
        }

        private void editExtendedPreferenceView1Presenter_VisibleChanged(object sender, EditExtendedPreferencesVisibleChangedEventArgs e)
        {
            if (tableLayoutPanel1.RowStyles.Count < 3)
                return;
            var height = 0F;
            if (e.ActivityVisible)
            {
                height = EditViewMaxHeight;
            }
            else if (e.Visible)
            {
                height = EditViewMinHeight;
            }
            tableLayoutPanel1.RowStyles[2].Height =
                height;
        }

        private void gridControl1_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            GridRangeInfo range = e.Range;
			if (range.IsRows)
			{
				range = GridRangeInfo.Cells(range.Top, range.Left + gridControl1.Cols.HeaderCount + 1, range.Bottom, gridControl1.ColCount);
			}
            if (e.Reason == GridSelectionReason.MouseUp)
                _presenter.OnSelectionChanged(range.Top, range.Bottom, range.Left, range.Right);
            if (e.Reason != GridSelectionReason.SetCurrentCell && e.Reason != GridSelectionReason.SelectRange) { return; }

            _presenter.OnSelectionChanged(range.Top, range.Bottom, range.Left, range.Right);
            RefreshExtendedPreference();
        }

        private void OnShowContextMenu(object sender, CancelEventArgs e)
        {
            GridRangeInfoList rangeList = gridControl1.Selections.Ranges;
            SetSelection(rangeList);
            foreach (GridRangeInfo range in gridControl1.Selections)
            {
                _presenter.OnTemplateCell(range.Top, range.Bottom, range.Left, range.Right);
            }
            ToggleStateContextMenuItemSaveAsTemplate(_presenter.ExtendedPreferenceTemplate);
        }

        public void PasteClip()
        {
            GridRangeInfoList rangeList = gridControl1.Selections.Ranges;
            SetSelection(rangeList);
            foreach (GridRangeInfo range in gridControl1.Selections)
            {
                _presenter.OnPasteCellDataClip(range.Top, range.Bottom, range.Left, range.Right);
            }
            gridControl1.Invalidate();
        }

        public void CutClip()
        {
            _presenter.CellDataClipHandler.Clear();
            GridRangeInfoList rangeList = gridControl1.Selections.Ranges;
            SetSelection(rangeList);
            foreach (GridRangeInfo range in gridControl1.Selections)
            {
                _presenter.OnSetCellDataCut(range.Top, range.Bottom, range.Left, range.Right);
            }
            gridControl1.Invalidate();
        }

        private void Delete()
        {
            foreach (GridRangeInfo range in gridControl1.Selections)
            {
                _presenter.OnDelete(range.Top, range.Bottom, range.Left, range.Right);
            }
            gridControl1.Invalidate();
        }

        void gridControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                GridRangeInfoList rangeList = gridControl1.Selections.Ranges;
                SetSelection(rangeList);
                Delete();
                gridControl1.Invalidate();
            }
        }

        void gridControl1_ClipboardCut(object sender, GridCutPasteEventArgs e)
        {
            _presenter.CellDataClipHandler.Clear();
            GridRangeInfoList rangeList = e.RangeList;
            SetSelection(rangeList);
            e.Handled = false;
            CutClip();
            e.Handled = true;
            gridControl1.Invalidate();
        }
        void gridControl1_ClipboardPaste(object sender, GridCutPasteEventArgs e)
        {
            GridRangeInfoList rangeList = gridControl1.Selections.Ranges;
            SetSelection(rangeList);

            PasteClip();
        }

        void gridControl1_ClipboardCopy(object sender, GridCutPasteEventArgs e)
        {
            _presenter.CellDataClipHandler.Clear();
            GridRangeInfoList rangeList = e.RangeList;
            SetSelection(rangeList);

            CopyToClipboard();
        }

        public void CopyToClipboard()
        {
            _presenter.CellDataClipHandler.Clear();
            GridRangeInfoList rangeList = gridControl1.Selections.Ranges;
            SetSelection(rangeList);
            foreach (GridRangeInfo range in gridControl1.Selections)
            {
                _presenter.OnSetCellDataClip(range.Top, range.Bottom, range.Left, range.Right);
            }
        }

        private void SetSelection(GridRangeInfoList rangeList)
        {
            if (rangeList.Info == "T")
                _presenter.OnSelectAll(gridControl1.Rows.HeaderCount, gridControl1.Cols.HeaderCount, gridControl1.ColCount, gridControl1.RowCount);
            else if (rangeList.ActiveRange.RangeType == GridRangeInfoType.Rows)
                _presenter.OnSelectRows(gridControl1.Cols.HeaderCount, gridControl1.ColCount, rangeList.ActiveRange.Top, rangeList.ActiveRange.Bottom);
            else if (rangeList.ActiveRange.RangeType == GridRangeInfoType.Cols)
                _presenter.OnSelectColumns(rangeList.ActiveRange.Left, rangeList.ActiveRange.Right, gridControl1.Rows.HeaderCount, gridControl1.RowCount);
        }

        public void SelectColumns(int left, int right, int top, int bottom)
        {
            gridControl1.Selections.Clear();
            gridControl1.Selections.SelectRange(GridRangeInfo.Cells(top, left, bottom, right), true);
        }

        public void SelectRows(int left, int right, int top, int bottom)
        {
            gridControl1.Selections.Clear();
            gridControl1.Selections.SelectRange(GridRangeInfo.Cells(top, left, bottom, right), true);
        }

        public void SelectAll(int left, int right, int top, int bottom)
        {
            gridControl1.Selections.Clear();
            gridControl1.Selections.SelectRange(GridRangeInfo.Cells(top, left, bottom, right), true);
        }

        private void gridControl1_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            e.Style.CellValue = "";
            IPreferenceCellData cellData;
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _presenter.OnQueryColumnHeaderText(e.ColIndex);
                return;
            }

            if (e.RowIndex > 0 && e.ColIndex == 0)
            {
                e.Style.CellType = "RestrictionWeekHeaderViewCellModel";
                e.Style.CellValue = _presenter.OnQueryWeekHeader(e.RowIndex);
                e.Style.CultureInfo = _model.CurrentCultureInfo();
                return;
            }

            if (_presenter.OnQueryCellInfo(e.ColIndex, e.RowIndex, out cellData))
            {
                e.Style.CellType = "RestrictionViewCellModel";
                if (NotValidatedSpecification.IsSatisfiedBy(cellData))
                {
                    e.Style.CellTipText = UserTexts.Resources.NotValidated;
                }
                else if (cellData.HasPersonalAssignmentOnly)
                {
                    e.Style.CellTipText = cellData.TipText;
                }
                else if (cellData.EffectiveRestriction != null && cellData.EffectiveRestriction.Invalid)
                {
                    e.Style.CellTipText = UserTexts.Resources.NoShiftsAvailableForThisPreference;
                }
                else if(cellData.ViolatesNightlyRest)
                {
                    e.Style.CellTipText = UserTexts.Resources.RestrictionViolatesNightRest;
                }
                else
                {
                    e.Style.CellTipText = string.Empty;
                }
                e.Style.CellValue = cellData;
                return;
            }
            return;
        }

        void IPreferenceView.CellDataLoaded()
        {
            gridControl1.RowCount = _presenter.CellDataCollection.Count / 7;
            //if (_presenter.CellDataCollection.Count % 7 > 0)
            //    gridControl1.RowCount = gridControl1.RowCount + 1;

            gridControl1.RowHeights.SetRange(1, gridControl1.RowCount, RowHeight);
            gridControl1.RowHeightEntries[0].Height = ColHeaderHeight;
            ResetCurrentSelection();
        }

        private void ResetCurrentSelection()
        {
            if (gridControl1.Selections.Ranges.Count == 0)
            {
                gridControl1.CurrentCell.MoveTo(1, 1);
            }
            if (gridControl1.Selections.Ranges.Count > 0)
            {
                gridControl1.Model.RaiseSelectionChanged(new GridSelectionChangedEventArgs(
                                                             gridControl1.Selections.Ranges[0],
                                                             gridControl1.Selections.Ranges,
                                                             GridSelectionReason.SetCurrentCell));
            }
        }

        public void AddShiftCategoryPreference(ShiftCategory shiftCategory)
        {
            var preference = PreferencePresenter.CreateEmptyPreference();
            preference.ShiftCategory = shiftCategory;
            AddPreference(preference);
        }

        public void AddAbsencePreference(Absence absence)
        {
            var preference = PreferencePresenter.CreateEmptyPreference();
            preference.Absence = absence;
            AddPreference(preference);
        }

        public void AddDayOffPreference(DayOff dayOff)
        {
            var preference = PreferencePresenter.CreateEmptyPreference();
            preference.DayOff = dayOff;
            AddPreference(preference);
        }

        public void AddTemplatePreference(ExtendedPreferenceTemplateDto templateDto)
        {
            var preference = PreferencePresenter.CreatePreferenceFromTemplate(templateDto);
            AddPreference(preference);
        }

        private void AddPreference(Preference preference)
        {
            GridRangeInfoList rangeList = gridControl1.Selections.Ranges;
            SetSelection(rangeList);

            foreach (GridRangeInfo range in gridControl1.Selections)
            {
                _presenter.OnAddPreference(range.Top, range.Bottom, range.Left, range.Right, preference);
            }
            gridControl1.Invalidate();
        }
        public void ToggleMustHave(bool mustHave)
        {
            GridRangeInfoList rangeList = gridControl1.Selections.Ranges;
            SetSelection(rangeList);

            foreach (GridRangeInfo range in gridControl1.Selections)
            {
                _presenter.OnToggleMustHave(range.Top, range.Bottom, range.Left, range.Right, mustHave);
            }
            gridControl1.Invalidate();
        }

        public void SetValidationInfoText(string text, Color color, string dayOffsText, Color dayOffsColor, string calculationInfo)
        {
            autoLabelPeriodInformation.Visible = true;
            autoLabelDayOffs.Visible = true;
            pictureBox1.Visible = false;
            autoLabelPeriodInformation.Text = text;
            autoLabelPeriodInformation.ForeColor = color;
            autoLabelDayOffs.Text = dayOffsText;
            autoLabelDayOffs.ForeColor = dayOffsColor;
            var permissionService = PermissionService.Instance();
            labelPeriodCalculationInfo.Visible = true;
            var canSeeCalculation = permissionService.IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.ViewSchedulePeriodCalculation);
            if (canSeeCalculation)
                labelPeriodCalculationInfo.Text = calculationInfo;
            else
                labelPeriodCalculationInfo.Visible = false;
        }
        public void SetValidationPicture(Bitmap picture)
        {
            autoLabelPeriodInformation.Visible = false;
            autoLabelDayOffs.Visible = false;
            pictureBox1.Visible = true;
            pictureBox1.AutoSize = true;
            pictureBox1.Image = picture;
            labelPeriodCalculationInfo.Visible = false;
        }

        #region Context menu

        public void ToggleStateContextMenuItemPaste(bool enable)
        {
            contextMenuStripPreference.Items["toolStripMenuItemPaste"].Enabled = enable;
        }

        public void ToggleStateContextMenuItemSaveAsTemplate(bool enabled)
        {
            if (enabled)
            {
                enabled =
                    PermissionService.Instance().IsPermitted(
                        ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.ModifyExtendedPreferences);
            }
            toolStripMenuItemSaveAsTemplate.Enabled = enabled;
        }

        public void ShowErrorItemNoLongerAvailable(string itemName)
        {
            MessageBoxHelper.ShowWarningMessage(this,
                                                string.Format(CultureInfo.CurrentUICulture,
                                                              UserTexts.Resources.CannotAddPreferenceSelectedItemNotAvailableParameter0, itemName),
                                                UserTexts.Resources.Preferences);
        }

        public void RefreshExtendedPreference()
        {
            var currentCellInfo = gridControl1.CurrentCellInfo;
            if (currentCellInfo != null)
            {
                IPreferenceCellData cellData;
                _presenter.OnQueryCellInfo(currentCellInfo.ColIndex, currentCellInfo.RowIndex, out cellData);
                editExtendedPreferenceView1.Presenter.SetPreference(cellData);
            }
        }

        public void SetDaysOff(IList<DayOff> daysOff)
        {
            editExtendedPreferenceView1.Presenter.SetDaysOff(daysOff);
        }

        public void SetShiftCategories(IList<ShiftCategory> shiftCategories)
        {
            editExtendedPreferenceView1.Presenter.SetShiftCategories(shiftCategories);
        }

        public void SetAbsences(IList<Absence> absences)
        {
            editExtendedPreferenceView1.Presenter.SetAbsences(absences);    
        }

        public void ClearContextMenus()
        {
            toolStripMenuItemAddShiftCategory.DropDownItems.Clear();
            toolStripMenuItemAddDayOff.DropDownItems.Clear();
			toolStripMenuItemAddAbsence.DropDownItems.Clear();
        }

        public void AddShiftCategoryToContextMenu(ShiftCategory shiftCategory)
        {
            if (shiftCategory == null)
                throw new ArgumentNullException("shiftCategory");
            ToolStripMenuItem item = new ToolStripMenuItem(shiftCategory.Name);
            item.Tag = shiftCategory;
            item.Click += toolStripMenuItemShiftCategory_Click;
            toolStripMenuItemAddShiftCategory.DropDownItems.Add(item);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void AddAbsenceToContextMenu(Absence absence)
        {
            if(absence == null)
                throw new ArgumentNullException("absence");
            ToolStripMenuItem item = new ToolStripMenuItem(absence.Name);
            item.Tag = absence;
            item.Click += toolStripMenuItemAbsence_Click;
            toolStripMenuItemAddAbsence.DropDownItems.Add(item);
        }

        public void AddDayOffToContextMenu(DayOff dayOff)
        {
            if (dayOff == null)
                throw new ArgumentNullException("dayOff");
            ToolStripMenuItem item = new ToolStripMenuItem(dayOff.Name);
            item.Tag = dayOff;
            item.Click += toolStripMenuItemDayOff_Click;
            toolStripMenuItemAddDayOff.DropDownItems.Add(item);
        }

        public void SetupContextMenu()
        {
            contextMenuStripPreference.Items["toolStripMenuItemCopy"].Click -= Copy_Click;
            contextMenuStripPreference.Items["toolStripMenuItemCut"].Click -= Cut_Click;
            contextMenuStripPreference.Items["toolStripMenuItemPaste"].Click -= Paste_Click;
            contextMenuStripPreference.Items["toolStripMenuItemDelete"].Click -= Delete_Click;
            contextMenuStripPreference.Items["toolStripMenuItemCopy"].Click += Copy_Click;
            contextMenuStripPreference.Items["toolStripMenuItemCut"].Click += Cut_Click;
            contextMenuStripPreference.Items["toolStripMenuItemPaste"].Click += Paste_Click;
            contextMenuStripPreference.Items["toolStripMenuItemDelete"].Click += Delete_Click;
        }

        void Copy_Click(object sender, EventArgs e)
        {
            CopyToClipboard();
        }
        void Cut_Click(object sender, EventArgs e)
        {
            CutClip();
            gridControl1.Invalidate();
        }
        void Paste_Click(object sender, EventArgs e)
        {
            PasteClip();
        }
        void Delete_Click(object sender, EventArgs e)
        {
            GridRangeInfoList rangeList = gridControl1.Selections.Ranges;
            SetSelection(rangeList);
            Delete();
            gridControl1.Invalidate();
        }

        void toolStripMenuItemDayOff_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
                AddDayOffPreference((DayOff)item.Tag);
        }

        void toolStripMenuItemShiftCategory_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
                AddShiftCategoryPreference((ShiftCategory)item.Tag);
        }

        void toolStripMenuItemAbsence_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
                AddAbsencePreference((Absence)item.Tag);
        }

        #endregion

        public PreferencePresenter Presenter
        {
            get { return _presenter; }
        }

        private void PreferenceView_Load(object sender, EventArgs e)
        {
            if (!DesignMode) SetTexts();
            ResetCurrentSelection();
        }

        private void editExtendedPreferenceView1_SavePreferenceCellData(object sender, SavePreferenceCellDataEventArgs e)
        {
            AddPreference(e.CellData.Preference);
            gridControl1.Invalidate();
        }

        private void OnSaveTemplateSelected(object sender, EventArgs e)
        {
            var createTemplate = new CreateExtendedPreferencesTemplate();
            if (DialogResult.OK != createTemplate.ShowDialog(this)) return;
            var rangeList = gridControl1.Selections.Ranges;
            SetSelection(rangeList);

            foreach (GridRangeInfo range in gridControl1.Selections)
            {
                _presenter.OnSaveTemplate(createTemplate.InputName, range.Top, range.Bottom, range.Left, range.Right);
            }
        }

        public bool IsValid(ExtendedPreferenceTemplateDto dto)
        {
            var isValid = false;
            var preference = PreferencePresenter.CreatePreferenceFromTemplate(dto);
            if (CheckPermission(preference))
            {
                isValid = true;
            }
            return isValid;
        }

        private bool CheckPermission(Preference preference)
        {
            return _presenter.OnCheckPermission(preference);
        }
    }
}