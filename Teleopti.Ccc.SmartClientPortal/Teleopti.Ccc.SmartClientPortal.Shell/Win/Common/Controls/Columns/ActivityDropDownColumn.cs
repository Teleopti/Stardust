using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class ActivityDropDownColumn<TData, TItems> : ColumnBase<TData>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly string _headerText;
        private readonly IEnumerable<TItems> _comboItems;
        private readonly string _displayMember;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly string _groupHeaderText;
		private readonly ImageList _masterImage = new ImageList();

        public ActivityDropDownColumn(string bindingProperty, string headerText, IEnumerable<TItems> comboItems, string displayMember, Image masterActivityImage) : base(bindingProperty,100)
        {
            _headerText = headerText;
            _comboItems = comboItems;
            _displayMember = displayMember;

			_masterImage.Images.Add(masterActivityImage);
        }

        public ActivityDropDownColumn(string bindingProperty, string headerText,
                              IEnumerable<TItems> comboItems,
                              string displayMember, string groupHeaderText, Image masterActivityImage) : this(bindingProperty,headerText,comboItems,displayMember,masterActivityImage)
        {
            _groupHeaderText = groupHeaderText;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            var headerRows = 2;
            if (string.IsNullOrEmpty(_groupHeaderText))
                headerRows = 1;

            if(e == null)return;
			if (e.RowIndex == 0 && e.ColIndex > 0)
			{
				e.Style.CellValue = string.IsNullOrEmpty(_groupHeaderText) ? _headerText : _groupHeaderText;
			}
			else if (e.RowIndex >= headerRows && dataItems.Count > 0)
            {
                TData dataItem = dataItems.ElementAt(e.RowIndex - headerRows);
				e.Style.CellType = "ActivityDropDownCell";
            	e.Style.ImageList = _masterImage;
            	e.Style.DataSource = _comboItems;
				e.Style.DropDownStyle = GridDropDownStyle.AutoComplete;
                e.Style.DisplayMember = _displayMember;
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
				e.Style.ImageIndex = (e.Style.CellValue is IMasterActivity ? 0 : -1);
				OnCellDisplayChanged(dataItem, e);
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (e == null) return;
            var headerRows = 2;
            if (string.IsNullOrEmpty(_groupHeaderText))
                headerRows = 1;
			if (e.RowIndex < headerRows) return;
            TData dataItem = dataItems.ElementAt(e.RowIndex - headerRows);
            if(e.Style.CellValue == null) return;
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

        private bool TryGetItemByDisplayMember(string selectedItem, out TItems comboItem)
        {
            foreach (TItems item in _comboItems)
            {
                string itemName = (_propertyReflector.GetValue(item, _displayMember)).ToString();
                if (itemName == selectedItem)
                {
                    comboItem = item;
                    return true;
                }
            }

            comboItem = _comboItems.ElementAtOrDefault(0);
            return false;
        }
    }
}