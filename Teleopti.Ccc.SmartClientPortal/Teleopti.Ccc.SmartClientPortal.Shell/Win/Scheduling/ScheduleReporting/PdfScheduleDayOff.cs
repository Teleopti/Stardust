using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
    public class PdfScheduleDayOff : PdfScheduleTemplate
    {

        public PdfScheduleDayOff(float columnWidth, IPersonAssignment dayOff, TimeZoneInfo timeZoneInfo, bool rightToLeft, CultureInfo culture)
        {
            CultureInfo = culture;
            Brush = new PdfSolidBrush(Color.DimGray);
           
            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

            Format = new PdfStringFormat {RightToLeft = rightToLeft};
            ColumnWidth = columnWidth;

            const float top = 2;
            Height = render(top, dayOff.Period.StartDateTimeLocal(timeZoneInfo), dayOff.DayOff().Description.Name);

            Template.Reset(new SizeF(columnWidth, Height));
            Height = render(top, dayOff.Period.StartDateTimeLocal(timeZoneInfo), dayOff.DayOff().Description.Name);
        }


        private float render(float top, DateTime startDateTime, string text)
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

            //RectangleF rectangle = new RectangleF(7, 2, 5, 50);
            //_graphics.DrawRectangle(tillingBrush, rectangle);
            var rectangle = new RectangleF(7, 28, ColumnWidth - 14, 5);
            Graphics.DrawRectangle(tillingBrush, rectangle);

            return top + RowSpace + 5;
        }
    }
}
