using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns
{
    public class NumericCellColumnPeriod<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private int _minValue;

        public NumericCellColumnPeriod(string bindingProperty, string headerText) : base(bindingProperty,110)
        {
            _headerText = headerText;
        }

        public NumericCellColumnPeriod(string bindingProperty, string headerText, int minValue) : this(bindingProperty,headerText)
        {
            _minValue = minValue;
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
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                e.Style.CellValue = _headerText;
            }
            
            if (IsContentRow(e.RowIndex,dataItems.Count))
            {
                T dataItem = dataItems[e.RowIndex - 1];
                object obj = _propertyReflector.GetValue(dataItem, BindingProperty);

                int value = (int)obj;

                if (value != -1)
                {
                    e.Style.CellValue = obj;
                }
                
                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
                OnCellDisplayChanged(dataItem, e);
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Right;
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                T dataItem = dataItems[e.RowIndex - 1];

                int value;

                if (!int.TryParse(e.Style.CellValue.ToString(), out value))
                    return;

                if (value >= _minValue && value < 100)
                {
                    _propertyReflector.SetValue(dataItem, BindingProperty, value);
                    OnCellChanged(dataItem, e);
                }
                e.Handled = true;
            }
        }
    }
}