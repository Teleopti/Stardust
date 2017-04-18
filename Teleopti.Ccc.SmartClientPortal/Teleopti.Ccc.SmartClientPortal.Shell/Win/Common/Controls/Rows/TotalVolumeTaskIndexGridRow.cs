using System;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    class TotalVolumeTaskIndexGridRow : GridRow
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly RowManager<TotalVolumeTaskIndexGridRow, TotalDayItem> _rowManager;
        private readonly bool _isChartRow;

        public TotalVolumeTaskIndexGridRow(RowManager<TotalVolumeTaskIndexGridRow, TotalDayItem> rowManager, string cellType,
                                           string displayMember, string rowHeaderText, bool isChartRow) 
            : base(cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
            _isChartRow = isChartRow;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is to be shown in chart.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is chart row; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-12-08
        /// </remarks>
        public bool IsChartRow
        {
            get { return _isChartRow; }
        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            if (cellInfo.ColIndex == 0)
            {
                cellInfo.Style.CellValue = RowHeaderText;
            }
            else
            {
                if (_rowManager.DataSource.Count == 0) return;
                if ((Math.Max(cellInfo.RowHeaderCount, cellInfo.ColIndex) - cellInfo.RowHeaderCount)>=_rowManager.DataSource.Count)
                    return;

                TotalDayItem totalDayItem = GetObjectAtPosition(_rowManager, cellInfo.ColIndex,cellInfo.RowHeaderCount);
                cellInfo.Style.CellType = CellType;
                cellInfo.Style.CellValue = GetValue(totalDayItem);
                if (totalDayItem.WorkloadDayIsClosed)
                    cellInfo.Style.ReadOnly = true;
            }
        }

        private object GetValue(TotalDayItem totalDayItem)
        {
            return _propertyReflector.GetValue(totalDayItem, DisplayMember);
        }

        private void SetValue(TotalDayItem totalDayItem, object value)
        {
            if (!totalDayItem.WorkloadDayIsClosed)
                _propertyReflector.SetValue(totalDayItem, DisplayMember, value);
        }

        public override void SaveCellInfo(CellInfo cellInfo)
        {
            if (_rowManager.DataSource.Count == 0) return;

            TotalDayItem totalDayItem = GetObjectAtPosition(_rowManager, cellInfo.ColIndex, cellInfo.RowHeaderCount);
            SetValue(totalDayItem, cellInfo.Style.CellValue);
        }
    }
}