using System.Collections.ObjectModel;
using System.Threading;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridDateOnlyColumn<T> : SFGridColumnBase<T>
    {
        private readonly string _toBeBold = string.Empty;

        public SFGridDateOnlyColumn(string bindingProperty, string headerText, int preferredWidth)
            : base(bindingProperty, headerText, preferredWidth)
        {
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
	        e.Style.CultureInfo = Thread.CurrentThread.CurrentCulture;
            e.Style.CellType = "DateOnly";
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            if (!string.IsNullOrEmpty(_toBeBold))
                e.Style.Font.Bold = (bool)PropertyReflectorHelper.GetValue(currentItem, _toBeBold);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
        }
    }
}