using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridDropDownColumn<T, TItem> : SFGridColumnBase<T>
    {
        private readonly IList<TItem> _comboItems;
        private readonly string _displayMember;
        private readonly Type _baseClass;

        private readonly DisabledObjectSpecification<T> _disabledObjectSpecification =
            new DisabledObjectSpecification<T>("IsTrackerDisabled");

        public SFGridDropDownColumn(string bindingProperty, string headerText, IList<TItem> comboItems, string displayMember, Type baseClass)
            : this(bindingProperty, headerText, null, comboItems, displayMember, baseClass)
        {
        }

        public SFGridDropDownColumn(string bindingProperty, string headerText, string groupHeaderText, IList<TItem> comboItems, string displayMember, Type baseClass)
            : base(bindingProperty, headerText)
        {
            GroupHeaderText = groupHeaderText;
            _comboItems = comboItems;
            _displayMember = displayMember;
            _baseClass = baseClass;
            UseDisablePropertyCheck = false;
        }

        public override int PreferredWidth
        {
            get { return 150; }
        }

        public event EventHandler<GridQueryCellInfoEventArgs> QueryComboItems;

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = GridCellModelConstants.CellTypeComboBox;
            OnQueryComboItems(e);
            e.Style.DisplayMember = _displayMember;
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
			e.Style.DropDownStyle = GridDropDownStyle.AutoComplete;
        	if (!UseDisablePropertyCheck) return;
        	if (!_disabledObjectSpecification.IsSatisfiedBy(currentItem)) return;
        	e.Style.Enabled = false;
        	e.Style.BackColor = ColorHelper.DisabledCellColor;
        }

        protected void OnQueryComboItems(GridQueryCellInfoEventArgs e)
        {
        	var handler = QueryComboItems;
            if (handler!= null)
            {
                handler(this, e);
            }
            else
            {
                e.Style.DataSource = _comboItems;
            }
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
			if(!e.Style.Enabled) return;
            if (e.Style.CellValue != null)
            {
                var tItemType = typeof (TItem);
                var cellValueType = e.Style.CellValue.GetType();
                if (Array.Exists(cellValueType.GetNestedTypes(),t=> t.Equals(tItemType)) ||
                    Array.Exists(cellValueType.GetInterfaces(), t => t.Equals(tItemType)) ||
                    cellValueType.IsAssignableFrom(tItemType))
                {
                    PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
                }
                else
                {
                    SetValueByDisplayMemberValue(currentItem, e.Style.CellValue.ToString());
                }
            }
            else
            {
                SetValueByDisplayMemberValue(currentItem, string.Empty);
            }
        }

        private void SetValueByDisplayMemberValue(T currentItem, string stringValue)
        {
            TItem item;
            if (TryGetItemByDisplayMember(stringValue, out item))
            {
                PropertyReflectorHelper.SetValue(currentItem, BindingProperty, item);
            }
        }

        private bool TryGetItemByDisplayMember(string displayMember, out TItem comboItem)
        {
            foreach (var comboItem1 in from comboItem1 in _comboItems
                                         let itemName = PropertyReflectorHelper.GetValue(comboItem1, _displayMember).ToString()
                                         where itemName == displayMember
                                         select comboItem1)
            {
            	comboItem = comboItem1;
            	return true;
            }

            comboItem = _comboItems[0];
            return false;
        }

        protected override void ApplyIgnoreCell(GridStyleInfo style)
        {
            base.ApplyIgnoreCell(style);
            if (style.CellType != "IgnoreCell" && _baseClass != null)
            {
                style.CellValueType = _baseClass;
            }
        }

        internal bool UseDisablePropertyCheck { get; set; }
    }
}