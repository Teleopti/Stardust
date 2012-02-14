using System;
using System.Drawing;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.ScheduleReporting
{
    public class PdfScheduleDayOff : PdfScheduleTemplate
    {

        public PdfScheduleDayOff(float columnWidth, PersonDayOffDto dayOff, bool rightToLeft)
        {
            Brush = new PdfSolidBrush(Color.DimGray);
           
            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

            Format = new PdfStringFormat {RightToLeft = rightToLeft};
            ColumnWidth = columnWidth;

            const float top = 2;
            Height = Render(top, dayOff.Period.LocalStartDateTime, dayOff.Name);

            Template.Reset(new SizeF(columnWidth, Height));
            Height = Render(top, dayOff.Period.LocalStartDateTime, dayOff.Name);
        }


        private float Render(float top, DateTime startDateTime, string text)
        {
            top = RenderDate(startDateTime, top);
            top = RenderText(text, top);
            top = RenderGraphics(top);
            return top;
        }

        protected float RenderGraphics(float top)
        {
            var rect = new RectangleF(0, 0, 5, 5);
            var tillingBrush = new PdfTilingBrush(rect);
            var pen = new PdfPen(Color.Gray, 1);
            tillingBrush.Graphics.DrawLine(pen, 0, 5, 5, 0);
            var rectangle = new RectangleF(7, 28, ColumnWidth - 14, 5);
            Graphics.DrawRectangle(tillingBrush, rectangle);

            return top + RowSpace + 5;
        }
    }
}
