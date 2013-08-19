using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class TimeOfDayColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _groupHeaderText;
        private string _bindingProperty;
        private int _preferredWidth = 150;

        public TimeOfDayColumn(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;

            if (!string.IsNullOrEmpty(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator))
                _preferredWidth += 12;
        }

        public TimeOfDayColumn(string bindingProperty, string headerText, string groupHeaderText)
            : this(bindingProperty, headerText)
        {
            _groupHeaderText = groupHeaderText;
        }

        public override int PreferredWidth
        {
            get { return _preferredWidth; }
        }

        public override string BindingProperty
        {
            get
            {
                return _bindingProperty;
            }
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

        #region Set Up Headers

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
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
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
                object cellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                e.Style.CellType = "TimeSpanTimeOfDayCellModel";
                if (cellValue == null)
                    e.Style.CellType = "Static";
                else
                    e.Style.ResetBackColor();
                e.Style.CellValue = cellValue;
                InvokeValidate(dataItem, e.Style, e.RowIndex, false);
            }
        }

        #endregion

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
                _propertyReflector.SetValue(dataItem, _bindingProperty, cellValue);
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