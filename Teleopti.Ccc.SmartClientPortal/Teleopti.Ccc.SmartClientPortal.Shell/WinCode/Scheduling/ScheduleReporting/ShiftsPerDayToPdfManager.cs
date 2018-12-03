using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting
{
    public class ShiftsPerDayToPdfManager
    {
        private PdfGraphics _graphics;
        private PdfSolidBrush _brush;
        private string _reportTitle;

        private const float RowSpace = 1;
		public PdfDocument Export(TimeZoneInfo timeZoneInfo, CultureInfo culture, IDictionary<IPerson, string> persons, DateOnlyPeriod period, 
            ISchedulingResultStateHolder stateHolder, ScheduleReportDetail details, bool publicNote)
        {
			if(persons == null)
				throw new ArgumentNullException("persons");

			if(stateHolder == null)
				throw new ArgumentNullException("stateHolder");

            var doc = new PdfDocument();
            if (details == ScheduleReportDetail.All || details == ScheduleReportDetail.Break)
                doc.PageSettings.Orientation = PdfPageOrientation.Landscape;
            
            _brush = new PdfSolidBrush(Color.DimGray);

            foreach (var dateOnly in period.DayCollection())
            {
                PdfPage page;
                float top = NewPage(doc, dateOnly, 10, true, culture, out page, details);

                foreach (var person in persons.Keys)
                {
                    if (top > page.GetClientSize().Height - 30)
                        top = NewPage(doc, dateOnly, 10, true, culture, out page, details);
                    IScheduleDictionary dic = stateHolder.Schedules;
                    IScheduleDay part = dic[person].ScheduledDay(dateOnly);
                   
                    var personPeriod = person.Period(dateOnly);
                    if(personPeriod != null)
                        top = DrawPersonSchedule(timeZoneInfo, top, part, details, publicNote, culture, doc.Pages[0].GetClientSize().Width, persons);
                }
            }

            AddHeader(doc, culture);
            AddFooter(doc, culture);
            return doc;
        }

        private static bool GetSchedulePartView(IScheduleDay part)
        {
            var significantPart = part.SignificantPart();
		    var significantPartForDisplay = part.SignificantPartForDisplay();
			if(part.PersonAssignment() != null && part.PersonAssignment().OvertimeActivities().Any())
				return false;

			return (significantPart == SchedulePartView.FullDayAbsence ||
                    significantPartForDisplay == SchedulePartView.FullDayAbsence
                    || significantPart == SchedulePartView.DayOff ||
                    significantPartForDisplay == SchedulePartView.DayOff
                    || significantPart == SchedulePartView.ContractDayOff ||
                    significantPartForDisplay == SchedulePartView.ContractDayOff);

        }


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting.ShiftsPerDayToPdfManager.DrawColumnData(System.Single,System.Single,System.String,System.Single,System.Boolean,System.Globalization.CultureInfo)")]
		private float DrawPersonSchedule(TimeZoneInfo timeZoneInfo, float top, IScheduleDay part, ScheduleReportDetail details, bool publicNote, CultureInfo culture, float pageWidth, IDictionary<IPerson, string> persons)
		{
			var personString = persons[part.Person];
            var font = PdfFontManager.GetFont(9f, PdfFontStyle.Regular, culture);
		    var stringWidthHandler = new StringWidthHandler(font, 130);
		    personString = stringWidthHandler.GetString(personString);
			var rowTop = top;
            float personTop = DrawColumnData(top, 0, personString, 130, culture);
            float break1Top = top;
            float lunchTop = top;
            float break2Top = top;
			float left = 490;

            var projection = part.ProjectionService().CreateProjection();
		
			if (!GetSchedulePartView(part) && projection.HasLayers && projection.Period().HasValue )
            {
	            left = 190;
                // ReSharper disable PossibleNullReferenceException
                DateTimePeriod period = projection.Period().Value;
                // ReSharper restore PossibleNullReferenceException
                DateTime start = period.StartDateTimeLocal(timeZoneInfo);
                DateTime end = period.EndDateTimeLocal(timeZoneInfo);
                DrawColumnData(top, 130, start.ToString("t", culture), 80, culture);
                
                if (details != ScheduleReportDetail.None)
                {
                    bool beforeLunch = true;
                    var hasLunch =
						projection.Select(layer => layer.Payload).OfType<IActivity>().Count(a => a.ReportLevelDetail == ReportLevelDetail.Lunch) != 0;

                    
                    // draw breaks too
                    foreach (IVisualLayer visualLayer in projection)
                    {
                        var activity = visualLayer.Payload as IActivity;
                        if (activity != null)
                        {
                            DateTimePeriod layerPeriod = visualLayer.Period;
                            string per = layerPeriod.TimePeriod(timeZoneInfo).ToShortTimeString(culture);
                            if (activity.ReportLevelDetail == ReportLevelDetail.ShortBreak)
                            {
                                if (beforeLunch)
                                {
                                    break1Top = DrawColumnData(break1Top, 190, per, 85, culture);
                                    if (!hasLunch) beforeLunch = false;
                                }
                                else
                                    break2Top = DrawColumnData(break2Top, 390, per, 85, culture);
                                
                            }
                            if (activity.ReportLevelDetail == ReportLevelDetail.Lunch)
                            {
                                lunchTop = DrawColumnData(lunchTop, 290, per, 85, culture);
                                beforeLunch = false;
                            }
                        }
                    }
                    left = 490;
                }
                top = DrawColumnData(top, left, end.ToString("t", culture), 80, culture);
                //if it was more than one of anything
                if (lunchTop > top) top = lunchTop;
                if (break1Top > top) top = break1Top;
                if (break2Top > top) top = break2Top;
            }
            
            if(publicNote)
            {
                var note = part.PublicNoteCollection().FirstOrDefault();
                var noteString = note != null ? note.GetScheduleNote(new NoFormatting()) : string.Empty;
				if (noteString.Length > 0) top = DrawColumnData(rowTop, left + 80, noteString, pageWidth - (left + 80), culture);
            }
			if (personTop > top) top = personTop;

            DrawLine(top - 3, pageWidth, 1);
            return top;
        }

        private float DrawColumnHeader(float top, float left, string text, float width, CultureInfo cultureInfo)
        {
            var format = new CccPdfStringFormat();

            const float fontSize = 10f;
            PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, cultureInfo);
            _graphics.DrawString(text, font, _brush, new RectangleF(left, top + RowSpace, width, fontSize + 2), format.PdfStringFormat);
            return top + fontSize + 2 + ((RowSpace + 1) * 2);
        }

        private float DrawColumnData(float top, float left, string text, float width, CultureInfo cultureInfo)
        {
            var format = new CccPdfStringFormat{WordWrapType = PdfWordWrapType.None};
           
            const float fontSize = 9f;
            PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, cultureInfo);

            var stringWidthHandler = new StringWidthHandler(font, width);
            var lines = stringWidthHandler.WordWrap(text);
           
            for (int i = 0; i < lines.Count(); i++ )
            {
                var line = lines[i];
                _graphics.DrawString(line, font, _brush, new RectangleF(left, top + RowSpace, width, fontSize + 2), format.PdfStringFormat);
                top = top + fontSize + 2 + ((RowSpace + 1) * 2);
            }

               // _graphics.DrawString(text, font, _brush, new RectangleF(left, top + RowSpace, width, fontSize + 2), format);
            return top;// + fontSize + 2 + ((RowSpace + 1) * 2);
        }

        private float NewPage(PdfDocument doc, DateOnly dateOnly, float top, bool newReport, CultureInfo culture, out PdfPage page, ScheduleReportDetail details)
        {
            page = doc.Pages.Add();
            _graphics = page.Graphics;
            _reportTitle = Resources.ShiftsPerDayHeader + " " + dateOnly.Date.ToString("d", culture);

            if (newReport)
                top = DrawReportHeader(top, page.GetClientSize().Width, _reportTitle, culture) + 3;
            float widthLeft = doc.Pages[0].GetClientSize().Width;

            DrawColumnHeader(top, 0, Resources.Name, 200, culture);
            DrawColumnHeader(top, 130, Resources.StartTime, 80, culture);
            float left = 190;
            if (details != ScheduleReportDetail.None)
            {
                DrawColumnHeader(top, 190, Resources.ReportLevelDetailShortBreak, 85, culture);
                DrawColumnHeader(top, 290, Resources.ReportLevelDetailLunch, 85,  culture);
                DrawColumnHeader(top, 390, Resources.ReportLevelDetailShortBreak, 85, culture);
                left = 490;
            }

            DrawColumnHeader(top, left, Resources.EndTime, 80, culture);

            top = DrawColumnHeader(top, left + 80, Resources.Note, widthLeft - (left + 85), culture);

            DrawLine(top - 3, widthLeft, 1.5f);
            return top;
        }

        private void DrawLine(float top, float width, float penWidth)
        {
            var pen = new PdfPen(Color.Gray, penWidth);
            _graphics.DrawLine(pen, 0, top, width, top);
        }

        private float DrawReportHeader(float top, float width, string text, CultureInfo cultureInfo)
        {
            var format = new CccPdfStringFormat{Alignment = PdfTextAlignment.Left, LineAlignment = PdfVerticalAlignment.Middle};
           
            const float fontSize = 14f;
            PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, cultureInfo);
            var headerRect = new RectangleF(0, top + RowSpace, width, fontSize + 4);
            PdfBrush backGround = new PdfSolidBrush(Color.PowderBlue);
            _graphics.DrawRectangle(backGround, headerRect);
            _graphics.DrawString(text, font, _brush, headerRect, format.PdfStringFormat);
            return top + fontSize + 4 + RowSpace;
        }

        private static void AddHeader(PdfDocument doc, CultureInfo cultureInfo)
        {
            var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            //Create page template
            var header = new PdfPageTemplateElement(rect);
            var brush = new PdfSolidBrush(Color.Gray);

            PdfFont font = PdfFontManager.GetFont(6f, PdfFontStyle.Regular, cultureInfo);

            var format = new CccPdfStringFormat {LineAlignment = PdfVerticalAlignment.Top, Alignment = PdfTextAlignment.Right};
            
            //Create page number field
            var pageNumber = new PdfPageNumberField(font, brush);
            var createdDate = new PdfCreationDateField(font, brush);
            createdDate.DateFormatString = CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern;
           
            //Create page count field
            var count = new PdfPageCountField(font, brush);

            var compositeField = new PdfCompositeField(font, brush, Resources.CreatedPageOf, createdDate, pageNumber, count)
            {
                StringFormat = format.PdfStringFormat,
                Bounds = header.Bounds
            };
            compositeField.Draw(header.Graphics);
            //Add header template at the top.
            doc.Template.Top = header;
        }

        private static void AddFooter(PdfDocument doc, CultureInfo cultureInfo)
        {
            var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            //Create a page template
            var footer = new PdfPageTemplateElement(rect);

            var brush = new PdfSolidBrush(Color.Gray);
            var font = PdfFontManager.GetFont(6, PdfFontStyle.Bold, cultureInfo);

            var format = new CccPdfStringFormat{Alignment = PdfTextAlignment.Center, LineAlignment = PdfVerticalAlignment.Bottom};
          
            footer.Graphics.DrawString(Resources.PoweredByTeleoptCCC, font, brush, rect, format.PdfStringFormat);

            doc.Template.Bottom = footer;

        }
    }
}
