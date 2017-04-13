using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    public class DynamicDropDownColumn<TData, TComboItemType> : ColumnBase<TData>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        private readonly string _dependantProperty;

        public DynamicDropDownColumn(string bindingProperty, string headerText, string dependantProperty) :base(bindingProperty,150)
        {
            _headerText = headerText;
            _dependantProperty = dependantProperty;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
				e.Style.CellType = GridCellModelConstants.CellTypeComboBox;
                TData dataItem = dataItems.ElementAt(e.RowIndex - 1);
                IList<TComboItemType> comboCollection =
                    (IList<TComboItemType>) _propertyReflector.GetValue(dataItem, _dependantProperty);
                e.Style.DataSource = comboCollection;
                TComboItemType selectedItem = (TComboItemType)_propertyReflector.GetValue(dataItem, BindingProperty);
                e.Style.CellValue = selectedItem;

                OnCellDisplayChanged(dataItem, e);
                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
            }
        }

        protected bool IsValidRow(int rowIndex)
        {
            return rowIndex > 0;
        }

        protected bool IsValidColumn(int colIndex)
        {
            return colIndex > 0;
        }

        protected bool IsSingleHeader()
        {
            return true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (!IsValidColumn(e.ColIndex)) return;
            if (!IsValidRow(e.RowIndex)) return;

            TData dataItem; // Holds selected item from collection.

            if (IsSingleHeader()) dataItem = dataItems.ElementAt(e.RowIndex - 1);
            else dataItem = dataItems.ElementAt(e.RowIndex - 2);

            Type tItemType = typeof(TComboItemType);
            Type cellValueType = e.Style.CellValue.GetType();
            if (tItemType == typeof(int) && cellValueType == typeof(string))
            {
                // test if can be converted to int
                int cellValue;
                if (Int32.TryParse(e.Style.CellValue.ToString(), out cellValue))
                    e.Style.CellValue = cellValue;
            }
            cellValueType = e.Style.CellValue.GetType();
            
            if (tItemType == cellValueType ||
                cellValueType.GetInterface(tItemType.Name) == tItemType)
            {
                _propertyReflector.SetValue(dataItem, BindingProperty, e.Style.CellValue);
                e.Handled = true;
            }
            OnCellChanged(dataItem, e);
        }
    }
}