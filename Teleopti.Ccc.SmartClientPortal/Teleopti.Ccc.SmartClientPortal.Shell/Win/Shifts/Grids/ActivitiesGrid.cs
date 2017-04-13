using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Events;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.WinCode.Shifts.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Shifts.Grids
{
    public class ActivitiesGrid : GridViewBase<IActivityPresenter, IActivityViewModel>
    {
        private readonly IEventAggregator _eventAggregator;
        private const int ActivityLengthMinTimeCell = 5;
        private const int ActivityLengthMaxTimeCell = 6;
        private const int ActivityPositionStartTimeCell = 8;
        private const int ActivityPositionEndTimeCell = 9;

        private RowHeaderColumn<IActivityViewModel> _rowHeaderColumn;

        private ColumnBase<IActivityViewModel> _workShiftRuleSet;
        private ColumnBase<IActivityViewModel> _isAutoPosition;
        private ColumnBase<IActivityViewModel> _count;
        private ColumnBase<IActivityViewModel> _relative;
        private ColumnBase<IActivityViewModel> _activity;
        private ColumnBase<IActivityViewModel> _alSegment;
        private ColumnBase<IActivityViewModel> _alMinTime;
        private ColumnBase<IActivityViewModel> _alMaxTime;
        private ColumnBase<IActivityViewModel> _apSegment;
        private EditableHourMinutesColumn<IActivityViewModel> _apStartTime;
        private EditableHourMinutesColumn<IActivityViewModel> _apEndTime;

        private ToolStripMenuItem _addNewMenuItem;
        private ToolStripMenuItem _deletemenuItem;
        private ToolStripMenuItem _addNewFromClipboard;
        private ToolStripMenuItem _moveUp;
        private ToolStripMenuItem _moveDown;

        public ActivitiesGrid(IActivityPresenter presenter, GridControl grid, IEventAggregator eventAggregator)
            : base(presenter, grid)
        {
            _eventAggregator = eventAggregator;
            if (grid == null) return;

            grid.CellModels.Add("TimeSpanTimeOfDayCellModel", new TimeSpanTimeOfDayCellModel(grid.Model));
            grid.CellModels.Add("TimeSpanLongHourMinutesCellModelHours", new TimeSpanDurationCellModel(grid.Model) { OnlyPositiveValues = true });
            grid.CellModels.Add("TimeSpanLongHourMinutesCellModelMinutes", new TimeSpanDurationCellModel(grid.Model) { OnlyPositiveValues = true, InterpretAsMinutes = true });
            if (!grid.CellModels.ContainsKey("ActivityDropDownCell"))
                grid.CellModels.Add("ActivityDropDownCell", new ActivityDropDownCellModel(grid.Model,Resources.MasterActivity16x16));
        }

        internal override ShiftCreatorViewType Type
        {
            get { return ShiftCreatorViewType.Activities; }
        }

        internal override void CreateHeaders()
        {
            _rowHeaderColumn = new RowHeaderColumn<IActivityViewModel>();
            AddColumn(_rowHeaderColumn);

            _workShiftRuleSet = new ReadOnlyTextColumn<IActivityViewModel>("WorkShiftRuleSet.Description.Name",
                                                                      UserTexts.Resources.RuleSet,
                                                                      UserTexts.Resources.RuleSet,
                                                                      makeStatic:true);
            AddColumn(_workShiftRuleSet);

            _isAutoPosition = new CheckColumn<IActivityViewModel>("IsAutoPosition",
                                                                 "true",
                                                                 "false",
                                                                 "1",
                                                                 typeof(bool),
                                                                 UserTexts.Resources.CombinedGridAutoPosition,
                                                                 UserTexts.Resources.CombinedGridAutoPosition);
            AddColumn(_isAutoPosition);

            _count = new EditableTextColumn<IActivityViewModel>("Count",
                                                           100,
                                                           UserTexts.Resources.Count,
                                                           UserTexts.Resources.CombinedGridAutoPosition);
            AddColumn(_count);

            _relative = new DropDownTypeColumn<IActivityViewModel, GridClassType>("TypeOfClass",
                                                              UserTexts.Resources.Type,
                                                              Presenter.Explorer.Model.ClassTypeCollection,
                                                              UserTexts.Resources.Type);
            ((DropDownTypeColumn<IActivityViewModel, GridClassType>)_relative).TypeChanged += TypeColumn_TypeChanged;
            _relative.SortCompare = delegate(IActivityViewModel left, IActivityViewModel right)
            {
                int value = -1;
                if (left.TypeOfClass != null && right.TypeOfClass != null)
                    value = string.Compare(left.TypeOfClass.Name,
                                            right.TypeOfClass.Name,
                                            StringComparison.CurrentCulture);
                return value;
            };

            AddColumn(_relative);

            _activity = new ActivityDropDownColumn<IActivityViewModel, IActivity>("CurrentActivity",
                                                                  UserTexts.Resources.Activity,
                                                                  Presenter.Explorer.Model.ActivityCollection,
                                                                  "Name",
																  UserTexts.Resources.Activity, Resources.MasterActivity16x16);
            _activity.SortCompare = delegate(IActivityViewModel left, IActivityViewModel right)
            {
                return string.Compare(left.CurrentActivity.Name,
                                      right.CurrentActivity.Name,
                                      StringComparison.CurrentCulture);
            };
            AddColumn(_activity);

            _alMinTime = new EditableHourMinutesColumn<IActivityViewModel>("ALMinTime",
                                                                      UserTexts.Resources.MinTime,
                                                                      UserTexts.Resources.ActivityLength);
            _alMinTime.Validate = ValidateALMinTime;
            AddColumn(_alMinTime);

            _alMaxTime = new EditableHourMinutesColumn<IActivityViewModel>("ALMaxTime",
                                                                      UserTexts.Resources.MaxTime,
                                                                      UserTexts.Resources.ActivityLength);
            _alMaxTime.Validate = ValidateALMaxTime;
            AddColumn(_alMaxTime);

            _alSegment = new EditableHourMinutesColumn<IActivityViewModel>("ALSegment",
                                                                      UserTexts.Resources.Segment,
                                                                      UserTexts.Resources.ActivityLength,
                                                                      cellTypeLength: "TimeSpanLongHourMinutesCellModelMinutes");
            _alSegment.Validate = ValidateALSegment;
            AddColumn(_alSegment);

            _apStartTime = new EditableHourMinutesColumn<IActivityViewModel>("APStartTime",
                                                                        UserTexts.Resources.EarlyStart,
                                                                        UserTexts.Resources.ActivityPosition,
                                                                        "IsTimeOfDay");
            _apStartTime.Validate = ValidateAPStartTime;
            AddColumn(_apStartTime);

            _apEndTime = new EditableHourMinutesColumn<IActivityViewModel>("APEndTime",
                                                                      UserTexts.Resources.LateStart,
                                                                      UserTexts.Resources.ActivityPosition,
                                                                      "IsTimeOfDay");
            _apEndTime.Validate = ValidateAPEndTime;
            AddColumn(_apEndTime);

            _apSegment = new EditableHourMinutesColumn<IActivityViewModel>("APSegment",
                                                                      UserTexts.Resources.Segment,
                                                                      UserTexts.Resources.ActivityPosition,
                                                                      cellTypeLength: "TimeSpanLongHourMinutesCellModelMinutes");
            _apSegment.Validate = ValidateAPSegment;
            AddColumn(_apSegment);
        }

        internal override void PrepareView()
        {
            ColCount = GridColumns.Count;
            RowCount = Presenter.ModelCollection.Count + 1;

            Grid.RowCount = RowCount;
            Grid.ColCount = ColCount - 1;

            Grid.Cols.HeaderCount = 0;
            Grid.Rows.HeaderCount = 1;

            Grid.Rows.FrozenCount = 1;

            Grid.ColWidths[1] = 169;
            Grid.ColWidths[4] = 129;
            Grid.ColWidths[5] = 259;
            Grid.ColWidths[9] = 80;
            Grid.ColWidths[10] = 80;

            Grid.Name = "ActivityGrid";
        }

        internal override void MergeHeaders()
        {
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 0, 1, 0));
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 1, 1, 1));
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 2, 0, 3));
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 4, 1, 4));
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 5, 1, 5));
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 6, 0, 8));
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 9, 0, 11));
        }

        internal override void CreateContextMenu()
        {
            Grid.ContextMenuStrip = new ContextMenuStrip();

            _addNewMenuItem = new ToolStripMenuItem(UserTexts.Resources.New);
            _addNewMenuItem.Click += delegate { Add(); };
        	_addNewMenuItem.Enabled = false;
            Grid.ContextMenuStrip.Items.Add(_addNewMenuItem);

            _addNewFromClipboard = new ToolStripMenuItem(UserTexts.Resources.PasteNew);
            _addNewFromClipboard.Click += delegate { PasteSpecial(); };
        	_addNewFromClipboard.Enabled = false;
            Grid.ContextMenuStrip.Items.Add(_addNewFromClipboard);

            _deletemenuItem = new ToolStripMenuItem(UserTexts.Resources.Delete);
            _deletemenuItem.Click += delegate { Delete(); };
        	_deletemenuItem.Enabled = false;
            Grid.ContextMenuStrip.Items.Add(_deletemenuItem);

            Grid.ContextMenuStrip.Items.Add(new ToolStripSeparator());

            _moveUp = new ToolStripMenuItem(UserTexts.Resources.MoveUp);
            _moveUp.Click += MoveUp;
        	_moveUp.Enabled = false;
            Grid.ContextMenuStrip.Items.Add(_moveUp);

            _moveDown = new ToolStripMenuItem(UserTexts.Resources.MoveDown);
            _moveDown.Click += MoveDown;
        	_moveDown.Enabled = false;
            Grid.ContextMenuStrip.Items.Add(_moveDown);

        	Grid.ShowContextMenu += Grid_ShowContextMenu;
        }

		internal void Grid_ShowContextMenu(object sender, Syncfusion.Windows.Forms.ShowContextMenuEventArgs e)
		{
			foreach(var menuItem in Grid.ContextMenuStrip.Items)
			{
				var item = menuItem as ToolStripMenuItem;
				if(item!=null)
					item.Enabled = Presenter.Explorer.Model.FilteredRuleSetCollection != null;
			}
			Grid.ContextMenuStrip.Show();
		}

        internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
                GridColumns[e.ColIndex].GetCellInfo(e, Presenter.ModelCollection);
            }
            if (e.RowIndex > 0 && e.ColIndex == 0)
                e.Style.CellValue = " "; //we have to set this to blank space otherwise syncfusion will override with a number.

            e.Handled = true;
        }

        internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
                GridColumns[e.ColIndex].SaveCellInfo(e, Presenter.ModelCollection);

                _eventAggregator.GetEvent<RuleSetChanged>().Publish(new List<IWorkShiftRuleSet> { Presenter.ModelCollection[e.RowIndex - 2].WorkShiftRuleSet });
            }

            e.Handled = true;
        }

        private void MoveUp<T>(object sender, T args)
        {
            HandleMove(delegate(IList<int> selectedList, int adapterCount)
            {
				if (selectedList.Count.Equals(adapterCount)) return false;
                return (selectedList.Count > 0 && (selectedList[0] > 1 && selectedList[0] <= adapterCount));
            }, MoveType.MoveUp);
        }

        private void MoveDown<T>(object sender, T args)
        {
            HandleMove(delegate(IList<int> selectedList, int adapterCount)
            {
				if (selectedList.Count.Equals(adapterCount)) return false;
                return (selectedList.Count > 0 && selectedList[0] < adapterCount);
            }, MoveType.MoveDown);
        }

        private delegate bool CheckSelectedList(IList<int> selectedList, int adapterCount);

        private void HandleMove(CheckSelectedList check, MoveType moveType)
        {
            int adapterCount = Presenter.ModelCollection.Count;
            IList<int> selectedList = GetSelectedRowsIndexList();
            if (check(selectedList, adapterCount))
                Presenter.ReOrderActivities(new ReadOnlyCollection<int>(selectedList), moveType);
            Grid.CurrentCell.MoveTo(Grid.RowCount, 0, GridSetCurrentCellOptions.SetFocus);
            _eventAggregator.GetEvent<RuleSetChanged>().Publish(Presenter.Explorer.Model.FilteredRuleSetCollection);
            Grid.Invalidate();
        }

        private void AddNewActivityNormal()
        {
            Presenter.AddAbsolutePositionActivity();
            RowCount = Presenter.ModelCollection.Count + 1;
            Grid.RowCount = RowCount;
            Grid.CurrentCell.MoveTo((RowCount), 1, GridSetCurrentCellOptions.SetFocus);
            _eventAggregator.GetEvent<RuleSetChanged>().Publish(Presenter.Explorer.Model.FilteredRuleSetCollection);
        }

        private void DeleteSelected()
        {
            IList<int> selectedList = GetSelectedRowsIndexList();

            Presenter.DeleteActivities(new ReadOnlyCollection<int>(selectedList));
            selectedList.Clear();

            RowCount = Presenter.ModelCollection.Count;
            Grid.RowCount = RowCount + 1;
            Grid.CurrentCell.MoveTo((RowCount), 0, GridSetCurrentCellOptions.SetFocus);
            Grid.Selections.Clear();
            Grid.Invalidate();
            _eventAggregator.GetEvent<RuleSetChanged>().Publish(Presenter.Explorer.Model.FilteredRuleSetCollection);
        }

        private IList<int> GetSelectedRowsIndexList()
        {
            IList<int> selectedList = new List<int>();
            GridRangeInfoList selectedRangeInfoList = Grid.Model.Selections.GetSelectedRows(true, false);
            foreach (GridRangeInfo rangeInfo in selectedRangeInfoList)
            {
                if (rangeInfo.RangeType == GridRangeInfoType.Rows)
                {
					for (var i = ExcludeHeaderRows(rangeInfo.Top); i <= rangeInfo.Bottom; i++)
                    {
                        selectedList.Add((i - 1));
                    }
                }
            }
            return selectedList;
        }

        internal void TypeColumn_TypeChanged(object sender, TypeChangeEventArgs e)
        {
            if (e.DataItem is AutoPositionViewModel)
                return;
            if (!((IActivityViewModel)e.DataItem).Validate())
            {
                e.IsDataItemValid = false;
                return;
            }

            var activityNormal = (AbsolutePositionViewModel)e.DataItem;
            IActivity activity = activityNormal.CurrentActivity;
            if (e.NewType == typeof(ActivityAbsoluteStartExtender))
            {
                var aasEntender = new ActivityAbsoluteStartExtender(activity, activityNormal.ContainedEntity.ActivityLengthWithSegment, activityNormal.ContainedEntity.ActivityPositionWithSegment);
                Presenter.ChangeExtenderType(activityNormal, aasEntender);
            }

            if (e.NewType == typeof(ActivityRelativeStartExtender))
            {
                var arsExtender = new ActivityRelativeStartExtender(activity, activityNormal.ContainedEntity.ActivityLengthWithSegment, activityNormal.ContainedEntity.ActivityPositionWithSegment);
                Presenter.ChangeExtenderType(activityNormal, arsExtender);
            }

            if (e.NewType == typeof(ActivityRelativeEndExtender))
            {
                var areExtender = new ActivityRelativeEndExtender(activity, activityNormal.ContainedEntity.ActivityLengthWithSegment, activityNormal.ContainedEntity.ActivityPositionWithSegment);
                Presenter.ChangeExtenderType(activityNormal, areExtender);
            }
            Grid.Invalidate();
        }

        private void RefreshCell(int cell, int row)
        {
            Grid.RefreshRange(GridRangeInfo.Cell(row, cell));
        }

        private void ValidateALMinTime(IActivityViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.ALMinTime.Equals(TimeSpan.Zero))
                valid = false;
            else if (dataItem.ALMinTime > dataItem.ALMaxTime)
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();

            if (inSaveMode)
                RefreshCell(ActivityLengthMinTimeCell, row);
        }

        private void ValidateALMaxTime(IActivityViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.ALMaxTime.Equals(TimeSpan.Zero))
                valid = false;
            else if (dataItem.ALMinTime > dataItem.ALMaxTime)
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();

            if (inSaveMode)
                RefreshCell(ActivityLengthMaxTimeCell, row);
        }

        private static void ValidateALSegment(IActivityViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.ALSegment.Equals(TimeSpan.Zero))
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();
        }

        private void ValidateAPStartTime(IActivityViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.APStartTime > dataItem.APEndTime)
                valid = false;
            if (dataItem.APStartTime == null)
            {
                valid = false;
                styleInfo.BackColor = Color.Gray;
            }
            if (valid)
                styleInfo.ResetBackColor();

            if (inSaveMode)
                RefreshCell(ActivityPositionStartTimeCell, row);
        }

        private void ValidateAPEndTime(IActivityViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.APStartTime > dataItem.APEndTime)
                valid = false;
            if (dataItem.APEndTime == null)
            {
                valid = false;
                styleInfo.BackColor = Color.Gray;
            }
            if (valid)
                styleInfo.ResetBackColor();

            if (inSaveMode)
                RefreshCell(ActivityPositionEndTimeCell, row);
        }

        private static void ValidateAPSegment(IActivityViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.APSegment.Equals(TimeSpan.Zero))
                valid = false;
	        if (valid)
                styleInfo.ResetBackColor();
        }

        #region Overriden Methods

        public override void Add()
        {
            AddNewActivityNormal();
        }

        public override void Delete()
        {
            DeleteSelected();
        }

        public override void Rename()
        {
        }

        public override void Sort(SortingMode mode)
        {
            int columnIndex = Grid.CurrentCell.ColIndex;
            if (columnIndex > 1)
            {
                SortingModes order = (mode == (SortingMode.Ascending)) ? SortingModes.Ascending : SortingModes.Descending;
                IList<IActivityViewModel> result = Sort((ISortColumn<IActivityViewModel>)GridColumns[columnIndex], Presenter.ModelCollection, order, columnIndex);
                Presenter.SetModelCollection(new ReadOnlyCollection<IActivityViewModel>(result));
                Grid.Invalidate();
            }
        }

        public override void RefreshView()
        {
            Grid.RowCount = Presenter.ModelCollection.Count + 1;
            Grid.Invalidate();
        }

        internal override void KeyDown(KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
                e.Handled = true;
        }

        #endregion
    }
}
