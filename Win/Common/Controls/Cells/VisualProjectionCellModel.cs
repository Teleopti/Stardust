using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Scheduling.Panels;
using Teleopti.Interfaces.Domain;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class VisualProjectionCellModel : GridStaticCellModel
    {
        public VisualProjectionCellModel(GridModel grid)
            : base(grid)
        {
        }

        protected VisualProjectionCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {

            if (info == null)
                throw new ArgumentNullException("info");

            //Hmm...
            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new VisualProjectionCellRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            return false;
        }

        public override bool ApplyText(GridStyleInfo style, string text)
        {
            return ApplyFormattedText(style, text, -1);
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            return string.Empty;
        }
    }

    public class VisualProjectionCellRenderer : GridStaticCellRenderer
    {
        private readonly IDictionary<object, IList<RectangleLayerInfo>> _rectangleCache =
            new Dictionary<object, IList<RectangleLayerInfo>>();

        public VisualProjectionCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
            : base(grid, cellModel)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString")]
        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
			if (style.Tag == null) return;
            DateTimePeriod timePeriod = (DateTimePeriod) style.Tag;
            LengthToTimeCalculator pixelConverter = new LengthToTimeCalculator(timePeriod, clientRectangle.Width);
			
			// we can't use this it cashes too much :-)
        	_rectangleCache.Remove(style.CellValue);

            foreach (var interval in timePeriod.AffectedHourCollection())
            {
                var position = pixelConverter.PositionFromDateTime(interval.StartDateTime, Grid.IsRightToLeft());
                var x = (int) Math.Round(position, 0);
                if (x > 0)
                    g.DrawLine(new Pen(Color.LightGray, 1), clientRectangle.Left + x, clientRectangle.Top,
                               clientRectangle.Left + x, clientRectangle.Bottom);
            }

            IList<RectangleLayerInfo> rectangleLayerInfoFromCache;
            if (!_rectangleCache.TryGetValue(style.CellValue, out rectangleLayerInfoFromCache))
            {
                rectangleLayerInfoFromCache = new List<RectangleLayerInfo>();
                foreach (IVisualLayer layer in (IList<IVisualLayer>)style.CellValue)
                {
                    Rectangle rect = pixelConverter.RectangleFromDateTimePeriod(layer.Period, new Point(0,clientRectangle.Y), 
                                                                                clientRectangle.Height,
                                                                                Grid.IsRightToLeft());
                    if (!rect.IsEmpty)
                    {
                        rectangleLayerInfoFromCache.Add(new RectangleLayerInfo
                                                            {
                                                                DisplayColor = layer.DisplayColor(),
                                                                Rectangle = rect,
                                                                Text =
                                                                    string.Format(CultureInfo.CurrentUICulture, "{0}: {1}",
                                                                                  layer.DisplayDescription(),
                                                                                  layer.Period.TimePeriodLocal().ToShortTimeString())
                                                            });
                    }
                }
				if (rectangleLayerInfoFromCache.Count>0)
				    _rectangleCache.Add(style.CellValue, rectangleLayerInfoFromCache);
            }

            foreach (var rectangleLayerInfo in rectangleLayerInfoFromCache)
            {
                Rectangle rectangle = rectangleLayerInfo.Rectangle;
                if (Grid.IsRightToLeft())
                {
                    rectangle.Offset(Grid.HScrollBar.Minimum - Grid.HScrollBar.Value,
                                     Grid.VScrollBar.Minimum - Grid.VScrollBar.Value);
                }
                else
                {
                    rectangle.Offset((Grid.HScrollBar.Minimum * 2) - Grid.HScrollBar.Value, Grid.VScrollBar.Minimum - Grid.VScrollBar.Value);
                }
                RectangleF rect2 = rectangle;
                rect2.Inflate(1, 1);
                using (
                    Brush brush = new LinearGradientBrush(rect2, Color.WhiteSmoke, rectangleLayerInfo.DisplayColor,
                                                          90, false))
                {
                    g.FillRectangle(brush, rectangle);
                }
            }
        }

        public void ShowToolTip(ToolTip toolTip, Point mousePosition, Point realPosition, IWin32Window owner, GridStyleInfo style)
        {
            Point positionToFind = realPosition;
            IList<RectangleLayerInfo> rectangleInfoFromCache;
            if (_rectangleCache.TryGetValue(style.CellValue, out rectangleInfoFromCache))
            {


                foreach (var layerInfo in rectangleInfoFromCache)
                {
                    Rectangle rectangle = layerInfo.Rectangle;
                    if (rectangle.Contains(positionToFind))
                    {
                        if (!toolTip.Active || toolTip.GetToolTip((Control)owner) != layerInfo.Text)
                        {
                            toolTip.Show(layerInfo.Text, owner, mousePosition);
                        }
                        return;
                    }
                }
            }
            if (toolTip.Active) toolTip.Hide(owner);
        }

        private class RectangleLayerInfo
        {
            public Rectangle Rectangle{ get; set; }
            public string Text{ get; set;}
            public Color DisplayColor { get; set;}
        }
    }
}