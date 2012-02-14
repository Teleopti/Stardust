using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting
{
    public class ShiftsPerDayToPdfManager
    {
        private PdfGraphics _graphics;
        private PdfSolidBrush _brush;
        private string _reportTitle;

        private const float RowSpace = 1;
		public PdfDocument Export(ICccTimeZoneInfo timeZoneInfo, CultureInfo culture, IDictionary<IPerson, string> persons,
            DateOnlyPeriod period, ISchedulingResultStateHolder stateHolder, bool rightToLeft, ScheduleReportDetail details)
        {
			if(persons == null)
				throw new ArgumentNullException("persons");

			if(stateHolder == null)
				throw new ArgumentNullException("stateHolder");

            var doc = new PdfDocument();
            if (details == ScheduleReportDetail.All)
                doc.PageSettings.Orientation = PdfPageOrientation.Landscape;
            
            _brush = new PdfSolidBrush(Color.DimGray);

            foreach (var dateOnly in period.DayCollection())
            {
                PdfPage page;
                float top = NewPage(doc, dateOnly, 10, true, rightToLeft, culture, out page, details);

                foreach (var person in persons.Keys)
                {
                    if (top > page.GetClientSize().Height - 30)
                        top = NewPage(doc, dateOnly, 10, true, rightToLeft, culture, out page, details);
                    IScheduleDictionary dic = stateHolder.Schedules;
                    IScheduleDay part = dic[person].ScheduledDay(dateOnly);
                   
                    var personPeriod = person.Period(dateOnly);
                    if(personPeriod != null)
                        top = DrawPersonSchedule(timeZoneInfo, top, part, rightToLeft, details, culture, doc.Pages[0].GetClientSize().Width, persons);
                }
            }

            AddHeader(doc, rightToLeft, culture);
            AddFooter(doc, rightToLeft, culture);
            return doc;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting.ShiftsPerDayToPdfManager.DrawColumnData(System.Single,System.Single,System.String,System.Single,System.Boolean,System.Globalization.CultureInfo)")]
		private float DrawPersonSchedule(ICccTimeZoneInfo timeZoneInfo, float top, IScheduleDay part, bool rightToLeft, ScheduleReportDetail details, CultureInfo culture, float pageWidth, IDictionary<IPerson, string> persons)
		{
			var personString = persons[part.Person];
		    var stringWidthHandler = new StringWidthHandler(9f, PdfFontStyle.Regular, 130, culture);
		    personString = stringWidthHandler.GetString(personString);

			float personTop = DrawColumnData(top, 0, personString, 130, rightToLeft, culture);
            float break1Top = top;
            float lunchTop = top;
            float break2Top = top;

            if (part.SignificantPart() == SchedulePartView.FullDayAbsence || part.SignificantPartForDisplay() == SchedulePartView.FullDayAbsence)
                return personTop;

            IVisualLayerCollection projection = part.ProjectionService().CreateProjection();
            if (projection.HasLayers && projection.Period().HasValue)
            {
                // ReSharper disable PossibleNullReferenceException
                DateTimePeriod period = projection.Period().Value;
                // ReSharper restore PossibleNullReferenceException
                DateTime start = period.StartDateTimeLocal(timeZoneInfo);
                DateTime end = period.EndDateTimeLocal(timeZoneInfo);
                DrawColumnData(top, 130, start.ToString("t", culture), 80, rightToLeft, culture);
                float left = 190;
                if (details != ScheduleReportDetail.None)
                {
                    bool beforeLunch = true;
                    // draw breaks too
                    foreach (IVisualLayer visualLayer in projection)
                    {
                        IActivity activity = visualLayer.Payload as Activity;
                        if (activity != null)
                        {
                            DateTimePeriod layerPeriod = visualLayer.Period;
                            string per = layerPeriod.TimePeriod(timeZoneInfo).ToShortTimeString(culture);
                            if (activity.ReportLevelDetail == ReportLevelDetail.ShortBreak)
                            {
                                if (beforeLunch)
                                {
                                    break1Top = DrawColumnData(break1Top, 190, per, 85, rightToLeft, culture);
                                }
                                else
                                {
                                    break2Top = DrawColumnData(break2Top, 390, per, 85, rightToLeft, culture);
                                }
                            }
                            if (activity.ReportLevelDetail == ReportLevelDetail.Lunch)
                            {
                                lunchTop = DrawColumnData(lunchTop, 290, per, 85, rightToLeft, culture);
                                beforeLunch = false;
                            }
                        }
                    }
                    left = 490;
                }
                top = DrawColumnData(top, left, end.ToString("t", culture), 80, rightToLeft, culture);
                //if it was more than one of anything
                if (lunchTop > top) top = lunchTop;
                if (break1Top > top) top = break1Top;
                if (break2Top > top) top = break2Top;
            }
            // if there was no layers
            if (personTop > top) top = personTop;
            DrawLine(top - 3, pageWidth, 1);
            return top;
        }

        private float DrawColumnHeader(float top, float left, string text, float width, bool rightToLeft, CultureInfo cultureInfo)
        {
            var format = new PdfStringFormat { RightToLeft = rightToLeft };

            const float fontSize = 10f;
            PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, cultureInfo);
            _graphics.DrawString(text, font, _brush, new RectangleF(left, top + RowSpace, width, fontSize + 2), format);
            return top + fontSize + 2 + ((RowSpace + 1) * 2);
        }

        private float DrawColumnData(float top, float left, string text, float width, bool rightToLeft, CultureInfo cultureInfo)
        {
            var format = new PdfStringFormat { RightToLeft = rightToLeft, WordWrap = PdfWordWrapType.None };

            const float fontSize = 9f;
            PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, cultureInfo);
            _graphics.DrawString(text, font, _brush, new RectangleF(left, top + RowSpace, width, fontSize + 2), format);
            return top + fontSize + 2 + ((RowSpace + 1) * 2);
        }



        private float NewPage(PdfDocument doc, DateOnly dateOnly, float top, bool newReport, bool rightToLeft, CultureInfo culture, out PdfPage page, ScheduleReportDetail details)
        {
            page = doc.Pages.Add();
            _graphics = page.Graphics;
            _reportTitle = Resources.ShiftsPerDayHeader + " " + dateOnly.Date.ToString("d", culture);

            if (newReport)
                top = DrawReportHeader(top, page.GetClientSize().Width, _reportTitle, rightToLeft, culture) + 3;
            float widthLeft = doc.Pages[0].GetClientSize().Width;

            DrawColumnHeader(top, 0, Resources.Name, 200, rightToLeft, culture);
            DrawColumnHeader(top, 130, Resources.StartTime, 80, rightToLeft, culture);
            float left = 190;
            if (details != ScheduleReportDetail.None)
            {
                DrawColumnHeader(top, 190, Resources.ReportLevelDetailShortBreak, 85, rightToLeft, culture);
                DrawColumnHeader(top, 290, Resources.ReportLevelDetailLunch, 85, rightToLeft, culture);
                DrawColumnHeader(top, 390, Resources.ReportLevelDetailShortBreak, 85, rightToLeft, culture);
                left = 490;
            }

            DrawColumnHeader(top, left, Resources.EndTime, 80, rightToLeft, culture);
            top = DrawColumnHeader(top, left + 80, Resources.Note, widthLeft - (left + 85), rightToLeft, culture);
            DrawLine(top - 3, widthLeft, 1.5f);
            return top;
        }

        private void DrawLine(float top, float width, float penWidth)
        {
            var pen = new PdfPen(Color.Gray, penWidth);
            _graphics.DrawLine(pen, 0, top, width, top);
        }

        private float DrawReportHeader(float top, float width, string text, bool rightToLeft, CultureInfo cultureInfo)
        {
            var format = new PdfStringFormat
            {
                RightToLeft = rightToLeft,
                Alignment = PdfTextAlignment.Left,
                LineAlignment = PdfVerticalAlignment.Middle
            };

            const float fontSize = 14f;
            PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, cultureInfo);
            var headerRect = new RectangleF(0, top + RowSpace, width, fontSize + 4);
            PdfBrush backGround = new PdfSolidBrush(Color.PowderBlue);
            _graphics.DrawRectangle(backGround, headerRect);
            _graphics.DrawString(text, font, _brush, headerRect, format);
            return top + fontSize + 4 + RowSpace;
        }

        private static void AddHeader(PdfDocument doc, bool rightToLeft, CultureInfo cultureInfo)
        {
            var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            //Create page template
            var header = new PdfPageTemplateElement(rect);
            var brush = new PdfSolidBrush(Color.Gray);

            PdfFont font = PdfFontManager.GetFont(6f, PdfFontStyle.Regular, cultureInfo);
            var format = new PdfStringFormat
            {
                RightToLeft = rightToLeft,
                LineAlignment = PdfVerticalAlignment.Top,
                Alignment = PdfTextAlignment.Right
            };
            //Create page number field
            var pageNumber = new PdfPageNumberField(font, brush);
            var createdDate = new PdfCreationDateField(font, brush);
            createdDate.DateFormatString = CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern;
           
            //Create page count field
            var count = new PdfPageCountField(font, brush);

            var compositeField = new PdfCompositeField(font, brush, Resources.CreatedPageOf, createdDate, pageNumber, count)
            {
                StringFormat = format,
                Bounds = header.Bounds
            };
            compositeField.Draw(header.Graphics);
            //Add header template at the top.
            doc.Template.Top = header;
        }

        private static void AddFooter(PdfDocument doc, bool rightToLeft, CultureInfo cultureInfo)
        {
            var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            //Create a page template
            var footer = new PdfPageTemplateElement(rect);

            var brush = new PdfSolidBrush(Color.Gray);
            var font = PdfFontManager.GetFont(6, PdfFontStyle.Bold, cultureInfo);

            var format = new PdfStringFormat
            {
                RightToLeft = rightToLeft,
                Alignment = PdfTextAlignment.Center,
                LineAlignment = PdfVerticalAlignment.Bottom
            };
            footer.Graphics.DrawString(Resources.PoweredByTeleoptCCC, font, brush, rect, format);

            doc.Template.Bottom = footer;

        }
    }
}
