using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Messaging.Management.Model;

namespace Teleopti.Messaging.Management.Controls
{
    public class PerformanceGrid : GridControl
    {
        public PerformanceGrid() : base(new PerformanceGridModel())
        {
            UseGDI = true;
            UseDoubleBuffer = false;
            Model.UseGridNonVirtualDataCache = true;
            OptimizeDrawBackground = true;
            OptimizeInsertRemoveCells = true;
            SupportsPrepareViewStyleInfo = false;
            bool drawBorder = true;
            if (drawBorder)
            {
                Model.TableStyle.Borders.Right = new GridBorder(GridBorderStyle.Solid, SystemColors.Control, GridBorderWeight.Thin);
                Model.TableStyle.Borders.Bottom = new GridBorder(GridBorderStyle.Solid, SystemColors.Control, GridBorderWeight.Thin);
                // Use solid line - faster than dotted line
            }
            else
                // No border at all is of course faster.
                Model.TableStyle.Borders.All = GridBorder.Empty;

        }

        public new PerformanceGridModel Model
        {
            get
            {
                return (PerformanceGridModel)base.Model;
            }
            set
            {
                base.Model = value;
            }
        }

        private bool _useGDI;

        /// <summary>
        /// Property UseGDI (bool)
        /// </summary>
        public bool UseGDI
        {
            get
            {
                return _useGDI;
            }
            set
            {
                if (_useGDI != value)
                {
                    _useGDI = value;
                    Invalidate();
                }
            }
        }

        private bool _useDoubleBuffer = true;

        /// <summary>
        /// Property UseDoubleBuffer (bool)
        /// </summary>
        public bool UseDoubleBuffer
        {
            get
            {
                return _useDoubleBuffer;
            }
            set
            {
                if (_useDoubleBuffer != value)
                {
                    _useDoubleBuffer = value;
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (UseDoubleBuffer)
                base.OnPaintBackground(pevent);
        }

        protected override void OnDrawCellDisplayText(GridDrawCellDisplayTextEventArgs e)
        {
            base.OnDrawCellDisplayText(e);

            if (!UseGDI || e.Cancel)
                return;

            e.Cancel = GridGdiPaint.Instance.DrawText(e.Graphics, e.DisplayText, e.TextRectangle, e.Style);
        }

        protected override void OnFillRectangleHook(GridFillRectangleHookEventArgs e)
        {
            base.OnFillRectangleHook(e);

            if (!UseGDI || e.Cancel)
                return;

            e.Cancel = GridGdiPaint.Instance.FillRectangle(e.Graphics, e.Bounds, e.Brush);
        }

    }
}
