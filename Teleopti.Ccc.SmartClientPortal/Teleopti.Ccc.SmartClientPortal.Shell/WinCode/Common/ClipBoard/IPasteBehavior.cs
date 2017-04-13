using Syncfusion.Windows.Forms.Grid;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Common.Clipboard
{
    /// <summary>
    /// Interface for PasteBehavior
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-09-04
    /// </remarks>
    public interface IPasteBehavior
    {
        IList<T> DoPaste<T>(GridControl gridControl, ClipHandler<T> clipHandler, IGridPasteAction<T> gridPasteAction, GridRangeInfoList rangeList);

    }
}
