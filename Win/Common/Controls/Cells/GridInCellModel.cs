
using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Diagnostics;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    /// <summary>
    /// Summary description for GridInCell.
    /// </summary>
    [Serializable]
    public class GridInCellModel : GridGenericControlCellModel
    {
        protected GridInCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public GridInCellModel(GridModel grid)
            : base(grid)
        {
            AllowFloating = false;
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new GridInCellRenderer(control, this);
        }


    }

    public class GridInCellRenderer : GridGenericControlCellRenderer
    {
        private CellEmbeddedGrid activeGrid;

        public GridInCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
            : base(grid, cellModel)
        {
            SupportsFocusControl = true;
        }

        protected override void OnDraw(System.Drawing.Graphics g, System.Drawing.Rectangle clientRectangle, int rowIndex, int colIndex, Syncfusion.Windows.Forms.Grid.GridStyleInfo style)
        {
            if (ShouldDrawFocused(rowIndex, colIndex))
            {
                if (style.Control is CellEmbeddedGrid)
                {
                    activeGrid = (CellEmbeddedGrid)style.Control;
                    AlignColumn(activeGrid);
                }
                base.OnDraw(g, clientRectangle, rowIndex, colIndex, style);
            }
            else
            {
                // Draw a static grid
                if (style.Control is CellEmbeddedGrid)
                {
                    var grid = (CellEmbeddedGrid)style.Control;
                    AlignColumn(grid);
                    grid.DrawGrid(g, clientRectangle, true);
                }
            }
        }

        private void AlignColumn(CellEmbeddedGrid cellEmbeddedGrid)
        {
            var hideState = true;
            if (cellEmbeddedGrid.RightToLeft == RightToLeft.Yes)
            {
                if (Grid.HScrollBar.Maximum < Grid.HScrollBar.Minimum + Grid.HScrollBar.Value + Grid.HScrollBar.LargeChange) { hideState = false; }
            }
            else
            {
                if (Grid.HScrollBar.Value < 3) { hideState = false; } //At this position the columns end up being unaligned 
            }
            cellEmbeddedGrid.Cols.Hidden[1] = hideState;
        }

        protected override bool ProcessKeyEventArgs(ref Message m)
        {
            TraceUtil.TraceCurrentMethodInfo(m.ToString());

            // forward keyboard events to child grid that would otherwise 
            // be handled by parent grid (right arrow, page down etc.)
            if (activeGrid != null && activeGrid.Focused)
                return activeGrid.InitiateProcessKeyEventArgs(ref m);

            return base.ProcessKeyEventArgs(ref m);
        }
    }

    public class CellEmbeddedGrid : GridControl
    {
        public CellEmbeddedGrid()//GridControl parent
        {
            this.RowHeights[0] = this.RowHeights[1];
            this.DefaultColWidth = 110;

            //this.FloatCellsMode = GridFloatCellsMode.OnDemandCalculation;
            this.VScrollBehavior = GridScrollbarMode.Disabled;
            this.HScrollBehavior = GridScrollbarMode.Disabled;
            //this.BorderStyle = BorderStyle.None;
            this.Location = new Point(-10000, -10000);
            //this.ActivateCurrentCellBehavior = GridCellActivateAction.PositionCaret;
            //this.ShowCurrentCellBorderBehavior = GridShowCurrentCellBorder.GrayWhenLostFocus;
            //this.VerticalThumbTrack = true;
            //this.HorizontalThumbTrack = true;
            //this.FillSplitterPane = false;
            //this.Properties.GridLineColor = System.Drawing.Color.Silver;
            //this.DefaultGridBorderStyle = GridBorderStyle.Solid;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            TraceUtil.TraceCurrentMethodInfo(e.KeyCode);
            base.OnKeyDown(e);

            //TODO: Need to refactory

            //try
            //{
            //    base.OnKeyDown(e);
            //}
            //catch (FormatException)
            //{
            //}

            //int rowIndex = base.CurrentCell.RowIndex;
            //int colIndex = base.CurrentCell.ColIndex;

            //base.CurrentCell.Deactivate(false);


            //switch (e.KeyCode)
            //{
            //    case Keys.Right:
            //        {
            //            base.CurrentCell.MoveTo(rowIndex, colIndex + 1);
            //            break;
            //        }
            //    case Keys.Left:
            //        {
            //            base.CurrentCell.MoveTo(rowIndex, colIndex - 1);
            //            break;
            //        }
            //    case Keys.Down:
            //        {
            //            base.CurrentCell.MoveTo(rowIndex + 1, colIndex);
            //            break;
            //        }
            //    case Keys.Up:
            //        {
            //            base.CurrentCell.MoveTo(rowIndex - 1, colIndex);
            //            break;
            //        }
            //    case Keys.Tab:
            //        {
            //            if (colIndex < base.ColCount)
            //            {
            //                base.CurrentCell.MoveTo(rowIndex, colIndex + 1);
            //            }

            //            else if (rowIndex < base.RowCount)
            //            {
            //                base.CurrentCell.MoveTo(rowIndex + 1, 2);
            //            }
            //            break;
            //        }
            //}


            //base.Invalidate();
        }

        internal bool InitiateProcessKeyEventArgs(ref Message m)
        {
            return base.ProcessKeyEventArgs(ref m);
        }
    }
}
