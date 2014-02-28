using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
    public class PersonalAccountGridView : DropDownGridViewBase
    {
        private const int DefaultWidth = 20;
        private const int DefaultHeight = 20;

        private Rectangle _rect;

        private GridControl _currentChildGrid;
        private int _currentParentRowIndex;

        private ToolStripMenuItem _addNewPersonPeriodMenuItem;
        private ToolStripMenuItem _deletePersonPeriodMenuItem;

        private ToolStripMenuItem _pasteSpecialPersonPeriodMenuItem;
        private ToolStripMenuItem _copySpecialPersonPeriodMenuItem;

        private IList<IPersonAccountModel> _currentSelectedPersonAccounts;

        // Holds the selcted person account collection - help for copy functionality
        private readonly IList<IAccount> _selectedPersonAccountCollection = new List<IAccount>();

        private ColumnBase<IPersonAccountModel> _pushButtonColumn;
        private ColumnBase<IPersonAccountModel> _gridInCellColumn;
        private ColumnBase<IPersonAccountModel> _fullNameColumn;
        private ColumnBase<IPersonAccountModel> _accountDateColumn;
        private ColumnBase<IPersonAccountModel> _absenceTypeColumn;
        private ColumnBase<IPersonAccountModel> _absenceUnitColumn;
        private ColumnBase<IPersonAccountModel> _balanceInColumn;
        private ColumnBase<IPersonAccountModel> _extraTimeAllowedColumn;
        private ColumnBase<IPersonAccountModel> _accruedTimeColumn;
        private ColumnBase<IPersonAccountModel> _usedTimeColumn;
        private ColumnBase<IPersonAccountModel> _balanceOutTimeColumn;
        private ColumnBase<IPersonAccountModel> _remainingTimeColumn;

        private readonly List<ColumnBase<IPersonAccountModel>> _gridColumns = new List<ColumnBase<IPersonAccountModel>>();

        private ColumnBase<IPersonAccountChildModel> _childRowHeaderColumn;
        private ColumnBase<IPersonAccountChildModel> _childFullNameColumn;
        private ColumnBase<IPersonAccountChildModel> _childAccountDateColumn;
        private ColumnBase<IPersonAccountChildModel> _childTrackerTypeColumn;
        private ColumnBase<IPersonAccountChildModel> _childTrackerUnitColumn;
        private ColumnBase<IPersonAccountChildModel> _childBalanceInColumn;
        private ColumnBase<IPersonAccountChildModel> _childExtraTimeAllowedColumn;
        private ColumnBase<IPersonAccountChildModel> _childAccruedTimeColumn;
        private ColumnBase<IPersonAccountChildModel> _childUsedTimeColumn;
        private ColumnBase<IPersonAccountChildModel> _childBalanceOutTimeColumn;
        private ColumnBase<IPersonAccountChildModel> _childRemainingTimeColumn;

        private readonly IList<IColumn<IPersonAccountChildModel>> _childGridColumns =
            new List<IColumn<IPersonAccountChildModel>>();

        private bool _expandedGridSelection = false;

        internal override ViewType Type
        {
            get { return ViewType.PersonalAccountGridView; }
        }

        public override int ParentGridLastColumnIndex
        {
            get
            {
                return 12;
            }
        }

        internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
                _gridColumns[e.ColIndex].SaveCellInfo(e, new ReadOnlyCollection<IPersonAccountModel>(
                                                             FilteredPeopleHolder.PersonAccountModelCollection));
            }
        }

        internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
                //Solusion to session close isse
                //PeopleWorksheet.StateHolder.UnitOfWork.Reassociate(FilteredPeople);
                _gridColumns[e.ColIndex].GetCellInfo(e, new ReadOnlyCollection<IPersonAccountModel>(
                                                        FilteredPeopleHolder.PersonAccountModelCollection));
            }
            base.QueryCellInfo(e);
        }

        internal override void CellButtonClicked(GridCellButtonClickedEventArgs e)
        {
            if (e.ColIndex == 1)
            {
                ToggleChildGrid(e.RowIndex);
            }
        }

        internal override void DrawCellButton(GridDrawCellButtonEventArgs e)
        {
            if (e.Style.CellType == "PushButton")
            {
                _rect = new Rectangle(e.Button.Bounds.X, e.Button.Bounds.Y, DefaultWidth, Grid.DefaultRowHeight);
                e.Button.Bounds = _rect;
            }
        }

        internal override void SelectedDateChange(object sender, EventArgs e)
        {
            CollapseAllExpandedRows();
            ReloadPersonAccounts();
        }

        private void ReloadPersonAccounts()
        {
            FilteredPeopleHolder.GetParentPersonAccounts();
            Grid.Invalidate();
        }

        private void CollapseAllExpandedRows()
        {
            for (var rowIndex = 0; rowIndex < Grid.RowCount; rowIndex++)
            {
                var actualRowIndex = rowIndex + 1;
                var gridInfo =
                    GridRangeInfo.Cells(actualRowIndex, GridInCellColumnIndex, actualRowIndex, ParentGridLastColumnIndex);

                if (FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].ExpandState)
                {
                    FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].ExpandState = false;
                    // Get child grid and dispose it
                    var gridControl = Grid[actualRowIndex, GridInCellColumnIndex].Control as GridControl;

                    if (gridControl != null)
                    {
                        FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].GridControl = null;

                        gridCreatorDispose(gridControl, gridInfo);
                        Grid.RowHeights[actualRowIndex] = Grid.DefaultRowHeight;
                    }
                }
            }
        }

        internal override void ClipboardCanCopy(object sender, GridCutPasteEventArgs e)
        {
            if (Grid.Model.CurrentCellInfo == null)
            {
                //TODO:Need to implement when person is not selected scenario 
                return;
            }

            int rowIndex = Grid.CurrentCell.RowIndex;

            _selectedPersonAccountCollection.Clear();
            CanCopyRow = false;
            CanCopyChildRow = false;

            if (rowIndex == 0) return;

            // Gets the actual item index
            rowIndex -= 1;

            if (!FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].ExpandState)
            {
                CanCopyRow = true;
                var accounts =
                    FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].Parent;

                foreach (IAccount account in accounts.AllPersonAccounts())
                {
                    // Parent processings
                    if (!_selectedPersonAccountCollection.Contains(account))
                        _selectedPersonAccountCollection.Add(account);
                }
            }
        }

        public bool ExpandedGridSelection
        {
            get { return _expandedGridSelection; }
        }

        internal override void SelectionChanged(GridSelectionChangedEventArgs e, bool eventCancel)
		{
			var rangeLength = Grid.Model.SelectedRanges.Count;

			if (rangeLength == 0)
			{
				return;
			}

			var accountList = new List<IAccount>();
			var gridDataList = new List<IPersonAccountModel>();

			for (var rangeIndex = 0; rangeIndex < rangeLength; rangeIndex++)
			{
				var rangeInfo = Grid.Model.SelectedRanges[rangeIndex];

				var top = 1; // This is to skip if Range is Empty.
				var length = 0;

				switch (rangeInfo.RangeType)
				{
					case GridRangeInfoType.Cols:
					case GridRangeInfoType.Table:
						top = 1;
						length = FilteredPeopleHolder.FilteredPersonCollection.Count;
						RowCount += length;
						break;

					case GridRangeInfoType.Rows:
					case GridRangeInfoType.Cells:
						top = rangeInfo.Top;
						length = top + rangeInfo.Height - 1;
						RowCount += rangeInfo.Height;
						break;
					default:
						break;
				}

                SetIfAnyRowInSelectionExpanded(rangeInfo);

                if ((rangeInfo.RangeType != GridRangeInfoType.Rows) ||
                    !FilteredPeopleHolder.PersonAccountModelCollection[rangeInfo.Top - 1].ExpandState)
                {
                    if (top > 0)
                    {
                        for (int index = top - 1; index < length; index++)
                        {
                            accountList.Add(FilteredPeopleHolder.AllAccounts[FilteredPeopleHolder.FilteredPersonCollection[index]]
                                .Find(FilteredPeopleHolder.SelectedDate).FirstOrDefault());
                            gridDataList.Add(FilteredPeopleHolder.PersonAccountModelCollection[index]);
                        }
                    }
                }
			}

			_currentSelectedPersonAccounts = gridDataList;

			CurrentGrid = Grid;
		}

        private void SetIfAnyRowInSelectionExpanded(GridRangeInfo rangeInfo)
        {
            _expandedGridSelection = false;
            if (rangeInfo.IsTable)
            {
                for(int index = 0; index < FilteredPeopleHolder.FilteredPersonCollection.Count; index++)
                {
                    if(FilteredPeopleHolder.PersonAccountModelCollection[index].ExpandState)
                    {
                        _expandedGridSelection = true;
                        return; 
                    }
                }
            }
            for (int i = rangeInfo.Top - 1; i < rangeInfo.Bottom; i++)
            {
                if (i>=0 && FilteredPeopleHolder.PersonAccountModelCollection[i].ExpandState)
                {
                    _expandedGridSelection = true;
                }
            }
        }

        void ParentTrackerDescription_Changed(object sender, SelectedItemChangeEventArgs<IPersonAccountModel, IAbsence> e)
        {
            Cursor.Current = Cursors.WaitCursor;

            // Gets the relevant data item
            var dataItem = e.DataItem;

            var selectedAbsence = e.SelectedItem;
            var accountCollection = dataItem.Parent;

            if (dataItem.AccountDate != null)
            {
                IAccount newAccount = selectedAbsence.Tracker.CreatePersonAccount(dataItem.AccountDate.Value);
                accountCollection.Remove(e.DataItem.CurrentAccount);
                accountCollection.Add(selectedAbsence, newAccount);
            }

            var parentPosition = FilteredPeopleHolder.PersonAccountModelCollection.IndexOf(dataItem);

            FilteredPeopleHolder.GetParentPersonAccountWhenUpdated(parentPosition);
	        PeopleWorksheet.StateHolder.GetChildPersonAccounts(parentPosition, FilteredPeopleHolder);
															


            FilteredPeopleHolder.PersonAccountModelCollection[parentPosition].ExpandState = true;
            Grid.Refresh();
            Invalidate();
            Cursor.Current = Cursors.Default;
        }

        void ParentColumn_CellChanged(object sender, ColumnCellChangedEventArgs<IPersonAccountModel> e)
        {
            e.DataItem.CanBold = true;

            PeopleAdminHelper.InvalidateGridRange(e.SaveCellInfoEventArgs.RowIndex, _gridColumns.Count, Grid);
            if (Grid != null)
            {
                Grid.InvalidateRange(Grid.ViewLayout.VisibleCellsRange);
            }
        }

        void ParentColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<IPersonAccountModel> e)
        {
            if (e.DataItem.CanBold)
                e.QueryCellInfoEventArg.Style.Font.Bold = true;
        }

        void AbsenceTypeColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<IPersonAccountModel> e)
        {
            if (e.DataItem.CanBold)
                e.QueryCellInfoEventArg.Style.Font.Bold = true;

            e.QueryCellInfoEventArg.Style.ResetDataSource();
            e.QueryCellInfoEventArg.Style.ClearCache();
            e.QueryCellInfoEventArg.Style.DataSource = FilteredPeopleHolder.FilteredAbsenceCollection;
        }

        internal override void ChildGridQueryColWidth(object sender, GridRowColSizeEventArgs e)
        {
            PeopleAdminHelper.CreateColWidthForChildGrid(e, ValidColumn(e.Index), _gridColumns.Count - 1,
                RenderingAddValue, Grid.ColWidths[e.Index + 2]);
        }

        internal override void ChildGridQueryRowHeight(object sender, GridRowColSizeEventArgs e)
        {
            PeopleAdminHelper.CreateRowHeightForChildGrid(e);
        }

        internal override void ChildGridQueryColCount(object sender, GridRowColCountEventArgs e)
        {
            PeopleAdminHelper.CreateColumnCountForChildGrid(e, _childGridColumns.Count - 1);
        }

        internal override void ChildGridQueryRowCount(object sender, GridRowColCountEventArgs e)
        {
            var personAccountChildGridViewCollection = ((GridControl)sender).Tag as ReadOnlyCollection<IPersonAccountChildModel>;

            if (personAccountChildGridViewCollection != null)
            {
                PeopleAdminHelper.CreateRowCountForChildGrid(e, personAccountChildGridViewCollection);
            }
        }

        internal override void ChildGridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            var gridControl = ((GridControl)sender);

            var personAccountChildCollection = gridControl.Tag as ReadOnlyCollection<IPersonAccountChildModel>;
            PeopleWorksheet.StateHolder.CurrentChildName = gridControl.Text;

            if (personAccountChildCollection != null)
            {
                if (ValidCell(e.ColIndex, e.RowIndex, gridControl.RowCount))
                {
                    _childGridColumns[e.ColIndex].GetCellInfo(e, personAccountChildCollection);
                }
            }
            base.ChildGridQueryCellInfo(sender, e);
        }

        internal override void ChildGridQuerySaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            var grid = (GridControl) sender;
            var personAccountChildCollection = grid.Tag as ReadOnlyCollection<IPersonAccountChildModel>;

            if (personAccountChildCollection != null)
            {
                if (ValidCell(e.ColIndex, e.RowIndex, grid.RowCount))
                {
                    _childGridColumns[e.ColIndex].SaveCellInfo(e, personAccountChildCollection);
                }
            }
        }

        internal override void ChildGridClipboardCanCopy(object sender, GridCutPasteEventArgs e)
        {
            _selectedPersonAccountCollection.Clear();
            CanCopyRow = false;

            CanCopyChildRow = false;
            var gridModel = sender as GridModel;

            if (gridModel == null) return;

            var grid = gridModel.ActiveGridView as GridControl;
            if (grid == null) return;

            // child copy processings
            var personPeriodChildCollection = grid.Tag as ReadOnlyCollection<IPersonAccountChildModel>;
            if (personPeriodChildCollection == null) return;

            CanCopyChildRow = true;
            var gridRangeInfoList = grid.Model.SelectedRanges;

            for (var index = gridRangeInfoList.Count; index > 0; index--)
            {
                var gridRangeInfo = gridRangeInfoList[index - 1];

                if (gridRangeInfo.Height == 1)
                {
                    var account = personPeriodChildCollection[gridRangeInfo.Top - 1].ContainedEntity;

                    if (!_selectedPersonAccountCollection.Contains(account))
                    {
                        _selectedPersonAccountCollection.Add(account);
                    }
                }
                else
                {
                    for (var row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
                    {
                        var account = personPeriodChildCollection[row - 1].ContainedEntity;

                        if (!_selectedPersonAccountCollection.Contains(account))
                        {
                            _selectedPersonAccountCollection.Add(account);
                        }
                    }
                }
            }
        }

        internal override void ChildGridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            var grid = sender as GridControl;

            if (grid != null)
            {
                var personAccountChildCollection = grid.Tag as ReadOnlyCollection<IPersonAccountChildModel>;

                if (personAccountChildCollection != null)
                {
                    var rangeLength = grid.Model.SelectedRanges.Count;

                    if (rangeLength != 0)
                    {
                        IList<IAccount> list = new List<IAccount>();
                        IList<IPersonAccountChildModel> gridDataList =
                            new List<IPersonAccountChildModel>();

                        for (var rangeIndex = 0; rangeIndex < rangeLength; rangeIndex++)
                        {
                            var rangeInfo = grid.Model.SelectedRanges[rangeIndex];

                            var top = 1; // This is to skip if Range is Empty.
                            var length = 0;

                            // TODO: Need to refactor this /kosalanp.

                            #region Prepare range information

                            switch (rangeInfo.RangeType)
                            {
                                case GridRangeInfoType.Cols:
                                case GridRangeInfoType.Table:
                                    top = 1;
                                    length = FilteredPeopleHolder.FilteredPersonCollection.Count;
                                    RowCount += length;
                                    break;
                                case GridRangeInfoType.Rows:
                                case GridRangeInfoType.Cells:
                                    top = rangeInfo.Top;
                                    length = top + rangeInfo.Height - 1;
                                    RowCount += rangeInfo.Height;
                                    break;
                                default:
                                    break;
                            }

                            #endregion

                            if (top > 0)
                            {
                                for (int index = top - 1; index < length; index++)
                                {
                                    if (personAccountChildCollection.Count < length)
                                        return;

                                    // Child Adding goes here
                                    list.Add(personAccountChildCollection[index].ContainedEntity);
                                    gridDataList.Add(personAccountChildCollection[index]);
                                }
                            }
                        }

                        _currentParentRowIndex = Grid.CurrentCell.RowIndex;
                        _currentChildGrid = grid;
                        CurrentGrid = grid;
                    }
                }
            }
        }

        void ChildTrackerDescriptionChanged(object sender, SelectedItemChangeEventArgs<IPersonAccountChildModel, IAbsence> e)
        {
            Cursor.Current = Cursors.WaitCursor;

            // Gets the relevant data item
            var dataItem = e.DataItem;
            var selectedAbsence = e.SelectedItem;
            var accountCollection = dataItem.Parent;

            if (dataItem.AccountDate != null)
            {
                var newAccount = selectedAbsence.Tracker.CreatePersonAccount(dataItem.AccountDate.Value);

                accountCollection.Remove(e.DataItem.ContainedEntity);
                accountCollection.Add(selectedAbsence, newAccount);
            }

            RefreshChildGridAfterAbsenceChanged();
            Cursor.Current = Cursors.Default;
        }

        void ChildColumn_CellChanged(object sender, ColumnCellChangedEventArgs<IPersonAccountChildModel> e)
        {
            e.DataItem.CanBold = true;

            var grid = FilteredPeopleHolder.PersonAccountModelCollection[Grid.CurrentCell.RowIndex - 1].GridControl;

            if (grid != null)
            {
                grid.InvalidateRange(grid.ViewLayout.VisibleCellsRange);
            }
        }

        void ChildColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<IPersonAccountChildModel> e)
        {
            if (e.DataItem.CanBold)
                e.QueryCellInfoEventArg.Style.Font.Bold = true;
        }

        void ChildTrackerTypeColumn_CellDisplayChanged(object sender,
            ColumnCellDisplayChangedEventArgs<IPersonAccountChildModel> e)
        {
            if (e.DataItem.CanBold)
                e.QueryCellInfoEventArg.Style.Font.Bold = true;

            e.QueryCellInfoEventArg.Style.ResetDataSource();
            e.QueryCellInfoEventArg.Style.ClearCache();
            e.QueryCellInfoEventArg.Style.DataSource = FilteredPeopleHolder.FilteredAbsenceCollection;
        }


        public PersonalAccountGridView(GridControl grid, FilteredPeopleHolder filteredPeopleHolder)
            : base(grid, filteredPeopleHolder)
        {
            Init();
        }

        private void Init()
        {
            Grid.CellModels.Add("GridInCell", new GridInCellModel(Grid.Model));
			if (!Grid.CellModels.ContainsKey("NumericCell"))
				Grid.CellModels.Add("NumericCell", new NumericCellModel(Grid.Model));
            Grid.CellModels.Add("TimeSpanLongHourMinutesCell", new TimeSpanDurationCellModel(Grid.Model));

            var cellModel = new GridDropDownMonthCalendarAdvCellModel(Grid.Model);
            cellModel.HideNoneButton();
            cellModel.HideTodayButton();
			Grid.CellModels.Add(GridCellModelConstants.CellTypeDatePickerCell, cellModel);
        }

        private void CreateParentGridHeaders()
        {
            _gridColumns.Add(new RowHeaderColumn<IPersonAccountModel>());

            _pushButtonColumn = new PushButtonColumn<IPersonAccountModel>(Resources.FullName, "PersonAccountCount");
            _gridColumns.Add(_pushButtonColumn);

            _gridInCellColumn = new GridInCellColumn<IPersonAccountModel>("GridControl");
            _gridColumns.Add(_gridInCellColumn);

            _fullNameColumn = new ReadOnlyTextColumn<IPersonAccountModel>("FullName", Resources.FullName);
            _fullNameColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
            _gridColumns.Add(_fullNameColumn);

            _accountDateColumn = new EditableDateOnlyColumnForPeriodGrids<IPersonAccountModel>("AccountDate",
                                                                                        Resources.Date)
                    {
                        ColumnComparer = new PersonAccountDateComparer()
                    };
            _accountDateColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
            _accountDateColumn.CellChanged += ParentColumn_CellChanged;
            _gridColumns.Add(_accountDateColumn);

            _absenceTypeColumn =
                new TextOrDropDownColumn<IPersonAccountModel, IAbsence>("TrackingAbsence",
                Resources.Absence, FilteredPeopleHolder.FilteredAbsenceCollection,
                new ParentTrackerDescriptionTextOrDropDownColumnComparer(), "Name");
            ((TextOrDropDownColumn<IPersonAccountModel, IAbsence>)_absenceTypeColumn).SelectedItemChanged +=
                ParentTrackerDescription_Changed;
            _absenceTypeColumn.ColumnComparer = new PersonAccountDescriptionComparer();
            _absenceTypeColumn.CellChanged += ParentColumn_CellChanged;
            _absenceTypeColumn.CellDisplayChanged += AbsenceTypeColumn_CellDisplayChanged;
            _gridColumns.Add(_absenceTypeColumn);

            _absenceUnitColumn = new ReadOnlyTextColumnPeriod<IPersonAccountModel>("AccountType", Resources.TrackerType, false)
                    {
                        ColumnComparer = new PersonAccountTypeComparer()
                    };
            _absenceTypeColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
            _gridColumns.Add(_absenceUnitColumn);

            _balanceInColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountModel>(
                "BalanceIn", Resources.BalanceIn, false, new PersonAccountParentViewAccruedColumnDisableCondition());
            _balanceInColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
            _balanceInColumn.CellChanged += ParentColumn_CellChanged;
            _balanceInColumn.ColumnComparer = new PersonAccountIntegerTimeSpanColumnComparer("BalanceIn");
            _gridColumns.Add(_balanceInColumn);

            _accruedTimeColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountModel>(
                "Accrued", Resources.Accrued, false, new PersonAccountParentViewAccruedColumnDisableCondition());
            _accruedTimeColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
            _accruedTimeColumn.CellChanged += ParentColumn_CellChanged;
            _accruedTimeColumn.ColumnComparer = new PersonAccountIntegerTimeSpanColumnComparer("Accrued");
            _gridColumns.Add(_accruedTimeColumn);

            _extraTimeAllowedColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountModel>(
                "Extra", Resources.Extra, false, null);
            _extraTimeAllowedColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
            _extraTimeAllowedColumn.CellChanged += ParentColumn_CellChanged;
            _extraTimeAllowedColumn.ColumnComparer = new PersonAccountIntegerTimeSpanColumnComparer("Extra");
            _gridColumns.Add(_extraTimeAllowedColumn);

            _balanceOutTimeColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountModel>
                ("BalanceOut", Resources.BalanceOut, true, null);
            _balanceOutTimeColumn.ColumnComparer = new PersonAccountIntegerTimeSpanColumnComparer("BalanceOut");
            _balanceOutTimeColumn.CellChanged += ParentColumn_CellChanged;
            _balanceOutTimeColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
            _gridColumns.Add(_balanceOutTimeColumn);

            _usedTimeColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountModel>
                ("Used", Resources.Used, true, null);
            _usedTimeColumn.ColumnComparer = new PersonAccountIntegerTimeSpanColumnComparer("Used");
            _usedTimeColumn.CellChanged += ParentColumn_CellChanged;
            _usedTimeColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
            _gridColumns.Add(_usedTimeColumn);

            _remainingTimeColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountModel>(
                "Remaining", Resources.Remaining, true, new PersonAccountParentViewAccruedColumnDisableCondition());
            _remainingTimeColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
            _remainingTimeColumn.CellChanged += ParentColumn_CellChanged;
            _remainingTimeColumn.ColumnComparer = new PersonAccountIntegerTimeSpanColumnComparer("Remaining");
            _gridColumns.Add(_remainingTimeColumn);

        }

        private void CreateChildGridHeader()
        {
            _childRowHeaderColumn = new RowHeaderColumn<IPersonAccountChildModel>();
            _childGridColumns.Add(_childRowHeaderColumn);

            _childFullNameColumn = new LineColumn<IPersonAccountChildModel>("FullName");
            _childGridColumns.Add(_childFullNameColumn);

            _childAccountDateColumn =
                new EditableDateOnlyColumnForPeriodGrids<IPersonAccountChildModel>("AccountDate", Resources.Date);
            _childAccountDateColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
            _childAccountDateColumn.CellChanged += ChildColumn_CellChanged;
            _childGridColumns.Add(_childAccountDateColumn);

            _childTrackerTypeColumn = new TextOrDropDownColumn<IPersonAccountChildModel, IAbsence>("TrackingAbsence",
                Resources.Description, FilteredPeopleHolder.FilteredAbsenceCollection,
                new ChildTrackerDescriptionTextOrDropDownColumnComparer(), "Name");
            ((TextOrDropDownColumn<IPersonAccountChildModel, IAbsence>)_childTrackerTypeColumn).SelectedItemChanged +=
                ChildTrackerDescriptionChanged;
            _childTrackerTypeColumn.CellDisplayChanged += ChildTrackerTypeColumn_CellDisplayChanged;
            _childTrackerTypeColumn.CellChanged += ChildColumn_CellChanged;
            _childGridColumns.Add(_childTrackerTypeColumn);

            _childTrackerUnitColumn = new ReadOnlyTextColumnPeriod<IPersonAccountChildModel>
                ("AccountType", Resources.Description, false);
            _childTrackerUnitColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
            _childGridColumns.Add(_childTrackerUnitColumn);

            _childBalanceInColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountChildModel>(
                "BalanceIn", Resources.BalanceIn, false, null);
            _childBalanceInColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
            _childBalanceInColumn.CellChanged += ChildColumn_CellChanged;
            _childGridColumns.Add(_childBalanceInColumn);

            _childAccruedTimeColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountChildModel>(
                "Accrued", Resources.Accrued, false, new PersonAccountChildViewAccruedColumnDisableCondition());
            _childAccruedTimeColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
            _childAccruedTimeColumn.CellChanged += ChildColumn_CellChanged;
            _childGridColumns.Add(_childAccruedTimeColumn);

            _childExtraTimeAllowedColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountChildModel>(
                "Extra", Resources.Extra, false, null);
            _childExtraTimeAllowedColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
            _childExtraTimeAllowedColumn.CellChanged += ChildColumn_CellChanged;
            _childGridColumns.Add(_childExtraTimeAllowedColumn);

            _childBalanceOutTimeColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountChildModel>(
                "BalanceOut", Resources.BalanceOut, true, null);
            _childBalanceOutTimeColumn.CellChanged += ChildColumn_CellChanged;
            _childBalanceOutTimeColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
            _childGridColumns.Add(_childBalanceOutTimeColumn);

            _childUsedTimeColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountChildModel>(
                "Used", Resources.Used, true, null);
            _childUsedTimeColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
            _childUsedTimeColumn.CellChanged += ChildColumn_CellChanged;
            _childGridColumns.Add(_childUsedTimeColumn);

            _childRemainingTimeColumn = new LongHourMinutesOrIntegerColumn<IPersonAccountChildModel>(
                "Remaining", Resources.Remaining, true, null);
            _childRemainingTimeColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
            _childRemainingTimeColumn.CellChanged += ChildColumn_CellChanged;
            _childGridColumns.Add(_childRemainingTimeColumn);
        }

        private void gridCreatorDispose(GridControl abstractGrid, GridRangeInfo gridRange)
        {
            Grid.CoveredRanges.Remove(gridRange);

            if (abstractGrid != null)
            {
                Grid.Controls.Remove(abstractGrid);
            }
        }

        private void LoadAbstractGrid(int rowIndex, int columnIndex, GridRangeInfo gridInfo)
        {
            BindDropDownGridEvents();

            GridCreator.GetGrid(Grid, gridInfo, rowIndex, columnIndex,
                    PeopleWorksheet.StateHolder.PersonAccountChildGridData, _childGridColumns.Count - 1,
                    PeopleWorksheet.StateHolder.CurrentChildName);
        }

        private void AddPersonAccount()
        {
            if (Grid.Model.CurrentCellInfo == null)
            {
                //TODO:Need to implement when person is not selected scenario 
                return;
            }

            InsertPersonAccount();
        }

        private void InsertPersonAccount()
        {
            GridRangeInfoList gridRangeInfoList = Grid.Model.SelectedRanges;

            for (int index = 0; index < gridRangeInfoList.Count; index++)
            {
                GridRangeInfo gridRangeInfo = gridRangeInfoList[index];

				if (gridRangeInfo.IsTable || gridRangeInfo.IsCols)
                {
                    // This scenario is used for when user is selecting entire grid using button give top in 
                    // that grid.
                    for (int i = 1; i <= Grid.RowCount; i++)
                        AddPersonAccount(i);
                }
                else
                {
                    if (gridRangeInfo.Height == 1)
                    {
                        AddPersonAccount(gridRangeInfo.Top);
                    }
                    else
                    {
                        for (int row = gridRangeInfo.Top; row <= gridRangeInfo.Bottom; row++)
                        {
                            AddPersonAccount(row);
                        }
                    }
                }
            }
        }

        private void GetChildPersonAccounts(int rowIndex, GridControl grid)
        {
            if (grid != null)
            {
                var cachedCollection = grid.Tag as ReadOnlyCollection<IPersonAccountChildModel>;

                PeopleWorksheet.StateHolder.GetChildPersonAccounts(rowIndex - 1, cachedCollection, FilteredPeopleHolder);
            }
            else
            {
				PeopleWorksheet.StateHolder.GetChildPersonAccounts(rowIndex - 1, FilteredPeopleHolder);
            }
        }

		private void AddPersonAccount(int rowIndex)
		{
			if (rowIndex == 0)
				return;

			var actualItemIndex = rowIndex - 1;
			FilteredPeopleHolder.AddPersonAccount(actualItemIndex);

			RefreshChildGrid(rowIndex);
		}

		private void RefreshChildGrid(int rowIndex)
		{
			if (rowIndex == 0)
				return;

			// Set grid range for covered ranged
			var gridInfo = GridRangeInfo.Cells(rowIndex, GridInCellColumnIndex, rowIndex, ParentGridLastColumnIndex);
			// Set out of focus form current cell.This helps  to fire save cell info in child grid.
			SetOutOfFocusFromCurrentCell();

			var actualItemIndex = rowIndex - 1;

			if (FilteredPeopleHolder.PersonAccountModelCollection[actualItemIndex].ExpandState)
			{
				ExpandChildGrid(rowIndex, gridInfo);
			}
			else
			{
				FilteredPeopleHolder.GetParentPersonAccountWhenUpdated(actualItemIndex);
				Grid.Invalidate();
			}
		}

		private void ToggleChildGrid(int rowIndex)
		{
			if (rowIndex == 0)
				return;

			// Set grid range for covered ranged
			var gridInfo = GridRangeInfo.Cells(rowIndex, GridInCellColumnIndex, rowIndex, ParentGridLastColumnIndex);
			// Set out of focus form current cell.This helps  to fire save cell info in child grid.
			SetOutOfFocusFromCurrentCell();

			var actualItemIndex = rowIndex - 1;

			if (!FilteredPeopleHolder.PersonAccountModelCollection[actualItemIndex].ExpandState)
			{
				FilteredPeopleHolder.PersonAccountModelCollection[actualItemIndex].ExpandState = true;
				ExpandChildGrid(rowIndex, gridInfo);
			}
			else
			{
				FilteredPeopleHolder.PersonAccountModelCollection[actualItemIndex].ExpandState = false;
				CollapseChildGrid(rowIndex, gridInfo);
			}
		}

		private void CollapseChildGrid(int rowIndex, GridRangeInfo gridInfo)
		{
			// Get child grid and dispose it
			var gridControl = Grid[rowIndex, GridInCellColumnIndex].Control as GridControl;
			gridCreatorDispose(gridControl, gridInfo);

			Grid.RowHeights[rowIndex] = DefaultRowHeight;
			FilteredPeopleHolder.GetParentPersonAccountWhenUpdated(rowIndex - 1);
			Grid.InvalidateRange(gridInfo);
		}

		private void ExpandChildGrid(int rowIndex, GridRangeInfo gridInfo)
		{
			var grid = FilteredPeopleHolder.PersonAccountModelCollection[rowIndex - 1].GridControl;

			GetChildPersonAccounts(rowIndex, grid);

			LoadAbstractGrid(rowIndex, GridInCellColumnIndex, gridInfo);
			Grid.CurrentCell.MoveTo(rowIndex, GridInCellColumnIndex);
		}

        private void RemoveChild(int rowIndex, bool isDeleteAll)
        {
            var childGrid = Grid[rowIndex, GridInCellColumnIndex].Control as CellEmbeddedGrid;

            if (childGrid != null)
            {
                if (isDeleteAll)
                {
                    PersonAccountChildDelete(rowIndex, childGrid);
                }
                else
                {
                    var gridRangeInfoList = childGrid.Model.SelectedRanges.GetRowRanges(GridRangeInfoType.Cells | GridRangeInfoType.Rows);

                    for (int index = gridRangeInfoList.Count; index > 0; index--)
                    {
                        var gridRangeInfo = gridRangeInfoList[index - 1];
                        var top = gridRangeInfo.Top;
                        var bottom = gridRangeInfo.Bottom;
                        if (top == 0) continue;
                        if (gridRangeInfo.Height == 1)
                        {
                            PersonAccountChildDelete(rowIndex, top - 1, childGrid);
                        }
                        else
                        {
                            for (int row = bottom; row >= top; row--)
                            {
                                PersonAccountChildDelete(rowIndex, row - 1, childGrid);
                            }
                        }
                    }
                }
            }
        }

        private void PersonAccountChildDelete(int rowIndex, CellEmbeddedGrid childGrid)
        {
            int actualItemIndex = rowIndex - 1;

            FilteredPeopleHolder.DeleteAllPersonAccounts(actualItemIndex);
            var personAccountChildCollection = childGrid.Tag as ReadOnlyCollection<IPersonAccountChildModel>;

            if (personAccountChildCollection != null)
            {
                IList<IPersonAccountChildModel> accountCollection =
                    new List<IPersonAccountChildModel>(personAccountChildCollection);
                accountCollection.Clear();

                GridRangeInfo gridInfo =
                    GridRangeInfo.Cells(rowIndex, GridInCellColumnIndex - 1, rowIndex, ParentGridLastColumnIndex);

                childGrid.Tag = new ReadOnlyCollection<IPersonAccountChildModel>(accountCollection);
                childGrid.RowCount = accountCollection.Count;
                childGrid.Invalidate();

                // remove child grid
                FilteredPeopleHolder.PersonAccountModelCollection[actualItemIndex].ExpandState = false;
                FilteredPeopleHolder.PersonAccountModelCollection[actualItemIndex].GridControl = null;

                //// Get child grid and dispose it
                var gridControl = Grid[rowIndex, GridInCellColumnIndex].Control as GridControl;

                gridCreatorDispose(gridControl, gridInfo);
                Grid.RowHeights[rowIndex] = Grid.DefaultRowHeight;

                FilteredPeopleHolder.GetParentPersonAccountWhenUpdated(actualItemIndex);
                Grid.InvalidateRange(gridInfo);
            }
        }

        private void PersonAccountChildDelete(int rowIndex, int childPersonAccountIndex, CellEmbeddedGrid childGrid)
        {
            var actualItemIndex = rowIndex - 1;
            FilteredPeopleHolder.DeletePersonAccount(actualItemIndex, childPersonAccountIndex);

            var personAccountChildCollection = childGrid.Tag as ReadOnlyCollection<IPersonAccountChildModel>;

            if (personAccountChildCollection != null)
            {
                IList<IPersonAccountChildModel> accountCollection =
                    new List<IPersonAccountChildModel>(personAccountChildCollection);
                accountCollection.RemoveAt(childPersonAccountIndex);

                GridRangeInfo gridInfo =
                    GridRangeInfo.Cells(rowIndex, GridInCellColumnIndex - 1, rowIndex, ParentGridLastColumnIndex);

                childGrid.Tag = new ReadOnlyCollection<IPersonAccountChildModel>(accountCollection);
                childGrid.RowCount = accountCollection.Count;
                childGrid.Invalidate();

                if (childGrid.RowCount == 0)
                    Grid.RowHeights[rowIndex] = DefaultRowHeight + RenderingAddValue;
                else
                    Grid.RowHeights[rowIndex] = childGrid.RowCount * DefaultRowHeight + RenderingAddValue;

                if (accountCollection.Count == 0)
                {
                    // remove child grid
                    FilteredPeopleHolder.PersonAccountModelCollection[actualItemIndex].ExpandState = false;
                    FilteredPeopleHolder.PersonAccountModelCollection[actualItemIndex].GridControl = null;

                    // Get child grid and dispose it
                    var gridControl = Grid[rowIndex, GridInCellColumnIndex].Control as GridControl;
                    RemoveChildGrid(gridControl, gridInfo);
                    Grid.RowHeights[rowIndex] = Grid.DefaultRowHeight;

                    FilteredPeopleHolder.GetParentPersonAccountWhenUpdated(actualItemIndex);
                    Grid.InvalidateRange(gridInfo);
                }
            }
        }

        private void RemoveChildGrid(GridControl childGrid, GridRangeInfo gridRange)
        {
            // Remove covered range and remove grid form control collection
            Grid.CoveredRanges.Remove(gridRange);
            Grid.Controls.Remove(childGrid);
        }

        private void CopyAllPersonAccounts(int rowIndex)
        {
            if (rowIndex < 0) return;

            //Current period of receiver replaced with current period of the sender ??
            var accounts = FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].Parent;
            bool isParent = !FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].ExpandState;

            if (accounts != null)
            {
                if (CanCopyRow)
                {
                    foreach (var personAccount in _selectedPersonAccountCollection)
                    {
                        if (!accounts.AllPersonAccounts().Contains(personAccount))
                        {
                            // Add person periods to person 
                            ValidateAndAddPersonAccount(accounts, personAccount);
                        }
                    }
                }

                if (CanCopyChildRow)
                {
                    foreach (IAccount personAccount in _selectedPersonAccountCollection)
                    {
                        ValidateAndAddPersonAccount(accounts, personAccount);
                    }
                }

                if (isParent)
                {
                    RefreshParentGridRange(rowIndex);
                }
                else
                {
                    RefreshChildGridRange(rowIndex);
                }
            }
        }

        private void ValidateAndAddPersonAccount(IPersonAccountCollection personPaste, IAccount account)
        {
            var accountClone = account.Clone() as IAccount;

            if (accountClone == null) return;

            accountClone.StartDate = PeriodDateService.GetValidPeriodDate(
					PeriodDateDictionaryBuilder.GetDateOnlyDictionary(FilteredPeopleHolder, ViewType.PersonalAccountGridView, personPaste.Person),
                    account.StartDate);

            var personAbsenceAccount =
                personPaste.PersonAbsenceAccounts().FirstOrDefault(p => p.Absence.Equals(account.Owner.Absence));
            if (personAbsenceAccount==null)
            {
                personAbsenceAccount = new PersonAbsenceAccount(personPaste.Person, account.Owner.Absence);
                personPaste.Add(personAbsenceAccount);
            }

            personAbsenceAccount.Add(accountClone);
        }

        private void RefreshParentGridRange(int index)
        {
            var actualIndex = index + 1;

            FilteredPeopleHolder.GetParentPersonAccountWhenUpdated(index);
            Grid.InvalidateRange(
                GridRangeInfo.Cells(actualIndex, GridInCellColumnIndex - 1, actualIndex, ParentGridLastColumnIndex));
        }

        private void RefreshChildGridRange(int index)
        {
            var actualIndex = index + 1;

	        PeopleWorksheet.StateHolder.GetChildPersonAccounts(index, FilteredPeopleHolder);

            var childGrid = Grid[actualIndex, GridInCellColumnIndex].Control as CellEmbeddedGrid;

            if (childGrid != null)
            {
                childGrid.Tag = PeopleWorksheet.StateHolder.PersonAccountChildGridData;
                // Merging name column's all cells
                childGrid.Model.CoveredRanges.Add(GridRangeInfo.Cells(1, 1, childGrid.RowCount + 2, 1));

                Grid.RowHeights[actualIndex] = PeopleWorksheet.StateHolder.PersonAccountChildGridData.Count *
                                               DefaultHeight + 2;
                childGrid.RowCount = PeopleWorksheet.StateHolder.PersonAccountChildGridData.Count;
                childGrid.Invalidate();
            }
        }

        private void DeleteWhenRangeSelected(GridRangeInfo gridRangeInfo)
        {
            var actualGridInColumnCellIndex = GridInCellColumnIndex - 1;

            if (gridRangeInfo.Height == 1)
            {
                var actualItemIndex = gridRangeInfo.Top - 1;

                // Child list remove
                if (FilteredPeopleHolder.PersonAccountModelCollection[actualItemIndex].ExpandState)
                {
                    RemoveChild(gridRangeInfo.Top, gridRangeInfo.IsRows);
                }
                else
                {
                    FilteredPeopleHolder.DeletePersonAccount(actualItemIndex);
                    FilteredPeopleHolder.GetParentPersonAccountWhenUpdated(actualItemIndex);
                    Grid.InvalidateRange(GridRangeInfo.Cells(gridRangeInfo.Top,
                                                             actualGridInColumnCellIndex, gridRangeInfo.Top,
                                                             ParentGridLastColumnIndex));
                }
            }
            else
            {
                for (var row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
                {
                    if (row != 0)
                    {
                        FilteredPeopleHolder.DeletePersonAccount(row - 1);

                        FilteredPeopleHolder.GetParentPersonAccountWhenUpdated(row - 1);
                        Grid.InvalidateRange(
                            GridRangeInfo.Cells(row, 1, row, ParentGridLastColumnIndex));
                    }
                }
            }
        }

        private bool IsValidRow()
        {
            return Grid.CurrentCell.RowIndex > 0;
        }

        private IPersonAccountModel CurrentPersonAccountView
        {
            get { return FilteredPeopleHolder.PersonAccountModelCollection[CurrentRowIndex]; }
        }

        private bool IsCurrentRowExpanded()
        {
            return IsValidRow()
                && CurrentPersonAccountView.ExpandState;
        }

        private int CurrentRowIndex
        {
            get { return Grid.CurrentCell.RowIndex - 1; }
        }

        private int GetColumnIndex()
        {
            int columnIndex;
            if (IsCurrentRowExpanded())
            {
                columnIndex = CurrentPersonAccountView.GridControl.CurrentCell.ColIndex + 2;
            }
            else
            {
                var parentColIndex = gridColumnIndex();
                columnIndex = (parentColIndex == 1) ? (3) : parentColIndex; // TODO: Use constants instead.
            }
            return columnIndex;
        }

		private int gridColumnIndex()
		{
			if (Grid.CurrentCell.ColIndex == -1)
				Grid.CurrentCell.MoveTo(0, 0);
			return Grid.CurrentCell.ColIndex;
		}

        private void DeletePersonAccounts()
        {
            if (Grid.Model.SelectedRanges.Count > 0)
            {
                var gridRangeInfoList = Grid.Model.SelectedRanges;
                for (var index = gridRangeInfoList.Count; index > 0; index--)
                {
                    var gridRangeInfo = gridRangeInfoList[index - 1];

					if (gridRangeInfo.IsTable || gridRangeInfo.IsCols)
                    {
                        DeleteWhenAllSelected();
                    }
                    else
                    {
                        DeleteWhenRangeSelected(gridRangeInfo);
                    }
                }
            }
        }

        private void DeleteWhenAllSelected()
        {
            // This scenario is used for when user is selecting entire grid using button give top in 
            // that grid.
            for (var i = 1; i <= Grid.RowCount; i++)
            {
                if (FilteredPeopleHolder.PersonPeriodGridViewCollection[i - 1].ExpandState)
                {
                    RemoveChild(i, true);
                }
                else
                {
                    FilteredPeopleHolder.DeletePersonAccount(i - 1);
                    FilteredPeopleHolder.GetParentPersonAccountWhenUpdated(i - 1);
                    Grid.InvalidateRange(GridRangeInfo.Cells(i, 1, i,
                                                             ParentGridLastColumnIndex));
                }
            }
        }

        private IList<IPerson> GetSelectedPersonsInGrd()
        {
            IList<IPerson> selectedPersons = new List<IPerson>();
            var gridRangeInfoList = Grid.Model.SelectedRanges;
            for (int index = gridRangeInfoList.Count; index > 0; index--)
            {
                var gridRangeInfo = gridRangeInfoList[index - 1];

                if (gridRangeInfo.Height == 1)
                {
                    selectedPersons.Add(FilteredPeopleHolder.PersonAccountModelCollection
                        [gridRangeInfo.Top - 1].Parent.Person);
                }
                else
                {
                    for (int row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
                    {
                        selectedPersons.Add(FilteredPeopleHolder.PersonAccountModelCollection
                            [row - 1].Parent.Person);
                    }
                }
            }
            return selectedPersons;
        }

        private void RefreshChildGridAfterAbsenceChanged()
        {
            if (_currentParentRowIndex > 0)
            {
                var rowIndex = _currentParentRowIndex;
                var actualItemIndex = rowIndex - 1;

                var childGrid = _currentChildGrid;

                if (childGrid != null)
                {
                    var personAccountChildCollection = childGrid.Tag as ReadOnlyCollection<IPersonAccountChildModel>;

                    if (personAccountChildCollection != null)
                    {
                        FilteredPeopleHolder.PersonAccountModelCollection[actualItemIndex].ExpandState = true;
                        GetChildPersonAccounts(rowIndex, childGrid);
                        personAccountChildCollection =
                            PeopleWorksheet.StateHolder.PersonAccountChildGridDataWhenAndChild;

                        if (personAccountChildCollection != null)
                        {
                            childGrid.Tag = personAccountChildCollection;
                            childGrid.RowCount = personAccountChildCollection.Count;
                            childGrid.Invalidate();

                            Grid.CurrentCell.MoveTo(rowIndex, GridInCellColumnIndex);
                        }
                    }
                }
            }
        }

        public override void Sort(bool isAscending)
        {
            // Gets the filtered people grid data as a collection
            var personAccountCollection = new List<IPersonAccountModel>(FilteredPeopleHolder.PersonAccountModelCollection);

            var columnIndex = GetColumnIndex();

            // Gets the sort column to sort
            var sortColumn = _gridColumns[columnIndex].BindingProperty;
            // Gets the coparer erquired to sort the data
            var comparer = _gridColumns[columnIndex].ColumnComparer;

            Grid.CurrentCell.MoveLeft();

            // Dispose the child grids
            DisposeChildGrids();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                // Holds the results of the sorting process
                IList<IPersonAccountModel> result;
                if (comparer != null)
                {
                    // Sorts the person collection in ascending order
                    personAccountCollection.Sort(comparer);
                    if (!isAscending)
                        personAccountCollection.Reverse();

                    result = personAccountCollection;
                }
                else
                {
                    // Gets the sorted people collection
                    result = GridHelper.Sort(
                        new Collection<IPersonAccountModel>(personAccountCollection),
                        sortColumn,
                        isAscending
                        );
                }

                // Sets the filtered list
                FilteredPeopleHolder.SetSortedPersonAccountFilteredList(result);

                Grid.CurrentCell.MoveRight();

                Invalidate();
            }
        }

        internal override void CreateHeaders()
        {
            CreateParentGridHeaders();
            CreateChildGridHeader();

            // Hide column which is used as a container for grid in cell implementation 
            var pushButtonCol = _gridColumns.IndexOf(_pushButtonColumn);
            Grid.Cols.Hidden[pushButtonCol + 1] = true;
        }

        internal override void PrepareView()
        {
            ColCount = _gridColumns.Count;
            RowCount = FilteredPeopleHolder.PersonAccountModelCollection.Count;

            Grid.RowCount = RowCount;
            Grid.ColCount = ColCount - 1;
            Grid.Model.Data.RowCount = RowCount;

            Grid.Cols.HeaderCount = 0;
            Grid.Rows.HeaderCount = 0;
            Grid.Name = "PersonalAccountView";

            int length = _gridColumns.Count;

            for (int index = 0; index < length; index++)
            {
                Grid.ColWidths[index] = _gridColumns[index].PreferredWidth + DefaultColumnWidthAddValue;
            }
            Grid.ColWidths[0] = _gridColumns[0].PreferredWidth;
        }

        internal override void MergeHeaders()
        {
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 1, 0, 3));
        }

        public override void Invalidate()
        {
            Grid.Invalidate();
        }

        internal override void CreateContextMenu()
        {
            Grid.ContextMenuStrip = new ContextMenuStrip();

            _addNewPersonPeriodMenuItem = new ToolStripMenuItem(Resources.New);
            _addNewPersonPeriodMenuItem.Click += AddNewGridRow;
            Grid.ContextMenuStrip.Items.Add(_addNewPersonPeriodMenuItem);

            _deletePersonPeriodMenuItem = new ToolStripMenuItem(Resources.Delete);
            _deletePersonPeriodMenuItem.Click += DeleteSelectedGridRows;
            Grid.ContextMenuStrip.Items.Add(_deletePersonPeriodMenuItem);

            _copySpecialPersonPeriodMenuItem = new ToolStripMenuItem(Resources.CopySpecial);
            _copySpecialPersonPeriodMenuItem.Click += CopySpecial;
            Grid.ContextMenuStrip.Items.Add(_copySpecialPersonPeriodMenuItem);

            _pasteSpecialPersonPeriodMenuItem = new ToolStripMenuItem(Resources.PasteNew);
            _pasteSpecialPersonPeriodMenuItem.Click += PasteSpecial;
            Grid.ContextMenuStrip.Items.Add(_pasteSpecialPersonPeriodMenuItem);
        }

        internal override void AddNewGridRow<T>(object sender, T eventArgs)
        {
            AddPersonAccount();
        }

        internal override void DeleteSelectedGridRows<T>(object sender, T eventArgs)
        {
            DeletePersonAccounts();
        }

        internal void PasteSpecial<T>(object sender, T eventArgs)
        {
            var gridRangeInfoList = Grid.Model.Selections.Ranges;

            if (!CanCopyRow && !CanCopyChildRow) return;

            for (var index = gridRangeInfoList.Count; index > 0; index--)
            {
                var gridRangeInfo = gridRangeInfoList[index - 1];

                if (gridRangeInfo.IsTable)
                {
                    for (var i = 1; i <= Grid.RowCount; i++)
                        CopyAllPersonAccounts(i - 1);
                }
                else
                {
                    if (gridRangeInfo.Height == 1)
                    {
                        CopyAllPersonAccounts(gridRangeInfo.Top - 1);
                    }
                    else
                    {
                        for (var row = gridRangeInfo.Top; row <= gridRangeInfo.Bottom; row++)
                        {
                            CopyAllPersonAccounts(row - 1);
                        }
                    }
                }
            }
        }

        public override void Reinitialize()
        {
            Grid.Refresh();
        }

        internal void CopySpecial<T>(object sender, T eventArgs)
        {
            if (Grid.Model.CurrentCellInfo == null)
            {
                //TODO:Need to implement when person is not selected scenario 
                return;
            }

            var rowIndex = Grid.CurrentCell.RowIndex;

            if (rowIndex == 0) return;

            rowIndex -= 1;
            _selectedPersonAccountCollection.Clear();
            CanCopyRow = false;
            CanCopyChildRow = false;

            if (!FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].ExpandState)
            {
                CanCopyRow = true;
                var accounts =
                    FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].Parent;

                foreach (var account in accounts.AllPersonAccounts())
                {
                    // Parent processings
                    if (!_selectedPersonAccountCollection.Contains(account))
                        _selectedPersonAccountCollection.Add(account);
                }
            }
            else
            {
                CanCopyChildRow = true;

                // child copy processings
                var grid = Grid[Grid.CurrentCell.RowIndex, GridInCellColumnIndex].Control as GridControl;
                if (grid == null) return;

                var personAccountChildCollection = grid.Tag as ReadOnlyCollection<IPersonAccountChildModel>;
                if (personAccountChildCollection == null) return;

                var gridRangeInfoList = grid.Model.SelectedRanges;
                for (var index = gridRangeInfoList.Count; index > 0; index--)
                {
                    var gridRangeInfo = gridRangeInfoList[index - 1];

                    if (gridRangeInfo.Height == 1)
                    {
                        var account = personAccountChildCollection[gridRangeInfo.Top - 1].ContainedEntity;

                        if (!_selectedPersonAccountCollection.Contains(account))
                        {
                            _selectedPersonAccountCollection.Add(account);
                        }
                    }
                    else
                    {
                        for (var row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
                        {
                            var account = personAccountChildCollection[row - 1].ContainedEntity;

                            if (!_selectedPersonAccountCollection.Contains(account))
                            {
                                _selectedPersonAccountCollection.Add(account);
                            }
                        }
                    }
                }
            }
        }

        internal override void AddNewGridRowFromClipboard<T>(object sender, T eventArgs)
        {
            // Content base copy paste is for grouping grid
            PasteSpecial(sender, eventArgs);
        }

        internal override void ViewDataSaved<T>(object sender, T eventArgs)
        {
            Grid.Invalidate();
        }

        internal override void TrackerDescriptionChanged<T>(object sender, SelectedItemChangeBaseEventArgs<T> eventArgs)
        {
            // Colleps all expanded rows
            for (var rowIndex = 0; rowIndex < Grid.RowCount; rowIndex++)
            {
                var actualIndex = rowIndex + 1;
                var gridInfo = GridRangeInfo.Cells(actualIndex, GridInCellColumnIndex, actualIndex, ParentGridLastColumnIndex);

                if (FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].ExpandState)
                {
                    FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].ExpandState = false;

                    // Get child grid and dispose it
                    var gridControl = Grid[actualIndex, GridInCellColumnIndex].Control as GridControl;

                    if (gridControl != null)
                    {
                        FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].GridControl = null;
                        gridCreatorDispose(gridControl, gridInfo);

                        Grid.RowHeights[actualIndex] = Grid.DefaultRowHeight;
                    }
                }
            }

            // ReCall Person accounts
            FilteredPeopleHolder.GetFilteredParentPersonAccounts();
            Grid.Invalidate();
        }

        internal override void DisposeChildGrids()
        {
            for (var rowIndex = 0; rowIndex < Grid.RowCount; rowIndex++)
            {
                if (FilteredPeopleHolder.PersonAccountModelCollection.Count <= rowIndex) break;

                var actualIndex = rowIndex + 1;
                var gridInfo = GridRangeInfo.Cells(actualIndex, GridInCellColumnIndex, actualIndex, ParentGridLastColumnIndex);

                if (FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].ExpandState)
                {
                    FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].ExpandState = false;

                    // Get child grid and dispose it
                    var gridControl = Grid[actualIndex, GridInCellColumnIndex].Control as GridControl;

                    if (gridControl != null)
                    {
                        FilteredPeopleHolder.PersonAccountModelCollection[rowIndex].GridControl = null;

                        gridCreatorDispose(gridControl, gridInfo);
                        Grid.RowHeights[actualIndex] = Grid.DefaultRowHeight;
                    }
                }
            }
        }

        internal override void SetSelectedPersons(IList<IPerson> selectedPersons)
        {
            // Selection events will not be raised
            Grid.Model.Selections.Clear(false);

			var ranges = new List<GridRangeInfo>();
            foreach (var person in selectedPersons)
            {
                for (int i = 0; i < FilteredPeopleHolder.PersonAccountModelCollection.Count; i++)
                {
                    int actualIndex = i + 1;
                    if (FilteredPeopleHolder.PersonAccountModelCollection[i].Parent.Person.Equals(person))
                    {
                        ranges.Add(GridRangeInfo.Row(actualIndex));
                    }
                }
            }
            ranges.ForEach(Grid.Selections.Add);
        }

        public ReadOnlyCollection<IPersonAccountModel> GetSelectedPersonAccounts
        {
			get
			{
				if (_currentSelectedPersonAccounts == null) return new ReadOnlyCollection<IPersonAccountModel>(new List<IPersonAccountModel>());
				return new ReadOnlyCollection<IPersonAccountModel>(_currentSelectedPersonAccounts);
			}
        }

        internal override IList<IPerson> GetSelectedPersons()
        {

            IList<IPerson> selectedPersons = new List<IPerson>();

            if (_currentSelectedPersonAccounts != null &&
                _currentSelectedPersonAccounts.Count > 0 &&
                _currentSelectedPersonAccounts[0] != null)
            {
                for (int i = 0; i < _currentSelectedPersonAccounts.Count; i++)
                {
                    if (_currentSelectedPersonAccounts[i].Parent != null)
                    {
                        selectedPersons.Add(_currentSelectedPersonAccounts[i].Parent.Person);
                    }
                }
            }
            else
            {
                selectedPersons = GetSelectedPersonsInGrd();
            }

            return selectedPersons;
        }

    }
}
