using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.Win.Forecasting.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls
{
    public class ValidatedVolumeDayGridControl : TeleoptiGridControl
    {
        private IList<IGridRow> _gridRows;
        private IList<DateOnly> _dateTimes;
        private readonly RowManager<ValidatedVolumeDayGridRow, IValidatedVolumeDay> _rowManagerValidatedVolumeDay;
        private readonly RowManager<DayOfWeeksGridRow, DayOfWeeks> _rowManagerDayOfWeeks;
        private Percent _deviationTasks = new Percent(0.1d); //TODO: Get from settings?
        private Percent _deviationTaskTime = new Percent(0.1d); //TODO: Get from settings?
        private Percent _deviationAfterTaskTime = new Percent(0.1d); //TODO: Get from settings?
        private IDictionary<DateOnly, IOutlier> _outliers = new Dictionary<DateOnly, IOutlier>();
        private bool _pasting;
        private readonly ISkillType _skillType;
        private IList<double> _modifiedItems;

        public Percent DeviationTasks
        {
            get { return _deviationTasks; }
            set { _deviationTasks = value; }
        }

        public Percent DeviationTaskTime
        {
            get { return _deviationTaskTime; }
            set { _deviationTaskTime = value; }
        }

        public Percent DeviationAfterTaskTime
        {
            get { return _deviationAfterTaskTime; }
            set { _deviationAfterTaskTime = value; }
        }

        public void SetOutliers(IDictionary<DateOnly, IOutlier> outliers)
        {
            _outliers = outliers;
        }

        public ValidatedVolumeDayGridControl(ISkillType skillType)
        {
            _rowManagerValidatedVolumeDay = new RowManager<ValidatedVolumeDayGridRow, IValidatedVolumeDay>(this, null, -1);
            _rowManagerDayOfWeeks = new RowManager<DayOfWeeksGridRow, DayOfWeeks>(this, null, -1);

            _skillType = skillType;
            CreateContextMenu();
			TeleoptiStyling = true;
        }

        public void CreateContextMenu()
        {
            var gridItemModify = new MenuItem(UserTexts.Resources.ModifySelection, ModifySelectionOnClick);
            var menu = new ContextMenu();
            menu.MenuItems.Add(gridItemModify);
            gridItemModify.Enabled = true;
            ContextMenu = menu;
        }

        private void ModifySelectionOnClick(object sender, EventArgs e)
        {
            var modifySelectedList = _modifiedItems;
            var numbers = new ModifyCalculator(modifySelectedList);
            var modifySelection = new ModifySelectionView(numbers);
            if (modifySelection.ShowDialog(this) != DialogResult.OK) return;
            var receivedValues = modifySelection.ModifiedList;
            GridHelper.ModifySelectionInput(this, receivedValues);
        }

        protected override void OnShowContextMenu(Syncfusion.Windows.Forms.ShowContextMenuEventArgs e)
        {
            bool enableMenu;
            _modifiedItems = new List<double>();
            _modifiedItems.Clear();
            GridHelper.ModifySelectionEnabled(this, out _modifiedItems, out enableMenu);
            ContextMenu.MenuItems[0].Enabled = enableMenu;
            base.OnShowContextMenu(e);
        }

        private void createGridRows()
        {
            _gridRows = new List<IGridRow>();
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.WeekDates, _dateTimes));
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.MonthDayNumber, _dateTimes));
            TextManager manager = new TextManager(_skillType);
			if (_skillType.ForecastSource != ForecastSource.InboundTelephony && _skillType.ForecastSource != ForecastSource.Chat)
            {
                _validatedVolumeDayGridRowTasks = new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
                "NumericReadOnlyCell", "OriginalTasks", manager.WordDictionary["OriginalTasks"]);
                _validatedVolumeDayGridRowTasks.QueryCellValue += validatedVolumeDayGridRowTasks_QueryCellValue;
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(_validatedVolumeDayGridRowTasks));
                _gridRows.Add(_rowManagerDayOfWeeks.AddRow(new DayOfWeeksGridRow(_rowManagerDayOfWeeks,
                    "NumericReadOnlyCell", "AverageTasks", manager.WordDictionary["AverageTasks"], _dateTimes)));
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
                    "NumericCell", "ValidatedTasks", manager.WordDictionary["ValidatedTasks"])));
                _validatedVolumeDayGridRowTaskTime = new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
                    "TimeSpanLongHourMinutesStaticCellModel", "OriginalAverageTaskTime", manager.WordDictionary["OriginalAverageTaskTime"]);
                _validatedVolumeDayGridRowTaskTime.QueryCellValue += validatedVolumeDayGridRowTaskTime_QueryCellValue;
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(_validatedVolumeDayGridRowTaskTime));
                _gridRows.Add(_rowManagerDayOfWeeks.AddRow(new DayOfWeeksGridRow(_rowManagerDayOfWeeks,
                    "TimeSpanLongHourMinutesStaticCellModel", "AverageTalkTime", manager.WordDictionary["AverageTalkTime"], _dateTimes)));
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
                    "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel", "ValidatedAverageTaskTime", manager.WordDictionary["ValidatedAverageTaskTime"])));
                _validatedVolumeDayGridRowAfterTaskTime = new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
                    "TimeSpanLongHourMinutesStaticCellModel", "OriginalAverageAfterTaskTime", manager.WordDictionary["OriginalAverageAfterTaskTime"]);
                _validatedVolumeDayGridRowAfterTaskTime.QueryCellValue += validatedVolumeDayGridRowAfterTaskTime_QueryCellValue;
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(_validatedVolumeDayGridRowAfterTaskTime));
                _gridRows.Add(_rowManagerDayOfWeeks.AddRow(new DayOfWeeksGridRow(_rowManagerDayOfWeeks,
                    "TimeSpanLongHourMinutesStaticCellModel", "AverageAfterWorkTime", manager.WordDictionary["AverageAfterWorkTime"], _dateTimes)));
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
                    "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel", "ValidatedAverageAfterTaskTime", manager.WordDictionary["ValidatedAverageAfterTaskTime"])));
            }
            else
            {
                _validatedVolumeDayGridRowTasks = new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
				"NumericReadOnlyCell", "OriginalTasks", manager.WordDictionary["OriginalTasks"]);
                _validatedVolumeDayGridRowTasks.QueryCellValue += validatedVolumeDayGridRowTasks_QueryCellValue;
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(_validatedVolumeDayGridRowTasks));
                _gridRows.Add(_rowManagerDayOfWeeks.AddRow(new DayOfWeeksGridRow(_rowManagerDayOfWeeks,
					"NumericReadOnlyCell", "AverageTasks", manager.WordDictionary["AverageTasks"], _dateTimes)));
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
					"NumericCell", "ValidatedTasks", manager.WordDictionary["ValidatedTasks"])));
                _validatedVolumeDayGridRowTaskTime = new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
					"TimeSpanTotalSecondsReadOnlyCell", "OriginalAverageTaskTime", manager.WordDictionary["OriginalAverageTaskTime"]);
                _validatedVolumeDayGridRowTaskTime.QueryCellValue += validatedVolumeDayGridRowTaskTime_QueryCellValue;
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(_validatedVolumeDayGridRowTaskTime));
                _gridRows.Add(_rowManagerDayOfWeeks.AddRow(new DayOfWeeksGridRow(_rowManagerDayOfWeeks,
					"TimeSpanTotalSecondsReadOnlyCell", "AverageTalkTime", manager.WordDictionary["AverageTalkTime"], _dateTimes)));
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
					"TimeSpanTotalSecondsCell", "ValidatedAverageTaskTime", manager.WordDictionary["ValidatedAverageTaskTime"])));
                _validatedVolumeDayGridRowAfterTaskTime = new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
					"TimeSpanTotalSecondsReadOnlyCell", "OriginalAverageAfterTaskTime", manager.WordDictionary["OriginalAverageAfterTaskTime"]);
                _validatedVolumeDayGridRowAfterTaskTime.QueryCellValue += validatedVolumeDayGridRowAfterTaskTime_QueryCellValue;
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(_validatedVolumeDayGridRowAfterTaskTime));
                _gridRows.Add(_rowManagerDayOfWeeks.AddRow(new DayOfWeeksGridRow(_rowManagerDayOfWeeks,
					"TimeSpanTotalSecondsReadOnlyCell", "AverageAfterWorkTime", manager.WordDictionary["AverageAfterWorkTime"], _dateTimes)));
                _gridRows.Add(_rowManagerValidatedVolumeDay.AddRow(new ValidatedVolumeDayGridRow(_rowManagerValidatedVolumeDay,
					"TimeSpanTotalSecondsCell", "ValidatedAverageAfterTaskTime", manager.WordDictionary["ValidatedAverageAfterTaskTime"])));
            }
        }

        private void validatedVolumeDayGridRowTasks_QueryCellValue(object sender, FromCellEventArgs<ITaskOwner> e)
        {
            IPeriodType periodTypeValue = GetAverageValues(e.Item);
            ValidatedVolumeDay vvd = e.Item as ValidatedVolumeDay;
            if (vvd == null || periodTypeValue == null) return;

            double diff = Math.Abs(vvd.OriginalTasks - periodTypeValue.AverageTasks);
            if (periodTypeValue.AverageTasks > 0d) diff /= periodTypeValue.AverageTasks;
            if (_deviationTasks.Value < diff) e.Style.TextColor = ColorHelper.GridControlGridCellNoTemplate();
        }

        private IPeriodType GetAverageValues(ITaskOwner taskOwner)
        {
            if (_rowManagerDayOfWeeks.DataSource.Count == 0) return null;

            int dayNumber = (int)taskOwner.CurrentDate.DayOfWeek + 1;
            IPeriodType periodTypeValue = _rowManagerDayOfWeeks.DataSource[0].PeriodTypeCollection[dayNumber];
            return periodTypeValue;
        }

        private void validatedVolumeDayGridRowTaskTime_QueryCellValue(object sender, FromCellEventArgs<ITaskOwner> e)
        {
            IPeriodType periodTypeValue = GetAverageValues(e.Item);
            ValidatedVolumeDay vvd = e.Item as ValidatedVolumeDay;
            if (vvd == null || periodTypeValue == null) return;

            double diff = Math.Abs(vvd.OriginalAverageTaskTime.TotalSeconds - periodTypeValue.AverageTalkTime.TotalSeconds);
            if (periodTypeValue.AverageTalkTime.TotalSeconds > 0d) diff /= periodTypeValue.AverageTalkTime.TotalSeconds;
            if (_deviationTaskTime.Value < diff) e.Style.TextColor = ColorHelper.GridControlGridCellNoTemplate();
        }

        private void validatedVolumeDayGridRowAfterTaskTime_QueryCellValue(object sender, FromCellEventArgs<ITaskOwner> e)
        {
            IPeriodType periodTypeValue = GetAverageValues(e.Item);
            ValidatedVolumeDay vvd = e.Item as ValidatedVolumeDay;
            if (vvd == null || periodTypeValue == null) return;

            double diff = Math.Abs(vvd.OriginalAverageAfterTaskTime.TotalSeconds - periodTypeValue.AverageAfterWorkTime.TotalSeconds);
            if (periodTypeValue.AverageAfterWorkTime.TotalSeconds > 0d) diff /= periodTypeValue.AverageAfterWorkTime.TotalSeconds;
            if (_deviationAfterTaskTime.Value < diff) e.Style.TextColor = ColorHelper.GridControlGridCellNoTemplate();
        }

        #region InitializeGrid

        private void initializeGrid()
        {
            Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow;
            Cols.Size[0] = ColorHelper.GridHeaderColumnWidth();
            DefaultColWidth = 50;
            RowCount = _gridRows.Count - 1;
            ColCount = _dateTimes.Count;
            BaseStylesMap["Header"].StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Center;
            ExcelLikeCurrentCell = true;
            Rows.HeaderCount = 1;
            Rows.SetFrozenCount(1, false);
            Model.MergeCells.DelayMergeCells(GridRangeInfo.Table());
            Refresh();
        }

        protected override void OnQueryRowCount(GridRowColCountEventArgs e)
        {
            base.OnQueryRowCount(e);

            e.Count = _gridRows.Count - 1;
            e.Handled = true;
        }

        protected override void OnQueryColCount(GridRowColCountEventArgs e)
        {
            base.OnQueryColCount(e);

            e.Count = _dateTimes.Count;
            e.Handled = true;
        }

        protected override void OnAfterPaste()
        {
            _pasting = false;
            base.OnAfterPaste();
            InitializeAllGridRowsToChart();
        }

        protected override void OnBeforePaste()
        {
            _pasting = true;
            base.OnBeforePaste();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            handleEnableOfContextMenuItems();

            base.OnMouseDown(e);
        }

        protected override void OnResizingColumns(GridResizingColumnsEventArgs e)
        {
            base.OnResizingColumns(e);
            if (e.Reason == GridResizeCellsReason.DoubleClick)
            {
                ColWidths.ResizeToFit(Selections.Ranges[0], GridResizeToFitOptions.IncludeCellsWithinCoveredRange);
                e.Cancel = true;
            }
        }

        private void handleEnableOfContextMenuItems()
        {
            int row = CurrentCell.RowIndex;

            if (row == 4 || row == 7 || row == 10)
            {
                _menuItemUseAverageOnDeviationDays.Enabled = true;
                _menuItemUseAverage.Enabled = true;
                _menuItemModifySelection.Enabled = true;
            }
            else
            {
                _menuItemUseAverageOnDeviationDays.Enabled = false;
                _menuItemUseAverage.Enabled = false;
                _menuItemModifySelection.Enabled = false;
            }
        }

        #endregion

        #region DrawValuesInGrid

        protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            base.OnQueryCellInfo(e);

            if (e.ColIndex < 0 || e.RowIndex < 0) return;
            if (e.ColIndex > _dateTimes.Count) return;

            if (e.ColIndex > 0)
            {
                formatCell(e.Style, _dateTimes[e.ColIndex - RowHeaderCount]);
            }
            _gridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));

            e.Handled = true;
        }

        private void formatCell(GridStyleInfo gridStyleInfo, DateOnly dateTime)
        {
            bool headerCell = (gridStyleInfo.BaseStyle == "Header");
            weekendOrWeekday(gridStyleInfo, dateTime, !headerCell, headerCell);
        }

        #endregion

        #region SetValuesInGrid

        protected override void OnSaveCellInfo(GridSaveCellInfoEventArgs e)
        {
            base.OnSaveCellInfo(e);

            if (e.ColIndex < 0 || e.RowIndex < 0) return;
            _gridRows[e.RowIndex].SaveCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));
            if (!_pasting)
            {
                RefreshRange(GridRangeInfo.Col(e.ColIndex), true);
                TriggerDataToChart(GridRangeInfo.Cells(e.RowIndex, RowHeaderCount, e.RowIndex, ColCount));
            }

            e.Handled = true;
        }

        #endregion

        #region ContextMenu stuff

        private ContextMenu _menuValidationDay;
        private MenuItem _menuItemAddOutlier;
        private MenuItem _menuItemUseAverage;
        private MenuItem _menuItemUseAverageOnDeviationDays;
        private MenuItem _menuItemModifySelection;
        private ValidatedVolumeDayGridRow _validatedVolumeDayGridRowTasks;
        private ValidatedVolumeDayGridRow _validatedVolumeDayGridRowTaskTime;
        private ValidatedVolumeDayGridRow _validatedVolumeDayGridRowAfterTaskTime;

        public event EventHandler<CustomEventArgs<DateOnly>> AddOutlier;
        
        private void InitializeContextMenu()
        {
            _menuValidationDay = new ContextMenu();
            _menuItemAddOutlier = new MenuItem(UserTexts.Resources.AddOutlierThreeDots, menuItemAddOutlier_Click);
            _menuItemAddOutlier.Visible = false;
            _menuValidationDay.MenuItems.Add(_menuItemAddOutlier);

            _menuItemUseAverage = new MenuItem(UserTexts.Resources.UseAverage, menuItemUseAverage_Click);
            _menuValidationDay.MenuItems.Add(_menuItemUseAverage);

            _menuItemUseAverageOnDeviationDays = new MenuItem(UserTexts.Resources.UseAverageOnDeviatingDays, menuItemUseAverageOnDeviationDays_Click);
            _menuValidationDay.MenuItems.Add(_menuItemUseAverageOnDeviationDays);

            _menuItemModifySelection = new MenuItem(UserTexts.Resources.ModifySelection, ModifySelectionOnClick);
            _menuValidationDay.MenuItems.Add(_menuItemModifySelection);

            ContextMenu = _menuValidationDay;
        }

        private void menuItemAddOutlier_Click(object sender, EventArgs e)
        {
        	var handler = AddOutlier;
            if (handler != null)
            {
                var date = _dateTimes[Selections.GetSelectedCols(false,true)[0].Left-1];
                handler.Invoke(this, new CustomEventArgs<DateOnly>(date));
            }
        }

        private void menuItemUseAverage_Click(object sender, EventArgs e)
        {
            applyAverageValues(false);
            Refresh();
        }

        private void menuItemUseAverageOnDeviationDays_Click(object sender, EventArgs e)
        {
            applyAverageValues(true);
            Refresh();
        }

        protected override void OnSelectionChanged(GridSelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            _menuItemAddOutlier.Visible = (e.Range.Left > 0);
        }

        #endregion


        private void applyAverageValues(bool useAverageOnDeviation)
        {
            GridRangeInfoList rangelist;

            // Get the selected ranges
            if (Selections.GetSelectedRanges(out rangelist, true))
            {
                //check if we have any rows or columns selected
                foreach (GridRangeInfo range in rangelist)
                {
                    for (int row = range.Top; row <= range.Bottom; row++)
                    {
                        for (int col = range.Left; col <= range.Right; col++)
                        {
                            if (col > Cols.HeaderCount)
                                setAverage(col - 1, row, useAverageOnDeviation);
                            else
                            {
                                GridRangeInfo gri = ActiveGridView.ScrollableGridRangeInfo;
                                for (int row2 = gri.Top; row2 <= gri.Bottom; row2++)
                                {
                                    for (int col2 = gri.Left - 1; col2 <= gri.Right - 1; col2++)
                                    {
                                        setAverage(col2, row, useAverageOnDeviation);
                                    }
                                }
                            }
                        }
                        //Need to refresh and write the chart here, fix for bug:3515
                        Refresh();
                        TriggerDataToChart(GridRangeInfo.Cells(row, RowHeaderCount, row, ColCount));
                    }
                }
            }
        }

        private void setAverage(int col, int row, bool useAverageOnDeviation)
        {
             IValidatedVolumeDay vd = _rowManagerValidatedVolumeDay.DataSource[col];
            IPeriodType pt = GetAverageValues(vd);

            if (row == 4)
                if (!useAverageOnDeviation)
                    vd.ValidatedTasks = pt.AverageTasks;
                else
                {
                    if (checkTaskDeviation(vd, pt))
                        vd.ValidatedTasks = pt.AverageTasks;
                }
            else if (row == 7)
            {
                if (!useAverageOnDeviation)
                    vd.ValidatedAverageTaskTime = pt.AverageTalkTime;
                else
                {
                    if (checkTalkTimeDeviation(vd, pt))
                        vd.ValidatedAverageTaskTime = pt.AverageTalkTime;
                }
            }
            else if (row == 10)
            {
                if (!useAverageOnDeviation)
                    vd.ValidatedAverageAfterTaskTime = pt.AverageAfterWorkTime;
                else
                {
                    if (checkACWDeviation(vd, pt))
                        vd.ValidatedAverageAfterTaskTime = pt.AverageAfterWorkTime;
                }
            }   
        }

        private bool checkTaskDeviation(IValidatedVolumeDay vvd, IPeriodType periodTypeValue)
        {
            double diff = Math.Abs(vvd.OriginalTasks - periodTypeValue.AverageTasks);
            if (periodTypeValue.AverageTasks > 0d)
                diff /= periodTypeValue.AverageTasks;
            if (_deviationTasks.Value < diff)
                return true;
            
            return false;
        }
        private bool checkTalkTimeDeviation(IValidatedVolumeDay vvd, IPeriodType periodTypeValue)
        {
            double diff = Math.Abs(vvd.OriginalAverageTaskTime.TotalSeconds - periodTypeValue.AverageTalkTime.TotalSeconds);
            if (periodTypeValue.AverageTalkTime.TotalSeconds > 0d) 
                diff /= periodTypeValue.AverageTalkTime.TotalSeconds;
            if (_deviationTaskTime.Value < diff)
                return true;

            return false;
        }
        private bool checkACWDeviation(IValidatedVolumeDay vvd, IPeriodType periodTypeValue)
        {
            double diff = Math.Abs(vvd.OriginalAverageAfterTaskTime.TotalSeconds - periodTypeValue.AverageAfterWorkTime.TotalSeconds);
            if (periodTypeValue.AverageAfterWorkTime.TotalSeconds > 0d) 
                diff /= periodTypeValue.AverageAfterWorkTime.TotalSeconds;

            if (_deviationAfterTaskTime.Value < diff)
                return true;

            return false;
        }

        public void RefreshGrid()
        {
            Refresh();
        }

        /// <summary>
        /// Mark weekend days in grid
        /// </summary>
        /// <param name="gridStyleInfo">The grid style info.</param>
        /// <param name="date">The date.</param>
        /// <param name="backColor">if set to <c>true</c> [back color].</param>
        /// <param name="textColor">if set to <c>true</c> [text color].</param>
        private void weekendOrWeekday(GridStyleInfo gridStyleInfo, DateOnly date, bool backColor, bool textColor)
        {
            if (DateHelper.IsWeekend(date, CultureInfo.CurrentCulture))
            {
                if (backColor)
                    gridStyleInfo.BackColor = ColorHolidayCell;
                if (textColor)
                    gridStyleInfo.TextColor = ColorHolidayHeader;
            }
            else
            {
                if (backColor)
                {
                    gridStyleInfo.BackColor = BackColor;
                }
                gridStyleInfo.TextColor = ForeColor;
            }

            IOutlier outlier;
            if (_outliers.TryGetValue(date,out outlier))
            {
                gridStyleInfo.CellTipText = outlier.Description.Name;
                gridStyleInfo.BackColor = ColorHelper.GridControlOutlierColor();
            }
        }

        public void UpdateHistoricStatistics(DayOfWeeks dayOfWeeks)
        {
            _rowManagerDayOfWeeks.SetDataSource(new List<DayOfWeeks> { dayOfWeeks });
            InitializeAllGridRowsToChart();
            RefreshGrid();
        }

        public void UpdateValidatedVolumeDays(IList<IValidatedVolumeDay> validatedVolumeDays)
        {
            _dateTimes = validatedVolumeDays.Select(v => v.VolumeDayDate).ToList();
            createGridRows();

            _rowManagerValidatedVolumeDay.SetDataSource(validatedVolumeDays);
            initializeGrid();
            InitializeAllGridRowsToChart();
            InitializeContextMenu();
        }

        public override WorkingInterval WorkingInterval
        {
            get { return WorkingInterval.Day; }
        }

        public override TimeSpan ChartResolution
        {
            get { return TimeSpan.FromDays(1); }
        }

        public override DateTime FirstDate
        {
            get
            {
                if (_dateTimes.IsEmpty())
                    return DateTime.Today;

                return _dateTimes.Min().Date;
            }
        }

        public override DateTime LastDate
        {
            get
            {
                if (_dateTimes.IsEmpty())
                    return DateTime.Today;

                return _dateTimes.Max().Date;
            }
        }

        protected override IDictionary<DateTime, double> GetRowDataForChart(GridRangeInfo gridRangeInfo)
        {
            IDictionary<DateTime, double> keyValueCollection = new Dictionary<DateTime, double>();
            for (int i = 0; i <= ColCount; i++)
            {
                GridStyleInfo cell = Model[gridRangeInfo.Top, i];
                if (cell.BaseStyle == "Header") continue;
                object cellValueAsObject = cell.CellValue;
                if (cellValueAsObject is string) continue;
                double cellValue;

                if (cellValueAsObject is int)
                    cellValue = Convert.ToDouble((int)cellValueAsObject);
                else if (cellValueAsObject is TimeSpan)
                {
                    cellValue = ((TimeSpan)cellValueAsObject).TotalSeconds;
                    //if (cellValue >= 120) 
                      if (_skillType.DisplayTimeSpanAsMinutes)
                        cellValue = cellValue / 60;
                }
                else if (cellValueAsObject is Percent)
                    cellValue = ((Percent)cellValueAsObject).Value * 100d;
                else
                    cellValue = (double)cellValueAsObject;

                var key = _dateTimes[i - RowHeaderCount];
                if (!keyValueCollection.ContainsKey(key.Date))
                keyValueCollection.Add(key.Date, cellValue);
            }

            return keyValueCollection;
        }

        protected override bool IsChartRow(int rowIndex)
        {
            return rowIndex != 3 &&
                   rowIndex != 6 &&
                   rowIndex != 9;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_menuValidationDay!=null)
                    _menuValidationDay.Dispose();
                if (_validatedVolumeDayGridRowAfterTaskTime != null)
                    _validatedVolumeDayGridRowAfterTaskTime.QueryCellValue -=
                        validatedVolumeDayGridRowAfterTaskTime_QueryCellValue;
                if (_validatedVolumeDayGridRowTaskTime != null)
                    _validatedVolumeDayGridRowTaskTime.QueryCellValue -=
                        validatedVolumeDayGridRowTaskTime_QueryCellValue;
                if (_validatedVolumeDayGridRowTasks != null)
                    _validatedVolumeDayGridRowTasks.QueryCellValue -= validatedVolumeDayGridRowTasks_QueryCellValue;
                if (_rowManagerDayOfWeeks != null)
                {
                    _rowManagerDayOfWeeks.DataSource.Clear();
                    _rowManagerDayOfWeeks.Rows.Clear();
                }
                if (_rowManagerValidatedVolumeDay!= null)
                {
                    _rowManagerValidatedVolumeDay.DataSource.Clear();
                    _rowManagerValidatedVolumeDay.Rows.Clear();
                }
            }
        }
    }
}
