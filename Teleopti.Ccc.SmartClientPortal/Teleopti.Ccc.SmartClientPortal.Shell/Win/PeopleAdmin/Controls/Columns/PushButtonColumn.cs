using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns
{
    public class PushButtonColumn<T> : ColumnBase<T>
    {
        private readonly string _headerText;
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        public PushButtonColumn(string headerText, string bindingProperty) : base(bindingProperty, 10)
        {
            _headerText = headerText;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            
            if (IsContentRow(e.RowIndex, dataItems.Count))
            {
                e.Style.CellType = "PushButton";
                
                var dataItem = dataItems[e.RowIndex - 1];
                var expandedState = (bool) _propertyReflector.GetValue(dataItem, "ExpandState");

                e.Style.Tag = expandedState;
                e.Style.Description = expandedState ? "-" : "+";
                e.Style.CellAppearance = GridCellAppearance.Flat;
                e.Style.Font.Size = 8;
                e.Style.Font.Bold = true;

                object obj = _propertyReflector.GetValue(dataItem, BindingProperty);
                int count;

                if (int.TryParse(obj.ToString(), out count) && count == 0)
                {
                    e.Style.CellType = "Test";
                    e.Style.Description = "";
                }
            }

            e.Handled = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }
    }
}