using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Helper
{
    /// <summary>
    /// Helper for syncfusion grid
    /// </summary>
    public static class GridHelper
    {
        public static LinearGradientBrush GetGradientBrush(Rectangle destinationRectangle, Color color)
        {
            //create a brush rectangle slightly bigger than dest rect
            Rectangle rect = new Rectangle(destinationRectangle.X, destinationRectangle.Y - 1, destinationRectangle.Width, destinationRectangle.Height + 2);
            //return brush
            return new LinearGradientBrush(rect, Color.WhiteSmoke, color, 90, false);
        }

        public static GridRangeInfoList GetGridSelectedRanges(GridControl grid, bool excludeHeaders)
        {
            // CreateProjection an object of rangeList
            GridRangeInfoList rangelist;
            GridRangeInfoList rangelistTemp = new GridRangeInfoList();

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
                            rangelistTemp.Add(GridRangeInfo.Cells(range.Top, range.Left + grid.Cols.HeaderCount + 1, range.Bottom, grid.ColCount));
                        else
                            rangelistTemp.Add(GridRangeInfo.Cells(range.Top, range.Left, range.Bottom, grid.ColCount));
                    }

                    //if we have col  selected we add cells in col to a temp list
                    if (range.IsCols)
                    {
                        if (excludeHeaders)
                            rangelistTemp.Add(GridRangeInfo.Cells(range.Top + grid.Rows.HeaderCount + 1, range.Left, grid.RowCount, range.Right));
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

        /// <summary>
        /// Copies the selected values and headers to public clipboard.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <remarks>
        /// If you just wants an ordinary copy call gridcontrol.CutPaste.Copy()
        /// Created by: micke
        /// Created date: 2008-05-26
        /// </remarks>
        public static void CopySelectedValuesAndHeadersToPublicClipboard(GridControl control)
        {
            string s = string.Empty;

            //selected range
            GridRangeInfoList rangeList = GetGridSelectedRanges(control, true);

            //loop all ranges
            foreach (GridRangeInfo range in rangeList)
            {
                //get column headers
                s += GetColHeadersString(control, range);

                //loop all rows
                for (int row = range.Top; row <= range.Bottom; row++)
                {
                    //if we find a new row add newline chars
                    if (row != range.Top)
                        s += Environment.NewLine;

                    //add rowheaderw
                    for (int i = 0; i <= control.Rows.HeaderCount; i++)
                        s += control[row, i].FormattedText + '\t';

                    //loop all columns
                    for (int col = range.Left; col <= range.Right; col++)
                    {
                        //if we find a new column add new column char
                        if (col != range.Left)
                            s += '\t';

                        //add cell value
                        s += control[row, col].FormattedText;
                    }
                }

                //add newline chars for each range
                s += Environment.NewLine;
            }

            //add string to clipboard
            Clipboard.SetDataObject(new DataObject(s), true);
        }

        private static string GetColHeadersString(GridControl control, GridRangeInfo range)
        {
            string s = string.Empty;

            //loop all columns
            for (int j = 0; j <= control.Rows.HeaderCount; j++)
            {
                //add space for rowheaders
                for (int i = 0; i <= control.Rows.HeaderCount; i++)
                    s += "\t";

                for (int col = range.Left; col <= range.Right; col++)
                {
                    //if we find a new column add new column char
                    if (col != range.Left)
                        s += '\t';

                    //add header value
                    s += control[j, col].FormattedText;
                }

                //add new line chars
                s += Environment.NewLine;
            }

            return s;
        }

        /// <summary>
        /// Fill a rounded rectangle
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="roundness">The roundness.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="inflate">The inflate.</param>
        public static void FillRoundedRectangle(Graphics graphics, Rectangle rectangle, int roundness, Brush brush, int inflate)
        {
            Rectangle rect = rectangle;
            rect.Inflate(inflate, inflate);
            graphics.DrawPath(Pens.LightBlue, GetRoundBox(rect, roundness));
            graphics.FillPath(brush, GetRoundBox(rect, roundness));
        }

        /// <summary>
        /// Get a round box
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="roundness"></param>
        /// <returns></returns>
        public static GraphicsPath GetRoundBox(Rectangle rect, int roundness)
        {
            GraphicsPath result = new GraphicsPath();
            if (roundness > 1)
            {
                roundness = Math.Min(Math.Min(rect.Width, rect.Height) / 3, roundness);

                float diameter = roundness * 2.0F;
                SizeF sizeF = new SizeF(diameter, diameter);
                RectangleF arc = new RectangleF(rect.Location, sizeF);

                result.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                result.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                result.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                result.AddArc(arc, 90, 90);

                result.CloseFigure();
            }
            else
            {
                result.AddRectangle(rect);
            }

            return result;
        }

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
            //gridControl.ResizeColsBehavior = GridResizeCellsBehavior.None;
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
                gridControl.Invalidate();  // refresh w/out calling handler. /kosalanp.
                e.Handled = true;
            }

            if (e.KeyCode == Keys.Tab)
            {
                //Trace.WriteLine("TAB");
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
                    gridControl.Invalidate();  // refresh w/out calling handler. /kosalanp.
                    e.Handled = true;
                }
            }

            //stop move to next cell when we are at last col(arrow right)
            if (e.KeyCode == Keys.Right && gridControl.CurrentCell.ColIndex == gridControl.ColCount)
                e.Handled = true;

            //stop move to next cell when we are att first col(arrow left)
            if (e.KeyCode == Keys.Left && gridControl.CurrentCell.ColIndex == gridControl.Cols.HeaderCount + 1)
                e.Handled = true;
        }

        private static void HandleTabOnGridFirstAndLastCell(GridControl gridControl, bool shift)
        {
            //get current cell
            GridCurrentCell cell = gridControl.CurrentCell;

            //check if we are on the first row, first col and shift is pressed
            if (cell.RowIndex == gridControl.Rows.HeaderCount + 1 && cell.ColIndex == gridControl.Cols.HeaderCount + 1 && shift)
            {
                Form form = gridControl.FindParentForm();
                if (!form.SelectNextControl(gridControl, false, true, false, true))
                {
                    Console.Out.WriteLine("Could not find a Control to set focus on");
                }
                return;
            }

            //check if we are on last row, last col in grid and shift is not pressed
            if (cell.RowIndex == gridControl.RowCount && cell.ColIndex == gridControl.ColCount && shift == false)
            {
                Form form = gridControl.FindParentForm();
                form.SelectNextControl(gridControl, true, true, true, true);
                return;
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
    }
}
