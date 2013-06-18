using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;
using Teleopti.Ccc.AgentPortalCode.Common.Clipboard;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView
{
	public partial class AvailabilityView : BaseUserControl, IStudentAvailabilityView, IHelpContext
    {
        private const int RowHeight = 90;
        private const int ColHeaderHeight = 30;
        private const int ColumnWidth = 135;
        private const int RowHeaderWidth = 140;
        private const float EditViewHeight = 57F;

        private readonly StudentAvailabilityPresenter _presenter;
        private readonly StudentAvailabilityModel _model;
        private static readonly NotValidatedSpecification NotValidatedSpecification = new NotValidatedSpecification();

        public AvailabilityView(IToggleButtonState parent)
        {
            InitializeComponent();
            IScheduleHelper scheduleHelper = new ScheduleHelper();
            _model = new StudentAvailabilityModel(StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson, scheduleHelper);
            _presenter = new StudentAvailabilityPresenter(_model, this, new ClipHandler<IStudentAvailabilityCellData>(), parent);
            gridControl1.Model.CellModels.Add("RestrictionViewCellModel", new RestrictionViewCellModel(gridControl1.Model));
            gridControl1.Model.CellModels.Add("RestrictionWeekHeaderViewCellModel", new RestrictionWeekHeaderViewCellModel(gridControl1.Model));
            gridControl1.ColWidths.SetRange(1, gridControl1.ColCount, ColumnWidth);
            gridControl1.ColWidthEntries[0].Width = RowHeaderWidth;
            gridControl1.ClipboardCopy += gridControl1_ClipboardCopy;
            gridControl1.ClipboardPaste += gridControl1_ClipboardPaste;
            gridControl1.ClipboardCut += gridControl1_ClipboardCut;
            gridControl1.KeyDown += gridControl1_KeyDown;
            gridControl1.SelectionChanged += gridControl1_SelectionChanged;
            contextMenuStripStudentAvailability.Opening += OnShowContextMenu;
            labelInfo.Text = _presenter.PeriodInfo();
            gridControl1.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
            Presenter.GetNextPeriod();
            editStudentAvailabilityView.Presenter.SaveStudentAvailabilityCellData += editExtendedStudentAvailabilityView_SaveStudentAvailabilityCellData;
            tableLayoutPanel1.RowStyles[2].Height = EditViewHeight;
        }

        private void editExtendedStudentAvailabilityView_SaveStudentAvailabilityCellData(object sender, SaveStudentAvailabilityCellDataEventArgs e)
        {
            AddStudentAvailabilityRestrictions(e.CellData.StudentAvailabilityRestrictions);
            gridControl1.Invalidate();
        }

        private void AddStudentAvailabilityRestrictions(IList<StudentAvailabilityRestriction> studentAvailabilityRestrictions)
        {
            GridRangeInfoList rangeList = gridControl1.Selections.Ranges;
            SetSelection(rangeList);

            foreach (GridRangeInfo range in gridControl1.Selections)
            {
                _presenter.OnAddStudentAvailabilityRestrictions(range.Top, range.Bottom, range.Left, range.Right, studentAvailabilityRestrictions);
            }
            gridControl1.Invalidate();
        }

        private void gridControl1_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            RefreshEditStudentAvailabilityView();
        }

        private void OnShowContextMenu(object sender, CancelEventArgs e)
        {
        }

        public void PasteClip()
        {
            GridRangeInfoList rangeList = gridControl1.Selections.Ranges;
            SetSelection(rangeList);
			foreach (GridRangeInfo range in rangeList)
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
            IStudentAvailabilityCellData cellData;
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
                else if (cellData.EffectiveRestriction != null &&
                    cellData.EffectiveRestriction.Invalid)
                {
                    e.Style.CellTipText = UserTexts.Resources.NoShiftsAvailableForThisStudentAvailabilityDot;
                }
                else if (cellData.HasPersonalAssignmentOnly)
                {
                    e.Style.CellTipText = cellData.TipText;
                }
                else
                {
                    e.Style.CellTipText = string.Empty;
                }
                e.Style.CellValue = cellData;
                return;
            }
        	e.Style.CellType = "Static";
        	e.Style.ReadOnly = true;
        }

        void IStudentAvailabilityView.CellDataLoaded()
        {
			var rowCount = _presenter.CellDataCollection.Count / 7;
            if (_presenter.CellDataCollection.Count % 7 > 0)
				rowCount = rowCount + 1;
        	gridControl1.RowCount = rowCount;

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
       
        public void SetValidationPicture(Bitmap picture)
        {
            autoLabelPeriodInformation.Visible = false;
            pictureBox1.Visible = true;
            pictureBox1.AutoSize = true;
            pictureBox1.Image = picture;
        }

        public void SetValidationInfoText(string text, Color color)
        {
            autoLabelPeriodInformation.Visible = true;
            pictureBox1.Visible = false;
            autoLabelPeriodInformation.Text = text;
            autoLabelPeriodInformation.ForeColor = color;
        }

        #region Context menu

        public void ToggleStateContextMenuItemPaste(bool enable)
        {
            contextMenuStripStudentAvailability.Items["toolStripMenuItemPaste"].Enabled = enable;
        }

        public void RefreshEditStudentAvailabilityView()
        {
            var currentCellInfo = gridControl1.CurrentCellInfo;
            if (currentCellInfo != null)
            {
                IStudentAvailabilityCellData cellData;
                _presenter.OnQueryCellInfo(currentCellInfo.ColIndex, currentCellInfo.RowIndex, out cellData);
                if (cellData != null && !cellData.Enabled)
                    editStudentAvailabilityView.Presenter.AllowInput(false);
                else
                {
                    editStudentAvailabilityView.Presenter.AllowInput(true);
                    editStudentAvailabilityView.Presenter.SetStudentAvailabilityRestrictions(cellData);
                }
            }
        }

        public void SetupContextMenu()
        {
            contextMenuStripStudentAvailability.Items["toolStripMenuItemCopy"].Click -= Copy_Click;
            contextMenuStripStudentAvailability.Items["toolStripMenuItemCut"].Click -= Cut_Click;
            contextMenuStripStudentAvailability.Items["toolStripMenuItemPaste"].Click -= Paste_Click;
            contextMenuStripStudentAvailability.Items["toolStripMenuItemDelete"].Click -= Delete_Click;
            contextMenuStripStudentAvailability.Items["toolStripMenuItemCopy"].Click += Copy_Click;
            contextMenuStripStudentAvailability.Items["toolStripMenuItemCut"].Click += Cut_Click;
            contextMenuStripStudentAvailability.Items["toolStripMenuItemPaste"].Click += Paste_Click;
            contextMenuStripStudentAvailability.Items["toolStripMenuItemDelete"].Click += Delete_Click;
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


        #endregion

        public StudentAvailabilityPresenter Presenter
        {
            get { return _presenter; }
        }

        private void StudentAvailabilityView_Load(object sender, EventArgs e)
        {
            if (!DesignMode) SetTexts();
            ResetCurrentSelection();
        }
   }
}