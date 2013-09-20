using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
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
        private string _bindingProperty;
        private IEnumerable<TItems> _comboItems;
        private string _displayMember;

        public DropDownColumn(string bindingProperty, string headerText,
                              IEnumerable<TItems> comboItems,
                              string displayMember)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
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

        /// <summary>
        /// Set up single header.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-21
        /// </remarks>
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
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
            	e.Style.DropDownStyle = GridDropDownStyle.AutoComplete;
                OnCellDisplayChanged(dataItem, e);
            }

        }

        /// <summary>
        /// Sets up multiple headers.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-21
        /// </remarks>
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
                object cellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
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
            if (string.IsNullOrEmpty(_groupHeaderText))
                return rowIndex > 0;
            return rowIndex > 1;
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

    }
}