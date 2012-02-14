﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Helper;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridColumnGridHelperEventArgs<T> : EventArgs
    {
        public T SourceEntity { get; set; }
    }

    public class SFGridColumnGridHelper<T>
    {
        private readonly GridControl grid;
        private readonly ReadOnlyCollection<SFGridColumnBase<T>> _gridColumns;
        private readonly IList<T> _sourceList;
        private readonly Dictionary<string, ListSortDirection> _sortDirections;

        public event EventHandler<SFGridColumnGridHelperEventArgs<T>> NewSourceEntityWanted;

        public IList<T> SourceList
        {
            get { return _sourceList; }
        }

        public SFGridColumnGridHelper(GridControl gridControl, ReadOnlyCollection<SFGridColumnBase<T>> gridColumns, IList<T> sourceList)
        {
            grid = gridControl;
            _gridColumns = gridColumns;
            _sourceList = sourceList;

            grid.QueryCellInfo += grid_QueryCellInfo;
            grid.SaveCellInfo += grid_SaveCellInfo;
            //grid.QueryColWidth += grid_QueryColWidth;
            grid.QueryColCount += grid_QueryColCount;
            grid.QueryRowCount += grid_QueryRowCount;
            grid.KeyDown += grid_KeyDown;
            grid.ClipboardPaste += grid_ClipboardPaste;

            GridHelper.GridStyle(grid);
            //overrride standard
            grid.ResizeColsBehavior = GridResizeCellsBehavior.ResizeSingle;
            _sortDirections = new Dictionary<string, ListSortDirection>();
            for (int index = 0; index <= grid.ColCount; index ++)
            {
                grid.ColWidths[index] = _gridColumns[index].PreferredWidth;
            }

            foreach (SFGridColumnBase<T> column in _gridColumns)
            {
                _sortDirections.Add(column.BindingProperty, ListSortDirection.Descending); 
            }

            grid.CurrentCell.MoveTo(grid.Rows.HeaderCount + 1, grid.Cols.HeaderCount + 1, GridSetCurrentCellOptions.SetFocus);
        }

        #region Public Methods

        public IList<T> GetSelectedObjects()
        {
            List<T> list = new List<T>();
            GridRangeInfoList selectedRangeInfoList = grid.Model.Selections.GetSelectedRows(false, true);
            
            if (selectedRangeInfoList.ActiveRange == GridRangeInfo.Table()) //Means that the whole grid is selected
                list.AddRange(_sourceList);
            
            foreach (GridRangeInfo gridRangeInfo in selectedRangeInfoList)
            {
                for (int i = gridRangeInfo.Top; i <= gridRangeInfo.Bottom; i++)
                {
                    int index = i + (grid.Rows.HeaderCount - (grid.Cols.HeaderCount + 1));
                    if(index>=0)
                        list.Add(_sourceList[index]);//(T)grid[i,0].Tag);
                }
            }
            return list;
        }

        public T OnNewSourceEntityWanted()
        {
            SFGridColumnGridHelperEventArgs<T> args = new SFGridColumnGridHelperEventArgs<T>();
            if (NewSourceEntityWanted != null)
            {
                NewSourceEntityWanted(this, args);
            }

            return args.SourceEntity;
        }

        #endregion

        #region Grid Events

        internal void grid_KeyDown(object sender, KeyEventArgs e)
        {
            GridHelper.HandleSelectionKeys(grid, e);
        }

        void grid_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex ==0 )
            {
                e.Handled = true;
                return;
            }
            if (ValidColumn(e.ColIndex))
                _gridColumns[e.ColIndex].GetCellInfo(e, new ReadOnlyCollection<T>(_sourceList));
        }

        //This makes the resizing fail?!?!?
        //private void grid_QueryColWidth(object sender, GridRowColSizeEventArgs e)
        //{
        //    if (ValidColumn(e.Index))
        //    {
        //        if(e.Size==0)
        //            e.Size = _gridColumns[e.Index].PreferredWidth;
        //    }
        //    e.Handled = true;
        //}

        void grid_QueryRowCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = _sourceList.Count;
            e.Handled = true;
        }

        void grid_QueryColCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = _gridColumns.Count - 1;
            e.Handled = true;
        }

        internal void grid_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
                _gridColumns[e.ColIndex].SaveCellInfo(e, new ReadOnlyCollection<T>(_sourceList));
        }

        internal void grid_ClipboardPaste(object sender, GridCutPasteEventArgs e)
        {

        }

        #endregion

        #region Private

        private bool ValidCell(int columnIndex, int rowIndex)
        {
            bool ret = false;
            if (ValidColumn(columnIndex) && ValidRow(rowIndex))
                ret = true;

            return ret;
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
            bool ret = false;
            if (rowIndex >= 0)
                ret = true;

            return ret;
        }

        #endregion

        public ListSortDirection SortDirection(string sortMember)
        {
            ListSortDirection dir = _sortDirections[sortMember];
            if (dir==ListSortDirection.Descending)
            {
                _sortDirections[sortMember] = ListSortDirection.Ascending;
                dir = ListSortDirection.Ascending;
            }
            else
            {
                _sortDirections[sortMember] = ListSortDirection.Descending;
                dir = ListSortDirection.Descending;
            }

            return dir;
        }
    }
}