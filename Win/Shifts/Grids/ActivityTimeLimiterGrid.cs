using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Events;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Shifts.Grids
{
    public class ActivityTimeLimiterGrid : GridViewBase<IActivityTimeLimiterPresenter, IActivityTimeLimiterViewModel>
    {
        private readonly IEventAggregator _eventAggregator;

        #region Variables

        private ColumnBase<IActivityTimeLimiterViewModel> _ruleSetColumn;
        private ColumnBase<IActivityTimeLimiterViewModel> _baseActivityColumn;
        private ColumnBase<IActivityTimeLimiterViewModel> _operatorColumn;
        private ColumnBase<IActivityTimeLimiterViewModel> _endTimeColumn;

        private ToolStripMenuItem _addNewLimiterMenuItem;
        private ToolStripMenuItem _addNewLimiterFromClipboardMenuItem;
        private ToolStripMenuItem _deleteSelectedLimitersMenuItem;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTimeLimiterGrid"/> class.
        /// </summary>
        /// <param name="presenter">The presenter.</param>
        /// <param name="grid">The grid.</param>
        /// <param name="eventAggregator"></param>
        public ActivityTimeLimiterGrid(IActivityTimeLimiterPresenter presenter, GridControl grid, IEventAggregator eventAggregator)
            : base(presenter, grid)
        {
            _eventAggregator = eventAggregator;
            if (grid == null) return;

            if (!grid.CellModels.ContainsKey("ActivityDropDownCell"))
                grid.CellModels.Add("ActivityDropDownCell", new ActivityDropDownCellModel(grid.Model));
        }

        internal override ShiftCreatorViewType Type
        {
            get { return ShiftCreatorViewType.Limitation; }
        }

        #region Create Grid

        internal override void CreateHeaders()
        {
            AddColumn(new RowHeaderColumn<IActivityTimeLimiterViewModel>());
            _ruleSetColumn = new ReadOnlyTextColumn<IActivityTimeLimiterViewModel>("WorkShiftRuleSet.Description", UserTexts.Resources.RuleSet);
            AddColumn(_ruleSetColumn);
            _baseActivityColumn = new ActivityDropDownColumn<IActivityTimeLimiterViewModel, IActivity>("TargetActivity",
                                                                                    UserTexts.Resources.Activity,
                                                                                    Presenter.Explorer.Model.ActivityCollection,
                                                                                   "Name");
            AddColumn(_baseActivityColumn);
            _operatorColumn = new DropDownColumn<IActivityTimeLimiterViewModel, string>("Operator", UserTexts.Resources.Limit, Presenter.Explorer.Model.OperatorLimitCollection, "OperatorLimit");
            AddColumn(_operatorColumn);
            _endTimeColumn = new EditableHourMinutesColumn<IActivityTimeLimiterViewModel>("Time", UserTexts.Resources.Time);
            AddColumn(_endTimeColumn);
        }

        internal override void PrepareView()
        {
            ColCount = GridColumns.Count;
            RowCount = Presenter.ModelCollection.Count;

            Grid.RowCount = RowCount;
            Grid.ColCount = ColCount - 1;

            Grid.Cols.HeaderCount = 0;
            Grid.Rows.HeaderCount = 0;

            for (int index = 0; index < ColCount; index++)
                Grid.ColWidths[index] = GridColumns[index].PreferredWidth;

            Grid.Name = "ActivityTimeLimiterGrid";
        }

        internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
                GridColumns[e.ColIndex].GetCellInfo(e, Presenter.ModelCollection);
            if (e.RowIndex > 0 && e.ColIndex == 0)
                e.Style.CellValue = " "; //we have to set this to blank space otherwise syncfusion will override with a number.

            e.Handled = true;
        }

        internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
                GridColumns[e.ColIndex].SaveCellInfo(e, Presenter.ModelCollection);
                _eventAggregator.GetEvent<RuleSetChanged>().Publish(new List<IWorkShiftRuleSet> { Presenter.ModelCollection[e.RowIndex - 1].WorkShiftRuleSet }); 
            }

            e.Handled = true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 5/28/2008
        /// </remarks>
        internal override void CreateContextMenu()
        {
            Grid.ContextMenuStrip = new ContextMenuStrip();
            Grid.ContextMenuStrip.Items.Clear();

            _addNewLimiterMenuItem = new ToolStripMenuItem(UserTexts.Resources.New);
            _addNewLimiterMenuItem.Click += delegate { Add(); };
        	_addNewLimiterMenuItem.Enabled = false;
            Grid.ContextMenuStrip.Items.Add(_addNewLimiterMenuItem);

            _addNewLimiterFromClipboardMenuItem = new ToolStripMenuItem(UserTexts.Resources.PasteNew);
            _addNewLimiterFromClipboardMenuItem.Click += delegate { Paste(); };
        	_addNewLimiterFromClipboardMenuItem.Enabled = false;
            Grid.ContextMenuStrip.Items.Add(_addNewLimiterFromClipboardMenuItem);

            _deleteSelectedLimitersMenuItem = new ToolStripMenuItem(UserTexts.Resources.Delete);
            _deleteSelectedLimitersMenuItem.Click += delegate { Delete(); };
        	_deleteSelectedLimitersMenuItem.Enabled = false;
            Grid.ContextMenuStrip.Items.Add(_deleteSelectedLimitersMenuItem);

			Grid.ShowContextMenu += Grid_ShowContextMenu;
		}

		internal void Grid_ShowContextMenu(object sender, Syncfusion.Windows.Forms.ShowContextMenuEventArgs e)
		{
			foreach (var menuItem in Grid.ContextMenuStrip.Items)
			{
				var item = menuItem as ToolStripMenuItem;
				if (item != null)
					item.Enabled = Presenter.Explorer.Model.FilteredRuleSetCollection != null;
			}
			Grid.ContextMenuStrip.Show();
		}
		
        //internal override void AddNewGridRow<T>(object sender, T eventArgs)
        //{
        //    AddNewLimiter();
        //    RefreshGrid();
        //}

        private void AddNewLimiter()
        {
            Presenter.AddAndSaveLimiter();
        }

        //internal override void AddNewGridRowFromClipboard<T>(object sender, T eventArgs)
        //{
        //    //AddFromClipBoard();
        //}

        internal void RefreshGrid()
        {
            RowCount = Presenter.ModelCollection.Count;
            Grid.RowCount = RowCount;
            Grid.CurrentCell.MoveTo((Grid.Model.CurrentCellInfo == null) ? 1 : RowCount,
                                    (Grid.Model.CurrentCellInfo == null) ? 1 : Grid.Model.CurrentCellInfo.ColIndex,
                                    GridSetCurrentCellOptions.SetFocus);
        }

        //internal void AddFromClipBoard()
        //{
        //    ClipHandler<string> clipHandler = GridHelper.ConvertClipboardToClipHandlerString();
        //    int rowSpan = clipHandler.RowSpan();
        //    for (int row = 0; row < rowSpan; row++)
        //        AddNewLimiter();

        //    RefreshGrid();
        //    SimpleTextPasteAction pasteAction = new SimpleTextPasteAction();
        //    GridHelper.HandlePaste(Grid, clipHandler, pasteAction);
        //    clipHandler.Clear();
        //}

        //internal override void DeleteSelectedGridRows<T>(object sender, T eventArgs)
        //{
        //    this.DeleteSelectedLimiter();
        //}

        private void DeleteSelectedLimiter()
        {
            IList<int> selectedList = new List<int>();
            GridRangeInfoList selectedRangeInfoList = Grid.Model.Selections.GetSelectedRows(true, false);
            foreach (GridRangeInfo rangeInfo in selectedRangeInfoList)
            {
                if (rangeInfo.RangeType == GridRangeInfoType.Rows)
                {
					for (int i = ExcludeHeaderRows(rangeInfo.Top); i <= rangeInfo.Bottom; i++)
                    {
                        selectedList.Add((i - 1));
                    }
                }
            }

            Presenter.DeleteLimiter(new ReadOnlyCollection<int>(selectedList));
            selectedList.Clear();

            RowCount = Presenter.ModelCollection.Count;
            Grid.RowCount = RowCount;
            Grid.CurrentCell.MoveTo((Grid.Model.CurrentCellInfo == null) ? 1 : Grid.Model.CurrentCellInfo.RowIndex + 1,
                (Grid.Model.CurrentCellInfo == null) ? 1 : Grid.Model.CurrentCellInfo.ColIndex,
                GridSetCurrentCellOptions.SetFocus);
            Grid.Selections.Clear();
            Grid.Invalidate();
        }

        //internal override void KeyUp(KeyEventArgs e)
        //{
        //    if (e.KeyValue == 46)
        //    {
        //        if (CheckForDelete())
        //            DeleteSelectedGridRows<EventArgs>(this, EventArgs.Empty);
        //    }
        //    e.Handled = true;
        //}

        #endregion

        public override void Sort(bool isAscending, int columnIndex)
        {
            if (columnIndex > 1)
            {
                SortingModes mode = isAscending ? SortingModes.Ascending : SortingModes.Descending;
                IList<IActivityTimeLimiterViewModel> result = Sort((ISortColumn<IActivityTimeLimiterViewModel>)GridColumns[columnIndex],
                    Presenter.ModelCollection, mode, columnIndex);
                Presenter.SetModelCollection(new ReadOnlyCollection<IActivityTimeLimiterViewModel>(result));
                Grid.Invalidate();
            }
        }

        #region Overriden Methods

        /// <summary>
        /// Adds the new.
        /// </summary>
        public override void Add()
        {
            AddNewLimiter();
            RefreshGrid();
        }

        /// <summary>
        /// Deletes the selected items.
        /// </summary>
        public override void Delete()
        {
            DeleteSelectedLimiter();
        }

        /// <summary>
        /// Sorts this instance.
        /// </summary>
        /// <param name="mode"></param>
        public override void Sort(SortingMode mode)
        {
            int columnIndex = Grid.CurrentCell.ColIndex;
            if (columnIndex > 1)
            {
                SortingModes order = (mode == (SortingMode.Ascending)) ? SortingModes.Ascending : SortingModes.Descending;
                IList<IActivityTimeLimiterViewModel> result = Sort((ISortColumn<IActivityTimeLimiterViewModel>)GridColumns[columnIndex], Presenter.ModelCollection, order, columnIndex);
                Presenter.SetModelCollection(new ReadOnlyCollection<IActivityTimeLimiterViewModel>(result));
                Grid.Invalidate();
            }
        }

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        public override void RefreshView()
        {
            Grid.RowCount = Presenter.ModelCollection.Count;
            Grid.Invalidate();
        }

        #endregion
    }
}
