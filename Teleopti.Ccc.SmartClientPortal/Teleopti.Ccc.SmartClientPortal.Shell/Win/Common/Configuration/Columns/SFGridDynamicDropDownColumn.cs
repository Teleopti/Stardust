using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{ 
    public class SFGridDynamicDropDownColumn<T, TItem> : SFGridColumnBase<T> 
    {
        private readonly string _displayMember;
        private readonly Type _baseClass;
	    private readonly string _comboItemsProperty;

        public SFGridDynamicDropDownColumn(string bindingProperty, string headerText, string groupHeaderText, string comboItemsProperty, string displayMember, Type baseClass)
            : base(bindingProperty, headerText)
        {
            GroupHeaderText = groupHeaderText;
            _comboItemsProperty = comboItemsProperty;
            _displayMember = displayMember;
            _baseClass = baseClass;
        }

        public override int PreferredWidth
        {
            get { return 150; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
			e.Style.CellType = GridCellModelConstants.CellTypeComboBox;
            var comboItems = (IList<TItem>)PropertyReflectorHelper.GetValue(currentItem, _comboItemsProperty);

	        e.Style.DataSource = comboItems;
            e.Style.DisplayMember = _displayMember;
            var selectedItem = (TItem)PropertyReflectorHelper.GetValue(currentItem, BindingProperty);

            e.Style.CellValue = !comboItems.Contains(selectedItem) ? comboItems[0] : selectedItem;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
        	if (e.Style.CellValue == null) return;
        	var tItemType = typeof(TItem);
        	var cellValueType = e.Style.CellValue.GetType();
        	if (Array.Exists(cellValueType.GetNestedTypes(), t => t.Equals(tItemType)) ||
        	    Array.Exists(cellValueType.GetInterfaces(), t => t.Equals(tItemType)) ||
        	    cellValueType.IsAssignableFrom(tItemType))
        	{
        		PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        	}
        	else
        	{
        		TItem item;
        		var comboItems = (IList<TItem>)PropertyReflectorHelper.GetValue(currentItem, _comboItemsProperty);
        		if (TryGetItemByDisplayMember(e.Style.CellValue.ToString(), out item, comboItems))
        		{
        			PropertyReflectorHelper.SetValue(currentItem, BindingProperty, item);
        		}
        	}
        }

        private bool TryGetItemByDisplayMember(string displayMember, out TItem comboItem, IList<TItem> comboItems)
        {
            foreach (var item in from item in comboItems
                                 let itemName = PropertyReflectorHelper.GetValue(item, _displayMember).ToString()
                                 where itemName == displayMember
                                 select item)
            {
            	comboItem = item;
            	return true;
            }

            comboItem = comboItems[0];
            return false;
        }

        protected override void ApplyIgnoreCell(GridStyleInfo style)
        {
            base.ApplyIgnoreCell(style);
            if (style.CellType!="IgnoreCell" && _baseClass!=null)
            {
                style.CellValueType = _baseClass;
            }
        }
    }
}