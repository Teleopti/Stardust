using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Styles;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridNumericCellColumn<T> : SFGridColumnBase<T>
    {
        private string _toBeBold = string.Empty;
        public SFGridNumericCellColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        {
        }

        public SFGridNumericCellColumn(string bindingProperty, string headerText, int preferredWidth)
            : base(bindingProperty, headerText, preferredWidth)
        {
        }

        public SFGridNumericCellColumn(string bindingProperty, string headerText, int preferredWidth, string toBeBold)
            : base(bindingProperty, headerText, preferredWidth)
        {
            _toBeBold = toBeBold;
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "NumericCell";
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            if (!string.IsNullOrEmpty(_toBeBold))
                e.Style.Font.Bold = (bool)PropertyReflectorHelper.GetValue(currentItem, _toBeBold);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }
    }
}