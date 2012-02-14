using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridVisualizeColumn<T> : SFGridColumnBase<T>
    {
        private readonly int _preferredWidth;

        public SFGridVisualizeColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        { }

        public SFGridVisualizeColumn(string bindingProperty, int preferredWidth, string headerText)
            : base(bindingProperty, headerText)
        {
            _preferredWidth = preferredWidth;
        }

        public override int PreferredWidth
        {
            get { return _preferredWidth; }
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