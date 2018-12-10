using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class DayOfWeeksGridRow : GridRow
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly RowManager<DayOfWeeksGridRow, DayOfWeeks> _rowManager;
        private readonly IList<DateOnly> _dateTimes;

        public DayOfWeeksGridRow(RowManager<DayOfWeeksGridRow, DayOfWeeks> rowManager, string cellType,
                                 string displayMember, string rowHeaderText, IList<DateOnly> dateTimes)
            : base(cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
            _dateTimes = dateTimes;
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

                IPeriodType dayOfWeeks = GetObjectAtPosition(cellInfo.ColIndex);
                cellInfo.Style.CellType = CellType;
                cellInfo.Style.CellValue = GetValue(dayOfWeeks);
            }
        }

        private IPeriodType GetObjectAtPosition(int columnIndex)
        {
            int dayNumber = (int)_dateTimes[columnIndex - 1].DayOfWeek + 1;
            return _rowManager.DataSource[0].PeriodTypeCollection[dayNumber];
        }

        private object GetValue(IPeriodType dayOfWeeks)
        {
            return _propertyReflector.GetValue(dayOfWeeks, DisplayMember);
        }
    }
}