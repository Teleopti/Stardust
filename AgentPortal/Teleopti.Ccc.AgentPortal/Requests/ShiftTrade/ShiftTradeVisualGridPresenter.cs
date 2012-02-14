using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Cells;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Rows;
using Teleopti.Ccc.AgentPortal.Common.Controls.Columns;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Requests.ShiftTrade
{
    public class ShiftTradeVisualGridPresenter
    {
        private readonly GridControl _grid;
        private IList<ScheduleGridColumnBase<ShiftTradeDetailModel>> _gridColumns;
        private IList<ShiftTradeDetailModel> _shiftTradeDetailModels;
        private ShiftTradeVisualProjectionColumn _scheduleColumn;

        public ShiftTradeVisualGridPresenter(GridControl grid, IList<ShiftTradeDetailModel> shiftTradeDetailModels)
        {
            _grid = grid;
            _shiftTradeDetailModels = shiftTradeDetailModels;
            configureGrid();
        }

        private void grid_KeyDown(object sender, KeyEventArgs e)
        {
            GridHelper.HandleSelectionKeys(_grid, e);
        }

        private void grid_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.Style.CellIdentity == null)
                return;

            if (ValidColumn(e.ColIndex))
                _gridColumns[e.ColIndex].GetCellInfo(e, _shiftTradeDetailModels);
        }

        private void grid_QueryRowCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = _shiftTradeDetailModels.Count;
            e.Handled = true;
        }

        private void grid_QueryColCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = _gridColumns.Count - 1;
            e.Handled = true;
        }

        private void grid_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
                _gridColumns[e.ColIndex].SaveCellInfo(e, _shiftTradeDetailModels);
        }

        private void grid_Resize(object sender, EventArgs e)
        {
            _grid.ColWidths[2] = _grid.ViewLayout.HscrollAreaBounds.Width;
        }

        private bool ValidCell(int columnIndex, int rowIndex)
        {
            return ValidColumn(columnIndex) && ValidRow(rowIndex);
        }

        private bool ValidColumn(int columnIndex)
        {
            bool ret = false;
            if ((columnIndex != -1) && (_gridColumns.Count > 0))
                if (columnIndex <= _gridColumns.Count - 1)
                    ret = true;

            return ret;
        }

        private static bool ValidRow(int rowIndex)
        {
            return (rowIndex >= 0);
        }

        private VisualProjectionColumnHeaderCellModel initializeVisualProjectionColumnHeaderCell()
        {
            VisualProjectionColumnHeaderCellModel cellModel = new VisualProjectionColumnHeaderCellModel(_grid.Model);
            return cellModel;
        }

        private VisualProjectionCellModel initializeVisualProjectionCell()
        {
            VisualProjectionCellModel cellModel = new VisualProjectionCellModel(_grid.Model);
            return cellModel;
        }

        private TimePeriod DisplayedPeriod()
        {
            TimeSpan minStart = TimeSpan.MaxValue;
            TimeSpan maxEnd = TimeSpan.MinValue;
            foreach (var shiftTradeDetailModel in _shiftTradeDetailModels)
            {
                var period = shiftTradeDetailModel.VisualProjection.Period();
                if (period.HasValue)
                {
                    if (period.Value.StartTime < minStart)
                        minStart = period.Value.StartTime;
                    if (period.Value.EndTime > maxEnd)
                        maxEnd = period.Value.EndTime;
                }
            }
            if (minStart == TimeSpan.MaxValue && maxEnd == TimeSpan.MinValue)
            {
                minStart = TimeSpan.FromHours(8);
                maxEnd = TimeSpan.FromHours(17);
            }
            return new TimePeriod(minStart.Add(TimeSpan.FromHours(-1)), maxEnd.Add(TimeSpan.FromHours(1)));
        }

        private void configureGrid()
        {
            _grid.BeginUpdate();
            _grid.QueryCellInfo += grid_QueryCellInfo;
            _grid.SaveCellInfo += grid_SaveCellInfo;
            _grid.QueryColCount += grid_QueryColCount;
            _grid.QueryRowCount += grid_QueryRowCount;
            _grid.KeyDown += grid_KeyDown;
            _grid.Resize += grid_Resize;

            _gridColumns = new List<ScheduleGridColumnBase<ShiftTradeDetailModel>>();
            _grid.Rows.HeaderCount = 0;
            _grid.Cols.HeaderCount = 1;
            _grid.Cols.FrozenCount = 1;
            _grid.Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeRowsInColumn;

            if (!_grid.CellModels.ContainsKey("VisualProjectionColumnHeaderCell"))
                _grid.CellModels.Add("VisualProjectionColumnHeaderCell", initializeVisualProjectionColumnHeaderCell());
            if (!_grid.CellModels.ContainsKey("VisualProjectionCell"))
                _grid.CellModels.Add("VisualProjectionCell", initializeVisualProjectionCell());

            // Grid must have a Header column
            _gridColumns.Add(new ShiftTradeVisualRowHeaderColumn()); //Date
            _gridColumns.Add(new ShiftTradeVisualRowHeaderColumn()); //Name
            _scheduleColumn = new ShiftTradeVisualProjectionColumn("LayerCollection", String.Empty, null);
            _scheduleColumn.Period = DisplayedPeriod();
            _gridColumns.Add(_scheduleColumn);

            _grid.RowCount = _shiftTradeDetailModels.Count + _grid.Rows.HeaderCount; //add number of headers, 1 header = 0, 2 headers = 1 ...
            _grid.ColCount = _gridColumns.Count - 1;  //col index starts on 0
            _grid.DefaultRowHeight = 25;
            _grid.RowHeights.SetSize(0, 35);
            _grid.Cols.Size.ResizeToFit(GridRangeInfo.Cols(0, 1), GridResizeToFitOptions.IncludeHeaders);
            _grid.CurrentCell.MoveTo(1, 1, GridSetCurrentCellOptions.SetFocus);
            _grid.ColWidths[2] = _grid.ViewLayout.HscrollAreaBounds.Width;
            _grid.Model.MergeCells.DelayMergeCells(GridRangeInfo.Table());

            GridHelper.GridStyle(_grid);
            _grid.EndUpdate();
        }

        public void UpdateDataSource(IList<ShiftTradeDetailModel> shiftTradeDetailModels)
        {
            _shiftTradeDetailModels = shiftTradeDetailModels;
            _scheduleColumn.Period = DisplayedPeriod();
            _grid.RowCount = _shiftTradeDetailModels.Count;
            _grid.Model.MergeCells.DelayMergeCells(GridRangeInfo.Table());
            _grid.InvalidateRange(GridRangeInfo.Table());
        }

        public ICollection<ShiftTradeDetailModel> SelectedShiftTradeDetailModels()
        {
            var details = new List<ShiftTradeDetailModel>();
            var rowRanges = _grid.Selections.GetSelectedRows(false, true);
            for (int i = 0; i < rowRanges.Count; i++)
            {
                for (int row = rowRanges[i].Top; row <= rowRanges[i].Bottom; row++)
                {
                    int index = row - (_grid.Rows.HeaderCount + 1);
                    if (index < 0 || index >= _shiftTradeDetailModels.Count) continue;

                    details.Add(_shiftTradeDetailModels[index]);
                }
            }
            return details;
        }
    }
}