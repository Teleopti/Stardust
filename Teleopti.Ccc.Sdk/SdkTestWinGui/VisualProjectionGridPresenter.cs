using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using GridTest;
using Syncfusion.Windows.Forms.Grid;
using WindowsFormsApplication1.Helper;

namespace SdkTestWinGui
{
    internal class VisualProjectionGridPresenter
    {
        private GridControl _grid;
        private IList<ScheduleGridColumnBase<VisualProjection>> _gridColumns = new List<ScheduleGridColumnBase<VisualProjection>>();
        private IList<VisualProjection> _schedules = new List<VisualProjection>();

        public VisualProjectionGridPresenter(GridControl grid)
        {
            _grid = grid;
            //GridHelper.GridStyle(_grid);
            //overrride standard
            _grid.ResizeColsBehavior = GridResizeCellsBehavior.ResizeSingle;

            _grid.QueryCellInfo += _grid_QueryCellInfo;
            //_grid.SaveCellInfo += _grid_SaveCellInfo;
            //_grid.QueryColCount += _grid_QueryColCount;
            //_grid.QueryRowCount += _grid_QueryRowCount;
            _grid.KeyDown += _grid_KeyDown;

            configureGrid();
        }

        public void SetDataSource(IList<VisualProjection> schedules)
        {
            _schedules = schedules;
            _grid.RowCount = _schedules.Count + _grid.Rows.HeaderCount;
            //_grid.CurrentCell.MoveTo(1, 1, GridSetCurrentCellOptions.SetFocus);
        }

        public IList<ScheduleGridColumnBase<VisualProjection>> GridColumns
        {
            get { return _gridColumns; }
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

        private void configureGrid()
        {
            _gridColumns = new List<ScheduleGridColumnBase<VisualProjection>>();
            _grid.Rows.HeaderCount = 0;
            _grid.RowCount = 0;

            if (!_grid.CellModels.ContainsKey("VisualProjectionColumnHeaderCell"))
                _grid.CellModels.Add("VisualProjectionColumnHeaderCell", initializeVisualProjectionColumnHeaderCell());
            if (!_grid.CellModels.ContainsKey("VisualProjectionCell"))
                _grid.CellModels.Add("VisualProjectionCell", initializeVisualProjectionCell());
            // Grid must have a Header column
            GridColumns.Add(new VisualProjectionGridRowHeaderColumn());
            ScheduleGridVisualProjectionColumn<VisualProjection> scheduleColumn =
                new ScheduleGridVisualProjectionColumn<VisualProjection>("LayerCollection", String.Empty, null);
            scheduleColumn.Period = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromDays(1));
            GridColumns.Add(scheduleColumn);

            _grid.RowCount = _schedules.Count + _grid.Rows.HeaderCount; //add number of headers, 1 header = 0, 2 headers = 1 ...
            _grid.ColCount = GridColumns.Count - 1;  //col index starts on 0
            _grid.DefaultRowHeight = 25;
            _grid.RowHeights.SetSize(0, 35);

            

        }

        #region Grid Events

// ReSharper disable InconsistentNaming
        void _grid_KeyDown(object sender, KeyEventArgs e)
// ReSharper restore InconsistentNaming
        {
            GridHelper.HandleSelectionKeys(_grid, e);
        }

// ReSharper disable InconsistentNaming
        void _grid_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
// ReSharper restore InconsistentNaming
        {
            if (e.Style.CellIdentity == null)
                return;

            if (validColumn(e.ColIndex))
                _gridColumns[e.ColIndex].GetCellInfo(e, new ReadOnlyCollection<VisualProjection>(_schedules));
        }

// ReSharper disable InconsistentNaming
        void _grid_QueryRowCount(object sender, GridRowColCountEventArgs e)
// ReSharper restore InconsistentNaming
        {
            e.Count = _schedules.Count;
            e.Handled = true;
        }

// ReSharper disable InconsistentNaming
        void _grid_QueryColCount(object sender, GridRowColCountEventArgs e)
// ReSharper restore InconsistentNaming
        {
            e.Count = _gridColumns.Count - 1;
            e.Handled = true;
        }

// ReSharper disable InconsistentNaming
        void _grid_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
// ReSharper restore InconsistentNaming
        {

            if (validCell(e.ColIndex, e.RowIndex))
                _gridColumns[e.ColIndex].SaveCellInfo(e, new ReadOnlyCollection<VisualProjection>(_schedules));
        }

        #endregion

        #region Private

        //private int gridFirstColumn()
        //{
        //    bool found = false;
        //    foreach (ScheduleGridColumnBase<VisualProjection> column in _gridColumns)
        //    {
        //        if (found)
        //        {
        //            if (column.GetType() != typeof(VisualProjectionGridRowHeaderColumn))
        //            {
        //                return _gridColumns.IndexOf(column);
        //            }
        //        }

        //        if (column.GetType() == typeof(VisualProjectionGridRowHeaderColumn))
        //            found = true;
        //    }

        //    return 0;
        //}

        private bool validCell(int columnIndex, int rowIndex)
        {
            bool ret = false;
            if (validColumn(columnIndex) && validRow(rowIndex))
                ret = true;

            return ret;
        }

        private bool validColumn(int columnIndex)
        {
            bool ret = false;
            if ((columnIndex != -1) && (_gridColumns.Count > 0))
                if (columnIndex <= _gridColumns.Count - 1)
                    ret = true;

            return ret;
        }

        private static bool validRow(int rowIndex)
        {
            bool ret = false;
            if (rowIndex >= 0)
                ret = true;

            return ret;
        }

        #endregion
    }
}