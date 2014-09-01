using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    internal class DropDownColumnForPeriodGrids<TData, TItems> : ColumnBase<TData>
    {
        private readonly string _valueMember;
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        private readonly IEnumerable<TItems> _comboItems;
        private readonly string _displayMember;
        private readonly Type _baseClass;
        private readonly bool _allowNullValue = true;

        public DropDownColumnForPeriodGrids(string bindingProperty, string headerText,
                                            IEnumerable<TItems> comboItems,
                                            string displayMember)
            : base(bindingProperty, 150)
        {
            _headerText = headerText;
            _comboItems = comboItems;
            _displayMember = displayMember;

            GridStyleInfoStore.CellValueProperty.IsCloneable = false;
        }

        public DropDownColumnForPeriodGrids(string bindingProperty, string headerText,
                                            IEnumerable<TItems> comboItems,
                                            string displayMember, Type baseClass)
            : this(bindingProperty, headerText, comboItems, displayMember)
        {
            _baseClass = baseClass;
        }

        public DropDownColumnForPeriodGrids(string bindingProperty, string headerText,
                                            IEnumerable<TItems> comboItems,
                                            string displayMember, string valueMember, Type baseClass)
            : this(bindingProperty, headerText, comboItems, displayMember, baseClass)
        {
            _valueMember = valueMember;
        }

        public DropDownColumnForPeriodGrids(string bindingProperty, string headerText, IEnumerable<TItems> comboItems,
                                            string displayMember, bool allwoNullValue)
            : this(bindingProperty, headerText, comboItems, displayMember)
        {
            _allowNullValue = allwoNullValue;
        }


        private bool tryGetItemByDisplayMember(string displayMember, out TItems comboItem)
        {
            foreach (TItems theComboItem in _comboItems)
            {
                string itemName = null;
                try
                {
                    itemName = _propertyReflector.GetValue(theComboItem, _displayMember).ToString();
                }
                catch (InvalidCastException)
                {
                }
                if (itemName == displayMember)
                {
                    comboItem = theComboItem;
                    return true;
                }
            }

            if (string.IsNullOrEmpty(displayMember) && _allowNullValue)
            {
                comboItem = default(TItems);
                return true;
            }
            comboItem = _comboItems.ElementAtOrDefault(0);
            return false;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            setUpSingleHeader(e, dataItems);
            e.Handled = true;
        }

        private void setUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }

            if (IsContentRow(e.RowIndex, dataItems.Count))
            {
                TData dataItem = dataItems[e.RowIndex - 1];

                e.Style.CellType = "ComboBox";
                e.Style.DataSource = _comboItems;
                e.Style.DisplayMember = _displayMember;
                e.Style.DropDownStyle = GridDropDownStyle.AutoComplete;

                if (!string.IsNullOrEmpty(_valueMember))
                {
                    e.Style.ValueMember = _valueMember;
                }

                OnCellDisplayChanged(dataItem, e);
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);

                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (dataItems.Count == 0) return;
                if (dataItems.Count < e.RowIndex) return;

                TData dataItem = dataItems[e.RowIndex - 1];

                var typeOfItem = typeof (TItems);
                if (!string.IsNullOrEmpty(_valueMember))
                {

                }

                if (typeOfItem.IsInstanceOfType(e.Style.CellValue) ||
                    typeOfItem == e.Style.CellValue.GetType().BaseType ||
                    typeOfItem == e.Style.CellValue.GetType() || _baseClass == e.Style.CellValue.GetType() ||
                    _baseClass == e.Style.CellValue.GetType().BaseType)
                {
                    _propertyReflector.SetValue(dataItem, BindingProperty, e.Style.CellValue);
                    e.Handled = true;
                }
                else
                {
                    try
                    {
                        TItems item;
                        if (tryGetItemByDisplayMember(e.Style.CellValue.ToString(), out item))
                        {
	                        if (!_allowNullValue && item == null)
		                        return;
                            _propertyReflector.SetValue(dataItem, BindingProperty, item);
                            e.Handled = true;
                        }
                    }
                    catch (NullReferenceException)
                    {
                    }
                }
                OnCellChanged(dataItem, e);
            }
        }
    }
}