using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Helper
{
    public static class GridHelper
    {
        public static void FillRoundedRectangle(Graphics graphics, Rectangle rectangle, int roundness, Brush brush, int inflate)
        {
            Rectangle rect = rectangle;
            rect.Inflate(inflate, inflate);
            graphics.DrawPath(Pens.LightBlue, GetRoundBox(rect, roundness));
            graphics.FillPath(brush, GetRoundBox(rect, roundness));
        }

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
            gridControl.ResizeRowsBehavior = GridResizeCellsBehavior.None;
        }

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
    }
}
