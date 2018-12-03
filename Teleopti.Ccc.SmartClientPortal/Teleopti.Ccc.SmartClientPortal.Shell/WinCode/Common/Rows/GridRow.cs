using System;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows
{
    public class GridRow : IGridRow
    {
        private readonly string _cellType;
        private readonly string _displayMember;
        private readonly string _rowHeaderText;
        private IChartSeriesSetting _chartSeriesSettings;

        public GridRow(string cellType, string displayMember, string rowHeaderText)
        {
            _cellType = cellType;
            _displayMember = displayMember;
            _rowHeaderText = rowHeaderText;
        }

        public string RowHeaderText
        {
            get { return _rowHeaderText; }
        }

        public string CellType
        {
            get { return _cellType; }
        }

        public string DisplayMember
        {
            get { return _displayMember; }
        }

        public IChartSeriesSetting ChartSeriesSettings
        {
            get { return _chartSeriesSettings; }
            set { _chartSeriesSettings = value; }
        }

        protected virtual bool AllowMerge
        {
            get { return true; }
        }

        #region IGridRow Members

        public virtual void QueryCellInfo(CellInfo cellInfo)
        {
            if (cellInfo.ColIndex == 0)
            {
                cellInfo.Style.CellValue = RowHeaderText;
            	cellInfo.Style.ReadOnly = true;
            }
        }

        public virtual void SaveCellInfo(CellInfo cellInfo)
        {
        }

        public virtual void OnSelectionChanged(GridSelectionChangedEventArgs e, int rowHeaders)
        {
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Gets the col span.
        /// </summary>
        /// <param name="rowManager">The row manager.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-08
        /// </remarks>
        public static int GetColSpan(IRowManager rowManager, DateTimePeriod period)
        {
            int theIntervalMinutes = (int)period.ElapsedTime().TotalMinutes;
            if (theIntervalMinutes <= rowManager.IntervalLength) return 1;
            int colSpan = theIntervalMinutes / rowManager.IntervalLength;
            return Math.Min(colSpan, rowManager.Grid.ColCount);
        }

        /// <summary>
        /// Gets the start position.
        /// </summary>
        /// <param name="rowManager">The row manager.</param>
        /// <param name="period">The period.</param>
        /// <param name="headerCols">The header cols.</param>
        /// <param name="visibleColSpan">The visible col span.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-08
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "headerCols-1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        public static int GetStartPosition(IRowManager rowManager, DateTimePeriod period, int headerCols, ref int visibleColSpan)
        {
            var item = rowManager.Intervals.FirstOrDefault(i => i.DateTime==period.StartDateTime);
            int index = Math.Max(rowManager.Intervals.IndexOf(item), 0);

            if (index + visibleColSpan + headerCols > rowManager.Grid.ColCount)
                visibleColSpan = rowManager.Grid.ColCount - index - (headerCols - 1);

            return index + headerCols;
        }

        public static TSource GetObjectAtPosition<TRow, TSource>(IRowManager<TRow, TSource> rowManager, int columnIndex,
                                                                 int rowHeaders) 
            where TRow : IGridRow
        {
            return rowManager.DataSource[Math.Max(rowHeaders, columnIndex) - rowHeaders];
        }

        public TSource GetObjectAtPositionForInterval<TRow, TSource>(IRowManager<TRow, TSource> rowManager, int columnIndex,
                                                                 int rowHeaders)
            where TRow : IGridRow
            where TSource : IPeriodized
        {
            var dateTimeToFind = rowManager.Intervals[Math.Max(rowHeaders, columnIndex) - rowHeaders].DateTime;

            TSource objectPeriod;
            if (AllowMerge)
            {
                objectPeriod = (from t in rowManager.DataSource
                                where t.Period.StartDateTime <= dateTimeToFind
                                orderby t.Period.StartDateTime descending
                                select t).FirstOrDefault();
            }
            else
            {
                objectPeriod = (from t in rowManager.DataSource
                                where t.Period.StartDateTime == dateTimeToFind
                                select t).FirstOrDefault();
            }

            return objectPeriod;
        }

        public static void VerticalRowHeaderSettings(CellInfo cellInfo, string parentRowHeaderText)
        {
            cellInfo.Style.MergeCell = GridMergeCellDirection.RowsInColumn;
            cellInfo.Style.Font.Orientation = 270;
            cellInfo.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            cellInfo.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
            cellInfo.Style.CellValue = parentRowHeaderText;
        }
        #endregion

    }
}