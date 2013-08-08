using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common.Controls.Columns;
using Teleopti.Ccc.AgentPortal.Helper;

namespace Teleopti.Ccc.AgentPortal.Reports.Grid
{
    internal class ScheduleGridColumnGridHelper<T>
    {
        private readonly GridControl grid;
        private readonly IList<ScheduleGridColumnBase<T>> _gridColumns;
        private readonly List<T> _sourceList;
        internal int RowCount { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public ScheduleGridColumnGridHelper(GridControl gridControl, IList<ScheduleGridColumnBase<T>> gridColumns, List<T> sourceList)
        {
            grid = gridControl;
            _gridColumns = gridColumns;
            _sourceList = sourceList;
            RowCount = _sourceList.Count;

            grid.QueryCellInfo += grid_QueryCellInfo;
            grid.SaveCellInfo += grid_SaveCellInfo;
            grid.QueryColCount += grid_QueryColCount;
            grid.QueryRowCount += grid_QueryRowCount;
            grid.KeyDown += grid_KeyDown;

            GridHelper.GridStyle(grid);
            //overrride standard
            grid.ResizeColsBehavior = GridResizeCellsBehavior.ResizeSingle;
        }

        void grid_KeyDown(object sender, KeyEventArgs e)
        {
            GridHelper.HandleSelectionKeys(grid, e);
        }

        void grid_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.Style.CellIdentity == null)
                return;

            if (validColumn(e.ColIndex))
                _gridColumns[e.ColIndex].GetCellInfo(e, new ReadOnlyCollection<T>(_sourceList));
        }

        void grid_QueryRowCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = RowCount;
            e.Handled = true;
        }

        void grid_QueryColCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = _gridColumns.Count - 1;
            e.Handled = true;
        }

        void grid_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (validCell(e.ColIndex, e.RowIndex))
                _gridColumns[e.ColIndex].SaveCellInfo(e, new ReadOnlyCollection<T>(_sourceList));
        }

        private bool validCell(int columnIndex, int rowIndex)
        {
            bool ret = validColumn(columnIndex) && validRow(rowIndex);

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
            bool ret = rowIndex >= 0;

            return ret;
        }
    }
}