using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Properties;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Events;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts.Grids
{
    public class GeneralTemplateGrid : GridViewBase<IGeneralTemplatePresenter, IGeneralTemplateViewModel>
    {
        private readonly IEventAggregator _eventAggregator;
        private const int StartPeriodStartTimeCell = 5;
        private const int StartPeriodEndTimeCell = 6;
        private const int EndPeriodStartTimeCell = 8;
        private const int EndPeriodEndTimeCell = 9;

        private ColumnBase<IGeneralTemplateViewModel> _ruleSet;
        private ColumnBase<IGeneralTemplateViewModel> _baseActivityColumn;
        private ColumnBase<IGeneralTemplateViewModel> _category;
        private ColumnBase<IGeneralTemplateViewModel> _accessibilityColumn;
        private ColumnBase<IGeneralTemplateViewModel> _startPeriodStartTime;
        private ColumnBase<IGeneralTemplateViewModel> _startPeriodEndTime;
        private ColumnBase<IGeneralTemplateViewModel> _startPeriodSegment;
        private ColumnBase<IGeneralTemplateViewModel> _endPeriodStartTime;
        private ColumnBase<IGeneralTemplateViewModel> _endPeriodEndTime;
        private ColumnBase<IGeneralTemplateViewModel> _endPeriodSegment;
        private ColumnBase<IGeneralTemplateViewModel> _workingStartTime;
        private ColumnBase<IGeneralTemplateViewModel> _workingEndTime;
        private ColumnBase<IGeneralTemplateViewModel> _workingSegment;
    	private ColumnBase<IGeneralTemplateViewModel> _onlyForRestrictions;
        private IList<int> _columnAmounts;

        public GeneralTemplateGrid(IGeneralTemplatePresenter presenter,
                                    GridControl grid, IEventAggregator eventAggregator)
            : base(presenter, grid)
        {
            _eventAggregator = eventAggregator;
            grid.CellModels.Add("TimeSpanTimeOfDayCellModel", new TimeSpanTimeOfDayCellModel(grid.Model));
            grid.CellModels.Add("TimeSpanLongHourMinutesCellModelHours", new TimeSpanDurationCellModel(grid.Model) { OnlyPositiveValues = true });
            grid.CellModels.Add("TimeSpanLongHourMinutesCellModelMinutes", new TimeSpanDurationCellModel(grid.Model) { OnlyPositiveValues = true,InterpretAsMinutes = true});
            if (!grid.CellModels.ContainsKey("ActivityDropDownCell"))
				grid.CellModels.Add("ActivityDropDownCell", new ActivityDropDownCellModel(grid.Model, Resources.MasterActivity16x16));
			if (!grid.CellModels.ContainsKey(GridCellModelConstants.CellTypeDropDownCellModel))
				grid.CellModels.Add(GridCellModelConstants.CellTypeDropDownCellModel, new DropDownCellModel(grid.Model));
        }

        internal override ShiftCreatorViewType Type
        {
            get { return ShiftCreatorViewType.General; }
        }

        #region Methods

        internal override void CreateHeaders()
        {
            AddColumn(new RowHeaderColumn<IGeneralTemplateViewModel>());

            _ruleSet = new EditableTextColumn<IGeneralTemplateViewModel>("WorkShiftRuleSet.Description.Name", 180, "RuleSet", UserTexts.Resources.RuleSet);
            AddColumn(_ruleSet);

            _baseActivityColumn = new ActivityDropDownColumn<IGeneralTemplateViewModel, IActivity>("BaseActivity",
                                                                                  "Activity",
                                                                                  Presenter.Explorer.Model.ActivityCollection,
																				  "Name", UserTexts.Resources.BaseActivity, Resources.MasterActivity16x16);
            AddColumn(_baseActivityColumn);

            _category = new DropDownColumn<IGeneralTemplateViewModel, IShiftCategory>("Category",
                                                                                "Category",
                                                                                Presenter.Explorer.Model.CategoryCollection,
                                                                                "Description",
                                                                                UserTexts.Resources.Category);
            AddColumn(_category);

            _accessibilityColumn = new DropDownColumn<IGeneralTemplateViewModel, string>("Accessibility",
                                                                                   UserTexts.Resources.Accessibility,
                                                                                   Presenter.Explorer.Model.AccessibilityCollection,
                                                                                   "",
                                                                                   UserTexts.Resources.Available);
            AddColumn(_accessibilityColumn);

            _startPeriodStartTime = new TimeOfDayColumn<IGeneralTemplateViewModel>("StartPeriodStartTime",
                                                                                       UserTexts.Resources.Early,
                                                                                       UserTexts.Resources.StartPeriod);
            _startPeriodStartTime.Validate = ValidateStartPeriodStartTime;
            AddColumn(_startPeriodStartTime);

            _startPeriodEndTime = new TimeOfDayColumn<IGeneralTemplateViewModel>("StartPeriodEndTime",
                                                                                     UserTexts.Resources.Late,
                                                                                     UserTexts.Resources.StartPeriod);
            _startPeriodEndTime.Validate = ValidateStartPeriodEndTime;
            AddColumn(_startPeriodEndTime);

            _startPeriodSegment = new EditableHourMinutesColumn<IGeneralTemplateViewModel>("StartPeriodSegment",
                                                                                    UserTexts.Resources.Segment,
                                                                                    UserTexts.Resources.StartPeriod,
                                                                                    cellTypeLength: "TimeSpanLongHourMinutesCellModelMinutes");
            _startPeriodSegment.Validate = ValidateStartPeriodSegment;
            AddColumn(_startPeriodSegment);

            _endPeriodStartTime = new TimeOfDayColumn<IGeneralTemplateViewModel>("EndPeriodStartTime",
                                                                                    UserTexts.Resources.Early,
                                                                                    UserTexts.Resources.EndPeriod);
            _endPeriodStartTime.Validate = ValidateEndPeriodStartTime;
            AddColumn(_endPeriodStartTime);

            _endPeriodEndTime = new TimeOfDayColumn<IGeneralTemplateViewModel>("EndPeriodEndTime",
                                                                                    UserTexts.Resources.Late,
                                                                                    UserTexts.Resources.EndPeriod);
            _endPeriodEndTime.Validate = ValidateEndPeriodEndTime;
            AddColumn(_endPeriodEndTime);

            _endPeriodSegment = new EditableHourMinutesColumn<IGeneralTemplateViewModel>("EndPeriodSegment",
                                                                                    UserTexts.Resources.Segment,
                                                                                    UserTexts.Resources.EndPeriod,
                                                                                    cellTypeLength: "TimeSpanLongHourMinutesCellModelMinutes");
            _endPeriodSegment.Validate = ValidateEndPeriodSegment;
            AddColumn(_endPeriodSegment);

            _workingStartTime = new EditableHourMinutesColumn<IGeneralTemplateViewModel>("WorkingStartTime",
                                                                                UserTexts.Resources.Min,
                                                                                UserTexts.Resources.Length);
            _workingStartTime.Validate = ValidateWorkingStartTime;
            AddColumn(_workingStartTime);

            _workingEndTime = new EditableHourMinutesColumn<IGeneralTemplateViewModel>("WorkingEndTime",
                                                                                UserTexts.Resources.Max,
                                                                                UserTexts.Resources.Length);
            _workingEndTime.Validate = ValidateWorkingEndTime;
            AddColumn(_workingEndTime);

            _workingSegment = new EditableHourMinutesColumn<IGeneralTemplateViewModel>("WorkingSegment",
                                                                                UserTexts.Resources.Segment,
                                                                                UserTexts.Resources.Length,
                                                                                cellTypeLength: "TimeSpanLongHourMinutesCellModelMinutes");
            _workingSegment.Validate = ValidateWorkingSegment;
            AddColumn(_workingSegment);


        	_onlyForRestrictions = new CheckColumn<IGeneralTemplateViewModel>("OnlyForRestrictions", "true", "false", "1",
        	                                                                  typeof (bool), "OnlyForRestrictions", UserTexts.Resources.UseOnlyForRestrictions);

			_onlyForRestrictions.CellChanged += OnlyForRestrictionsCellChanged;
			AddColumn(_onlyForRestrictions);

        }

		void OnlyForRestrictionsCellChanged(object sender, ColumnCellChangedEventArgs<IGeneralTemplateViewModel> e)
		{
			Presenter.InvokeOnlyForRestrictionsCellChanged();		
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

            Grid.ColWidths[0] = 45;
            Grid.ColWidths[1] = 199;
            Grid.ColWidths[2] = 156;
            Grid.ColWidths[3] = 179;
            Grid.ColWidths[5] = 80;
            Grid.ColWidths[6] = 80;
            Grid.ColWidths[8] = 80;
            Grid.ColWidths[9] = 80;
        	Grid.ColWidths[14] = 250;

            Grid.Name = "GeneralGrid";
        }

        internal override void MergeHeaders()
        {
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 0, 1, 0)); // Hearder ?
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 1, 1, 1)); // Rule Set ?
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 2, 1, 2)); // Base Activity ?
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 3, 1, 3)); // Category ?
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 4, 1, 4)); // Accessibility ?
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 5, 0, 7));
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 8, 0, 10));
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 11, 0, 13));
			Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 14, 1, 14)); //Only for restrictions

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

        internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            
            if (ValidCell(e.ColIndex, e.RowIndex))
                GridColumns[e.ColIndex].GetCellInfo(e, Presenter.ModelCollection);
            if (e.RowIndex > 0 && e.ColIndex == 0)
            {
                if (_columnAmounts == null)
                    return;
                if ((e.RowIndex >= 2) && (e.RowIndex <= (_columnAmounts.Count + 1)))
                    e.Style.CellValue = _columnAmounts[e.RowIndex - 2];
            }
            e.Handled = true;
        }

        private void RefreshCell(int cell, int row)
        {
            if (Grid.InvokeRequired)
            {
                Grid.BeginInvoke(new Action<int, int>(RefreshCell), cell, row);
                return;
            }
            Grid.InvalidateRange(GridRangeInfo.Cell(row, cell));
        }

        private void ValidateStartPeriodStartTime(IGeneralTemplateViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.StartPeriodStartTime > dataItem.StartPeriodEndTime || dataItem.StartPeriodEndTime > new TimeSpan(23, 59, 59))
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();

            if (inSaveMode)
                RefreshCell(StartPeriodEndTimeCell, row);
        }

        private void ValidateStartPeriodEndTime(IGeneralTemplateViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.StartPeriodEndTime < dataItem.StartPeriodStartTime || dataItem.StartPeriodEndTime > new TimeSpan(23, 59, 59))
                valid = false;
            if (dataItem.StartPeriodEndTime > new TimeSpan( 23, 59, 59))
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();

            if (inSaveMode)
                RefreshCell(StartPeriodStartTimeCell, row);
        }

        private static void ValidateStartPeriodSegment(IGeneralTemplateViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.StartPeriodSegment.Equals(TimeSpan.Zero))
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();
        }

        private void ValidateEndPeriodStartTime(IGeneralTemplateViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.EndPeriodStartTime > dataItem.EndPeriodEndTime)
                valid = false;
            if (dataItem.EndPeriodStartTime > new TimeSpan(1, 23, 59, 59))
                valid = false;
            if (dataItem.EndPeriodStartTime < dataItem.StartPeriodStartTime)
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();

            if (inSaveMode)
                RefreshCell(EndPeriodEndTimeCell, row);
        }

        private void ValidateEndPeriodEndTime(IGeneralTemplateViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.EndPeriodStartTime > dataItem.EndPeriodEndTime)
                valid = false;
            if (dataItem.EndPeriodEndTime > new TimeSpan(1, 23, 59, 59))
                valid = false;
            if (dataItem.EndPeriodEndTime < dataItem.StartPeriodEndTime)
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();

            if (inSaveMode)
                RefreshCell(EndPeriodStartTimeCell, row);
        }

        private static void ValidateEndPeriodSegment(IGeneralTemplateViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.EndPeriodSegment.Equals(TimeSpan.Zero))
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();
        }

        private void ValidateWorkingStartTime(IGeneralTemplateViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.WorkingStartTime > dataItem.WorkingEndTime)
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();

            if (inSaveMode)
                RefreshCell(12, row);
        }

        private void ValidateWorkingEndTime(IGeneralTemplateViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.WorkingStartTime > dataItem.WorkingEndTime)
                valid = false;
            if (dataItem.WorkingEndTime> new TimeSpan(36,0,0))
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();

            if (inSaveMode)
                RefreshCell(11, row);
        }

        private static void ValidateWorkingSegment(IGeneralTemplateViewModel dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode)
        {
            bool valid = true;
            styleInfo.BackColor = Color.Red;
            if (dataItem.WorkingSegment.Equals(TimeSpan.Zero))
                valid = false;
            if (valid)
                styleInfo.ResetBackColor();
        }

        #endregion

        public override void Add()
        {
        }

        public override void Delete()
        {
        }

        public override void Rename()
        {
        }

        #region Overriden Methods

        public override void Amounts(IList<int> shiftAmount)
        {
            _columnAmounts = new List<int>();
            _columnAmounts = shiftAmount;
        }

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        public override void RefreshView()
        {
            Grid.RowCount = Presenter.ModelCollection.Count + 1;
            Grid.Invalidate();
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
                IList<IGeneralTemplateViewModel> result = Sort((ISortColumn<IGeneralTemplateViewModel>)GridColumns[columnIndex], Presenter.ModelCollection, order, columnIndex);
                Presenter.SetModelCollection(new ReadOnlyCollection<IGeneralTemplateViewModel>(result));
                Grid.Invalidate();
            }
        }

        #endregion
    }
}
