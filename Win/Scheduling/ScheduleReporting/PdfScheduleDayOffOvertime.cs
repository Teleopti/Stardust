using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
    public class PdfScheduleDayOffOvertime : PdfScheduleTemplate
    {
        private const int MAX_NUMBER_OF_CHARACTERS = 20;

        public PdfScheduleDayOffOvertime(float columnWidth, IScheduleDay schedulePart, IPersonAssignment dayOff, TimeZoneInfo timeZoneInfo, bool rightToLeft, ScheduleReportDetail details, CultureInfo culture)
        {
            CultureInfo = culture;
            Brush = new PdfSolidBrush(Color.DimGray);

            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

            Format = new PdfStringFormat {RightToLeft = rightToLeft};
            ColumnWidth = columnWidth;

            const float top = 2;
            IVisualLayerCollection projection = schedulePart.ProjectionService().CreateProjection();
            Height = render(top, dayOff.Period.StartDateTimeLocal(timeZoneInfo), dayOff.DayOff().Description.Name, projection, details, timeZoneInfo);

            Template.Reset(new SizeF(columnWidth, Height));
            Height = render(top, dayOff.Period.StartDateTimeLocal(timeZoneInfo), dayOff.DayOff().Description.Name, projection, details, timeZoneInfo);
        }


        private float render(float top, DateTime startDateTime, string text, IVisualLayerCollection projection, ScheduleReportDetail details, TimeZoneInfo timeZoneInfo)
        {
            top = RenderDate(startDateTime, top);
            top = RenderText(text, top);
            top = RenderGraphics(top);
            top = RenderSplitter(Color.Gray, top, 1);
            top = render(top, projection, timeZoneInfo, details);
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

            return top + RowSpace + 21;
        }
        private float render(float top, IVisualLayerCollection payLoads, TimeZoneInfo timeZoneInfo, ScheduleReportDetail details)
        {
            if (details != ScheduleReportDetail.None)
            {
                foreach (IVisualLayer visualLayer in payLoads)
                {
                    if (details == ScheduleReportDetail.Break)
                    {
                        var activty = visualLayer.Payload as IActivity;
                        if (activty != null)
                        {
                            if (activty.ReportLevelDetail != ReportLevelDetail.None)
                            {
                                top = RenderPayLoad(visualLayer, top, timeZoneInfo);
                            }
                        }
                    }
                    else
                    {
                        top = RenderPayLoad(visualLayer, top, timeZoneInfo);
                    }
                }
            }
            return top;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString")]
        private float RenderPayLoad(IVisualLayer visualLayer, float top, TimeZoneInfo timeZoneInfo)
        {
            Format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 7f;

            PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, CultureInfo);
            var timeRect = new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2);

            Graphics.DrawString(visualLayer.Period.TimePeriod(timeZoneInfo).ToShortTimeString(), font, Brush, timeRect, Format);
            top = top + fontSize + 2;
            var nameRect = new RectangleF(0, top, ColumnWidth, fontSize + 2);
  
            if (visualLayer.DefinitionSet != null)
            {
                string overtimeText = string.Concat(visualLayer.DisplayDescription().Name, ", ",
                                                    visualLayer.DefinitionSet.Name);
                if (overtimeText.Length > MAX_NUMBER_OF_CHARACTERS)
                {
                    Graphics.DrawString(string.Concat(visualLayer.DisplayDescription().Name, ", "), font, Brush, nameRect, Format);
                    nameRect = new RectangleF(0, top + fontSize + 2, ColumnWidth, fontSize + 2);
                    Graphics.DrawString(visualLayer.DefinitionSet.Name, font, Brush, nameRect, Format);
                }
                else
                    Graphics.DrawString(overtimeText, font, Brush, nameRect, Format);
            }
            else
                Graphics.DrawString(visualLayer.DisplayDescription().Name, font, Brush, nameRect, Format);

            float lineStart = timeRect.Top;
            float lineEnd = nameRect.Bottom;

            Graphics.DrawLine(new PdfPen(visualLayer.DisplayColor(), 5), 7, lineStart, 7, lineEnd);

            return nameRect.Bottom + 2;
        }
    }
}
