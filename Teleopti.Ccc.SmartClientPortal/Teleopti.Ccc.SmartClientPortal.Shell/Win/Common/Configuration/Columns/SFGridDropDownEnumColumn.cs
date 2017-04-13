using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{
    public class SFGridDropDownEnumColumn<TSource, TComboItem, TEnum> : SFGridColumnBase<TSource>
    {
        private readonly IList<TComboItem> _comboItems;
        private readonly string _displayMember, _valueMember;

        public SFGridDropDownEnumColumn(string bindingProperty, string headerText, IList<TComboItem> comboItems, string displayMember, string valueMember)
            : base(bindingProperty, headerText)
        {
            _comboItems = comboItems;
            _displayMember = displayMember;
            _valueMember = valueMember;
        }


        public override int PreferredWidth
        {
            get { return 140; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TSource> dataItems, TSource currentItem)
        {
            var value = currentItem.GetType().GetProperty(BindingProperty).GetValue(currentItem,null);
            if (value != null)
            {
	            e.Style.CellType = GridCellModelConstants.CellTypeComboBox;
                e.Style.DataSource = _comboItems;
                e.Style.DisplayMember = _displayMember;
                if (!string.IsNullOrEmpty(_valueMember)) e.Style.ValueMember = _valueMember;

                e.Style.Enabled = true;
                e.Style.CellValue = GetDisplayMemberByItems((TEnum) PropertyReflectorHelper.GetValue(currentItem, BindingProperty));
            }
            else
            {
                e.Style.CellValue = new Ignore();
                e.Style.Enabled = false;
                e.Style.CellType = "IgnoreCell";
            }
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<TSource> dataItems, TSource currentItem)
        {
            if (e.Style.CellValue == null || e.Style.CellValue is Ignore)
                return;

        	if (typeof(TComboItem) != e.Style.CellValue.GetType() && typeof(TEnum) != e.Style.CellValue.GetType())
            {
            	TComboItem comboItem;
            	if (TryGetItemByDisplayMember((string)e.Style.CellValue, out comboItem))
                {
                    TEnum value;
                    if (TryGetValueFromItemString((string)PropertyReflectorHelper.GetValue(comboItem, _displayMember), out value))
                    {
                        PropertyReflectorHelper.SetValue(currentItem, BindingProperty, value);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_valueMember))
                {
                    PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
                }
                else
                {
                    TEnum value;
                    if (TryGetValueFromItemString(
                            (string)PropertyReflectorHelper.GetValue(e.Style.CellValue, _displayMember), out value))
                    {
                        PropertyReflectorHelper.SetValue(currentItem, BindingProperty, value);
                    }
                }
            }
        }

        private bool TryGetValueFromItemString(string displayMember, out TEnum value)
        {
            foreach (var comboItem in _comboItems)
            {
                if (!PropertyReflectorHelper.GetValue(comboItem, _displayMember).Equals(displayMember)) continue;
                value = (TEnum)PropertyReflectorHelper.GetValue(comboItem, BindingProperty);
                return true;
            }

            value = (TEnum)PropertyReflectorHelper.GetValue(_comboItems[0], BindingProperty);
            return false;
        }

        private string GetDisplayMemberByItems(TEnum value)
        {
            foreach (var comboItem in _comboItems)
            {
                if (PropertyReflectorHelper.GetValue(comboItem, BindingProperty).Equals(value))
                {
                    return (string)PropertyReflectorHelper.GetValue(comboItem, _displayMember);
                }
            }

            return "";
        }

        private bool TryGetItemByDisplayMember(string displayMember, out TComboItem comboItem)
        {
            foreach (TComboItem item in _comboItems)
            {
                string itemName;
                if (!string.IsNullOrEmpty(_valueMember))
					itemName = ((TEnum)PropertyReflectorHelper.GetValue(item, _valueMember)).ToString();

                else
					itemName = (string)PropertyReflectorHelper.GetValue(item, _displayMember);
            	if (itemName != displayMember) continue;
            	comboItem = item;
            	return true;
            }

            comboItem = _comboItems[0];
            return false;
        }
    }
}
