using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms;
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
    public class AccessibilityDateGrid : GridViewBase<IAccessibilityDatePresenter, IAccessibilityDateViewModel>
    {
        private readonly IEventAggregator _eventAggregator;

        #region Variables

        private ColumnBase<IAccessibilityDateViewModel> _workShiftRule;
        private ColumnBase<IAccessibilityDateViewModel> _accesability;
        private ColumnBase<IAccessibilityDateViewModel> _date;

        private ToolStripMenuItem _addNewMenuItem;
        private ToolStripMenuItem _deletemenuItem;
        private ToolStripMenuItem _addNewLimiterFromClipboardMenuItem;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessibilityDateGrid"/> class.
        /// </summary>
        /// <param name="presenter">The presenter.</param>
        /// <param name="grid">The grid.</param>
        /// <param name="eventAggregator"></param>
        public AccessibilityDateGrid(IAccessibilityDatePresenter presenter, GridControl grid, IEventAggregator eventAggregator)
            : base(presenter, grid)
        {
            _eventAggregator = eventAggregator;
            using (GridDropDownMonthCalendarAdvCellModel cellModel = new GridDropDownMonthCalendarAdvCellModel(grid.Model))
            {
                cellModel.HideNoneButton();
                cellModel.HideTodayButton();
                if (!Grid.CellModels.ContainsKey("DatePickerCell"))
                    Grid.CellModels.Add("DatePickerCell", cellModel);
            }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        internal override ShiftCreatorViewType Type
        {
            get { return ShiftCreatorViewType.DateExclusion; }
        }

        /// <summary>
        /// Creates the headers.
        /// </summary>
        internal override void CreateHeaders()
        {
            AddColumn(new RowHeaderColumn<IAccessibilityDateViewModel>());
            _workShiftRule = new ReadOnlyTextColumn<IAccessibilityDateViewModel>("WorkShiftRuleSet.Description.Name", UserTexts.Resources.RuleSet, true);
            AddColumn(_workShiftRule);
            _accesability = new ReadOnlyTextColumn<IAccessibilityDateViewModel>("AccessibilityText", UserTexts.Resources.Available, true);
            AddColumn(_accesability);
            _date = new EditableDateTimeColumn<IAccessibilityDateViewModel>("Date", UserTexts.Resources.Date);
            AddColumn(_date);
        }

        //internal override void KeyUp(KeyEventArgs e)
        //{
        //    if (e.KeyValue == 46)
        //    {
        //        if (Presenter.Explorer.View.AskForDelete())
        //            DeleteSelectedGridRows(this, EventArgs.Empty);
        //    }
        //    e.Handled = true;
        //}

        internal override void PrepareView()
        {
            ColCount = GridColumns.Count;
            RowCount = Presenter.ModelCollection.Count;

            Grid.RowCount = RowCount;
            Grid.ColCount = ColCount - 1;

            Grid.Cols.HeaderCount = 0;
            Grid.Rows.HeaderCount = 0;

            Grid.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);

            Grid.ColWidths[1] = 200;
            Grid.ColWidths[2] = 200;
            Grid.ColWidths[3] = 150;

            Grid.Name = "AccessibilityDatesGrid";
        }

        internal override void CreateContextMenu()
        {
            Grid.ContextMenuStrip = new ContextMenuStrip();

            _addNewMenuItem = new ToolStripMenuItem(UserTexts.Resources.New);
            _addNewMenuItem.Click += delegate { Add(); };
        	_addNewMenuItem.Enabled = false;
            Grid.ContextMenuStrip.Items.Add(_addNewMenuItem);

            _addNewLimiterFromClipboardMenuItem = new ToolStripMenuItem(UserTexts.Resources.PasteNew);
            _addNewLimiterFromClipboardMenuItem.Click += delegate { Paste(); };
        	_addNewLimiterFromClipboardMenuItem.Enabled = false;
            Grid.ContextMenuStrip.Items.Add(_addNewLimiterFromClipboardMenuItem);

            _deletemenuItem = new ToolStripMenuItem(UserTexts.Resources.Delete);
            _deletemenuItem.Click += delegate { Delete(); };
        	_deletemenuItem.Enabled = false;
            Grid.ContextMenuStrip.Items.Add(_deletemenuItem);

			Grid.ShowContextMenu += Grid_ShowContextMenu;
		}

		internal void Grid_ShowContextMenu(object sender, ShowContextMenuEventArgs e)
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
        //    AddNewAccebilityDates();
        //    RefreshGrid();
        //}

        internal void AddNewAccebilityDates()
        {
            Presenter.AddAccessibilityDate();
            _eventAggregator.GetEvent<RuleSetChanged>().Publish(Presenter.Explorer.Model.FilteredRuleSetCollection);
        }

        internal void DeleteSelectedDates()
        {
            IList<int> selectedList = new List<int>();
            GridRangeInfoList selectedRangeInfoList = Grid.Model.Selections.GetSelectedRows(true, false);
            foreach (GridRangeInfo rangeInfo in selectedRangeInfoList)
            {
                if (rangeInfo.RangeType == GridRangeInfoType.Rows)
                {
					for (int i = ExcludeHeaderRows(rangeInfo.Top); i <= rangeInfo.Bottom; i++)
                        selectedList.Add((i - 1));
                }
            }

            Presenter.RemoveSelectedAccessibilityDates(new ReadOnlyCollection<int>(selectedList));
            RefreshGrid();
            _eventAggregator.GetEvent<RuleSetChanged>().Publish(Presenter.Explorer.Model.FilteredRuleSetCollection);
        }

        internal void RefreshGrid()
        {
            RowCount = Presenter.ModelCollection.Count;
            Grid.RowCount = RowCount;
            Grid.CurrentCell.MoveTo((Grid.Model.CurrentCellInfo == null) ? 1 : RowCount,
                                    (Grid.Model.CurrentCellInfo == null) ? 1 : Grid.Model.CurrentCellInfo.ColIndex,
                                    GridSetCurrentCellOptions.SetFocus);
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

        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-15
        /// </remarks>
        internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
                GridColumns[e.ColIndex].GetCellInfo(e, Presenter.ModelCollection);
            if (e.RowIndex > 0 && e.ColIndex == 0)
                e.Style.CellValue = " "; //we have to set this to blank space otherwise syncfusion will override with a number.

            e.Handled = true;
        }
              
        #region Overriden Methods

        /// <summary>
        /// Adds the new.
        /// </summary>
        public override void Add()
        {
            AddNewAccebilityDates();
            RefreshGrid();
        }

        ///// <summary>
        ///// Pastes this instance.
        ///// </summary>
        //public override void Paste()
        //{
        //    ClipHandler handler = (ClipHandler)ClipHandlerStateHolder.Current.Get();
        //    ClipHandler<string> clipHandler = GridHelper.ConvertClipHandler(handler);

        //    if (clipHandler.ClipList.Count > 0)
        //    {
        //        int rowSpan = clipHandler.RowSpan();
        //        for (int row = 0; row < rowSpan; row++)
        //            Add();
        //        SimpleTextPasteAction pasteAction = new SimpleTextPasteAction();
        //        GridHelper.HandlePaste(Grid, clipHandler, pasteAction);
        //    }
        //}

        /// <summary>
        /// Deletes the selected items.
        /// </summary>
        public override void Delete()
        {
            DeleteSelectedDates();
        }

        public override void Sort(SortingMode mode)
        {
            int columnIndex = Grid.CurrentCell.ColIndex;
            if (columnIndex > 1)
            {
                SortingModes order = (mode == (SortingMode.Ascending)) ? SortingModes.Ascending : SortingModes.Descending;
                IList<IAccessibilityDateViewModel> result = Sort((ISortColumn<IAccessibilityDateViewModel>)GridColumns[columnIndex], Presenter.ModelCollection, order, columnIndex);
                Presenter.SetAccessibilityDates(new ReadOnlyCollection<IAccessibilityDateViewModel>(result));
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
