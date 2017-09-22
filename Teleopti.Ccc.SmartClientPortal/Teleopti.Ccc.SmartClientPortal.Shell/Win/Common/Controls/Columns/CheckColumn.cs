using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    class CheckColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _groupHeaderText;
        private string _checkedValue;
        private string _indetermValue;
        private string _uncheckedValue;
        private Type _valueType;

        public CheckColumn(string bindingProperty, string trueValue, string falseValue, string intermValue, Type valueType, string headerText) : base(bindingProperty,100)
        {
            _headerText = headerText;
            _checkedValue = trueValue;
            _indetermValue = intermValue;
            _uncheckedValue = falseValue;
            _valueType = valueType;
        }

        public CheckColumn(string bindingProperty, string trueValue, string falseValue, string intermValue, Type valueType, string headerText, string groupHeaderText) : this(bindingProperty,trueValue,falseValue,intermValue,valueType,headerText)
        {
            _groupHeaderText = groupHeaderText;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, System.Collections.ObjectModel.ReadOnlyCollection<T> dataItems)
        {
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                if (e.RowIndex == 0 && e.ColIndex > 0)
                {
                    e.Style.CellValue = _headerText;
                }
                else
                {
                    // HACK: Do something about this code?
                    if (dataItems.Count > 0 && dataItems.Count > e.RowIndex - 1)
                    {
                        T dataItem = dataItems[e.RowIndex - 1];
                        e.Style.CellType = "CheckBox";
                        e.Style.TriState = true;
                        e.Style.CheckBoxOptions.CheckedValue = _checkedValue;
                        e.Style.CheckBoxOptions.IndetermValue = _indetermValue;
                        e.Style.CheckBoxOptions.UncheckedValue = _uncheckedValue;
                        e.Style.CellValueType = _valueType;
                        e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
                        e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                    }
                }
            }
            else
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
                    if (dataItems.Count > 0)
                    {
                        T dataItem = dataItems[e.RowIndex - 2];

                        e.Style.CellType = "CheckBox";
                        e.Style.TriState = true;
                        e.Style.CheckBoxOptions.CheckedValue = _checkedValue;
                        e.Style.CheckBoxOptions.IndetermValue = _indetermValue;
                        e.Style.CheckBoxOptions.UncheckedValue = _uncheckedValue;
                        e.Style.CellValueType = _valueType;
                        e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
                        e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                    }
                }
            }


            e.Handled = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Boolean.TryParse(System.String,System.Boolean@)")]
        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, System.Collections.ObjectModel.ReadOnlyCollection<T> dataItems)
        {
            int rowIndex = e.RowIndex;
            T dataItem;
            object cellValue = e.Style.CellValue;

            if (cellValue is DBNull)
                cellValue = false;

            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                if (e.ColIndex > 0 && e.RowIndex > 0)
                    rowIndex -= 1;
            }
            else
            {
                if (e.ColIndex > 0 && e.RowIndex > 0)
                    rowIndex -= 2;
            }

            if(rowIndex < 0)
                return;

            dataItem = dataItems[rowIndex];

            if (cellValue.GetType() == _valueType)
            {
                _propertyReflector.SetValue(dataItem, BindingProperty, cellValue);
                OnCellChanged(dataItem);
            }

            e.Handled = true;
        }
    }
}