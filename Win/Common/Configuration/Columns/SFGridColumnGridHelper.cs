using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Shared;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridColumnGridHelper<T> : IDisposable
    {
        private readonly GridControl _grid;
        private readonly ReadOnlyCollection<SFGridColumnBase<T>> _gridColumns;
        private IList<T> _sourceList;
        private readonly List<T> _originalCopy = new List<T>();
        private readonly int _rowHeaders;
		private readonly int _colHeaders;
        public event EventHandler<SFGridColumnGridHelperEventArgs<T>> NewSourceEntityWanted;
        public event EventHandler PasteFromClipboardFinished;

	    public SFGridColumnGridHelper(GridControl gridControl, ReadOnlyCollection<SFGridColumnBase<T>> gridColumns,
		    IList<T> sourceList): this(gridControl, gridColumns, sourceList, true)
	    {}

        public SFGridColumnGridHelper(GridControl gridControl, ReadOnlyCollection<SFGridColumnBase<T>> gridColumns, IList<T> sourceList, bool styleIt)
        {
            _grid = gridControl;
            _gridColumns = gridColumns;
            _sourceList = sourceList;
            _originalCopy.AddRange(_sourceList);

			_grid.Refresh();

            BindEvents();

            _rowHeaders = _grid.Rows.HeaderCount + 1;
            _colHeaders = _grid.Cols.HeaderCount + 1;

			if (styleIt)
				GridHelper.GridStyle(_grid);
            // Overrrides standard behavior.
            _grid.ResizeColsBehavior = GridResizeCellsBehavior.ResizeSingle;

            for (int index = 0; index <= _grid.ColCount; index++)
            {
                _grid.Model.ColWidths[index] = _gridColumns[index].PreferredWidth;
            }

            _grid.CurrentCell.MoveTo(_rowHeaders, _colHeaders, GridSetCurrentCellOptions.SetFocus);
        }

        private void BindEvents()
        {
            // Binds events to the grid.
            _grid.QueryCellInfo += GridQueryCellInfo;
            _grid.SaveCellInfo += GridSaveCellInfo;
            _grid.QueryColCount += GridQueryColCount;
            _grid.QueryRowCount += GridQueryRowCount;
            _grid.KeyDown += GridKeyDown;
            _grid.ClipboardPaste += GridClipboardPaste;
        }

        private void UnbindEvents()
        {
            // Binds events to the grid.
            _grid.QueryCellInfo -= GridQueryCellInfo;
            _grid.SaveCellInfo -= GridSaveCellInfo;
            _grid.QueryColCount -= GridQueryColCount;
            _grid.QueryRowCount -= GridQueryRowCount;
            _grid.KeyDown -= GridKeyDown;
            _grid.ClipboardPaste -= GridClipboardPaste;
        }

        public void SetSourceList(IList<T> sourceList)
        {
            _originalCopy.Clear();
            _sourceList = sourceList;
            _originalCopy.AddRange(_sourceList);
        	_grid.Refresh();
        }

        public void UnbindClipboardPasteEvent()
        {
            _grid.ClipboardPaste -= GridClipboardPaste;
        }

        public IList<T> SourceList
        {
            get { return _sourceList; }
        }

        public bool AllowExtendedCopyPaste { get; set; }

        public T FindSelectedItem()
        {
            return _sourceList[_grid.CurrentCell.RowIndex - _rowHeaders];
        }

        public ReadOnlyCollection<T> FindSelectedItems()
        {
            var list = new List<T>();
            foreach (GridRangeInfo range in _grid.Selections)
            {
                list.AddRange(GetItemsForRange(range, _colHeaders));
            }

            return new ReadOnlyCollection<T>(list);
        }

		public ReadOnlyCollection<T> FindSelectedItems(int colHeaders)
		{
			var list = new List<T>();
			
			foreach (GridRangeInfo range in _grid.Selections)
			{
				list.AddRange(GetItemsForRange(range, colHeaders));
			}

			for (var i = list.Count -1; i >= 0; i--)
			{
				if(list[i] == null)
					list.RemoveAt(i);
			}

			return new ReadOnlyCollection<T>(list);
		}

        public ReadOnlyCollection<T> FindItemsBySelectionOrPoint(Point point)
        {
            ReadOnlyCollection<T> selectedItems = FindSelectedItems();
            T itemByPoint = FindItemByPoint(point);
            
            if (selectedItems.Count > 0)
            {
                if (itemByPoint == null || selectedItems.Contains(itemByPoint))
                {
                    // Item that mouse points to exist in the selection - return selected items
                    return selectedItems;
                }

            }
            // Get item(s) from mouse point instead
            return new ReadOnlyCollection<T>(new List<T>{itemByPoint});
        }

        public T FindItemByPoint(Point point)
        {
            T retValue = default(T);
            GridRangeInfo gridRangeInfo = _grid.PointToRangeInfo(point);
            
            if(gridRangeInfo.Top>=_rowHeaders)
                retValue = GetItemForRow(gridRangeInfo.Top);

            return retValue;
        }

        public T GetItemForRow(int rowIndex)
        {
            T retValue = default(T);
            if (ValidRow(rowIndex) && ((rowIndex - _rowHeaders) < _sourceList.Count))
            {
                retValue = _sourceList[rowIndex - _rowHeaders];
            }
            return retValue;
        }

        protected ICollection<T> GetItemsForRange(GridRangeInfo range, int colHeaders)
        {
        	int startIndex, count;
        	var parsed = TryGetRange(range, out startIndex, out count);

            ICollection<T> collection = new Collection<T>();
            if (parsed)
            {
                if (startIndex < 0 && count < 0)
                {
                    startIndex = colHeaders;
                    count = _sourceList.Count;
                }

                var endIndex = startIndex + count;

                for (int index = startIndex; index < endIndex; index++)
                {
                    collection.Add(GetItemForRow(index));
                }
            }
            return collection;
        }

        private static bool TryGetRange(GridRangeInfo range, out int startIndex, out int count)
        {
            bool res = false;
            startIndex = -1;
            count = -1;

            switch (range.RangeType)
            {
                case GridRangeInfoType.Cols:
                case GridRangeInfoType.Table:
                    res = true;
                    break;

                case GridRangeInfoType.Cells:
                case GridRangeInfoType.Rows:
                    startIndex = range.Top;
                    count = range.Height;
                    res = true;
                    break;
            }

            return res;
        }

        public bool HasColumnSelected(SFGridColumnBase<T> column)
        {
            int colIndex = _gridColumns.IndexOf(column);
            bool colFound = false;
            if (colIndex > -1)
            {
                foreach (GridRangeInfo range in _grid.Selections)
                {
                    switch (range.RangeType)
                    {
                        case GridRangeInfoType.Table:
                        case GridRangeInfoType.Rows:
                            colFound = true;
                            break;

                        case GridRangeInfoType.Cells:
                        case GridRangeInfoType.Cols:
                            colFound = colIndex >= range.Left && colIndex <= range.Right;
                            break;
                    }

                    if (colFound) break;
                }
            }

            return colFound;
        }

        public void RestoreToOriginalList()
        {
            // Is this really right? it seems to restore the identical list of referenced objects? /Klas Mellbourn
            _sourceList.Clear();
            ((List<T>)_sourceList).AddRange(_originalCopy);
        }

        public void Add(IRepository<T> rep)
        {
            if (_grid.CurrentCell != null)
            {
                int colIndex = _grid.CurrentCell.ColIndex;
                _grid.CurrentCell.MoveTo(_grid.RowCount, colIndex, GridSetCurrentCellOptions.SetFocus);
            }

            Insert(rep);
        }

        public void Insert(IRepository<T> rep)
        {
            T source = OnNewSourceEntityWanted();
            _sourceList.Insert(_grid.CurrentCell.RowIndex, source);
            _originalCopy.Insert(_grid.CurrentCell.RowIndex, source);
            //MarkForInsert(source);
            _grid.Select();
            _grid.RowCount = _sourceList.Count;
            _grid.CurrentCell.MoveTo(_grid.CurrentCell.RowIndex + 1, _grid.CurrentCell.ColIndex, GridSetCurrentCellOptions.NoSetFocus);
            if (rep != null)
            {
                rep.Add(source);
            }
        }

        public void AddFromClipboard(IRepository<T> rep)
        {
        	if (_grid.Model.CurrentCellInfo == null) return;
        	_grid.CurrentCell.MoveTo(_grid.RowCount, _grid.Model.CurrentCellInfo.ColIndex,
        	                         GridSetCurrentCellOptions.SetFocus);
        	PasteFromClipboard(rep, true);
        }

        public void PasteFromClipboard()
        {
            PasteFromClipboard(null, false);
        }

        private void PasteFromClipboard(IRepository<T> rep, bool insert)
        {
            ClipHandler<string> clipHandler = GridHelper.ConvertClipboardToClipHandlerString();
            if (insert)
            {
                ICollection<T> list = new List<T>();
                int clipRowSpan = clipHandler.RowSpan(); // Clipboard row span reference.
                for (int row = 0; row < clipRowSpan; row++)
                {
                    T source = OnNewSourceEntityWanted();
                    list.Add(source);
                    rep.Add(source);
                }
                int currentRowIndex = _grid.Model.CurrentCellInfo.RowIndex; // Grid model current cell row index reference.
                ((List<T>)_sourceList).InsertRange(currentRowIndex, list);
                _originalCopy.InsertRange(currentRowIndex, list);
                
                int currentColIndex = _grid.Model.CurrentCellInfo.ColIndex; // Grid model current cell col index reference.
                int firstCol = GridFirstColumn();   // Grid model 1st column index reference.
                if (currentColIndex < firstCol)
                    currentColIndex = firstCol;

                _grid.RowCount = _sourceList.Count;
                _grid.Selections.Clear();
                _grid.CurrentCell.MoveTo(currentRowIndex + 1, currentColIndex, GridSetCurrentCellOptions.SetFocus);
            }

            IGridPasteAction<string> pasteAction;
            if (AllowExtendedCopyPaste)
                pasteAction = new ExtendedTextPasteAction();
            else
                pasteAction = new SimpleTextPasteAction();

            GridHelper.HandlePaste(_grid, clipHandler, pasteAction);

            OnPasteFromClipboardFinished();
        }

        private void OnPasteFromClipboardFinished()
        {
        	var handler = PasteFromClipboardFinished;
            if (handler!= null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public T OnNewSourceEntityWanted()
        {
            var args = new SFGridColumnGridHelperEventArgs<T>();
        	var handler = NewSourceEntityWanted;
            if (handler != null)
            {
                handler(this, args);
            }

            return args.SourceEntity;
        }

        void GridKeyDown(object sender, KeyEventArgs e)
        {
            GridHelper.HandleSelectionKeys(_grid, e);
        }

        void GridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.Style.CellIdentity == null)
                return;

            if (ValidColumn(e.ColIndex))
            {
                _gridColumns[e.ColIndex].GetCellInfo(e, new ReadOnlyCollection<T>(_sourceList));
            }
        }

        void GridQueryRowCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = _sourceList.Count + (_rowHeaders - 1);
            e.Handled = true;
        }

        void GridQueryColCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = _gridColumns.Count - 1;
            e.Handled = true;
        }

        void GridSaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {

            if (ValidCell(e.ColIndex, e.RowIndex))
                _gridColumns[e.ColIndex].SaveCellInfo(e, new ReadOnlyCollection<T>(_sourceList));
        }

        private void GridClipboardPaste(object sender, GridCutPasteEventArgs e)
        {
            PasteFromClipboard();
            e.Handled = true;
        }

        public bool ValidateGridData()
        {
            for(int colIndex = 0; colIndex < _grid.ColCount; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < _grid.RowCount; rowIndex++ )
                {
                	if (_gridColumns[colIndex].CellValidator == null) continue;
                	if (!ValidCell(colIndex, rowIndex + _rowHeaders)) continue;
                	T dataItem = _sourceList[rowIndex];
                	if (!_gridColumns[colIndex].CellValidator.ValidateCell(dataItem))
                	{
                		return false;
                	}
                }
            }
            return true;
        }


        private int GridFirstColumn()
        {
            bool found = false;
            foreach (SFGridColumnBase<T> column in _gridColumns)
            {
                if (found)
                {
                    if (column.GetType() != typeof(SFGridRowHeaderColumn<T>))
                    {
                        return _gridColumns.IndexOf(column);
                    }
                }

                if (column.GetType() == typeof(SFGridRowHeaderColumn<T>))
                    found = true;
            }

            return 0;
        }

        private bool ValidCell(int columnIndex, int rowIndex)
        {
            bool ret = ValidColumn(columnIndex) && ValidRow(rowIndex);

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

        private bool ValidRow(int rowIndex)
        {
            bool ret = rowIndex >= _rowHeaders;

            return ret;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnbindEvents();
            }
        }
    }
}