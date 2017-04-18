using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows
{
    public interface IGridRow
    {
        void QueryCellInfo(CellInfo cellInfo);

        void SaveCellInfo(CellInfo cellInfo);

        void OnSelectionChanged(GridSelectionChangedEventArgs e, int rowHeaders);
    }
}