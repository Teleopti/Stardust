using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns
{
    public class ReadOnlyCollectionColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private string _headerText;

        public ReadOnlyCollectionColumn(string bindingProperty, string headerText) : this(bindingProperty, headerText, 100)
        {
        }

        public ReadOnlyCollectionColumn(string bindingProperty, string headerText, int preferredWidth) : base(bindingProperty,preferredWidth)
        {
            _headerText = headerText;            
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
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
                e.Style.ReadOnly = true;
            }
            
            e.Handled = true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (dataItems.Count == 0) return;

                T dataItem = dataItems.ElementAt(e.RowIndex - 1);
                _propertyReflector.SetValue(dataItem, BindingProperty, e.Style.CellValue.ToString());
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }
        }
    }
}