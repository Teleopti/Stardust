using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;


namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridRowHeaderColumn<T> : SFGridColumnBase<T>
    {
        public SFGridRowHeaderColumn(string headerText) : base(string.Empty, headerText)
        {}

        public override int PreferredWidth
        {
            get { return 30; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellValue = "";
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {}
    }
}