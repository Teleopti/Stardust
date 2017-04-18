using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public interface IGridPasteAction<T>
    {
        T Paste(GridControl gridControl, Clip<T> clip, int rowIndex, int columnIndex);

        //void PasteISchedulePart(GridControl gridControl, ClipHandler<T> clipHandler, IGridPasteAction<T> gridPasteAction,
        //                        GridRangeInfoList rangelist);
        IPasteBehavior PasteBehavior { get; }

		PasteOptions PasteOptions { get; }
    }
}
