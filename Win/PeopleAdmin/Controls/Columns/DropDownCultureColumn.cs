using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;
using System.Collections.ObjectModel;
using Teleopti.Ccc.WinCode.PeopleAdmin;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    class DropDownCultureColumn<TData, TItems> : ColumnBase<TData>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _bindingProperty;
        private IEnumerable<TItems> _comboItems;
        private string _displayMember;

        public DropDownCultureColumn(string bindingProperty, string headerText,
                              IEnumerable<TItems> comboItems,
                              string displayMember)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _comboItems = comboItems;
            _displayMember = displayMember;
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

        public override IComparer<TData> ColumnComparer { get; set; }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            SetUpSingleHeader(e, dataItems);
            e.Handled = true;
        }

        /// <summary>
        /// Set up single header.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-21
        /// </remarks>
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
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
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
                    _propertyReflector.SetValue(dataItem, _bindingProperty, e.Style.CellValue);
                }
                else
                {
                    _propertyReflector.SetValue(dataItem, _bindingProperty, Culture.GetLanguageInfoByDisplayName(e.Style.CellValue.ToString()));
                }
                e.Handled = true;
                OnCellChanged(dataItem, e);
            }
        }
    }
}