using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Reports;
using Teleopti.Ccc.AgentPortal.Schedules;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Cells
{
    [Serializable]
    public class ScheduleAdherenceCellModel : GridStaticCellModel
    {
        public ScheduleAdherenceCellModel(GridModel grid)
            : base(grid)
        {
        }

        protected ScheduleAdherenceCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
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
            return new ScheduleAdherenceCellRenderer(control, this);
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

    public class ScheduleAdherenceCellRenderer : GridStaticCellRenderer
    {

        public ScheduleAdherenceCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
            : base(grid, cellModel)
        {
        }

        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            ScheduleAdherence cellValue = (ScheduleAdherence) style.CellValue;
            TimePeriod timePeriod = (TimePeriod)style.Tag;
            PixelConverter pixelConverter = new PixelConverter(clientRectangle.Width, timePeriod, Grid.IsRightToLeft());

            for (int i = (int)timePeriod.StartTime.TotalHours; i <= (int)timePeriod.EndTime.TotalHours; i++)
            {
                int x = pixelConverter.GetPixelFromTimeSpan(TimeSpan.FromHours(i));
                if (x > 0)
                    g.DrawLine(new Pen(Color.LightGray, 1), clientRectangle.Left + x, clientRectangle.Top, clientRectangle.Left + x, clientRectangle.Bottom);
            }



            foreach (ActivityVisualLayer layer in cellValue.VisualProjection.LayerCollection)
            {
                RectangleF c = clientRectangle;
                c.Inflate(0, -(clientRectangle.Height / 4));
                c.Y = clientRectangle.Y + ((clientRectangle.Height / 4)/2);
                RectangleF rect = getLayerRectangle(pixelConverter, layer.Period, c);
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

            foreach (AdherenceLayer adherenceLlayer in cellValue.AdherenceLayerCollection)
            {
                RectangleF rect = getLayerRectangle(pixelConverter, adherenceLlayer.Period, clientRectangle);
                if (!rect.IsEmpty)
                {
                    RectangleF rect2 = rect;
                    rect2.Inflate(1, 1);

                    Font font = new Font("Arial", 8, FontStyle.Regular);
                    string displayString = Math.Round(adherenceLlayer.Adherence, 0).ToString(CultureInfo.InvariantCulture);
                    SizeF size = g.MeasureString(displayString, font);

                    g.DrawLine(new Pen(Color.DarkGray, 2), rect2.X, clientRectangle.Top + 6, rect2.X, clientRectangle.Bottom - 6);
                    g.DrawLine(new Pen(Color.DarkGray, 2), rect2.X + rect2.Width - 2, clientRectangle.Top + 6, rect2.X + rect2.Width - 2, clientRectangle.Bottom - 6);
                    if(size.Width < rect2.Width)
                        g.DrawString(displayString, font, Brushes.Black,
                                     new PointF(rect2.X + (rect2.Width / 2) - (size.Width / 2),
                                                clientRectangle.Top + clientRectangle.Height/2 + 3));

                }
            }

            
        }

        private RectangleF getLayerRectangle(PixelConverter pixelConverter, TimePeriod period, RectangleF clientRect)
        {
            if (clientRect.Height < 9)
                return RectangleF.Empty;

            int x1 = pixelConverter.GetPixelFromTimeSpan(period.StartTime);
            int x2 = pixelConverter.GetPixelFromTimeSpan(period.EndTime);

            if (Grid.IsRightToLeft())
            {
                int tmp = x1;
                x1 = x2;
                x2 = tmp;
            }

            if (x2 - x1 < 1)
                return RectangleF.Empty;
            return new RectangleF(clientRect.Left + x1, clientRect.Top + 4, x2 - x1, clientRect.Height - 8);
        }
    }
}
