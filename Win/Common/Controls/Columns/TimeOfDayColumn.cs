using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class TimeOfDayColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        private readonly string _groupHeaderText;
        
        public TimeOfDayColumn(string bindingProperty, string headerText, string groupHeaderText)
            : base(bindingProperty, 80)
        {
            _headerText = headerText;
            _groupHeaderText = groupHeaderText;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                SetUpSingleHeader(e, dataItems);
            }
            else
            {
                SetUpMultipleHeaders(e, dataItems);
            }
            e.Handled = true;
        }

        private void SetUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
                e.Style.CellType = "TimeSpanTimeOfDayCellModel";
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
                InvokeValidate(dataItem, e.Style, e.RowIndex, false);
            }
        }

        private void SetUpMultipleHeaders(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0)
            {
                e.Style.CellValue = _groupHeaderText;
            }
            else if (e.RowIndex == 1 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
                e.Style.BackColor = System.Drawing.Color.Gray;
                if (dataItems.Count == 0)
                    return;

                T dataItem = dataItems[e.RowIndex - 2];
                object cellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
                e.Style.CellType = "TimeSpanTimeOfDayCellModel";
                if (cellValue == null)
                    e.Style.CellType = "Static";
                else
                    e.Style.ResetBackColor();
                e.Style.CellValue = cellValue;
                InvokeValidate(dataItem, e.Style, e.RowIndex, false);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.TimeSpan.TryParse(System.String,System.TimeSpan@)")]
        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            int rowIndex = 0;
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                if (e.ColIndex > 0 && e.RowIndex > 0)
                {
                    rowIndex = e.RowIndex - 1;
                }
            }
            else
            {
                if (e.ColIndex > 0 && e.RowIndex > 1)
                {
                    rowIndex = e.RowIndex - 2;
                }
            }
            T dataItem = dataItems[rowIndex];
            if (e.Style.CellValue != null)
            {
                TimeSpan cellValue;
                if (e.Style.CellValue is TimeSpan)
                {
                    cellValue = (TimeSpan) e.Style.CellValue;
                }
                else
                {
                    TimeSpan.TryParse(e.Style.CellValue.ToString(), out cellValue);
                }
                _propertyReflector.SetValue(dataItem, BindingProperty, cellValue);
            }

            InvokeValidate(dataItem, e.Style, e.RowIndex, true);

            OnCellChanged(dataItem);
            e.Handled = true;

        }

        private void InvokeValidate(T dataItem, GridStyleInfo style, int rowIndex, bool inSaveMode)
        {
        	var handler = Validate;
            if (handler != null)
            {
                handler.Invoke(dataItem, style, rowIndex, inSaveMode);
            }
        }
    }
}