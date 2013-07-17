using System.ComponentModel;
using System.Drawing;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction
{
    public class RestrictionSummaryGrid : GridControl, IRestrictionSummaryGrid
    {
        private SingleAgentRestrictionPresenter _singleAgentRestrictionPresenter;
        private IRestrictionSummaryView _restrictionSummeryView;
        private CommentMouseController _commentMouseController;
        public RestrictionSummaryGrid()
        { 
            InitializeComponent();
            HScrollBehavior = GridScrollbarMode.Automatic;
            HScrollPixel = true;
            Rows.FrozenCount = 1;
            ExcelLikeCurrentCell = true;

            CellModels.Add("NumericCell", new NumericReadOnlyCellModel(Model));
            CellModels.Add("TimeSpan", new TimeSpanDurationStaticCellModel(Model));
            CellModels.Add("ColumnHeaderCell", new GridSortColumnHeaderCellModel(Model));
        }
        public SingleAgentRestrictionPresenter SingleAgentRestrictionPresenter
        {
            get { return _singleAgentRestrictionPresenter; }
        }

        public void Initialize(SingleAgentRestrictionPresenter singleAgentRestrictionPresenter, IScheduleViewBase preferenceView)
        {
            _singleAgentRestrictionPresenter = singleAgentRestrictionPresenter;
            _restrictionSummeryView = (RestrictionSummaryView)preferenceView;
            _commentMouseController = new CommentMouseController(this);
            MouseControllerDispatcher.Add(_commentMouseController);
			((RestrictionSummaryPresenter)_restrictionSummeryView.Presenter).GetNextPeriod(null);
        }

        
        protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (SingleAgentRestrictionPresenter == null)
                return;
            if (e.RowIndex > 1 && e.ColIndex >= 0)
                e.Style.CellType = SingleAgentRestrictionPresenter.OnQueryCellType(e.ColIndex);
            else if (e.RowIndex == 1)
                e.Style.CellType = "ColumnHeaderCell";
            e.Style.CellValue = SingleAgentRestrictionPresenter.OnQueryCellInfo(e.RowIndex, e.ColIndex);
        }

		
        void RestrictionSummaryGridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
			if (e.Reason == GridSelectionReason.SetCurrentCell)
			{
				UpdateRestrictionSummaryView();
				_restrictionSummeryView.UpdateEditor();
			}
        }
        private void grid_PrepareViewStyleInfo(object sender, GridPrepareViewStyleInfoEventArgs e)
        {
            if (e.RowIndex > Model.Rows.HeaderCount && e.ColIndex >= 0//Model.Cols.HeaderCount
                && CurrentCell.HasCurrentCellAt(e.RowIndex))
            {
                e.Style.Interior = new BrushInfo(Color.FromArgb(209, 229, 250));
                e.Style.TextColor = Color.Black;
            }
        }
        private void grid_CurrentCellDeactivated(object sender, GridCurrentCellDeactivatedEventArgs e)
        {
            if (!CurrentCell.IsInMoveTo || CurrentCell.MoveToRowIndex != CurrentCell.MoveFromRowIndex)
            {
                RefreshRange(GridRangeInfo.Row(e.RowIndex), GridRangeOptions.MergeAllSpannedCells);
            }
        }		
        private void grid_CurrentCellActivated(object sender, System.EventArgs e)
        {
            if (!CurrentCell.IsInMoveTo || CurrentCell.MoveToRowIndex != CurrentCell.MoveFromRowIndex
            || !CurrentCell.MoveFromActiveState)
            {
                RefreshRange(GridRangeInfo.Row(CurrentCell.RowIndex), GridRangeOptions.MergeAllSpannedCells);
            }
        }
        public void KeepSelection(bool keep)
        {
            if (keep)
            {
                int currentRow = CurrentCell.RowIndex;
                GridRangeInfo info = GridRangeInfo.Cell(currentRow, 0);
                CurrentCell.Activate(currentRow, 0, GridSetCurrentCellOptions.SetFocus);
                Selections.ChangeSelection(info, info, true);
            	CurrentCell.MoveTo(info, GridSetCurrentCellOptions.ScrollInView);
            }
            else
            {
                GridRangeInfo info = GridRangeInfo.Cell(2, 0);
                CurrentCell.Activate(2, 0, GridSetCurrentCellOptions.SetFocus);
                Selections.ChangeSelection(info, info, true);
            }
        }
        public void TipText(int rowIndex, int colIndex, string text)
        {
            //set the comment using custom style property, ExcelTipText
        	var style = new GridExcelTipStyleProperties(this[rowIndex, colIndex]);
        	style.ExcelTipText = text;
        }

        private void ClearTipText()
        {
            for (int i = 0; i <= RowCount; i++)
            {
                for (int j = 0; j <= ColCount; j++)
                {
                    var style = new GridExcelTipStyleProperties(this[i, j]);
                    style.ResetExcelTipText();
                }
            }
        }
        public void SetSelections(int rowIndex, bool update)
        {
            Selections.Clear(true);
            GridRangeInfo info = GridRangeInfo.Cell(rowIndex, 0);
            CurrentCell.Activate(rowIndex, 0, GridSetCurrentCellOptions.SetFocus);
            Selections.ChangeSelection(info, info, update);
            CurrentCell.MoveTo(info, GridSetCurrentCellOptions.ScrollInView);
            if (update)
                UpdateRestrictionSummaryView();
        }

        public int HeaderCount
        {
            get { return Model.Rows.HeaderCount; }
            set {Model.Rows.HeaderCount = value;}
        }
        public void ResizeToFit()
        {
            Model.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
        }

        public int CurrentCellRowIndex
        {
            get { return CurrentCell.RowIndex; }
        }

        public void UpdateRestrictionSummaryView()
        {
            if (SingleAgentRestrictionPresenter == null)
                return;

            AgentInfoHelper agentInfoHelper = SingleAgentRestrictionPresenter.SelectedAgentInfo();
            if (_restrictionSummeryView != null && _restrictionSummeryView.Presenter != null && agentInfoHelper != null)
            {
                _singleAgentRestrictionPresenter.SetSelectedPersonDate(CurrentCell.RowIndex);
                ((RestrictionSummaryPresenter) _restrictionSummeryView.Presenter).GetNextPeriod(agentInfoHelper);
            }
        }

        void RestrictionSummaryGrid_CellClick(object sender, GridCellClickEventArgs e)
        {
            if (e.RowIndex == 1)
            {
                for (int col1 = 0; col1 <= ColCount; ++col1)
                {
                    if (col1 != e.ColIndex)
                    {
                        this[1, col1].CellType = "Header";
                        this[1, col1].Tag = null;
                    }
                }
                ListSortDirection dir = ListSortDirection.Ascending;
                if (this[1, e.ColIndex].HasTag)
                {
                    if (this[1, e.ColIndex].Tag != null)
                    {
                        if ((ListSortDirection)this[1, e.ColIndex].Tag == ListSortDirection.Ascending)
                            dir = ListSortDirection.Descending;
                    }
                }
                this[1, e.ColIndex].Tag = dir;
                bool ascending = false;
                if (dir == ListSortDirection.Ascending)
                    ascending = true;
                _singleAgentRestrictionPresenter.Sort(e.ColIndex, ascending);
                ClearTipText();
                _singleAgentRestrictionPresenter.WriteWarnings(true);
            }

			else if (e.ColIndex == 0 && e.RowIndex > 1)
			{
				UpdateRestrictionSummaryView();
				_restrictionSummeryView.UpdateEditor();
			}
        }

        protected override void OnLeave(System.EventArgs e)
        {
        }

        public void RegisterEvents()
        {
            PrepareViewStyleInfo += grid_PrepareViewStyleInfo;
            CurrentCellDeactivated += grid_CurrentCellDeactivated;
            CurrentCellActivated += grid_CurrentCellActivated;
            CellClick += RestrictionSummaryGrid_CellClick;
            SelectionChanged += RestrictionSummaryGridSelectionChanged;
        }
        private void InitializeComponent()
        {
            ((ISupportInitialize)(this)).BeginInit();
            SuspendLayout();
            // 
            // RestrictionSummaryGrid
            // 
            Properties.FixedLinesColor = Color.Red;
            PrepareViewStyleInfo += grid_PrepareViewStyleInfo;
            CurrentCellDeactivated += grid_CurrentCellDeactivated;
            CurrentCellActivated += grid_CurrentCellActivated;
            CellClick += RestrictionSummaryGrid_CellClick;
            SelectionChanged += RestrictionSummaryGridSelectionChanged;
            ((ISupportInitialize)(this)).EndInit();
            ResumeLayout(false);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_commentMouseController != null)
                    _commentMouseController.Dispose();
                _restrictionSummeryView = null;
                _singleAgentRestrictionPresenter = null;
            }
            base.Dispose(disposing);
        }
    }
}