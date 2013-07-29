using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    public class EditableTextColumnForPeriodGrids<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly int _maxLength;
        private readonly string _headerText;
        
        public EditableTextColumnForPeriodGrids(string bindingProperty, int maxLength, string headerText) : base(bindingProperty,100)
        {
            _maxLength = maxLength;
            _headerText = headerText;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            SetUpSingleHeader(e, dataItems);
            e.Handled = true;
        }

        private void SetUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }

            if (IsContentRow(e.RowIndex,dataItems.Count))
            {
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);

                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
                OnCellDisplayChanged(dataItem, e);
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (((string) e.Style.CellValue).Length > _maxLength) return;

                T dataItem = dataItems[e.RowIndex - 1];
                _propertyReflector.SetValue(dataItem, BindingProperty, e.Style.CellValue);
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }
        }
    }
}