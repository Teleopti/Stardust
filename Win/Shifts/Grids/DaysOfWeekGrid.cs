﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Events;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Shifts.Grids
{
    public class DaysOfWeekGrid : GridViewBase<IDaysOfWeekPresenter, IDaysOfWeekViewModel>
    {
        private readonly IEventAggregator _eventAggregator;

        private ColumnBase<IDaysOfWeekViewModel> _ruleSetColumn;
        private ColumnBase<IDaysOfWeekViewModel> _accessibilityColumn;
        private ColumnBase<IDaysOfWeekViewModel> _sundayColumn;
        private ColumnBase<IDaysOfWeekViewModel> _mondayColumn;
        private ColumnBase<IDaysOfWeekViewModel> _tuesdayColumn;
        private ColumnBase<IDaysOfWeekViewModel> _wednesdayColumn;
        private ColumnBase<IDaysOfWeekViewModel> _thursdayColumn;
        private ColumnBase<IDaysOfWeekViewModel> _fridayColumn;
        private ColumnBase<IDaysOfWeekViewModel> _saturdayColumn;

        private Dictionary<DayOfWeek, ColumnBase<IDaysOfWeekViewModel>> _columnDictionary;

        public DaysOfWeekGrid(IDaysOfWeekPresenter presenter, GridControl grid, IEventAggregator eventAggregator)
            : base(presenter, grid)
        {
            _eventAggregator = eventAggregator;
        }

        internal override ShiftCreatorViewType Type
        {
            get { return ShiftCreatorViewType.WeekdayExclusion; }
        }

        internal override void CreateHeaders()
        {
            _columnDictionary = new Dictionary<DayOfWeek, ColumnBase<IDaysOfWeekViewModel>>();

            IList<DayOfWeek> daysOfWeek = DateHelper.GetDaysOfWeek(TeleoptiPrincipal.Current.Regional.Culture);
            
            AddColumn(new RowHeaderColumn<IDaysOfWeekViewModel>());
            
            _ruleSetColumn = new ReadOnlyTextColumn<IDaysOfWeekViewModel>("WorkShiftRuleSet.Description.Name", UserTexts.Resources.RuleSet, UserTexts.Resources.RuleSet, true);
            AddColumn(_ruleSetColumn);

            _accessibilityColumn = new ReadOnlyTextColumn<IDaysOfWeekViewModel>("AccessibilityText", UserTexts.Resources.IncludeExclude, UserTexts.Resources.Available, true);
            AddColumn(_accessibilityColumn);

            _mondayColumn = new CheckColumn<IDaysOfWeekViewModel>("Monday", "true", "false", "1", typeof(bool), UserTexts.Resources.Monday, UserTexts.Resources.WeekDay);
            _columnDictionary.Add(DayOfWeek.Monday, _mondayColumn);
            _tuesdayColumn = new CheckColumn<IDaysOfWeekViewModel>("Tuesday", "true", "false", "1", typeof(bool), UserTexts.Resources.Tuesday, UserTexts.Resources.WeekDay);
            _columnDictionary.Add(DayOfWeek.Tuesday, _tuesdayColumn);
            _wednesdayColumn = new CheckColumn<IDaysOfWeekViewModel>("Wednesday", "true", "false", "1", typeof(bool), UserTexts.Resources.Wednesday, UserTexts.Resources.WeekDay);
            _columnDictionary.Add(DayOfWeek.Wednesday, _wednesdayColumn);
            _thursdayColumn = new CheckColumn<IDaysOfWeekViewModel>("Thursday", "true", "false", "1", typeof(bool), UserTexts.Resources.Thursday, UserTexts.Resources.WeekDay);
            _columnDictionary.Add(DayOfWeek.Thursday, _thursdayColumn);
            _fridayColumn = new CheckColumn<IDaysOfWeekViewModel>("Friday", "true", "false", "1", typeof(bool), UserTexts.Resources.Friday, UserTexts.Resources.WeekDay);
            _columnDictionary.Add(DayOfWeek.Friday, _fridayColumn);
            _saturdayColumn = new CheckColumn<IDaysOfWeekViewModel>("Saturday", "true", "false", "1", typeof(bool), UserTexts.Resources.Saturday, UserTexts.Resources.WeekDay);
            _columnDictionary.Add(DayOfWeek.Saturday, _saturdayColumn);
            _sundayColumn = new CheckColumn<IDaysOfWeekViewModel>("Sunday", "true", "false", "1", typeof(bool), UserTexts.Resources.Sunday, UserTexts.Resources.WeekDay);
            _columnDictionary.Add(DayOfWeek.Sunday, _sundayColumn);

            foreach (DayOfWeek dayOfWeek in daysOfWeek)
                AddColumn(_columnDictionary[dayOfWeek]);                
        }
               
        internal override void PrepareView()
        {
            ColCount = GridColumns.Count;
            RowCount = Presenter.ModelCollection.Count + 1;

            Grid.RowCount = RowCount;
            Grid.ColCount = ColCount - 1;

            Grid.Cols.HeaderCount = 0;
            Grid.Rows.HeaderCount = 1;

            Grid.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);

            Grid.Name = "DaysOfWeekGrid";
        }

        internal override void MergeHeaders()
        {
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 0, 1, 0));
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 1, 1, 1));
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 2, 1, 2));
            Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 3, 0, 9));
            
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
                e.Style.CellValue = " "; //we have to set this to blank space otherwise syncfusion will override with a number.

            e.Handled = true;
        }
        
        private void ResetSelected()
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

            foreach (int index in selectedList)
            {
                IDaysOfWeekViewModel weekView = Presenter.ModelCollection[index - 1];
                weekView.Saturday = false;
                weekView.Sunday = false;
                weekView.Monday = false;
                weekView.Tuesday = false;
                weekView.Wednesday = false;
                weekView.Thursday = false;
                weekView.Friday = false;
            }
            Grid.Invalidate();
        }

        public override void Sort(bool isAscending, int columnIndex)
        {
            if (columnIndex > 1)
            {
                SortingModes mode = isAscending ? SortingModes.Ascending : SortingModes.Descending;
                IList<IDaysOfWeekViewModel> result = this.Sort((ISortColumn<IDaysOfWeekViewModel>) GridColumns[columnIndex],
                                                         Presenter.ModelCollection, mode, columnIndex);
                Presenter.SetModelCollection(new ReadOnlyCollection<IDaysOfWeekViewModel>(result));
                Grid.Invalidate();
            }
        }

        #region Overriden Methods

        public override void Add()
        {
        }

        public override void Delete()
        {
            ResetSelected();
            _eventAggregator.GetEvent<RuleSetChanged>().Publish(Presenter.Explorer.Model.FilteredRuleSetCollection);
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
                IList<IDaysOfWeekViewModel> result = Sort((ISortColumn<IDaysOfWeekViewModel>)GridColumns[columnIndex], Presenter.ModelCollection, order, columnIndex);
                Presenter.SetModelCollection(new ReadOnlyCollection<IDaysOfWeekViewModel>(result));
                Grid.Invalidate();
            }
        }

        public override void RefreshView()
        {
            Grid.RowCount = (Presenter.ModelCollection.Count + 1);
            Grid.Invalidate();
        }

        #endregion
    }
}
