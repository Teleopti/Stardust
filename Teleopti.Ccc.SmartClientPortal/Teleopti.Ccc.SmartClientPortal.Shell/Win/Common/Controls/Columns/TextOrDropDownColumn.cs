using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    public interface ITextOrDropDownColumnComparer<T>
    {
        bool Compare(T dataItem);
    }

    public class TextOrDropDownColumn<TData, TItem> : ColumnBase<TData>
    {
        private string _headerText;
        private string _displayMember;

        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private IList<TItem> _comboItems;
        private ITextOrDropDownColumnComparer<TData> _dropDownVisibleConditionComparer;
        public event EventHandler<SelectedItemChangeEventArgs<TData, TItem>> SelectedItemChanged;

        public TextOrDropDownColumn(string bindingProperty, string headerText, IList<TItem> comboItems,
            ITextOrDropDownColumnComparer<TData> dropDownVisibleConditionComparer, string displayMember) : base(bindingProperty,130)
        {
            InParameter.NotNull("comboItems", comboItems);
            InParameter.NotNull("dropDownVisibleConditionComparer", dropDownVisibleConditionComparer);

            _headerText = headerText;
            _comboItems = comboItems;
            _dropDownVisibleConditionComparer = dropDownVisibleConditionComparer;
            _displayMember = displayMember;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }

            if (e.RowIndex > 0 && e.ColIndex > 0)
            {
                TData dataItem = dataItems[e.RowIndex - 1];

                object bindingProperty = _propertyReflector.GetValue(dataItem, BindingProperty);

                if (_dropDownVisibleConditionComparer.Compare(dataItem))
                {
					e.Style.CellType = GridCellModelConstants.CellTypeComboBox;
                    e.Style.DataSource = _comboItems;
                    e.Style.DisplayMember = _displayMember;

                    if (bindingProperty != null)
                    {
                        var text = _propertyReflector.GetValue(bindingProperty, _displayMember);
                        e.Style.CellValue = text;
                        e.Style.ApplyText(text.ToString());
                        e.Style.ApplyFormattedText(text.ToString());
                    }
                }
                else
                {
                    e.Style.TextColor = Color.DimGray;
                    e.Style.CellType = "Static";

                    if (bindingProperty != null)
                    {
                        e.Style.CellValue = _propertyReflector.GetValue(bindingProperty, _displayMember);
                    }
                }

                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
                OnCellDisplayChanged(dataItem, e);
            }

            e.Handled = true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                TData dataItem = dataItems[e.RowIndex - 1];

                if (e.Style.CellValue is TItem)
                {
                    OnTypeChanged(dataItem, ((TItem) e.Style.CellValue));
                    OnCellChanged(dataItem, e);
                }
                e.Handled = true;
            }
        }

        public virtual void OnTypeChanged(TData dataItem, TItem selectedItem)
        {
        	var handler = SelectedItemChanged;
            if (handler!= null)
            {
            	var args = new SelectedItemChangeEventArgs<TData, TItem>(dataItem, selectedItem);
                handler(this, args);
            }
        }
    }
}
