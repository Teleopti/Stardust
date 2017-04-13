using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns
{
    public class ReadOnlyTextColumnPeriod<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _bindingProperty;
        private bool _makeRightAlign;

        public ReadOnlyTextColumnPeriod(string bindingProperty, string headerText, bool makeRightAlign) : base(bindingProperty,110)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _makeRightAlign = makeRightAlign;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            SetUpSingleHeader(e, dataItems);
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
                if (dataItems.Count == 0) return;
                e.Style.ReadOnly = true;
                T dataItem = dataItems[e.RowIndex - 1];

                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                
                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
              
                e.Style.TextColor = Color.DimGray;
                e.Style.HorizontalAlignment = (_makeRightAlign) ? GridHorizontalAlignment.Right : GridHorizontalAlignment.Left;
                e.Handled = true;
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }
    }
}
