using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    public class EditableHourMinutesColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _groupHeaderText;
        private readonly string _bindingCellTypeTimeOfDay;
        private string _cellTypeLength = "TimeSpanLongHourMinutesCellModelHours";
        private string _cellTypeTimeOfDay = "TimeSpanTimeOfDayCellModel";

        public EditableHourMinutesColumn(string bindingProperty, string headerText) : base(bindingProperty, 150)
        {
            _headerText = headerText;
        }

        public EditableHourMinutesColumn(string bindingProperty, string headerText, string groupHeaderText) : this(bindingProperty,headerText)
        {
            _groupHeaderText = groupHeaderText;
        }

        public EditableHourMinutesColumn(string bindingProperty, string headerText, string groupHeaderText, string bindingCellTypeTimeOfDay = null, string cellTypeLength = null) : this(bindingProperty,headerText,groupHeaderText)
        {
            _bindingCellTypeTimeOfDay = bindingCellTypeTimeOfDay;

            if (!string.IsNullOrEmpty(cellTypeLength))
            {
                _cellTypeLength = cellTypeLength;
            }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                SetUpSingleHeader(e,dataItems);
            }
            else
            {
                SetUpMultipleHeaders(e,dataItems);
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
                if(dataItems.Count == 0) return;

                T dataItem = dataItems[e.RowIndex - 1];
                setCellTypeAndValue(e, dataItem);
            }
        }

        private void setCellTypeAndValue(GridQueryCellInfoEventArgs e, T dataItem)
        {
            e.Style.BackColor = System.Drawing.Color.Gray;
                
            var cellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
            var cellType = _cellTypeLength;
            if (cellTypeIsTimeOfDay(dataItem))
            {
                cellType = _cellTypeTimeOfDay;
            }
            e.Style.CellType = cellType;

            if (cellValue == null)
                e.Style.CellType = "Static";
            else
                e.Style.ResetBackColor();

            e.Style.CellValue = cellValue;
            
            InvokeValidate(dataItem, e.Style, e.RowIndex, false);
        }

        private bool cellTypeIsTimeOfDay(T dataItem)
        {
            if (string.IsNullOrEmpty(_bindingCellTypeTimeOfDay))
            {
                return false;
            }

            var value = _propertyReflector.GetValue(dataItem, _bindingCellTypeTimeOfDay);
            return (value is bool && (bool) value);
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
                if(dataItems.Count == 0)
                    return;

                T dataItem = dataItems[e.RowIndex - 2];
                setCellTypeAndValue(e,dataItem);
            }
        }

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
        	if (dataItems != null && dataItems.Count > 0)
        	{
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
        	}
		   	e.Handled = true;
        }

        private void InvokeValidate(T dataItem, GridStyleInfo style, int rowIndex, bool inSaveMode)
        {
        	var handler = Validate;
            if(handler != null)
            {
                IAsyncResult result = handler.BeginInvoke(dataItem, style, rowIndex, inSaveMode, null, null);
                handler.EndInvoke(result);
            }
        }
    }
}