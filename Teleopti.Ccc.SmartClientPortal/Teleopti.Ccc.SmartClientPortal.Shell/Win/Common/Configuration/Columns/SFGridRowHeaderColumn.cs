using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{
    public class SFGridRowHeaderColumn<T> : SFGridColumnBase<T>
    {
        private readonly int _preferredWidth;
        public SFGridRowHeaderColumn(string headerText) : base(string.Empty, headerText)
        {
            _preferredWidth = 30;
        }

        public SFGridRowHeaderColumn(string headerText, int preferredWidth)
            : base(string.Empty, headerText)
        {
            _preferredWidth = preferredWidth;
        }

        public override int PreferredWidth
        {
            get { return _preferredWidth; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellValue = "";
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {}
    }
}