using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class RestrictionSummaryView : ScheduleViewBase, IRestrictionSummaryView
    {
        private const int RowHeight = 80;
        private const int ColHeaderHeight = 30;
        private const int ColumnWidth = 120;
        private const int RowHeaderWidth = 140;
        private readonly RestrictionSummaryModel _model;
        private readonly SingleAgentRestrictionPresenter _singleAgentRestrictionPresenter;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public RestrictionSummaryView(GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler,
            SingleAgentRestrictionPresenter singleAgentRestrictionPresenter, IWorkShiftWorkTime workShiftWorkTime, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, 
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
            : base(grid)
        {
            _model = new RestrictionSummaryModel(schedulerState.SchedulingResultState, workShiftWorkTime, schedulerState, new PreferenceNightRestChecker());
            Presenter = new RestrictionSummaryPresenter(this, schedulerState, lockManager, clipHandler,
                                                        schedulePartFilter, _model, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag);
            InitializePreferenceView();
            grid.Name = "RestrictionSummaryView";
            _singleAgentRestrictionPresenter = singleAgentRestrictionPresenter;
        }
        public void InitializePreferenceView()
        {
            ViewGrid.Rows.HeaderCount = 0;
            ViewGrid.Cols.HeaderCount = 0;
            ViewGrid.ColWidths.SetRange(1, ViewGrid.ColCount, ColumnWidth);
            ViewGrid.ColWidthEntries[0].Width = RowHeaderWidth;
            ViewGrid.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
            ViewGrid.NumberedRowHeaders = false;
        }

        public int RestrictionGridRowCount
        {
            get { return ViewGrid.RowCount; }
        }

        protected override int CellWidth()
        {
            return 120;
        }
        internal override void QueryColWidth(object sender, GridRowColSizeEventArgs e)
        {
            if (e.Index == (int)ColumnType.None)
            {
                e.Size = CellWidth();
                e.Handled = true;
            }
            else if (e.Index >= (int)ColumnType.RowHeaderColumn)
            {
                e.Size = CellWidth();
                e.Handled = true;
            }
        }

        public override void AddSelectedSchedulesInColumnToList(GridRangeInfo range, int colIndex, ICollection<IScheduleDay> selectedSchedules)
        {
            for (int j = range.Top; j <= range.Bottom; j++)
            {
                if (colIndex >= 0)
                {
                    IScheduleDay schedulePart = ViewGrid.Model[j, colIndex].CellValue as IScheduleDay;
                    
                    if (schedulePart != null)
                        selectedSchedules.Add(schedulePart);
                }

            }
        }

        public void CellDataLoaded()
        {
            ViewGrid.RowCount = ((RestrictionSummaryPresenter)Presenter).CellDataCollection.Count / 7;
            if (((RestrictionSummaryPresenter)Presenter).CellDataCollection.Count % 7 > 0)
                ViewGrid.RowCount = ViewGrid.RowCount + 1;

            ViewGrid.RowHeights.SetRange(1, ViewGrid.RowCount, RowHeight);
            ViewGrid.RowHeightEntries[0].Height = ColHeaderHeight;
        }
        internal override void QueryColCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = Presenter.ColCount;
            e.Handled = true;
        }
        internal override void QueryRowCount(object sender, GridRowColCountEventArgs e)
        {

            e.Count = Presenter.RowCount;
            e.Handled = true;
        }
        public void UpdateRowCount()
        {
            ViewGrid.RowCount = Presenter.RowCount;
        }
        internal override void CreateHeaders()
        {
            ViewGrid.Rows.HeaderCount = 0; 
            ViewGrid.Cols.HeaderCount = 0;
            ViewGrid.Rows.FrozenCount = 0;
            ViewGrid.Cols.FrozenCount = 0;
            // Reset merge if grid is reused
            ViewGrid.Model.Options.MergeCellsMode = GridMergeCellsMode.None;
        }

        public override bool PartIsEditable()
        {
            IPreferenceCellData cellData;
            return (((RestrictionSummaryPresenter)Presenter).OnQueryCellInfo(ViewGrid.CurrentCell.ColIndex, ViewGrid.CurrentCell.RowIndex, out cellData) && cellData.Enabled);
        }
        internal override void CellClick(object sender, GridCellClickEventArgs e)
        {
            //handle selection when click on col header
            if (e.RowIndex == 0 )
                e.Cancel = true;
        }
        public override Point GetCellPositionForAgentDay(IEntity person, System.DateTime date)
        {
            Point point = new Point(-1 , -1);

            for (int i = 1; i <= ViewGrid.RowCount; i++)
            {
                for (int j = 1; j <= ViewGrid.ColCount; j++)
                {
                    IScheduleDay schedulePart = ViewGrid.Model[i, j].CellValue as IScheduleDay;

                    if (schedulePart != null && schedulePart.Period.Contains(date))
                    {
                        point = new Point(j, i);
                        break;
                    }
                }
            }

            return point;
        }

        public override void SelectFirstDayInGrid()
        {
            GridRangeInfo info = GridRangeInfo.Cell(TheGrid.Rows.HeaderCount + 1, TheGrid.Cols.HeaderCount + 1);
            TheGrid.Selections.Clear(true);
            TheGrid.CurrentCell.Activate(TheGrid.Rows.HeaderCount + 1, TheGrid.Cols.HeaderCount + 1, GridSetCurrentCellOptions.SetFocus);
            TheGrid.Selections.ChangeSelection(info, info, true);
            TheGrid.CurrentCell.MoveTo(TheGrid.Rows.HeaderCount + 1, TheGrid.Cols.HeaderCount + 1);
        }

        public override DateOnly SelectedDateLocal()
        {
            DateOnly tag;
            if (ViewGrid.CurrentCell.ColIndex >= (int)ColumnType.StartScheduleColumns)
            {
                tag = (DateOnly)ViewGrid.Model[1, ViewGrid.CurrentCell.ColIndex].Tag;
            }
            else
            {
                tag = Presenter.SelectedPeriod.DateOnlyPeriod.StartDate;
            }

            return tag;
        }

        public override void InvalidateSelectedRows(IEnumerable<IScheduleDay> schedules)
        {
            if (_singleAgentRestrictionPresenter == null)
                return;
            AgentInfoHelper agentInfoHelper = _singleAgentRestrictionPresenter.SelectedAgentInfo();
            if (agentInfoHelper != null)
                ((RestrictionSummaryPresenter)Presenter).GetNextPeriod(agentInfoHelper);

            var personsToReload = new HashSet<IPerson>();
            foreach (IScheduleDay schedulePart in schedules)
            {
                personsToReload.Add(schedulePart.Person);
                Point point = GetCellPositionForAgentDay(schedulePart.Person, schedulePart.Period.StartDateTime);

                if (point.X != -1 && point.Y != -1)
                {
                    TheGrid.InvalidateRange(GridRangeInfo.Row(point.X));
                }
            }
            _singleAgentRestrictionPresenter.Reload(personsToReload);
        }
        internal override void CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            IScheduleDay cellValue = e.Style.CellValue as IScheduleDay;
            if (cellValue != null && _singleAgentRestrictionPresenter.SelectedAgentInfo() != null &&((RestrictionSchedulingOptions)_singleAgentRestrictionPresenter.SelectedAgentInfo().SchedulingOptions).UseScheduling)
                AddMarkersToCell(e, cellValue, cellValue.SignificantPart());
        }

		public void UpdateEditor()
		{
			UpdateShiftEditor();
		}
    }
}
