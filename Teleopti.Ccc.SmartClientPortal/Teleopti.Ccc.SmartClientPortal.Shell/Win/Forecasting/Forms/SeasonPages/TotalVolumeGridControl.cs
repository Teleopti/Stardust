using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Forecasting.Forms;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SeasonPages
{
    public class TotalVolumeGridControl : TeleoptiGridControl
    {
        private IList<IGridRow> _gridRows;
        private IList<DateOnly> _dateTimes;
        private readonly RowManager<TotalVolumeTaskIndexGridRow, TotalDayItem> _rowManagerTotalDayItem;
        private TotalVolume _totalVolume;
        private IDictionary<DateOnly, IOutlier> _outliers = new Dictionary<DateOnly, IOutlier>();
        private readonly ISkillType _skillType;
        private bool _pasting;
        private IList<double> _modifiedItems;

        public TotalVolumeGridControl(TotalVolume totalVolume, ISkillType skillType)
        {
            _skillType = skillType;
            _totalVolume = totalVolume;
            _rowManagerTotalDayItem = new RowManager<TotalVolumeTaskIndexGridRow, TotalDayItem>(this, null, -1);
            CreateContextMenu();
			TeleoptiStyling = true;
        }

        public void CreateContextMenu()
        {
            var gridItemModify = new MenuItem(Resources.ModifySelection, ModifySelectionOnClick);
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

        public void SetOutliers(IDictionary<DateOnly, IOutlier> outliers)
        {
            _outliers = outliers;
        }

        #region InitializeGrid

        private void initializeGrid()
        {
            Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow;
            Cols.Size[0] = ColorHelper.GridHeaderColumnWidth();// 100;
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

        protected override void OnResizingColumns(GridResizingColumnsEventArgs e)
        {
            base.OnResizingColumns(e);
            if (e.Reason == GridResizeCellsReason.DoubleClick)
            {
                this.ColWidths.ResizeToFit(Selections.Ranges[0],GridResizeToFitOptions.IncludeCellsWithinCoveredRange);
                e.Cancel = true;
            }
        }

        #endregion

        /// <summary>
        /// Determines whether the chart row is to be shown in grid.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <returns>
        /// 	<c>true</c> if [is chart row] [the specified row index]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-12-08
        /// </remarks>
        protected override bool IsChartRow(int rowIndex)
        {
            TotalVolumeTaskIndexGridRow row = (TotalVolumeTaskIndexGridRow)_gridRows[rowIndex];
            return row.IsChartRow;
        }
        public void UpdateTotalVolumeDayItems()
        {
            _dateTimes = _totalVolume.WorkloadDayCollection.Select(v => v.CurrentDate).ToList();
            createGridRows();
         
            _rowManagerTotalDayItem.SetDataSource(_totalVolume.TotalDayItemCollection);
            initializeGrid();
            InitializeAllGridRowsToChart();
            
            //this.ResizeToFit();
        }

        public void UpdateTotalVolume(TotalVolume totalVolume)
        {
            _totalVolume = totalVolume;
            UpdateTotalVolumeDayItems();
            
            Refresh();
        }

        private void createGridRows()
        {
            _gridRows = new List<IGridRow>();
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.WeekDates, _dateTimes));
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.MonthDayNumber, _dateTimes));

            if (_skillType.ForecastSource != ForecastSource.InboundTelephony)
            {
                TextManager manager = new TextManager(_skillType);
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                "NumericCell", "TaskIndex", manager.WordDictionary["TaskIndex"], false)));
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                    "NumericCell", "Tasks", manager.WordDictionary["TotalTasks"], true)));
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                    "NumericCell", "TalkTimeIndex", manager.WordDictionary["TalkTimeIndex"], false)));
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                    "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel", "TalkTime", manager.WordDictionary["TotalAverageTaskTime"], true)));
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                    "NumericCell", "AfterTalkTimeIndex", manager.WordDictionary["AfterTalkTimeIndex"], false)));
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                    "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel", "AfterTalkTime", manager.WordDictionary["TotalAverageAfterTaskTime"], true)));
            }
            else
            {
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                "NumericCell", "TaskIndex", UserTexts.Resources.IndexCalls, false)));
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                    "NumericCell", "Tasks", UserTexts.Resources.TotalCalls, true)));
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                    "NumericCell", "TalkTimeIndex", UserTexts.Resources.IndexTalkTime, false)));
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                    "PositiveTimeSpanTotalSecondsCell", "TalkTime", UserTexts.Resources.TalkTime, true)));
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                    "NumericCell", "AfterTalkTimeIndex", UserTexts.Resources.IndexACW, false)));
                _gridRows.Add(_rowManagerTotalDayItem.AddRow(new TotalVolumeTaskIndexGridRow(_rowManagerTotalDayItem,
                    "PositiveTimeSpanTotalSecondsCell", "AfterTalkTime", UserTexts.Resources.ACW, true)));
            }
            
        }

        #region DrawValuesInGrid

        protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            base.OnQueryCellInfo(e);

            if (e.ColIndex < 0 || e.RowIndex < 0) return;
            
            _gridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));
            if (e.ColIndex > 0 && e.ColIndex <= _dateTimes.Count)
            {
                formatCell(e.Style, _dateTimes[e.ColIndex - RowHeaderCount]);
            }

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
                if (IsChartRow(e.RowIndex))
                    TriggerDataToChart(GridRangeInfo.Cells(e.RowIndex, RowHeaderCount, e.RowIndex, ColCount));
            }

            e.Handled = true;
        }

        #endregion

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
                    cellValue = ((TimeSpan) cellValueAsObject).TotalSeconds;
                    //if (cellValue >= 120)
                    if (_skillType.DisplayTimeSpanAsMinutes)
                        cellValue = cellValue / 60;
                }
                else if (cellValueAsObject is Percent)
                    cellValue = ((Percent)cellValueAsObject).Value * 100d;
                else
                    cellValue = (double)cellValueAsObject;

                keyValueCollection.Add(_dateTimes[i - RowHeaderCount].Date, cellValue);
            }

            return keyValueCollection;
        }

        public override WorkingInterval WorkingInterval
        {
            get { return WorkingInterval.Day; }
        }

        public override TimeSpan ChartResolution
        {
            get
            {
                int days;
                TimeSpan diff = LastDate.Subtract(FirstDate);
                if ((int)diff.TotalDays / 20 > 5)
                    days = (int)diff.TotalDays / 20;
                else
                    days = 1;
                return TimeSpan.FromDays(days);
            }
        }

        public override DateTime FirstDate
        {
            get { return _dateTimes.Min().Date; }
        }

        public override DateTime LastDate
        {
            get { return _dateTimes.Max().Date; }
        }

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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_gridRows!=null) _gridRows.Clear();
                if (_rowManagerTotalDayItem != null)
                {
                    _rowManagerTotalDayItem.DataSource.Clear();
                    _rowManagerTotalDayItem.Rows.Clear();
                }
            }
        }
    }
}
