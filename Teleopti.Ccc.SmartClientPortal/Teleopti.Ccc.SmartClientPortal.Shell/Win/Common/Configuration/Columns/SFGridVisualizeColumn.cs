using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{
    public class SFGridVisualizeColumn<T> : SFGridColumnBase<T>
    {
        public SFGridVisualizeColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        { }

        public override int PreferredWidth
        {
            get { return 0; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, 
                                          ReadOnlyCollection<T> dataItems, 
                                          T currentItem)
        {
            e.Style.ReadOnly = true;
            e.Style.CellType = "Static";
            e.Style.Borders.All = GridBorder.Empty;
            e.Style.ReadOnly = true;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, 
                                           ReadOnlyCollection<T> dataItems, 
                                           T currentItem)
        {

        }
    }
}