﻿using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
    public class PdfScheduleAssignment : PdfScheduleTemplate
    {
        public PdfScheduleAssignment(float columnWidth, IScheduleDay schedulePart, ICccTimeZoneInfo timeZoneInfo, bool rightToLeft, ScheduleReportDetail details, CultureInfo culture)
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
            DateTime start = projection.Period().Value.StartDateTimeLocal(timeZoneInfo);
            DateTime end = projection.Period().Value.EndDateTimeLocal(timeZoneInfo);
            IShiftCategory category = schedulePart.PersonAssignmentCollection()[0].MainShift.ShiftCategory;


            Height = render(top, start, category.Description.Name, end, projection.ContractTime(),
                             category.DisplayColor, headerTop, projection, timeZoneInfo, details);


            Template.Reset(new SizeF(columnWidth, Height));
            Height = render(top, start, category.Description.Name, end, projection.ContractTime(),
                             category.DisplayColor, headerTop, projection, timeZoneInfo, details);
        }


        private float render(float top, DateTime startDateTime, string category, DateTime endDateTime, TimeSpan contractTime,
            Color categoryColor, float headerTop, IVisualLayerCollection payLoads, ICccTimeZoneInfo timeZoneInfo, ScheduleReportDetail details)
        {
            top = RenderDate(startDateTime, top);
            top = RenderCategory(category, top);
            top = RenderPeriod(startDateTime, endDateTime, top);
            top = RenderContractTime(contractTime, top);
            float headerBottom = top - 3;

            Graphics.DrawLine(new PdfPen(categoryColor, 5), 7, headerTop, 7, headerBottom);

            if (details != ScheduleReportDetail.None)
            {
                if (payLoads.Count() > 0)
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
        private float RenderPayLoad(IVisualLayer visualLayer, float top, ICccTimeZoneInfo timeZoneInfo)
        {
            Format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 7f;

            PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, CultureInfo);
            var timeRect = new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2);

            Graphics.DrawString(visualLayer.Period.TimePeriod(timeZoneInfo).ToShortTimeString(), font, Brush, timeRect, Format);
            top = top + fontSize + 2;
            var nameRect = new RectangleF(0, top, ColumnWidth, fontSize + 2);

            var meetingPayload = visualLayer.Payload as IMeetingPayload;
            if (meetingPayload != null)
            {
                string subjectText = meetingPayload.Meeting.Subject;
                string locationText = meetingPayload.Meeting.Location;
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
            }

            float lineStart = timeRect.Top;
            float lineEnd = nameRect.Bottom;

            Graphics.DrawLine(new PdfPen(visualLayer.DisplayColor(), 5), 7, lineStart, 7, lineEnd);

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