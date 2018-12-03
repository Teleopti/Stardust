using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class DateHeaderGridRow:IGridRow
    {
        private readonly IList<DateOnly> _dateTimes;
        private readonly DateHeaderType _dateHeaderType;

        public DateHeaderGridRow(DateHeaderType dateHeaderType, IList<DateOnly> dateTimes)
        {
            _dateHeaderType = dateHeaderType;
            _dateTimes = dateTimes;
        }

        private DateOnly GetCurrentDateTime(int columnIndex, int colHeaders)
        {
            DateOnly dateTimeToReturn;
            columnIndex -= colHeaders;
            if (columnIndex < _dateTimes.Count)
            {
                dateTimeToReturn = _dateTimes[columnIndex];
            }
            else
            {
                dateTimeToReturn = _dateTimes.Last();
            }
            return dateTimeToReturn;
        }

        public void QueryCellInfo(CellInfo cellInfo)
        {
            if (cellInfo.Style.GetGridModel() == null)
                return;

            if (cellInfo.ColIndex < cellInfo.RowHeaderCount) return;

            cellInfo.Style.BaseStyle = "Header";
            var dateTime = GetCurrentDateTime(cellInfo.ColIndex, cellInfo.RowHeaderCount).Date;
            switch (_dateHeaderType)
            {
                case DateHeaderType.Date:
                    cellInfo.Style.CellValue = dateTime.ToShortDateString();
                    break;
                case DateHeaderType.MonthName:
					cellInfo.Style.CellValue = DateHelper.GetMonthName(dateTime, CultureInfo.CurrentUICulture).Capitalize();
                    break;
                case DateHeaderType.MonthNameYear:
                    cellInfo.Style.CellValue = string.Concat(
						DateHelper.GetMonthName(dateTime, CultureInfo.CurrentUICulture).Capitalize(), " ",
                        CultureInfo.CurrentCulture.Calendar.GetYear(dateTime).ToString(CultureInfo.CurrentUICulture));
                    break;
                case DateHeaderType.WeekDates:
                    cellInfo.Style.WrapText = false;
                    dateTime = DateHelper.GetFirstDateInWeek(dateTime, CultureInfo.CurrentCulture);
		            cellInfo.Style.CellValue = string.Format(UserTexts.Resources.WeekAbbreviationDot,
			            DateHelper.WeekNumber(dateTime, CultureInfo.CurrentCulture),
			            dateTime.ToShortDateString());
                    break;
                case DateHeaderType.WeekdayName:
					cellInfo.Style.CellValue = CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(dateTime.DayOfWeek).Capitalize();
                    break;
                case DateHeaderType.MonthDayNumber:
                    cellInfo.Style.CellValue = CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(dateTime);
					cellInfo.Style.CellTipText = CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(dateTime.DayOfWeek).Capitalize() +
                                          " " + Environment.NewLine + " " + dateTime.ToShortDateString();
                    cellInfo.Style.Tag = dateTime;
                    break;
                case DateHeaderType.WeekNumber:
                    cellInfo.Style.CellValue = DateHelper.WeekNumber(dateTime, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentUICulture);
                    break;
                case DateHeaderType.Year:
                    cellInfo.Style.CellValue = CultureInfo.CurrentCulture.Calendar.GetYear(dateTime).ToString(CultureInfo.CurrentUICulture);
                    break;
				case DateHeaderType.Period:
            		cellInfo.Style.CellValue = string.Concat(_dateTimes.First().ToShortDateString(CultureInfo.CurrentCulture), " - ", _dateTimes.Last().ToShortDateString(CultureInfo.CurrentCulture));
            		break;
            }
            cellInfo.Style.MergeCell = GridMergeCellDirection.ColumnsInRow;
        }

        public void SaveCellInfo(CellInfo cellInfo)
        {
        }

        public void OnSelectionChanged(GridSelectionChangedEventArgs e, int rowHeaders)
        {
        }
    }
}