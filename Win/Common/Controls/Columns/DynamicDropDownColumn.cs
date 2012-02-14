using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    /// <summary>
    /// TData is the class and TComboItemType is the type of the items in the drop down
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TComboItemType"></typeparam>
    public class DynamicDropDownColumn<TData, TComboItemType> : ColumnBase<TData>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        private readonly string _bindingProperty;
        private readonly string _dependantProperty;

        public DynamicDropDownColumn(string bindingProperty, string headerText,
                              string dependantProperty)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _dependantProperty = dependantProperty;
        }

        public override int PreferredWidth
        {
            get { return 150; }
        }

        public override string BindingProperty
        {
            get
            {
                return _bindingProperty;
            }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
                e.Style.CellType = "ComboBox";
                TData dataItem = dataItems.ElementAt(e.RowIndex - 1);
                IList<TComboItemType> comboCollection =
                    (IList<TComboItemType>) _propertyReflector.GetValue(dataItem, _dependantProperty);
                e.Style.DataSource = comboCollection;
                //e.Style.DisplayMember = _displayMember;
                TComboItemType selectedItem = (TComboItemType)_propertyReflector.GetValue(dataItem, BindingProperty);
                e.Style.CellValue = selectedItem;

                OnCellDisplayChanged(dataItem, e);
                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
            }
        }

        #region Set Up Headers

        #endregion

        /// <summary>
        /// Determines whether specified row index is valid row.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <returns>
        /// 	<c>true</c> if the specified row index is valid row; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 9/5/2008
        /// </remarks>
        protected bool IsValidRow(int rowIndex)
        {
            return rowIndex > 0;
        }
        /// <summary>
        /// Determines whether specified col index is valid column.
        /// </summary>
        /// <param name="colIndex">Index of the col.</param>
        /// <returns>
        /// 	<c>true</c> if the specified col index is valid column; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 9/5/2008
        /// </remarks>
        protected bool IsValidColumn(int colIndex)
        {
            return colIndex > 0;
        }
        /// <summary>
        /// Determines whether column is single header.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if single header column; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 9/5/2008
        /// </remarks>
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
                _propertyReflector.SetValue(dataItem, _bindingProperty, e.Style.CellValue);
                e.Handled = true;
            }
            OnCellChanged(dataItem, e);
        }

     

    }
}