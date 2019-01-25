using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.ScheduleReporting
{
    public class PdfScheduleAssignment : PdfScheduleTemplate
    {
        public PdfScheduleAssignment(float columnWidth, IScheduleDay schedulePart, TimeZoneInfo timeZoneInfo, bool rightToLeft, ScheduleReportDetail details, CultureInfo culture)
        {
            CultureInfo = culture;
            Brush = new PdfSolidBrush(Color.DimGray);

            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

            Format = new PdfStringFormat {RightToLeft = rightToLeft};
            ColumnWidth = columnWidth;

            const float top = 2;
            const float headerTop = top;

            IVisualLayerCollection projection = schedulePart.ProjectionService().CreateProjection();
        	var projectedPeriod = projection.Period().GetValueOrDefault();
            DateTime start = projectedPeriod.StartDateTimeLocal(timeZoneInfo);
            DateTime end = projectedPeriod.EndDateTimeLocal(timeZoneInfo);

        	var categoryName = string.Empty;
        	var categoryColor = Color.Empty;

					var personAssignment = schedulePart.PersonAssignment();
			if (personAssignment !=null && personAssignment.ShiftCategory!=null)
			{
				IShiftCategory category = personAssignment.ShiftCategory;
				categoryColor = category.DisplayColor;
				categoryName = category.Description.Name;
			}

        	Height = render(top, start, categoryName, end, projection.ContractTime(), categoryColor, headerTop, projection, timeZoneInfo, details, schedulePart.Person);


            Template.Reset(new SizeF(columnWidth, Height));
            Height = render(top, start, categoryName, end, projection.ContractTime(), categoryColor, headerTop, projection, timeZoneInfo, details, schedulePart.Person);
        }


        private float render(float top, DateTime startDateTime, string category, DateTime endDateTime, TimeSpan contractTime,
            Color categoryColor, float headerTop, IVisualLayerCollection payLoads, TimeZoneInfo timeZoneInfo, ScheduleReportDetail details, IPerson person)
        {
            top = RenderDate(startDateTime, top);
            top = RenderCategory(category, top);
            top = RenderPeriod(startDateTime, endDateTime, top);
            top = RenderContractTime(contractTime, top);
            float headerBottom = top - 3;

            Graphics.DrawLine(new PdfPen(categoryColor, 5), 7, headerTop, 7, headerBottom);

            if (details != ScheduleReportDetail.None)
            {
                if (payLoads.Any())
                    top = RenderSplitter(Color.Gray, top, 1);

                foreach (IVisualLayer visualLayer in payLoads)
                {
                    if(details == ScheduleReportDetail.Break)
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
            }
            return top;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private float RenderCategory(string category, float top)
        {
            Format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 9f;
            PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, CultureInfo);
            Graphics.DrawString(category, font, Brush, new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2), Format);
            return top + fontSize + 2 + RowSpace;
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

        private const int MAX_NUMBER_OF_CHARACTERS = 20;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString")]
        private float RenderPayLoad(IVisualLayer visualLayer, float top, TimeZoneInfo timeZoneInfo, IPerson person)
        {
            Format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 7f;

            PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, CultureInfo);
            var timeRect = new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2);

            Graphics.DrawString(visualLayer.Period.TimePeriod(timeZoneInfo).ToShortTimeString(), font, Brush, timeRect, Format);
            top = top + fontSize + 2;
            var nameRect = new RectangleF(0, top, ColumnWidth, fontSize + 2);
			Graphics.DrawString(visualLayer.Payload.ConfidentialDescription_DONTUSE(person).Name, font, Brush, nameRect, Format);
			top = top + fontSize + 2;
			nameRect = new RectangleF(0, top, ColumnWidth, fontSize + 2);

            var meetingPayload = visualLayer.Payload as IMeetingPayload;
            if (meetingPayload != null)
            {
                string subjectText = meetingPayload.Meeting.GetSubject(new NoFormatting());
                string locationText = meetingPayload.Meeting.GetLocation(new NoFormatting());
                if (subjectText.Length > MAX_NUMBER_OF_CHARACTERS)
                {
                    subjectText = subjectText.Substring(0, MAX_NUMBER_OF_CHARACTERS);
                }
                if (locationText.Length > MAX_NUMBER_OF_CHARACTERS)
                {
                    locationText = locationText.Substring(0, MAX_NUMBER_OF_CHARACTERS);
                }
                Graphics.DrawString(subjectText, font, Brush, nameRect, Format);
                nameRect = new RectangleF(0, top + fontSize + 2, ColumnWidth, fontSize + 2);
                Graphics.DrawString(locationText, font, Brush, nameRect, Format);
            }
            else
            {
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
            }

            float lineStart = timeRect.Top;
            float lineEnd = nameRect.Bottom;

            Graphics.DrawLine(new PdfPen(visualLayer.Payload.ConfidentialDisplayColor_DONTUSE(person), 5), 7, lineStart, 7, lineEnd);

            return nameRect.Bottom + 2;
            //return top + fontSize + 2 + RowSpace;
        }

        private static string ShortTimeString(TimeSpan timeSpan)
        {
            DateTime dt = DateTime.MinValue.Add(timeSpan);
            return dt.ToShortTimeString();
        }
    }
}