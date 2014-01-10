using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common.Clipboard;

namespace Teleopti.Ccc.WinCode.Common
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
