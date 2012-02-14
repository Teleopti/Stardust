using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;

namespace GridTest
{
    [Serializable]
    public class VisualProjectionCellModel : GridStaticCellModel
    {
        private VisualProjection _cellValue;

        public VisualProjectionCellModel(GridModel grid)
            : base(grid)
        {
        }

        protected VisualProjectionCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected internal VisualProjection CellValue
        {
            get { return _cellValue; }
            set { _cellValue = value; }
        }

        
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
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

        public VisualProjectionCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
            : base(grid, cellModel)
        {
        }

        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            TimePeriod timePeriod = (TimePeriod) style.Tag;
            PixelConverter pixelConverter = new PixelConverter(clientRectangle.Width, timePeriod, false);
            
            for (int i = (int)timePeriod.StartTime.TotalHours; i <= (int)timePeriod.EndTime.TotalHours ; i++)
            {
                int x = pixelConverter.GetPixelFromTimeSpan(TimeSpan.FromHours(i));
                if (x > 0)
                    g.DrawLine(new Pen(Color.LightGray, 1), clientRectangle.Left + x, clientRectangle.Top, clientRectangle.Left + x, clientRectangle.Bottom);
            }
            foreach (VisualLayer layer in ((IList<VisualLayer>)style.CellValue))
            {
                RectangleF rect = getLayerRectangle(pixelConverter, layer.Period, clientRectangle);
                if (!rect.IsEmpty)
                {
                    RectangleF rect2 = rect;
                    rect2.Inflate(1, 1);
                    using (Brush brush = new LinearGradientBrush(rect2, Color.WhiteSmoke, layer.Color, 90, false))
                    {
                        g.FillRectangle(brush, rect);
                    }
                }
            }
        }

        private static RectangleF getLayerRectangle(PixelConverter pixelConverter, TimePeriod period, RectangleF clientRect)
        {
            if (clientRect.Height < 9)
                return RectangleF.Empty;
            
            int x1 = pixelConverter.GetPixelFromTimeSpan(period.StartTime);
            int x2 = pixelConverter.GetPixelFromTimeSpan(period.EndTime);
            if(x2-x1 < 1)
                return RectangleF.Empty;
            return new RectangleF(clientRect.Left + x1, clientRect.Top + 4, x2-x1, clientRect.Height - 8);
        }
    }

    
}