using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Forecasting
{
    public partial class ForecastGraphsControl : UserControl
    {
        private readonly IDrawingBehavior _drawing;
        private readonly IDrawSmartPart _drawSmartPart;
        private readonly SuperToolTip _forecasterToolTip = new SuperToolTip();
        private readonly string[] _monthNames;
        private NavigationControl _navigator;
        private const int DefaultColCount = 2;
        private int NameColumnWidth = 125;
        private const float NameColumnStartPosition = 0f;
        private Point _cachingPoint;

        public GridControl ProgressColumn
        {
            get { return gridControlProgress; }
        }

        public bool Timeline
        {
            set { gridControlTimeline.Visible = value; }
            get { return gridControlTimeline.Visible; }
        }

        public ForecastGraphsControl(IDrawingBehavior drawingBehavior, IDrawSmartPart drawSmartPart)
        {
            InitializeComponent();

            _drawing = drawingBehavior;
            _drawSmartPart = drawSmartPart;
            _monthNames = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture.DateTimeFormat.AbbreviatedMonthNames;

            InitializeDrawingGrid();
        }

        private void InitializeDrawingGrid()
        {
            gridControlProgress.ColCount = DefaultColCount;
            gridControlProgress.ColWidths[1] = NameColumnWidth;
            gridControlProgress.ColWidths[2] = gridControlProgress.Width - NameColumnWidth;
        }

        public void AddNavigator(NavigationControl navigator)
        {
            _navigator = navigator;
            tableLayoutPanelBase.Controls.Add(navigator, 0, 0);
        }

        private void gridControlProgress_CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            var drawProperties = MakeDrawProperties(e);
            _drawing.DrawNames(drawProperties);
            _drawing.DrawProgressGraphs(drawProperties);
            SyncronizeProgressAndTimeLine(e);
        }

        private void SyncronizeProgressAndTimeLine(EventArgs e)
        {
            if (gridControlTimeline.Width != gridControlProgress.GetVisibleBounds().Width - NameColumnWidth)
                OnResize(e);
        }

        private void gridControlProgress_CellMouseHover(object sender, GridCellMouseEventArgs e)
        {
            var drawPositionAndWidth = MakeDrawPositionAndWidth(e);
            Point cursor = (gridControlProgress.PointToClient(Cursor.Position));
            Point point = Cursor.Position;
            point.X += 10;
            point.Y += 20;
            if (point == _cachingPoint)
            {
                e.Cancel = true;
                return;
            }
            ToolTipInfo toolTipInfo = _drawing.SetTooltip(drawPositionAndWidth, cursor.X);
            if (!string.IsNullOrEmpty(toolTipInfo.Body.Text))
            {
                toolTipInfo.Header.TextAlign = ContentAlignment.TopCenter;
                toolTipInfo.Header.Font = _drawSmartPart.DefaultFontBold;
                toolTipInfo.BackColor = Color.White;
                _forecasterToolTip.Show(toolTipInfo, point);
                _cachingPoint = point;
            }
            else
                _forecasterToolTip.Hide();
        }

        private void gridControlProgress_CellMouseHoverLeave(object sender, GridCellMouseEventArgs e)
        {
            _forecasterToolTip.Hide();
        }

        private void gridControlTimeline_CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            Graphics graphics = e.Graphics;
            int noOfMonths = _monthNames.Length - 1;
            float pixelsPerMonth = (gridControlProgress.GetVisibleBounds().Width - NameColumnWidth) / (noOfMonths * 1.0f);
            int pointX = e.Bounds.X;

            for (int t = 0; t < noOfMonths; t++)
            {
                if (t > 0)
                {
                    Point top = new Point(pointX, e.Bounds.Y);
                    Point bottom = new Point(pointX, e.Bounds.Y + 14);
                    graphics.DrawLine(Pens.DarkBlue, top, bottom);
                }

                string monthName = _monthNames[t];
                float stringWidth = (graphics.MeasureString(monthName, _drawSmartPart.DefaultFont)).Width;
                float stringStartX = (pointX + pixelsPerMonth / 2) - (stringWidth / 2);
                graphics.DrawString(monthName, _drawSmartPart.DefaultFont, Brushes.DarkBlue, stringStartX, (e.Bounds.Y + 1));

                //for february
                pointX = t == 1 ? Convert.ToInt32(pointX + (pixelsPerMonth - 3)) : Convert.ToInt32(pointX + pixelsPerMonth);
            }
        }

        private DrawProperties MakeDrawProperties(GridDrawCellEventArgs e)
        {
            return new DrawProperties
            {
                ProgressStartPosition = NameColumnWidth,
                DrawingWidth = gridControlProgress.GetVisibleBounds().Width - NameColumnWidth,
                NameColumnStartPosition = NameColumnStartPosition,
                Graphics = e.Graphics,
                Bounds = e.Bounds,
                RowIndex = e.RowIndex,
                ColIndex = e.ColIndex
            };
        }

        private DrawPositionAndWidth MakeDrawPositionAndWidth(GridCellMouseEventArgs e)
        {
            return new DrawPositionAndWidth
            {
                ProgressStartPosition = NameColumnWidth,
                DrawingWidth = gridControlProgress.GetVisibleBounds().Width - NameColumnWidth,
                RowIndex = e.RowIndex,
                ColIndex = e.ColIndex,
                NameColumnStartPosition = NameColumnStartPosition
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_navigator != null)
                _navigator.Refresh();
            gridControlProgress.SetColWidth(2, 2, gridControlProgress.GetVisibleBounds().Width - NameColumnWidth);
            gridControlTimeline.Width = gridControlProgress.GetVisibleBounds().Width - NameColumnWidth;
            gridControlTimeline.Refresh();
            gridControlProgress.Refresh();
        }

        private void gridControlProgress_GridControlMouseDown(object sender, CancelMouseEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
