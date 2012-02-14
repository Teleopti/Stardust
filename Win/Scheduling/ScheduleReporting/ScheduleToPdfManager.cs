using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
    public class ScheduleToPdfManager
    {
        private PdfGraphics _graphics;
        private PdfSolidBrush _brush;
        private PdfPen _pen;
        private float _scheduleColumnWidth;
        private float _headerColumnWidth;
        private bool _rtl;
        private string _reportTitle;

        private const float RowSpace = 1;

		public void ExportIndividual(ICccTimeZoneInfo timeZoneInfo, CultureInfo culture, IDictionary<IPerson, string> persons,
            DateOnlyPeriod period, ISchedulingResultStateHolder stateHolder, bool rightToLeft, ScheduleReportDetail details,
            Control owner, bool singleFile, string path)
        {
			if(culture == null)
				throw new ArgumentNullException("culture");

            DayOfWeek firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
            var outerStart = new DateOnly(DateHelper.GetFirstDateInWeek(period.StartDate.Date, culture).Date);
            var outerEnd = new DateOnly(DateHelper.GetLastDateInWeek(period.EndDate.Date, culture).Date);
            var fullWeekPeriod = new DateOnlyPeriod(outerStart, outerEnd);

            _brush = new PdfSolidBrush(Color.DimGray);
            _pen = new PdfPen(Color.Gray, 1);
            _rtl = rightToLeft;

            individualPages(rightToLeft, persons, firstDayOfWeek, fullWeekPeriod, stateHolder, timeZoneInfo,
                            details, owner, singleFile, path, culture);
            //doc.Pages.Add();

        }

		public static void ExportShiftPerDayTeamViewGraphical(CultureInfo culture, IDictionary<IPerson, string> persons, DateOnlyPeriod period, ISchedulingResultStateHolder stateHolder, bool rightToLeft, Control owner, string path, ScheduleReportDialogGraphicalModel model)
		{
			var shiftsPerDayGraphicalToPdfManager = new ShiftsPerDayGraphicalToPdfManager(culture, persons, period, stateHolder, rightToLeft, model);
			var doc = shiftsPerDayGraphicalToPdfManager.ExportTeamView();
			openDocument(doc ,owner , path);
		}

		public static void ExportShiftPerDayAgentViewGraphical(CultureInfo culture, IDictionary<IPerson, string> persons, DateOnlyPeriod period, ISchedulingResultStateHolder stateHolder, bool rightToLeft, Control owner, string path, ScheduleReportDialogGraphicalModel model)
		{
			if(persons == null)
				throw new ArgumentNullException("persons");

			if(model == null)
				throw new ArgumentNullException("model");

			if (model.OneFileForSelected)
			{
				var shiftsPerDayGraphicalToPdfManager = new ShiftsPerDayGraphicalToPdfManager(culture, persons, period, stateHolder, rightToLeft, model);
				var doc = shiftsPerDayGraphicalToPdfManager.ExportAgentView();
				openDocument(doc, owner, path);
			}
			else
			{
				foreach (var keyValuePair in persons)
				{
					IDictionary<IPerson, string> person = new Dictionary<IPerson, string>();

					person.Add(keyValuePair);

					var shiftsPerDayGraphicalToPdfManager = new ShiftsPerDayGraphicalToPdfManager(culture, person, period, stateHolder, rightToLeft, model);
					var doc = shiftsPerDayGraphicalToPdfManager.ExportAgentView();
					var fullPath = path + "\\" + keyValuePair.Value + Path.GetRandomFileName() + ".PDF";
					openDocument(doc, owner, fullPath);
				}
			}
		}

		public static void ExportShiftsPerDay(ICccTimeZoneInfo timeZoneInfo, CultureInfo culture, IDictionary<IPerson, string> persons,
            DateOnlyPeriod period, ISchedulingResultStateHolder stateHolder, bool rightToLeft, ScheduleReportDetail details, Control owner, string path)
        {
            var shiftsPerDayToPdfManager = new ShiftsPerDayToPdfManager();
            PdfDocument doc = shiftsPerDayToPdfManager.Export(timeZoneInfo, culture, persons,
                                                              period, stateHolder,
                                                              rightToLeft, details);
            openDocument(doc, owner, path);

        }

		public void ExportTeam(ICccTimeZoneInfo timeZoneInfo, CultureInfo culture, IDictionary<IPerson, string> persons,
            DateOnlyPeriod period, ISchedulingResultStateHolder stateHolder, bool rightToLeft, ScheduleReportDetail details, Control owner, string path)
        {
			if(culture == null)
				throw new ArgumentNullException("culture");

            _headerColumnWidth = 70;
            DayOfWeek firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
            var outerStart = new DateOnly(DateHelper.GetFirstDateInWeek(period.StartDate.Date, culture).Date);
            var outerEnd = new DateOnly(DateHelper.GetLastDateInWeek(period.EndDate.Date, culture).Date);
            var fullWeekPeriod = new DateOnlyPeriod(outerStart, outerEnd);

            _brush = new PdfSolidBrush(Color.DimGray);
            _pen = new PdfPen(Color.Gray, 1);
            _rtl = rightToLeft;

            //Create a new document.
            var doc = new PdfDocument
                          {
                              PageSettings = {Orientation = PdfPageOrientation.Landscape}
                          };

            teamPages(rightToLeft, doc, persons, firstDayOfWeek, fullWeekPeriod, stateHolder, timeZoneInfo, details, culture);

            AddHeader(doc,culture);
            AddFooter(doc, Resources.PoweredByTeleoptCCC,culture);

            openDocument(doc, owner, path);

        }

		private void individualPages(bool rightToLeft, IDictionary<IPerson, string> persons, DayOfWeek firstDayOfWeek, DateOnlyPeriod fullWeekPeriod, ISchedulingResultStateHolder stateHolder,
            ICccTimeZoneInfo timeZoneInfo, ScheduleReportDetail details, Control owner, bool singleFile, string path, CultureInfo culture)
        {
            var doc = new PdfDocument();
            PdfPage page;
            if (singleFile)
            {
                doc = new PdfDocument
                          {
                              PageSettings = {Orientation = PdfPageOrientation.Landscape}
                          };
            }

            foreach (IPerson person in persons.Keys)
            {
                if (!singleFile)
                {
                    doc = new PdfDocument
                              {
                                  PageSettings = {Orientation = PdfPageOrientation.Landscape}
                              };
                }
            	_reportTitle = persons[person];
                float top = newPage(doc, firstDayOfWeek, 10, true, out page, culture);

                int offset = 0;
                while (offset < fullWeekPeriod.DayCount())
                {
                    //skapa en PdfSchedule per dag hela veckan
                    IList<IPdfScheduleTemplate> weekList = createWeek(rightToLeft, person, fullWeekPeriod, offset, stateHolder, timeZoneInfo, details, culture);
                    float height = getHeight(weekList);

                    if (top + height > page.GetClientSize().Height)
                    {
                        top = newPage(doc, firstDayOfWeek, 0, true, out page, culture);
                    }

                    for (int i = 0; i < 7; i++)
                    {
                        float left = _headerColumnWidth + (i * _scheduleColumnWidth);
                        drawLine(top, page.GetClientSize().Width);
                        weekList[i].Template.Draw(page, left, top);
                    }

                    top += height + 3;
                    offset += 7;
                }

                if (!singleFile)
                {
                    AddHeader(doc, culture);
                    AddFooter(doc, Resources.PoweredByTeleoptCCC, culture);
                    openDocument(doc, owner, path + "\\" + person.Id.Value + ".PDF");
                }
            }

            if (singleFile)
            {
                AddHeader(doc, culture);
                AddFooter(doc, Resources.PoweredByTeleoptCCC, culture);
                openDocument(doc, owner, path);
            }
        }

		private void teamPages(bool rightToLeft, PdfDocument doc, IDictionary<IPerson, string> persons, DayOfWeek firstDayOfWeek, DateOnlyPeriod fullWeekPeriod, ISchedulingResultStateHolder stateHolder, ICccTimeZoneInfo timeZoneInfo, ScheduleReportDetail details, CultureInfo cultureInfo)
        {
            PdfPage page;

            int offset = 0;
            while (offset < fullWeekPeriod.DayCount())
            {
                _reportTitle = string.Concat(fullWeekPeriod.DayCollection()[offset].ToShortDateString(CultureInfo.CurrentCulture), " -- ",
                                             fullWeekPeriod.DayCollection()[offset + 6].ToShortDateString(CultureInfo.CurrentCulture));
                float top = newPage(doc, firstDayOfWeek, 10, true, out page, cultureInfo);

                foreach (IPerson person in persons.Keys)
                {
                    //skapa en PdfSchedule per dag hela veckan
                    IList<IPdfScheduleTemplate> weekList = createWeek(rightToLeft, person, fullWeekPeriod, offset, stateHolder, timeZoneInfo,
                                                details, cultureInfo);
                    float height = getHeight(weekList);

                    if (top + height > page.GetClientSize().Height)
                    {
                        top = newPage(doc, firstDayOfWeek, 0, false, out page, cultureInfo);
                    }

                    var rect = new RectangleF(new PointF(0, top), new SizeF(_headerColumnWidth, height));
					drawRowHeader(rect, persons[person], cultureInfo);

                    for (int i = 0; i < 7; i++)
                    {
                        float left = _headerColumnWidth + (i * _scheduleColumnWidth);
                        drawLine(top, page.GetClientSize().Width);
                        weekList[i].Template.Draw(page, left, top);
                    }

                    top += height + 3;

                }
                offset += 7;
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private static void openDocument(PdfDocument doc, Control owner, string fullPath)
        {
            try
            {
                doc.Save(fullPath);
            }
            catch (IOException ex)
            {
                MessageDialogs.ShowError(owner, ex.Message, Resources.Scheduling);
				return;
            }
			catch(Exception ex)
			{
				MessageDialogs.ShowError(owner, ex.Message, Resources.Scheduling);
				return;
			}

            try
            {
                //Launching the PDF file using the default Application.[Acrobat Reader]
                Process.Start(fullPath);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageDialogs.ShowError(owner, ex.Message, Resources.Scheduling);
            }
        }

        //private static void openDocument(PdfDocument doc, Control owner, string path)
        //{
        //    string fileName = "16887";
        //    openDocument(doc, owner, fileName);
        //}

        private static float getHeight(IEnumerable<IPdfScheduleTemplate> templateList)
        {
            float ret = 0;
            foreach (IPdfScheduleTemplate template in templateList)
            {
                if (template.Height > ret)
                    ret = template.Height;
            }

            return ret;
        }

        private IList<IPdfScheduleTemplate> createWeek(bool rtl, IPerson person, DateOnlyPeriod fullWeekPeriod, int offset,
            ISchedulingResultStateHolder stateHolder, ICccTimeZoneInfo timeZoneInfo, ScheduleReportDetail details, CultureInfo culture)
        {
            IList<IPdfScheduleTemplate> weekList = new List<IPdfScheduleTemplate>();
            for (int i = 0; i < 7; i++)
            {
                DateOnly date = fullWeekPeriod.DayCollection()[i + offset];
                IScheduleDay part = stateHolder.Schedules[person].ScheduledDay(date);
                //SchedulePartView significantPart = part.SignificantPart();
                SchedulePartView significantPart = part.SignificantPartForDisplay();
                //IVirtualSchedulePeriod virtualSchedulePeriod = person.VirtualSchedulePeriod(date);
                //if (!virtualSchedulePeriod.IsValid)
                //    significantPart = SchedulePartView.None;

                var personPeriod = person.Period(date);
                if (personPeriod == null)
                    significantPart = SchedulePartView.None;

                switch (significantPart)
                {
                    case SchedulePartView.MainShift:
                        IPdfScheduleTemplate schedule = new PdfScheduleAssignment(_scheduleColumnWidth,
                                                                        part,
                                                                        timeZoneInfo, rtl, details, culture);
                        weekList.Add(schedule);
                        break;

                    case SchedulePartView.DayOff:

                        if (part.PersonAssignmentCollection().Count > 0 && part.PersonAssignmentCollection()[0].OvertimeShiftCollection.Count > 0 && details == ScheduleReportDetail.All)
                        {
                            schedule = new PdfScheduleDayOffOvertime(_scheduleColumnWidth,
                                                                 part, part.PersonDayOffCollection()[0],
                                                                 timeZoneInfo, rtl, details, culture);
                            weekList.Add(schedule);
                        }
                        else
                        {
                            IPdfScheduleTemplate dayOff = new PdfScheduleDayOff(_scheduleColumnWidth,
                                                                     part.PersonDayOffCollection()[0], timeZoneInfo, rtl, culture);
                            weekList.Add(dayOff);
                        }
                        break;

                    case SchedulePartView.FullDayAbsence:
                        IPdfScheduleTemplate fullDayAbsence = new PdfScheduleFullDayAbsence(_scheduleColumnWidth,
                                                                                             part, timeZoneInfo,
                                                                                             rtl, culture);
                        weekList.Add(fullDayAbsence);
                        break;

                    default:
                        IPdfScheduleTemplate empty = new PdfScheduleNotScheduled(_scheduleColumnWidth, date, rtl, culture);
                        weekList.Add(empty);
                        break;
                }
            }

            return weekList;
        }

        private float newPage(PdfDocument doc, DayOfWeek firstDayOfWeek, float top, bool newReport, out PdfPage page, CultureInfo cultureInfo)
        {
            page = doc.Pages.Add();
            _graphics = page.Graphics;

            _scheduleColumnWidth = (page.GetClientSize().Width - _headerColumnWidth) / 7;
            if (newReport)
                top = drawReportHeader(top, page.GetClientSize().Width, _reportTitle, cultureInfo) + 3;
            else
                top = 14f;
            drawColumns(top, page.GetClientSize().Height);
            top = drawWeekHeaders(firstDayOfWeek, top, _rtl, cultureInfo);
            return top;
        }

        private void drawLine(float top, float width)
        {
            _graphics.DrawLine(_pen, 0, top, width, top);
        }

        private void drawColumns(float top, float height)
        {
            for (int i = 0; i < 7; i++)
            {
                float left = _headerColumnWidth + (i * _scheduleColumnWidth);
                _graphics.DrawLine(_pen, left, top, left, height);
            }

        }

        private float drawWeekHeaders(DayOfWeek firstDayOfWeek, float top, bool rtl,CultureInfo cultureInfo)
        {
            float height = new PdfDayOfWeekHeader(_scheduleColumnWidth, (int)firstDayOfWeek, rtl, cultureInfo).Height;  //dynamic
            for (int i = 0; i < 7; i++)
            {
                DayOfWeek dow;
                if ((int)firstDayOfWeek + i < 7)
                {
                    dow = (DayOfWeek)i + (int)firstDayOfWeek;
                }
                else
                {
                    dow = (DayOfWeek)i + (int)firstDayOfWeek - 7;
                }
                float left = _headerColumnWidth + (i * _scheduleColumnWidth);

                var header = new PdfDayOfWeekHeader(_scheduleColumnWidth, (int)dow, rtl, cultureInfo);
                header.Template.Draw(_graphics, left, top);
            }

            return top + height;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void drawRowHeader(RectangleF rect, string text, CultureInfo cultureInfo)
        {
            var format = new PdfStringFormat
                             {
                                 RightToLeft = _rtl,
                                 Alignment = PdfTextAlignment.Left,
                                 LineAlignment = PdfVerticalAlignment.Middle
                             };
            const float fontSize = 8f;
            var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, cultureInfo);
            _graphics.DrawString(text, font, _brush, rect, format);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private float drawReportHeader(float top, float width, string text, CultureInfo cultureInfo)
        {
            var format = new PdfStringFormat
                             {
                                 RightToLeft = _rtl,
                                 Alignment = PdfTextAlignment.Left,
                                 LineAlignment = PdfVerticalAlignment.Middle
                             };

            const float fontSize = 14f;
            var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, cultureInfo);
            var headerRect = new RectangleF(0, top + RowSpace, width, fontSize + 4);
            PdfBrush backGround = new PdfSolidBrush(Color.PowderBlue);
            _graphics.DrawRectangle(backGround, headerRect);
            _graphics.DrawString(text, font, _brush, headerRect, format);
            return top + fontSize + 4 + RowSpace;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void AddFooter(PdfDocument doc, string footerText, CultureInfo cultureInfo)
        {
            var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 40);

            //Create a page template
            var footer = new PdfPageTemplateElement(rect);

            var brush = new PdfSolidBrush(Color.Gray);
            const float fontSize = 6f;
            var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, cultureInfo);
            var format = new PdfStringFormat
                             {
                                 RightToLeft = _rtl,
                                 Alignment = PdfTextAlignment.Center,
                                 LineAlignment = PdfVerticalAlignment.Bottom
                             };
            footer.Graphics.DrawString(footerText, font, brush, rect, format);

            //format = new PdfStringFormat();
            //format.Alignment = PdfTextAlignment.Right;
            //format.LineAlignment = PdfVerticalAlignment.Bottom;

            ////Create page number field
            //PdfPageNumberField pageNumber = new PdfPageNumberField(font, brush);

            ////Create page count field
            //PdfPageCountField count = new PdfPageCountField(font, brush);

            //PdfCompositeField compositeField = new PdfCompositeField(font, brush, "Page {0} of {1}", pageNumber, count);
            //compositeField.Bounds = footer.Bounds;
            //compositeField.StringFormat = format;
            //compositeField.Draw(footer.Graphics); // new PointF(doc.Pages[0].GetClientSize().Width - 100, 40));

            //Add the footer template at the bottom
            doc.Template.Bottom = footer;

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void AddHeader(PdfDocument doc, CultureInfo cultureInfo)
        {
            var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            //Create page template
            var header = new PdfPageTemplateElement(rect);
            var brush = new PdfSolidBrush(Color.Gray);

            const float fontSize = 6f;
            var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, cultureInfo);
            var format = new PdfStringFormat
                             {
                                 RightToLeft = _rtl,
                                 LineAlignment = PdfVerticalAlignment.Top,
                                 Alignment = PdfTextAlignment.Right
                             };
            //format.Alignment = PdfTextAlignment.Left;
            //header.Graphics.DrawString("hej", font, brush,rect, format);

            //Create page number field
            var pageNumber = new PdfPageNumberField(font, brush);

            //Create page count field
            var count = new PdfPageCountField(font, brush);

            var createdDate = new PdfCreationDateField(font, brush);
            createdDate.DateFormatString = CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern;
           
            var compositeField = new PdfCompositeField(font, brush, Resources.CreatedPageOf, createdDate, pageNumber, count)
                                     {
                                         StringFormat = format,
                                         Bounds = header.Bounds
                                     };
            compositeField.Draw(header.Graphics); //, new PointF(doc.Pages[0].GetClientSize().Width - 200, 40));
            //Add header template at the top.
            doc.Template.Top = header;

        }
    }
}