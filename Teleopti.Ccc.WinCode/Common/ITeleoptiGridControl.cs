using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.WinCode.Common
{
    public interface ITeleoptiGridControl
    {
        GridModelRowColOperations Rows { get; }
        GridModelRowColOperations Cols { get; }
        int RowCount { get; set; }
        int ColCount { get; set; }
        void AddCoveredRange(GridRangeInfo rangeInfo);
    }
}