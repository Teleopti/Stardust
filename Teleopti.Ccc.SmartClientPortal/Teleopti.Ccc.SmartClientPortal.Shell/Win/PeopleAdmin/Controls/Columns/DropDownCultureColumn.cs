using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.PeopleAdmin;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns
{
    class DropDownCultureColumn<TData, TItems> : ColumnBase<TData>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private IEnumerable<TItems> _comboItems;
        private string _displayMember;

        public DropDownCultureColumn(string bindingProperty, string headerText,
                              IEnumerable<TItems> comboItems,
                              string displayMember) : base(bindingProperty,150)
        {
            _headerText = headerText;
            _comboItems = comboItems;
            _displayMember = displayMember;
        }

        public override IComparer<TData> ColumnComparer { get; set; }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            SetUpSingleHeader(e, dataItems);
            e.Handled = true;
        }

        private void SetUpSingleHeader(GridQueryCellInfoEventArgs e, IList<TData> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }

            if (IsContentRow(e.RowIndex, dataItems.Count))
            {
                TData dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellType = GridCellModelConstants.CellTypeDropDownCultureCell;
                e.Style.DataSource = _comboItems;
                e.Style.DisplayMember = _displayMember;
				e.Style.DropDownStyle = GridDropDownStyle.AutoComplete;
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
                OnCellDisplayChanged(dataItem, e);
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                TData dataItem = dataItems.ElementAt(e.RowIndex - 1);

                if (typeof(TItems) == e.Style.CellValue.GetType())
                {
                    _propertyReflector.SetValue(dataItem, BindingProperty, e.Style.CellValue);
                }
                else
                {
                    _propertyReflector.SetValue(dataItem, BindingProperty, Culture.GetLanguageInfoByDisplayName(e.Style.CellValue.ToString()));
                }
                e.Handled = true;
                OnCellChanged(dataItem, e);
            }
        }
    }
}