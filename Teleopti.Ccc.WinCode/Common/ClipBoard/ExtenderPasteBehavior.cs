#region Imports

using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;

#endregion

namespace Teleopti.Ccc.WinCode.Common.Clipboard
{

    /// <summary>
    /// Represents the extend grid paste behaviour
    /// </summary>
    public class ExtenderPasteBehavior : PasteBehavior, IPasteBehavior
    {
        #region Fields - Instance Member

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - ExtenderPasteBehavior Members

        #endregion

        #endregion

        #region Events - Instance Member

        #endregion

        #region IPasteBehavior Members

        public IList<T> DoPaste<T>(GridControl gridControl, 
                                   ClipHandler<T> clipHandler, 
                                   IGridPasteAction<T> gridPasteAction, 
                                   GridRangeInfoList rangeList)
        {
            IList<T> pasteList = new List<T>();
            T pasteResult;

			gridControl.BeginUpdate();
            if(clipHandler.ClipList.Count > 0)
            {
                GridRangeInfo gridRange = rangeList.ActiveRange;

                int copiedRowCount = clipHandler.RowSpan();
                int copiedColCount = clipHandler.ColSpan();

                int selectedRowCount = (gridRange.Bottom - gridRange.Top);
                selectedRowCount = (selectedRowCount == 0) ? 1 : (selectedRowCount + 1);

                int selectedColCount = (gridRange.Right - gridRange.Left);
                selectedColCount = (selectedColCount == 0) ? 1 : (selectedColCount + 1);

                int remainder;
                int rowsToBePasted = Math.DivRem(selectedRowCount, copiedRowCount, out remainder);
                rowsToBePasted = (rowsToBePasted == 0) ? 1 : rowsToBePasted;

                rowsToBePasted *= copiedRowCount;

                if (selectedRowCount == copiedRowCount)
                    rowsToBePasted = copiedRowCount;

                int colsToBePasted = Math.DivRem(selectedColCount, copiedColCount, out remainder);
                colsToBePasted = (colsToBePasted == 0) ? 1 : colsToBePasted;

                colsToBePasted *= copiedColCount;

                int clipIndex = 0;
                for (int row = gridRange.Top; row < (gridRange.Top + rowsToBePasted); row++)
                {
                    for (int col = gridRange.Left; col <= (gridRange.Left + colsToBePasted - 1); col++)
                    {
                        if ((col <= gridControl.ColCount) && (row <= gridControl.RowCount))
                        {
                            if (row > gridControl.Rows.HeaderCount && col > gridControl.Cols.HeaderCount)
                            {
                                Clip<T> clip = clipHandler.ClipList[clipIndex];
                                pasteResult = gridPasteAction.Paste(gridControl, clip, row, col);
                                if (pasteResult != null)
                                    pasteList.Add(pasteResult);
                            }
                        }
                        clipIndex++;
                        if ((clipIndex == clipHandler.ClipList.Count) && (rowsToBePasted >= 1))
                            clipIndex = 0;
                    }
                }                
            }
			gridControl.EndUpdate(true);

            return pasteList;
        }

        #endregion
    }

}
