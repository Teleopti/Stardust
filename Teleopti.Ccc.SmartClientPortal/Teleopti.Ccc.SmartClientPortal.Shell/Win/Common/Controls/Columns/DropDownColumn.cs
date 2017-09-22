using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    /// <summary>
    /// T is the class and L is the type of the items in the drop down
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TItems"></typeparam>
    public class DropDownColumn<TData, TItems> : ColumnBase<TData>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _groupHeaderText;
        private IEnumerable<TItems> _comboItems;
        private string _displayMember;

        public DropDownColumn(string bindingProperty, string headerText,
                              IEnumerable<TItems> comboItems,
                              string displayMember) : base(bindingProperty,150)
        {
            _headerText = headerText;
            _comboItems = comboItems;
            _displayMember = displayMember;

            GridStyleInfoStore.CellValueProperty.IsCloneable = false;
        }

        public DropDownColumn(string bindingProperty, string headerText,
                              IEnumerable<TItems> comboItems,
                              string displayMember, string groupHeaderText) : this(bindingProperty,headerText,comboItems,displayMember)
        {
            _groupHeaderText = groupHeaderText;
        }

        private bool TryGetItemByDisplayMember(string selectedItem, out TItems comboItem)
        {
            foreach (TItems currentComboItem in _comboItems)
            {
                string itemName = _propertyReflector.GetValue(currentComboItem, _displayMember).ToString().Trim();
                if (itemName.StartsWith(selectedItem,StringComparison.CurrentCultureIgnoreCase))
                {
                    comboItem = currentComboItem;
                    return true;
                }
            }

            comboItem = _comboItems.FirstOrDefault();
            return false;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
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

        private void SetUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else if (dataItems.Count > 0)
            {
                TData dataItem = dataItems.ElementAt(e.RowIndex - 1);
				e.Style.CellType = GridCellModelConstants.CellTypeDropDownCellModel;
                e.Style.DataSource = _comboItems;
                e.Style.DisplayMember = _displayMember;
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
            	e.Style.DropDownStyle = GridDropDownStyle.AutoComplete;
                OnCellDisplayChanged(dataItem, e);
            }
        }

        private void SetUpMultipleHeaders(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
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
                TData dataItem = dataItems.ElementAt(e.RowIndex - 2);
                e.Style.CellType = GridCellModelConstants.CellTypeDropDownCellModel;
                e.Style.DataSource = _comboItems;
                e.Style.DisplayMember = _displayMember;
                object cellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
                e.Style.BackColor = System.Drawing.Color.Gray;
                if (cellValue == null)
                    e.Style.CellType = "Static";
                else
                    e.Style.ResetBackColor();
				e.Style.CellValue = cellValue;
				e.Style.DropDownStyle = GridDropDownStyle.AutoComplete;
                OnCellDisplayChanged(dataItem, e);
            }
        }

        protected bool IsValidRow(int rowIndex)
        {
            if (string.IsNullOrEmpty(_groupHeaderText))
                return rowIndex > 0;
            return rowIndex > 1;
        }

        protected bool IsValidColumn(int colIndex)
        {
            return colIndex > 0;
        }

        protected bool IsSingleHeader()
        {
            return string.IsNullOrEmpty(_groupHeaderText);
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (!IsValidColumn(e.ColIndex)) return;
            if (!IsValidRow(e.RowIndex)) return;

            TData dataItem; // Holds selected item from collection.

            if (IsSingleHeader()) dataItem = dataItems.ElementAt(e.RowIndex - 1);
            else dataItem = dataItems.ElementAt(e.RowIndex - 2);

            Type cellValueType = e.Style.CellValue.GetType();
            Type tItemType = typeof(TItems);

            if (tItemType == cellValueType ||
                cellValueType.GetInterface(tItemType.Name) == tItemType)
            {
                _propertyReflector.SetValue(dataItem, BindingProperty, e.Style.CellValue);
                e.Handled = true;
            }
            else
            {
                TItems item;
                if (TryGetItemByDisplayMember(e.Style.CellValue.ToString(), out item))
                {
                    _propertyReflector.SetValue(dataItem, BindingProperty, item);
                    e.Handled = true;
                }
            }
            OnCellChanged(dataItem, e);
        }
    }
}