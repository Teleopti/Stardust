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
    class DropDownColumnForPeriodGrids<TData, TItems> : ColumnBase<TData>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        private readonly string _bindingProperty;
        private readonly IEnumerable<TItems> _comboItems;
        private readonly string _displayMember;
        private readonly Type _baseClass;

        public DropDownColumnForPeriodGrids(string bindingProperty, string headerText,
                              IEnumerable<TItems> comboItems,
                              string displayMember)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
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

        private bool tryGetItemByDisplayMember(string displayMember, out TItems comboItem)
        {
            foreach (TItems theComboItem in _comboItems)
            {
                string itemName = null;
                try { itemName = (string)_propertyReflector.GetValue(theComboItem, _displayMember); }
                catch (InvalidCastException) { }
                if (itemName == displayMember)
                {
                    comboItem = theComboItem;
                    return true;
                }
            }
			//it must be possible to empty the BudgetGroup
			if (_baseClass.Equals(typeof(Domain.Budgeting.BudgetGroup)) && string.IsNullOrEmpty(displayMember))
			{
				comboItem = default(TItems);
				return true;
			}
            comboItem = _comboItems.ElementAtOrDefault(0);
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
            	
                OnCellDisplayChanged(dataItem, e);
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);

                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (dataItems.Count == 0) return;
                TData dataItem = dataItems.ElementAt(e.RowIndex - 1);

                if (typeof(TItems).IsAssignableFrom(e.Style.CellValue.GetType()) ||
                    typeof(TItems) == e.Style.CellValue.GetType().BaseType ||
                    typeof(TItems) == e.Style.CellValue.GetType() || _baseClass == e.Style.CellValue.GetType() || _baseClass == e.Style.CellValue.GetType().BaseType)
                {
                    _propertyReflector.SetValue(dataItem, _bindingProperty, e.Style.CellValue);
                    e.Handled = true;
                }
                else
                {
                    // TODO: Need to implement proper enumeration
                    try
                    {
                        TItems item;
                        if (tryGetItemByDisplayMember(e.Style.CellValue.ToString(), out item))
                        {
                            _propertyReflector.SetValue(dataItem, _bindingProperty, item);
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