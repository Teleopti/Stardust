using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class ActivityDropDownColumn<TData, TItems> : ColumnBase<TData>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly string _headerText;
        private readonly string _bindingProperty;
        private readonly IEnumerable<TItems> _comboItems;
        private readonly string _displayMember;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private string _groupHeaderText;

        public ActivityDropDownColumn(string bindingProperty, string headerText,
                              IEnumerable<TItems> comboItems,
                              string displayMember)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _comboItems = comboItems;
            _displayMember = displayMember;
        }

        public ActivityDropDownColumn(string bindingProperty, string headerText,
                              IEnumerable<TItems> comboItems,
                              string displayMember, string groupHeaderText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _comboItems = comboItems;

            _displayMember = displayMember;
            _groupHeaderText = groupHeaderText;
        }

        public override int PreferredWidth
        {
            get { return 100; }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            var headerRows = 2;
            if (string.IsNullOrEmpty(_groupHeaderText))
                headerRows = 1;

            if(e == null)return;
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
			else if (e.RowIndex >= headerRows)
            {
                //TData dataItem = dataItems.ElementAt(0);
                TData dataItem = dataItems.ElementAt(e.RowIndex - headerRows);
                e.Style.CellType = "ActivityDropDownCell";
                e.Style.DataSource = _comboItems;
                e.Style.DisplayMember = _displayMember;
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                OnCellDisplayChanged(dataItem, e);
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (e == null) return;
            var headerRows = 2;
            if (string.IsNullOrEmpty(_groupHeaderText))
                headerRows = 1;
			if (e.RowIndex < headerRows) return;
            TData dataItem = dataItems.ElementAt(e.RowIndex - headerRows);
            if(e.Style.CellValue == null) return;
            Type cellValueType = e.Style.CellValue.GetType();
            Type tItemType = typeof(TItems);

            if (tItemType == cellValueType ||
                cellValueType.GetInterface(tItemType.Name) == tItemType)
            {
                _propertyReflector.SetValue(dataItem, _bindingProperty, e.Style.CellValue);
                e.Handled = true;
            }
            else
            {
                TItems item;
                if (TryGetItemByDisplayMember(e.Style.CellValue.ToString(), out item))
                {
                    _propertyReflector.SetValue(dataItem, _bindingProperty, item);
                    e.Handled = true;
                }
            }
            OnCellChanged(dataItem, e);
        }

        private bool TryGetItemByDisplayMember(string selectedItem, out TItems comboItem)
        {
            foreach (TItems item in _comboItems)
            {
                string itemName = (_propertyReflector.GetValue(item, _displayMember)).ToString();
                if (itemName == selectedItem)
                {
                    comboItem = item;
                    return true;
                }
            }

            comboItem = _comboItems.ElementAtOrDefault(0);
            return false;
        }

    }
}