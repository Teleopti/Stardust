using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory.ScheduleReporting
{
    internal class PdfScheduleAssignment : PdfScheduleTemplate
    {
        public PdfScheduleAssignment(float columnWidth, IScheduleDay schedulePart, TimeZoneInfo timeZoneInfo, bool rightToLeft, ScheduleReportDetail details, CultureInfo culture)
        {
            Brush = new PdfSolidBrush(Color.DimGray);

            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

	        Format = new PdfStringFormat {RightToLeft = rightToLeft};
	        ColumnWidth = columnWidth;

            float top = 2;
            float headerTop = top;

            IVisualLayerCollection projection = schedulePart.ProjectionService().CreateProjection();
            DateTime start = projection.Period().Value.StartDateTimeLocal(timeZoneInfo);
            DateTime end = projection.Period().Value.EndDateTimeLocal(timeZoneInfo);
            IShiftCategory category = schedulePart.PersonAssignment().ShiftCategory;


            Height = render(top, start, category.Description.Name, end, projection.ContractTime(),
                            category.DisplayColor, headerTop, projection, timeZoneInfo, details,culture, schedulePart.Person);


            Template.Reset(new SizeF(columnWidth, Height));
            Height = render(top, start, category.Description.Name, end, projection.ContractTime(),
                            category.DisplayColor, headerTop, projection, timeZoneInfo, details,culture, schedulePart.Person);

        }

        private float render(float top, DateTime startDateTime, string category, DateTime endDateTime, TimeSpan contractTime,
                             Color categoryColor, float headerTop, IVisualLayerCollection payLoads, TimeZoneInfo timeZoneInfo, ScheduleReportDetail details, CultureInfo culture, IPerson agent)
        {
            top = RenderDate(startDateTime, top,culture);
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
                        IActivity activty = visualLayer.Payload as IActivity;
                        if (activty != null)
                        {
                            if (activty.ReportLevelDetail != ReportLevelDetail.None)
                            {
                                top = RenderPayLoad(visualLayer, top, timeZoneInfo,agent);
                            }
                        }
                    }
                    else
                    {
                        top = RenderPayLoad(visualLayer, top, timeZoneInfo,agent);
                    }
                    
                    
                }
            }
            return top;
        }

        private float RenderCategory(string category, float top)
        {
            Format.Alignment = PdfTextAlignment.Center;
            float fontSize = 9f;
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, fontSize, PdfFontStyle.Bold);
            Graphics.DrawString(category, font, Brush, new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2), Format);
            return top + fontSize + 2 + RowSpace;
        }

        private float RenderPeriod(DateTime startDateTime, DateTime endDateTime, float top)
        {
            
            Format.Alignment = PdfTextAlignment.Center;
            float fontSize = 8f;
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, fontSize, PdfFontStyle.Bold);
            Graphics.DrawString(
                ShortTimeString(startDateTime.TimeOfDay) + "--" + ShortTimeString(endDateTime.TimeOfDay), font, Brush,
                new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2), Format);
            return top + fontSize + 2 + RowSpace;
        }

        private const int MAX_NUMBER_OF_CHARACTERS = 20;

        private float RenderPayLoad(IVisualLayer visualLayer, float top, TimeZoneInfo timeZoneInfo, IPerson agent)
        {
            Format.Alignment = PdfTextAlignment.Center;
            float fontSize = 7f;

            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, fontSize, PdfFontStyle.Regular);
            RectangleF timeRect = new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2);

            Graphics.DrawString(visualLayer.Period.TimePeriod(timeZoneInfo).ToShortTimeString(), font, Brush, timeRect, Format);
            top = top + fontSize + 2;
            RectangleF nameRect = new RectangleF(0, top, ColumnWidth, fontSize + 2);

            IMeetingPayload meetingPayload = visualLayer.Payload as IMeetingPayload;
            if (meetingPayload != null)
            {
                string subjectText = meetingPayload.Meeting.GetSubject(new NormalizeText());
                string locationText = meetingPayload.Meeting.GetLocation(new NormalizeText());
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
                Graphics.DrawString(visualLayer.Payload.ConfidentialDescription_DONTUSE(agent).Name, font, Brush, nameRect, Format);
            }

            float lineStart = timeRect.Top;
            float lineEnd = nameRect.Bottom;

            Graphics.DrawLine(new PdfPen(visualLayer.Payload.ConfidentialDisplayColor_DONTUSE(agent), 5), 7, lineStart, 7, lineEnd);

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