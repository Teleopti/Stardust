﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers
{
    public sealed class DropDownGridCreator:IDisposable
    {
        private GridControl _grid;
        public event EventHandler<GridQueryCellInfoEventArgs> DropDownGridQueryCellInfo;
        public event EventHandler<GridSaveCellInfoEventArgs> DropDownGridQuerySaveCellInfo;
        public event EventHandler<GridRowColCountEventArgs> DropDownGridQueryRowCount;
        public event EventHandler<GridRowColCountEventArgs> DropDownGridQueryColCount;
        public event EventHandler<GridRowColSizeEventArgs> DropDownGridQueryRowHeight;
        public event EventHandler<GridRowColSizeEventArgs> DropDownGridQueryColWidth;
        public event EventHandler<EventArgs> DropDownGridLeaveAndComplete;
        public event EventHandler<CancelEventArgs> DropDownGridCurrentCellValidating;
        public event EventHandler<GridSelectionChangedEventArgs> DropDownGridSelectionChanged;
        public event EventHandler<GridCutPasteEventArgs> DropDownGridClipboardCanCopy;
        public event EventHandler<GridCutPasteEventArgs> DropDownGridClipboardPaste;
        public event EventHandler<GridCellsChangingEventArgs> DropDownGridCellsChanging;
        public event EventHandler<EventArgs> DropDownGridLostFocus;

        private const int gridDefaultRowHeight = 20;
        private const int gridRenderingRowHeight = 2;
        
        public GridControl GetGrid<T>(GridControl container, GridRangeInfo gridRange, int rowIndex, int columnIndex,
                               ReadOnlyCollection<T> collection, int colCount, string headerText)
        {
            //place the new celltype into each of covered cells using upper-left corner
            //also set a different _grid into the Tag of each cell
            //container[rowIndex, columnIndex].CellType = "GridinCell";
            container.CoveredRanges.Add(gridRange);
            
            _grid = new CellEmbeddedGrid();

            _grid.CellModels.Add("HourMinutes", new TimeSpanHourMinutesOrEmptyCellModel(_grid.Model));
            _grid.CellModels.Add(GridCellModelConstants.CellTypeNumericCell, new NumericCellModel(_grid.Model));
            _grid.CellModels.Add(GridCellModelConstants.CellTypeTimeSpanLongHourMinutesCell, new TimeSpanLongHourMinutesCellModel(_grid.Model));
            _grid.CellModels.Add(GridCellModelConstants.CellTypeTimeSpanLongHourMinutesOrEmptyCell, new TimeSpanLongHourMinutesOrEmptyCellModel(_grid.Model));

            var percentCellModel = new PercentCellModel(_grid.Model) {MinMax = new MinMax<double>(-1, 1)};
            _grid.CellModels.Add(GridCellModelConstants.CellTypePercentCell, percentCellModel);
         
            // TODO  Send in if the button shall be visible
            // and set the texts if visible
            var cellModel = new GridDropDownMonthCalendarAdvCellModel(_grid.Model);
            cellModel.HideNoneButton();
            cellModel.HideTodayButton();

			_grid.CellModels.Add(GridCellModelConstants.CellTypeDatePickerCell, cellModel);

            _grid.ColCount = colCount;
            _grid.RowCount = collection.Count;
            _grid.Tag = collection;
            _grid.Text = headerText;

            
            // Show the grid like an Excel Worksheet.
            GridHelper.GridStyle(_grid);

            // Bind Abstract grid events
            getEventsBindings(_grid);
            
            // Set and add grid to container grid
            container[rowIndex, columnIndex].Control = _grid;
            container.Controls.Add(_grid);

            // Merging name column's all cells
            _grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(1, 1, _grid.RowCount, 1));

            container.RowHeights[rowIndex] = collection.Count * gridDefaultRowHeight + 
                                                                gridRenderingRowHeight;
            _grid.ReadOnly = container.ReadOnly; 
            return _grid;
        }

        private void getEventsBindings(GridControl grid)
        {
            grid.ResetVolatileData();

            // Binding Events for abstract grid
            grid.QueryCellInfo += GridQueryCellInfo;
            grid.SaveCellInfo += GridSaveCellInfo;
            grid.QueryRowCount += GridQueryRowCount;
            grid.QueryColCount += GridQueryColCount;
            grid.QueryRowHeight += GridQueryRowHeight;
            grid.QueryColWidth += GridQueryColWidth;
            grid.CurrentCellEditingComplete += GridValidationForLeaveAndComplete;
            grid.Leave += GridValidationForLeaveAndComplete;
            grid.CurrentCellValidating += grid_CurrentCellValidating;
            grid.SelectionChanged += grid_SelectionChanged;
            grid.ClipboardCanCopy += grid_ClipboardCanCopy;
            grid.ClipboardPaste += grid_ClipboardPaste;
            grid.CellsChanging += grid_CellsChanging;
            grid.LostFocus += grid_LostFocus;
            grid.GridBoundsChanged += grid_GridBoundsChanged;

        }

        void grid_GridBoundsChanged(object sender, EventArgs e)
        {
            _grid.CurrentCell.MoveTo(0, 0);
            _grid.Selections.Clear(false);
        }

        void grid_LostFocus(object sender, EventArgs e)
        {
        	var handler = DropDownGridLostFocus;
           if(handler != null)
           {
               handler(sender, e);
           }
        }

        void grid_CellsChanging(object sender, GridCellsChangingEventArgs e)
        {
        	var handler = DropDownGridCellsChanging;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        void grid_ClipboardPaste(object sender, GridCutPasteEventArgs e)
        {
        	var handler = DropDownGridClipboardPaste;
            if (handler!= null)
            {
                var gridModel = sender as GridModel;
                if(gridModel == null) return;

                var grid = gridModel.ActiveGridView as GridControl;
                if (grid == null) return;

                PasteFromClipboard(grid);

                handler(sender, e);
            }
        }

        void grid_ClipboardCanCopy(object sender, GridCutPasteEventArgs e)
        {
        	var handler = DropDownGridClipboardCanCopy;
            if (handler != null)
            {
            	handler(sender, e);
            }
        }

        void grid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
        	var handler = DropDownGridSelectionChanged;
            if (handler != null)
            {
            	handler(sender, e );
            }
          }

        void grid_CurrentCellValidating(object sender, System.ComponentModel.CancelEventArgs e)
        {
        	var handler = DropDownGridCurrentCellValidating;
            if (handler!= null)
            {
            	handler(sender, e);
            }
        }

        private void GridValidationForLeaveAndComplete(object sender, EventArgs e)
        {
        	var handler = DropDownGridLeaveAndComplete;
            if (handler != null)
            {
            	handler(sender, e);
            }
        }

        void GridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
        	var handler = DropDownGridQueryCellInfo;
            if (handler!= null)
            {
            	handler(sender, e);
            }
        }

        void GridSaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
        	var handler = DropDownGridQuerySaveCellInfo;
            if (handler != null)
            {
            	handler(sender, e);
            }
        }

        void GridQueryRowCount(object sender, GridRowColCountEventArgs e)
        {
        	var handler = DropDownGridQueryRowCount;
            if (handler!= null)
            {
            	handler(sender, e);
            }
        }

        void GridQueryColCount(object sender, GridRowColCountEventArgs e)
        {
        	var handler = DropDownGridQueryColCount;
            if (handler!= null)
            {
            	handler(sender, e);
            }
        }

        void GridQueryRowHeight(object sender, GridRowColSizeEventArgs e)
        {
        	var handler = DropDownGridQueryRowHeight;
            if (handler!= null)
            {
            	handler(sender, e);
            }
        }

        void GridQueryColWidth(object sender, GridRowColSizeEventArgs e)
        {
        	var handler = DropDownGridQueryColWidth;
            if (handler!= null)
            {
            	handler(sender, e);
            }
        }

        private SimpleTextPasteAction _gridPasteAction = new SimpleTextPasteAction();

        private SimpleTextPasteAction GridPasteAction
        {
            get { return _gridPasteAction; }
        }

        private static ClipHandler<string> GridClipHandler
        {
            get { return PeopleAdminHelper.ConvertClipboardToClipHandler(); }
        }

        public void PasteFromClipboard(GridControl grid)
        {
            GridHelper.HandlePaste(grid, GridClipHandler, GridPasteAction);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-27
        /// </remarks>
        public void Dispose()
        {
           _grid.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}