using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Diagnostics;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
	[System.ComponentModel.DesignerCategory("Code")]
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

        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            if (ShouldDrawFocused(rowIndex, colIndex))
            {
                var embeddedGrid = style.Control as CellEmbeddedGrid;
                if (embeddedGrid != null)
                {
                    activeGrid = embeddedGrid;
                    AlignColumn(activeGrid);
                }
                base.OnDraw(g, clientRectangle, rowIndex, colIndex, style);
            }
            else
            {
                // Draw a static grid
                var embeddedGrid = style.Control as CellEmbeddedGrid;
                if (embeddedGrid != null)
                {
                    var grid = embeddedGrid;
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
        public CellEmbeddedGrid()
        {
            RowHeights[0] = RowHeights[1];
            DefaultColWidth = 110;
            VScrollBehavior = GridScrollbarMode.Disabled;
            HScrollBehavior = GridScrollbarMode.Disabled;
            Location = new Point(-10000, -10000);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            TraceUtil.TraceCurrentMethodInfo(e.KeyCode);
            base.OnKeyDown(e);
        }

        internal bool InitiateProcessKeyEventArgs(ref Message m)
        {
            return base.ProcessKeyEventArgs(ref m);
        }
    }
}
