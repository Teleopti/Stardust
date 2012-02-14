using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;

namespace Teleopti.Ccc.Win.Shifts
{
    public abstract class GridViewBase : ICommonOperation, IDisposable
    {
        private GridControl _grid;
        /*private bool _isCtrlPressing = false;*/

        /// <summary>
        /// Initializes a new instance of the <see cref="GridViewBase"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        protected GridViewBase(GridControl grid)
        {
            _grid = grid;
            /*_grid.KeyDown += grid_KeyDown;
            _grid.KeyUp += grid_KeyUp;*/
            Init();
        }

        /*void grid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C)
            {
                if(_isCtrlPressing)
                {
                    _isCtrlPressing = false;
                    GridRangeInfoList selectedRangeInfoList = Grid.Model.Selections.GetSelectedRows(true, false);
                    if (selectedRangeInfoList.Count > 0)
                        ClipboardCopy();
                }
            }
        }

        void grid_KeyDown(object sender, KeyEventArgs e)
        {
            _isCtrlPressing = false;
            if (e.Control)
                _isCtrlPressing = true;
        }*/

        private void Init()
        {
            GridHelper.GridStyle(_grid);
            _grid.ResizeColsBehavior = GridResizeCellsBehavior.ResizeSingle;
            _grid.Dock = DockStyle.Fill;
            _grid.Location = new System.Drawing.Point(0, 0);
            _grid.Visible = true;
            _grid.CellModels.Add("HourMinutes", new TimeSpanHourMinutesCellModel(_grid.Model));
        }

        internal abstract ShiftCreatorViewType Type { get; }

        public GridControl Grid
        {
            get
            {
                return _grid;
            }
            //private set
            //{
            //    _grid = value;
            //}
        }

        public virtual void ClearView()
        {
            RowCount = 0;
            ColCount = 0;
            Grid.ColCount = 0;
            Grid.RowCount = 0;

            Grid.RowHeights.ResetModified();
        }

        public int ColCount { get; set; }

        public int RowCount { get; set; }

        public int ColHeaders
        {
            get { return Grid.Cols.HeaderCount; }
        }

        public int RowHeaders
        {
            get { return Grid.Rows.HeaderCount; }
        }

        public bool IsRightToLeft
        {
            get { return Grid.RightToLeft == System.Windows.Forms.RightToLeft.Yes; }
            set
            {
                if (value) Grid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
                else Grid.RightToLeft = System.Windows.Forms.RightToLeft.No;
            }
        }

        internal virtual bool ValidCell(int columnIndex, int rowIndex)
        {
            return ValidColumn(columnIndex) && ValidRow(rowIndex);
        }

        internal virtual bool ValidColumn(int columnIndex)
        {
            bool ret = false;
            if ((columnIndex != -1) && (ColCount > 0))
                if (columnIndex <= ColCount - 1)
                    ret = true;

            return ret;
        }

        internal virtual bool ValidRow(int rowIndex)
        {
            return rowIndex >= 0;
        }

        internal virtual void CreateHeaders()
        {
        }

        internal virtual void CreateContextMenu()
        {
            if (Grid.ContextMenuStrip != null)
            {
                Grid.ContextMenuStrip.Items.Clear();
            }
        }

        internal virtual void MergeHeaders() { }

        internal virtual void KeyUp(KeyEventArgs e)
        {
            GridHelper.HandleSelectionKeys(Grid, e);
        }
        internal virtual void KeyDown(KeyEventArgs e)
        {
            
        }

        //internal void QueryColCount(object sender, GridRowColCountEventArgs e)
        //{
        //    e.Count = ColCount - 1;
        //}

        internal virtual void QueryCellInfo(GridQueryCellInfoEventArgs e)
        {
        }

        internal virtual void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
        }

        internal virtual void SelectionChanged(GridSelectionChangedEventArgs e)
        {
        }



        internal virtual void ClipboardPaste(GridCutPasteEventArgs e)
        {
            PasteFromClipboard(false);
        }
        internal virtual void CellButtonClicked(GridCellButtonClickedEventArgs e)
        {
        }

        internal virtual void DrawCellButton(GridDrawCellButtonEventArgs e)
        {
        }

        protected void PasteFromClipboard(bool insert)
        {
            if (insert) InsertClipHandlerToGrid();
            GridHelper.HandlePaste(Grid, GridClipHandler, GridPasteAction);
        }

		protected int ExcludeHeaderRows(int firstRow)
		{
			var firstNonHeaderRow = firstRow <= Grid.Rows.HeaderCount ? Grid.Rows.HeaderCount + 1 : firstRow;
			return firstNonHeaderRow;
		}

        protected static ClipHandler<string> GridClipHandler
        {
            get { return ConvertClipboardToClipHandler(); }
        }

        private readonly ExtendedTextPasteAction _gridPasteAction = new ExtendedTextPasteAction();

        internal ExtendedTextPasteAction GridPasteAction
        {
            get { return _gridPasteAction; }
        }

        protected virtual void InsertClipHandlerToGrid() { }

        internal virtual void PrepareView()
        {
            RowCount = 0;
        }

        internal virtual void CellClick(object sender, GridCellClickEventArgs e)
        {

        }

        private static ClipHandler<string> ConvertClipboardToClipHandler()
        {
            ClipHandler<string> clipHandler = new ClipHandler<string>();

            if (Clipboard.ContainsText() == true)
            {
                string clipboardText = Clipboard.GetText();
                clipboardText = clipboardText.Replace("\n", "");
                clipboardText = clipboardText.TrimEnd();
                string[] clipBoardRows = clipboardText.Split('\r');

                int row = 0;
                foreach (string rowString in clipBoardRows)
                {
                    string[] clipBoardCols = rowString.Split('\t');
                    int col = 0;
                    foreach (string columnString in clipBoardCols)
                    {
                        clipHandler.AddClip(row, col, columnString);
                        col++;
                    }
                    row++;
                }
            }
            return clipHandler;
        }

        internal virtual void AddNewGridRow<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void InsertNewGridRow<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void AddNewGridRowFromClipboard<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void InsertNewGridRowFromClipboard<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void DeleteSelectedGridRows<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void ClipboardCopy()
        {
            ClipHandler clipHandler = new ClipHandler();
            GridHelper.GridCopySelection(Grid, clipHandler, true);
            ClipHandlerStateHolder.Current.Set(clipHandler);
        }

        protected virtual void Dispose(bool disposing) { }

        public virtual void Sort(bool isAscending, int columnIndex)
        {

        }

        public IList<T> Sort<T>(ISortColumn<T> column,
                                ReadOnlyCollection<T> collection,
                                SortingModes mode, int columnIndex)
        {
            IList<T> sortedList = null;

            if (columnIndex != -1)
            {
                ISort<T> iSort = new SortingBase<T>();
                sortedList = iSort.Sort(column, collection, mode);
            }
            return sortedList;
        }

        #region ICommonOperation Members

        /// <summary>
        /// Adds the new.
        /// </summary>
        public virtual void Add()
        {

        }

        /// <summary>
        /// Deletes the selected items.
        /// </summary>
        public virtual void Delete()
        {

        }

        /// <summary>
        /// Renames this instance.
        /// </summary>
        public virtual void Rename()
        {

        }

        /// <summary>
        /// Sorts this instance.
        /// </summary>
        /// <param name="mode"></param>
        public virtual void Sort(SortingMode mode)
        {

        }

        /// <summary>
        /// Cuts this instance.
        /// </summary>
        public virtual void Cut()
        {
            Copy();
            Delete();

        }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        public virtual void Copy()
        {
            ClipboardCopy();
        }

        /// <summary>
        /// Pastes this instance.
        /// </summary>
        public virtual void Paste()
        {
            //ClipHandler<string> clipHandler = GridHelper.ConvertClipboardToClipHandlerString();
            ClipHandler handler = (ClipHandler)ClipHandlerStateHolder.Current.Get();
            ClipHandler<string> clipHandler = GridHelper.ConvertClipHandler(handler);
            
            if (clipHandler.ClipList.Count > 0)
            {
                PerformPaste(clipHandler);
            }
        }

        private void PerformPaste(ClipHandler<string> clipHandler)
        {
            ExtendedTextPasteAction pasteAction = new ExtendedTextPasteAction();
            GridHelper.HandlePaste(Grid, clipHandler, pasteAction);
        }

        public virtual void PasteSpecial()
        {
            ClipHandler handler = (ClipHandler)ClipHandlerStateHolder.Current.Get();
            ClipHandler<string> clipHandler = GridHelper.ConvertClipHandler(handler);

            Paste(clipHandler);
        }

        private void Paste(ClipHandler<string> clipHandler)
        {
            if (clipHandler.ClipList.Count > 0)
            {
                int rowSpan = clipHandler.RowSpan();
                for (int row = 0; row < rowSpan; row++)
                    Add();
                PerformPaste(clipHandler);
            }
        }

        /// <summary>
        /// Moves up.
        /// </summary>
        public virtual void MoveUp()
        {

        }

        /// <summary>
        /// Moves down.
        /// </summary>
        public virtual void MoveDown()
        {

        }

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        public virtual void RefreshView()
        {

        }

        public virtual void Amounts(IList<int> shiftAmount)
        {

        }

        /// <summary>
        /// Clears the view.
        /// </summary>
        public virtual void Clear()
        {
            
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    public abstract class GridViewBase<T, TColumnType> : GridViewBase
    {
        private readonly T _presenter;

        private readonly IList<IColumn<TColumnType>> _gridColumns;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridViewBase&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="presenter">The presenter.</param>
        /// <param name="grid">The grid.</param>
        protected GridViewBase(T presenter, GridControl grid) 
            : base(grid)
        {
            _presenter = presenter;
            _gridColumns = new List<IColumn<TColumnType>>();
        }

        /// <summary>
        /// Gets the presenter.
        /// </summary>
        /// <value>The presenter.</value>
        public T Presenter
        {
            get
            {
                return _presenter;
            }
        }

        /// <summary>
        /// Gets the grid columns.
        /// </summary>
        /// <value>The grid columns.</value>
        public ReadOnlyCollection<IColumn<TColumnType>> GridColumns
        {
            get
            {
                return new ReadOnlyCollection<IColumn<TColumnType>>(_gridColumns);
            }
        }

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="column">The column.</param>
        public void AddColumn(IColumn<TColumnType> column)
        {
            _gridColumns.Add(column);
        }

        /// <summary>
        /// Sets the row count.
        /// </summary>
        public virtual void SetRowCount()
        {
            Grid.RowCount = ((ICommon<T>) Presenter).ModelCollection.Count;
        }

        internal override void KeyUp(KeyEventArgs e)
        {
            if (e.KeyValue == 46)
                Delete();
            e.Handled = true;
        }

        
    }

    public class ClipHandlerStateHolder
    {
        private object value;

        private static ClipHandlerStateHolder _instance;

        public static ClipHandlerStateHolder Current
        {
            get
            {
                if (_instance == null)
                    _instance = new ClipHandlerStateHolder();
                return _instance;
            }
        }

        public void Set(object newValue)
        {
            this.value = newValue;
        }

        public object Get()
        {
            return this.value;
        }
    }
}
