using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.ScheduleReporting
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

			var projectedPeriod = projection.Period().GetValueOrDefault();
			var start = projectedPeriod.StartDateTimeLocal(timeZoneInfo);
			var end = projectedPeriod.EndDateTimeLocal(timeZoneInfo);

			Height = render(top, dayOff.Period.StartDateTimeLocal(timeZoneInfo), UserTexts.Resources.Overtime, projection, details, timeZoneInfo, start, end, schedulePart.Person);

            Template.Reset(new SizeF(columnWidth, Height));
            Height = render(top, dayOff.Period.StartDateTimeLocal(timeZoneInfo), UserTexts.Resources.Overtime, projection, details, timeZoneInfo, start, end, schedulePart.Person);
        }


        private float render(float top, DateTime startDateTime, string text, IVisualLayerCollection projection, ScheduleReportDetail details, TimeZoneInfo timeZoneInfo, DateTime start, DateTime end, IPerson person)
        {
            top = RenderDate(startDateTime, top);
            top = RenderText(text, top);
            top = RenderGraphics(top);
	        top = RenderPeriod(start, end, top);
	        if (details == ScheduleReportDetail.None) return top;
	        top = RenderSplitter(Color.Gray, top, 1);
	        top = render(top, projection, timeZoneInfo, details, person);
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

	        return top + RowSpace + 10;
        }
        private float render(float top, IVisualLayerCollection payLoads, TimeZoneInfo timeZoneInfo, ScheduleReportDetail details, IPerson person)
        {
	        if (details == ScheduleReportDetail.None) return top;

	        foreach (var visualLayer in payLoads)
	        {
		        if (details == ScheduleReportDetail.Break)
		        {
			        var activty = visualLayer.Payload as IActivity;
			        if (activty != null)
			        {
				        if (activty.ReportLevelDetail != ReportLevelDetail.None)
				        {
					        top = RenderPayLoad(visualLayer, top, timeZoneInfo, person);
				        }
			        }
		        }
		        else
		        {
			        top = RenderPayLoad(visualLayer, top, timeZoneInfo, person);
		        }
	        }
	        return top;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private float RenderPeriod(DateTime startDateTime, DateTime endDateTime, float top)
		{

			Format.Alignment = PdfTextAlignment.Center;
			const float fontSize = 8f;
			PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, CultureInfo);
			Graphics.DrawString(
				ShortTimeString(startDateTime.TimeOfDay) + "--" + ShortTimeString(endDateTime.TimeOfDay), font, Brush,
				new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2), Format);
			return top + fontSize + 2 + RowSpace;
		}

		private static string ShortTimeString(TimeSpan timeSpan)
		{
			DateTime dt = DateTime.MinValue.Add(timeSpan);
			return dt.ToShortTimeString();
		}

        private float RenderPayLoad(IVisualLayer visualLayer, float top, TimeZoneInfo timeZoneInfo, IPerson person)
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
                string overtimeText = string.Concat(visualLayer.Payload.ConfidentialDescription_DONTUSE(person).Name, ", ",
                                                    visualLayer.DefinitionSet.Name);
                if (overtimeText.Length > MAX_NUMBER_OF_CHARACTERS)
                {
                    Graphics.DrawString(string.Concat(visualLayer.Payload.ConfidentialDescription_DONTUSE(person).Name, ", "), font, Brush, nameRect, Format);
                    nameRect = new RectangleF(0, top + fontSize + 2, ColumnWidth, fontSize + 2);
                    Graphics.DrawString(visualLayer.DefinitionSet.Name, font, Brush, nameRect, Format);
                }
                else
                    Graphics.DrawString(overtimeText, font, Brush, nameRect, Format);
            }
            else
                Graphics.DrawString(visualLayer.Payload.ConfidentialDescription_DONTUSE(person).Name, font, Brush, nameRect, Format);

            float lineStart = timeRect.Top;
            float lineEnd = nameRect.Bottom;

            Graphics.DrawLine(new PdfPen(visualLayer.Payload.ConfidentialDisplayColor_DONTUSE(person), 5), 7, lineStart, 7, lineEnd);

            return nameRect.Bottom + 2;
        }
    }
}
