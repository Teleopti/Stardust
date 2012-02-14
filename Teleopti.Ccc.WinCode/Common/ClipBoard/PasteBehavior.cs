﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Clipboard
{

    /// <summary>
    /// Baseclass for PasteBehavior
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-09-04
    /// </remarks>
    public abstract class PasteBehavior
    {
        protected bool IsPasteRangeOk(GridRangeInfo range, GridControl grid, int clipRowOffset, int clipColOffset, int row, int col)
        {
            return FitsInsideGrid(grid, clipRowOffset, clipColOffset, row, col) &&
                   FitsInsideRange(range, clipRowOffset, clipColOffset, row, col);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        protected bool FitsInsideGrid(GridControl grid, int clipRowOffset, int clipColOffset, int row, int col)
        {
            return ((grid.RowCount >= clipRowOffset + row) &&
                (grid.ColCount >= clipColOffset + col) &&
                (clipRowOffset + row >= 0) &&
                (clipColOffset + col + row >= 0));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        protected bool FitsInsideRange(GridRangeInfo range, int clipRowOffset, int clipColOffset, int row, int col)
        {
            //henrika 08-09-03: This was set to OK when empty before moved to testable code, I dont know why but did not remove:
            bool isEmpty = (range.Top == range.Bottom && range.Left == range.Right);

            bool columnOk = ((clipColOffset + col <= range.Right) && (clipColOffset + col >= range.Left));
            bool rowOk = ((clipRowOffset + row <= range.Bottom) && (clipRowOffset + row >= range.Top));
            return ((columnOk && rowOk) || isEmpty);
        }


    }

}


