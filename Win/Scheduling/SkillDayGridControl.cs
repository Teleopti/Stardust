﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Rows;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Chart;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    
    public class SkillDayGridControl : TeleoptiGridControl, ITaskOwnerGrid, IHelpContext
    {
        private AbstractDetailView _owner;
        private const int RowHeaderWidth = 200;
        private const int CellWidth = 50;
        private const string SettingName = "SchedulerSkillDayGridAndChart";
        private RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime,IList<ISkillStaffPeriod>>> _rowManager;
        //todo :(    -> guihelper is there for a reason
        private static readonly Color ColorCells = Color.AntiqueWhite;

        private IList<IGridRow> _gridRows;
        private GridRow _currentSelectedGridRow;
        private IList<DateOnly> _dates;
        private readonly ChartSettings _chartSettings;
        private readonly ChartSettings _defaultChartSettings = new ChartSettings();

        //constructor
        public SkillDayGridControl()
        {
            initializeComponent();
            initializeGrid();
            setupChartDefault();
            //flytta, i alla fall från konstruktor
            using(IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _chartSettings = new PersonalSettingDataRepository(uow).FindValueByKey(SettingName, _defaultChartSettings);
            }
        }

        //initialize component
        private void initializeComponent()
        {
            QueryColWidth += gridSkillDataQueryColWidth;
            QueryCellInfo += gridSkillDataQueryCellInfo;
            Model.ClipboardCanCopy +=Model_ClipboardCanCopy;

            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            ResumeLayout(false);
        }

        private void Model_ClipboardCanCopy(object sender, GridCutPasteEventArgs e)
        {
            e.Result = true;
            e.Handled = true;
        }

        private void setupChartDefault()
        {
            _defaultChartSettings.SelectedRows.Add("ForecastedHours");
            _defaultChartSettings.SelectedRows.Add("ScheduledHours");
            _defaultChartSettings.SelectedRows.Add("RelativeDifference");
        }

        //initialize grid
        private void initializeGrid()
        {
            CellModels.Add("TimeCell", timeSpanLongHourMinutesStaticCellModel());
            CellModels.Add("TimeSpanCell", initializeCallTimeSpanCell());
            CellModels.Add("ReadOnlyPercentCell", initializeCallPercentReadOnlyCell());
            CellModels.Add("PercentCellModel", initializeCallPercentReadOnlyPercentCell());
        }

        private GridCellModelBase timeSpanLongHourMinutesStaticCellModel()
        {
            return new TimeSpanLongHourMinutesStaticCellModel(Model);
        }

        private GridCellModelBase initializeCallTimeSpanCell()
        {
            return new TimeSpanLongHourMinutesStaticCellModel(Model);
        }

        private GridCellModelBase initializeCallPercentReadOnlyCell()
        {
            PercentReadOnlyCellModel cellModel = new PercentReadOnlyCellModel(Model);
            cellModel.NumberOfDecimals = 0;
            return cellModel;
        }

        private GridCellModelBase initializeCallPercentReadOnlyPercentCell()
        {
            PercentFromPercentReadOnlyCellModel cellModel = new PercentFromPercentReadOnlyCellModel(Model);
            cellModel.NumberOfDecimals = 0;
            return cellModel;
        }

        //event col width
        private void gridSkillDataQueryColWidth(object sender, GridRowColSizeEventArgs e)
        {
            if (e.Index <= Cols.HeaderCount)
            {
                e.Size = RowHeaderWidth;
            }

            if (e.Index > Cols.HeaderCount)
            {
                e.Size = CellWidth;
                e.Handled = true;
            }
        }

        private void gridSkillDataQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex < 0 || e.RowIndex < 0) return;
            if (_gridRows == null) return;
            if (e.ColIndex == 0 && e.RowIndex == 1)
                e.Style.CellValue = " ";

            if(e.RowIndex < _gridRows.Count)
                _gridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style,e.ColIndex,e.RowIndex));

            if (e.ColIndex > 0)
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;

            if (e.RowIndex == 1 && e.ColIndex > 0)
            {
                if(e.Style.Tag is DateTime)
                    WeekendOrWeekday(e, (DateTime)e.Style.Tag, false, true);
            }
            
            e.Handled = true;
        }

        private IChartSeriesSetting configureSetting(string key)
        {
            IChartSeriesSetting ret = _chartSettings.DefinedSetting(key, new ChartSettingsManager().ChartSettingsDefault);
            ret.Enabled = _chartSettings.SelectedRows.Contains(key);
            return ret;
        }

        private void createGridRows(ISkill skill,IList<DateOnly> dates, ISchedulerStateHolder schedulerStateHolder)
        {
            ((NumericReadOnlyCellModel)CellModels["NumericReadOnlyCell"]).NumberOfDecimals = 2;
            DateOnly baseDate;

            _gridRows = new List<IGridRow>();
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.WeekDates, dates));
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.MonthDayNumber, dates));

            if (dates.Count > 0)
                baseDate = dates.First();
            else
                baseDate = DateOnly.MinValue;

            _rowManager = new RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime, IList<ISkillStaffPeriod>>>(this, new List<IntervalDefinition>(), 15, schedulerStateHolder);
            _rowManager.BaseDate = baseDate;

        	SkillDayGridRow gridRow;
			if (skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
			{
				gridRow = new SkillDayGridRowMaxSeatsIssues(_rowManager, "NumericReadOnlyCell", "MaxUsedSeats", UserTexts.Resources.MaxUsedSeats);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));
			}

			if (skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
			{
				gridRow = new SkillDayGridRow(_rowManager, "TimeCell", "ForecastedHours", UserTexts.Resources.ForecastedHours);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

                if(!skill.IsVirtual)
                {
                    gridRow = new SkillDayGridRowMinMaxIssues(_rowManager, "TimeCell", "ScheduledHours", UserTexts.Resources.ScheduledHours);
                    gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                    _gridRows.Add(_rowManager.AddRow(gridRow));
                }
                else
                {
                    gridRow = new SkillDayGridRowScheduleHoursSummary(_rowManager, "TimeCell", "ScheduledHours",
                                                                     UserTexts.Resources.ScheduledHours, skill);
                    gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                    _gridRows.Add(_rowManager.AddRow(gridRow));
                }

				gridRow = new SkillDayGridRow(_rowManager, "TimeSpanCell", "AbsoluteDifference", UserTexts.Resources.AbsoluteDifference);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

                if (!skill.IsVirtual)
                {
                    gridRow = new SkillDayGridRowStaffingIssues(_rowManager, "ReadOnlyPercentCell", "RelativeDifference",
                                                                UserTexts.Resources.RelativeDifference, skill);
                    gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                    _gridRows.Add(_rowManager.AddRow(gridRow));
                }
                else
                {
                    gridRow = new SkillDayGridRowStaffingIssuesSummary(_rowManager, "ReadOnlyPercentCell", "RelativeDifference",
                                                                UserTexts.Resources.RelativeDifference);
                    gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                    _gridRows.Add(_rowManager.AddRow(gridRow));
                }

			    gridRow = new SkillDayGridRowBoostedRelativeDifference(_rowManager, "NumericReadOnlyCell", "RelativeBoostedDifference", UserTexts.Resources.AdjustedDifference, skill);
                gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                _gridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillDayGridRow(_rowManager, "NumericReadOnlyCell", "RootMeanSquare", UserTexts.Resources.RMS);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillDayGridRow(_rowManager, "NumericReadOnlyCell", "DailySmoothness", UserTexts.Resources.StandardDeviation);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillDayGridRow(_rowManager, "NumericReadOnlyCell", "HighestDeviationInPeriod", UserTexts.Resources.HighestStdev);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillDayGridRow(_rowManager, "PercentCellModel", "EstimatedServiceLevel", UserTexts.Resources.ESL);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));
			}
           

            if (skill.SkillType.ForecastSource == ForecastSource.Email || skill.SkillType.ForecastSource == ForecastSource.Backoffice || skill.SkillType.ForecastSource == ForecastSource.Time)
            {
                gridRow = new SkillDayGridRow(_rowManager, "TimeCell", "ForecastedHoursIncoming", UserTexts.Resources.ForecastedHoursIncoming);
                gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                _gridRows.Add(_rowManager.AddRow(gridRow));

                gridRow = new SkillDayGridRow(_rowManager, "TimeCell", "ScheduledHoursIncoming", UserTexts.Resources.ScheduledHoursIncoming);
                gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                _gridRows.Add(_rowManager.AddRow(gridRow));

                gridRow = new SkillDayGridRow(_rowManager, "TimeSpanCell", "AbsoluteIncomingDifference", UserTexts.Resources.AbsoluteDifferenceIncoming);
                gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                _gridRows.Add(_rowManager.AddRow(gridRow));

                gridRow = new SkillDayGridRowStaffingIssues(_rowManager, "ReadOnlyPercentCell", "RelativeIncomingDifference", UserTexts.Resources.RelativeDifferenceIncoming, skill);
                gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
                _gridRows.Add(_rowManager.AddRow(gridRow));
            }

            this.Rows.HeaderCount = 1;
        }

        public void DrawDayGrid(ISchedulerStateHolder stateHolder,ISkill skill)
        {
           
            if (stateHolder == null || skill==null) return;

            using (PerformanceOutput.ForOperation("DrawSkillDayGrid"))
            {
                Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation |
                                               GridMergeCellsMode.MergeColumnsInRow;

                var dateTimePeriods = stateHolder.RequestedPeriod.WholeDayCollection();
                _dates = dateTimePeriods.Select(d => new DateOnly(TimeZoneHelper.ConvertFromUtc(d.StartDateTime, stateHolder.TimeZoneInfo))).ToList();
                createGridRows(skill, _dates, stateHolder);

                // in ViewPoint
                var timeZone = stateHolder.TimeZoneInfo;
                // user timeZone
                var userTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
                var diff = userTimeZone.BaseUtcOffset - timeZone.BaseUtcOffset;
                var viewPointPeriod = stateHolder.RequestedPeriod.MovePeriod(diff);

                dateTimePeriods = viewPointPeriod.WholeDayCollection();
                var dataSource = createDataSourceDictionary(dateTimePeriods, stateHolder, skill);

                _rowManager.SetDataSource(new List<IDictionary<DateTime, IList<ISkillStaffPeriod>>> {dataSource});
                ColCount = _dates.Count;
                RowCount = _gridRows.Count - 1;
                ColWidths[0] = RowHeaderWidth;

                Model.MergeCells.DelayMergeCells(GridRangeInfo.Table());
            }
        }

        private static IDictionary<DateTime, IList<ISkillStaffPeriod>> createDataSourceDictionary(IEnumerable<DateTimePeriod> dateTimePeriods, ISchedulerStateHolder stateHolder, ISkill skill)
        {
            ISkillStaffPeriodDictionary skillStaffPeriods;
            IAggregateSkill aggregateSkillSkill = skill;
            if (!aggregateSkillSkill.IsVirtual)
            {
                if (
                    !stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.
                         TryGetValue(skill, out skillStaffPeriods))
                    skillStaffPeriods = new SkillStaffPeriodDictionary(skill);
            }
            else
            {
                skillStaffPeriods = new SkillStaffPeriodDictionary(skill);
                foreach (DateTimePeriod period in dateTimePeriods)
                {
                    ISkillStaffPeriodDictionary temp = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill,
                                                                                                  period,
                                                                                                  true);
                    foreach (KeyValuePair<DateTimePeriod, ISkillStaffPeriod> keyValuePair in temp)
                    {
                        skillStaffPeriods.Add(keyValuePair);
                    }
                }
            }

            IDictionary<DateTime, IList<ISkillStaffPeriod>> returnDictionary = new Dictionary<DateTime, IList<ISkillStaffPeriod>>();
            var sortedSkillStaffPeriods = skillStaffPeriods.OrderBy(k => k.Key.StartDateTime).ToList();
            int count = sortedSkillStaffPeriods.Count;
            
            foreach (DateTimePeriod period in dateTimePeriods)
            {
                int currentIndex = 0;
                var skillStaffPeriodList = new List<ISkillStaffPeriod>();
                while (currentIndex<count)
                {
                    if (sortedSkillStaffPeriods[currentIndex].Key.Intersect(period))
                        skillStaffPeriodList.Add(sortedSkillStaffPeriods[currentIndex].Value);
                    currentIndex++;
                }
                returnDictionary.Add(period.StartDateTime,skillStaffPeriodList);
            }

            return returnDictionary;
        }

        public void WeekendOrWeekday(GridQueryCellInfoEventArgs e, DateTime date, bool backColor, bool textColor)
        {
            if (DateHelper.IsWeekend(date, CultureInfo.CurrentCulture))
            {
                if (backColor)
                    e.Style.BackColor = ColorHolidayCell;
                if (textColor)
                    e.Style.TextColor = ColorHolidayHeader;
            }
            else
            {
                if(backColor)
                    e.Style.BackColor = ColorCells;
                   
                e.Style.TextColor = ForeColor;
            }
        }

        //refresh grid
        public void RefreshGrid()
        {
            using (PerformanceOutput.ForOperation("Refreshing SkillDayGridControl"))
            {
                Refresh();
            }
        }

        //refresh grid on selected dates
        public void RefreshGrid(IList<DateOnly> localDates)
        {
            if (_rowManager==null || _rowManager.DataSource==null || _rowManager.DataSource.Count == 0) return;

            using (PerformanceOutput.ForOperation("Refreshing SkillDayGridControl on dates"))
            {
                foreach(DateOnly date in localDates)
                {
                    RefreshRange(GridRangeInfo.Col(getColumnIndexFromDate(date)), true); 
                }
            }
        }

        //get colIndex from a date
        private int getColumnIndexFromDate(DateTime localDate)
        {
            for (int i = 0; i < ColCount; i++)
            {
                if (_dates[i] == TimeZoneHelper.ConvertToUtc(localDate))
                    return i + 1;
            }
            
            return -1;
        }

        public AbstractDetailView Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public void GoToDate(DateTime theDate)
        {
            RefreshGrid();
        }

        public void SetRowVisibility(string key, bool enabled)
        {
            if(enabled)
                _chartSettings.SelectedRows.Add(key);
            else
            {
                _chartSettings.SelectedRows.Remove(key);
            }  
        }

        public DateTime GetLocalCurrentDate(int column)
        {
            throw new NotImplementedException();
        }

        public IList<GridRow> EnabledChartGridRowsMicke65()
        {
            IList<GridRow> ret = new List<GridRow>();
            foreach (string key in _chartSettings.SelectedRows)
            {
                foreach (GridRow gridRow in _gridRows.OfType<GridRow>())
                {
                    if(gridRow.DisplayMember == key)
                        ret.Add(gridRow);
                }
            }

            return ret;
        }

        public IDictionary<int, GridRow> EnabledChartGridRows
        {
            get
            {

                if (_gridRows == null) 
                    return new Dictionary<int, GridRow>();

                IDictionary<int, GridRow> settings = (from r in _gridRows.OfType<GridRow>()
                                                      where r.ChartSeriesSettings != null &&
                                                            r.ChartSeriesSettings.Enabled
                                                      select r).ToDictionary(k => _gridRows.IndexOf(k), v => v);

                return settings;
            }
        }

        public void SaveSetting()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                new PersonalSettingDataRepository(uow).PersistSettingValue(_chartSettings);
                uow.PersistAll();
            }
        }

        public ReadOnlyCollection<GridRow> AllGridRows
        {
            get
            {
                if (_gridRows == null) return new ReadOnlyCollection<GridRow>(new List<GridRow>());
                return new ReadOnlyCollection<GridRow>(new List<GridRow>(_gridRows.OfType<GridRow>()));
            }
        }

        public int MainHeaderRow
        {
            get { return 1; }
        }

        public bool HasColumns
        {
            get { return _dates.Count > 0; }
        }

        public GridRow CurrentSelectedGridRow
        {
            get { return _currentSelectedGridRow; }
        }

        protected override void OnSelectionChanged(GridSelectionChangedEventArgs e)
        {
            if (e.Range.Top > 1)
            {
                GridRow gridRow = _gridRows[e.Range.Top] as GridRow;

                if (gridRow != null)
                {
                    _currentSelectedGridRow = gridRow;
                }
            }
            base.OnSelectionChanged(e);
        }

        #region IHelpContext Members

        public bool HasHelp
        {
            get { return true; }
        }

        public string HelpId
        {
            get { return Name; }
        }

        #endregion
    }
}
