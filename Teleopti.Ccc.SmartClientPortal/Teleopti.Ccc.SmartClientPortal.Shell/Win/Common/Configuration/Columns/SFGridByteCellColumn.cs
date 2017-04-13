using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{
    public class SFGridByteCellColumn<T> : SFGridColumnBase<T>
    {
        public override int PreferredWidth
        {
            get { return 50; }
        }

        public SFGridByteCellColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        {
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "NumericCell";
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            byte value;
            bool parseResult = true;
            if (e.Style.CellValue is byte)
            {
                value = (byte) e.Style.CellValue;
            }
            else
            {
                parseResult = byte.TryParse(e.Style.CellValue.ToString(), out value);
            }
            if (parseResult)
            {
                PropertyReflectorHelper.SetValue(currentItem, BindingProperty, value);
            }
        }
    }
}
