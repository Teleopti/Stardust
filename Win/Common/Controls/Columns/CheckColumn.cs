using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    class CheckColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _groupHeaderText;
        private string _bindingProperty;
        private string _checkedValue;
        private string _indetermValue;
        private string _uncheckedValue;
        private Type _valueType;

        public CheckColumn(string bindingProperty, string trueValue, string falseValue, string intermValue, Type valueType, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _checkedValue = trueValue;
            _indetermValue = intermValue;
            _uncheckedValue = falseValue;
            _valueType = valueType;
        }
        public CheckColumn(string bindingProperty, string trueValue, string falseValue, string intermValue, Type valueType, string headerText, string groupHeaderText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _checkedValue = trueValue;
            _indetermValue = intermValue;
            _uncheckedValue = falseValue;
            _valueType = valueType;
            _groupHeaderText = groupHeaderText;
        }
        public override int PreferredWidth
        {
            get { return 100; }
        }

        public override string BindingProperty
        {
            get
            {
                return _bindingProperty;
            }
        }

        public override void GetCellInfo(Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e, System.Collections.ObjectModel.ReadOnlyCollection<T> dataItems)
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
                    if (dataItems.Count > 0)
                    {
                        T dataItem = dataItems[e.RowIndex - 1];
                        e.Style.CellType = "CheckBox";
                        e.Style.TriState = true;
                        e.Style.CheckBoxOptions.CheckedValue = _checkedValue;
                        e.Style.CheckBoxOptions.IndetermValue = _indetermValue;
                        e.Style.CheckBoxOptions.UncheckedValue = _uncheckedValue;
                        e.Style.CellValueType = _valueType;
                        e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
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
                        e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                        e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                    }
                }
            }


            e.Handled = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Boolean.TryParse(System.String,System.Boolean@)")]
        public override void SaveCellInfo(Syncfusion.Windows.Forms.Grid.GridSaveCellInfoEventArgs e, System.Collections.ObjectModel.ReadOnlyCollection<T> dataItems)
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
                _propertyReflector.SetValue(dataItem, _bindingProperty, cellValue);
                OnCellChanged(dataItem);
            }

            e.Handled = true;
        }
    }
}