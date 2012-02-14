using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;

namespace WindowsFormsApplication1.Helper
{
    /// <summary>
    /// Helper for syncfusion grid
    /// </summary>
    public static class GridHelper
    {
        #region GridStyle

        public static void GridStyle(GridControl gridControl)
        {
            gridControl.GridVisualStyles = GridVisualStyles.Office2007Blue;
            gridControl.ActivateCurrentCellBehavior = GridCellActivateAction.DblClickOnCell;
            gridControl.ExcelLikeSelectionFrame = true;
            gridControl.ExcelLikeCurrentCell = true;
            gridControl.Office2007ScrollBars = true;
            gridControl.Office2007ScrollBarsColorScheme = Office2007ColorScheme.Blue;
            gridControl.SerializeCellsBehavior = GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            gridControl.ThemesEnabled = true;
            gridControl.UseRightToLeftCompatibleTextBox = true;
            gridControl.CutPaste.ClipboardFlags &= ~GridDragDropFlags.Styles;
            gridControl.HorizontalThumbTrack = true;
            gridControl.VerticalThumbTrack = true;
            gridControl.Model.Options.WrapCellBehavior = GridWrapCellBehavior.WrapRow;
            gridControl.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
            gridControl.ResizeColsBehavior = GridResizeCellsBehavior.None;
            gridControl.ResizeRowsBehavior = GridResizeCellsBehavior.None;
        }

        public static void GridStyles(GridListControl gridControl)
        {
            gridControl.ThemesEnabled = true;
            gridControl.GridVisualStyles = GridVisualStyles.Office2007Blue;
            gridControl.Properties.GridLineColor = Color.FromArgb(((208)), ((215)), ((229)));
            gridControl.BorderStyle = BorderStyle.FixedSingle;
            gridControl.BackColor = SystemColors.Window;
            gridControl.Dock = DockStyle.Fill;
            gridControl.Properties.GridLineColor = Color.FromArgb(208, 215, 229);
            gridControl.Grid.RowHeights.ResizeToFit(GridRangeInfo.Table());
            gridControl.AutoSize = true;
            gridControl.Grid.Dock = DockStyle.Fill;
            gridControl.FillLastColumn = true;
        }

        #endregion

        #region Selection

        public static void HandleSelectionKeys(GridControl gridControl, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                gridControl.BeginUpdate();
                handleSelectAll(gridControl);
                gridControl.EndUpdate();
                gridControl.Refresh();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.Tab)
            {
                HandleTabOnGridFirstAndLastCell(gridControl, e.Shift);
            }

            if (e.Shift && e.KeyCode != Keys.ShiftKey)
            {
                GridRangeInfo newGridRangeInfo = null;
                switch (e.KeyCode)
                {
                    case Keys.End:
                        newGridRangeInfo =
                            GridRangeInfo.Auto(gridControl.CurrentCell.RowIndex, gridControl.CurrentCell.ColIndex,
                                               gridControl.CurrentCell.RowIndex, gridControl.ColCount);
                        break;
                    case Keys.Home:
                        newGridRangeInfo =
                            GridRangeInfo.Auto(gridControl.CurrentCell.RowIndex, gridControl.Cols.HeaderCount + 1,
                                               gridControl.CurrentCell.RowIndex, gridControl.CurrentCell.ColIndex);
                        break;
                    case Keys.PageDown:
                        newGridRangeInfo =
                            GridRangeInfo.Auto(gridControl.CurrentCell.RowIndex, gridControl.CurrentCell.ColIndex,
                                               gridControl.RowCount, gridControl.CurrentCell.ColIndex);
                        break;
                    case Keys.PageUp:
                        newGridRangeInfo =
                            GridRangeInfo.Auto(gridControl.Rows.HeaderCount + 1, gridControl.CurrentCell.ColIndex,
                                               gridControl.CurrentCell.RowIndex, gridControl.CurrentCell.ColIndex);
                        break;
                    default:
                        break;
                }
                if (newGridRangeInfo != null)
                {
                    gridControl.BeginUpdate();
                    gridControl.Selections.Clear();
                    gridControl.Selections.SelectRange(newGridRangeInfo, true);
                    gridControl.EndUpdate();
                    e.Handled = true;
                }
            }
        }

        private static void handleSelectAll(GridControl gridControl)
        {
            GridRangeInfo dataOnlyGridRangeInfo =
                GridRangeInfo.Auto(gridControl.Rows.HeaderCount + 1, gridControl.Cols.HeaderCount + 1,
                                   gridControl.RowCount, gridControl.ColCount);
            GridRangeInfo dataAndHeadersGridRangeInfo =
                GridRangeInfo.Auto(0, 0, gridControl.RowCount + gridControl.Rows.HeaderCount,
                                   gridControl.ColCount + gridControl.Cols.HeaderCount);
            if (gridControl.Selections.Count == 1)
            {
                GridRangeInfo existingGridRangeInfo = gridControl.Selections.Ranges[0];
                gridControl.Selections.Clear();

                if (existingGridRangeInfo == dataOnlyGridRangeInfo)
                {
                    gridControl.Selections.Add(dataAndHeadersGridRangeInfo);
                }
                else if (existingGridRangeInfo != dataOnlyGridRangeInfo && existingGridRangeInfo != dataAndHeadersGridRangeInfo)
                {
                    gridControl.Selections.Add(dataOnlyGridRangeInfo);
                }
            }
            else
            {
                gridControl.Selections.Clear();
                gridControl.Selections.Add(dataOnlyGridRangeInfo);
            }
        }

        //TODO kolla om denna kan erätta ovansående "handleSelectAll"
        public static void SelectAll(GridControl grid)
        {
            int top = grid.Rows.HeaderCount + 1;
            int left = grid.Cols.HeaderCount + 1;
            int right = grid.ColCount;
            int bottom = grid.RowCount;
            grid.Selections.SelectRange(GridRangeInfo.Cells(top, left, bottom, right), true);
        }
        #endregion

        public static bool IsPasteRangeOk(GridRangeInfo range, GridControl grid, int clipRowOffset, int clipColOffset, int row, int col)
        {
            //check clip fits inside selected range, rows
            if (((clipRowOffset + row <= range.Bottom) && (clipRowOffset + row >= range.Top)) || (range.Top == range.Bottom && range.Left == range.Right))
            {
                //check clip fits inside selected range, cols
                if (((clipColOffset + col <= range.Right) && (clipColOffset + col >= range.Left)) || (range.Top == range.Bottom && range.Left == range.Right))
                {
                    //check clip fits inside grid
                    if ((grid.RowCount >= clipRowOffset + row) && (grid.ColCount >= clipColOffset + col))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public static GridRangeInfoList GetGridSelectedRanges(GridControl grid, bool excludeHeaders)
        {
            // CreateProjection an object of rangeList
            GridRangeInfoList rangelist;
            GridRangeInfoList rangelistTemp = new GridRangeInfoList(); ;


            // Get the selected ranges
            if (grid.Selections.GetSelectedRanges(out rangelist, true))
            {
                //check if we have any rows or columns selected
                foreach (GridRangeInfo range in rangelist)
                {
                    //if we have rows selected we add cells in row to a temp list
                    if (range.IsRows)
                    {
                        if (excludeHeaders)
                            rangelistTemp.Add(GridRangeInfo.Cells(range.Top, range.Left + grid.Rows.HeaderCount + 1, range.Bottom, grid.ColCount));
                        else
                            rangelistTemp.Add(GridRangeInfo.Cells(range.Top, range.Left, range.Bottom, grid.ColCount));
                    }

                    //if we have col  selected we add cells in col to a temp list
                    if (range.IsCols)
                    {
                        if (excludeHeaders)
                            rangelistTemp.Add(GridRangeInfo.Cells(range.Top + grid.Cols.HeaderCount + 1, range.Left, grid.RowCount, range.Right));
                        else
                            rangelistTemp.Add(GridRangeInfo.Cells(range.Top, range.Left, grid.RowCount, range.Right));
                    }

                    //if we have table selected we add cells in table to a temp list
                    if (range.IsTable)
                    {
                        if (excludeHeaders)
                            rangelistTemp.Add(GridRangeInfo.Cells(range.Top + grid.Cols.HeaderCount + 1, range.Left + grid.Rows.HeaderCount + 1, grid.RowCount, grid.ColCount));
                        else
                            rangelistTemp.Add(GridRangeInfo.Cells(range.Top, range.Left, grid.RowCount, grid.ColCount));
                    }
                }

                //add cells from selected rows and cols to range list
                foreach (GridRangeInfo range in rangelistTemp)
                {
                    rangelist.Add(range);
                }
            }


            return rangelist;
        }

        public static ClipHandler<string> ConvertClipboardToClipHandler()
        {
            ClipHandler<string> clipHandler = new ClipHandler<string>();

            //check if there is any data in clipboard
            if (Clipboard.ContainsText() == true)
            {
                //get text in clipboard
                string clipboardText = Clipboard.GetText();
                //remove "\n"
                clipboardText = clipboardText.Replace("\n", "");
                //remove empty string at end
                clipboardText = clipboardText.TrimEnd();
                //split on rows
                string[] clipBoardRows = clipboardText.Split('\r');

                int row = 0;
                //loop each row
                foreach (string rowString in clipBoardRows)
                {
                    //split on columns
                    string[] clipBoardCols = rowString.Split('\t');

                    int col = 0;
                    //loop each column
                    foreach (string columnString in clipBoardCols)
                    {
                        //add to cliphandler
                        clipHandler.AddClip(row, col, columnString);

                        col++;
                    }

                    row++;
                }
            }

            return clipHandler;
        }

        

        private static void HandleTabOnGridFirstAndLastCell(GridControl gridControl, bool shift)
        {
            //get current cell
            GridCurrentCell cell = gridControl.CurrentCell;

            //check if we are on the first row, first col and shift is pressed
            if (cell.RowIndex == gridControl.Rows.HeaderCount + 1 && cell.ColIndex == gridControl.Cols.HeaderCount + 1 && shift == true)
            {
                Form form = gridControl.FindParentForm();
                form.SelectNextControl(gridControl, false, true, false, true);
                return;
            }

            //check if we are on last row, last col in grid and shift is not pressed
            if (cell.RowIndex == gridControl.RowCount && cell.ColIndex == gridControl.ColCount && shift == false)
            {
                Form form = gridControl.FindParentForm();
                form.SelectNextControl(gridControl, true, true, false, true);
                return;
            }
        }
    }
}
