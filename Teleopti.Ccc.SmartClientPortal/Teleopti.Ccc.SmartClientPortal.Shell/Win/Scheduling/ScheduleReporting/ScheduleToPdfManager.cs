using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.ScheduleReporting
{
    public class ScheduleToPdfManager
    {
        private PdfGraphics _graphics;
        private PdfSolidBrush _brush;
        private PdfSolidBrush _greyBrush;
        private PdfPen _pen;
        private float _scheduleColumnWidth;
        private float _headerColumnWidth;
        private bool _rtl;
        private string _reportTitle;

        private const float RowSpace = 1;

		public void ExportIndividual(TimeZoneInfo timeZoneInfo, CultureInfo culture, IDictionary<IPerson, string> persons,
            DateOnlyPeriod period, ISchedulingResultStateHolder stateHolder, bool rightToLeft, ScheduleReportDetail details,
            Control owner, bool singleFile, string path)
        {
			if(culture == null)
				throw new ArgumentNullException(nameof(culture));

            DayOfWeek firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
			var outerStart = new DateOnly(DateHelper.GetFirstDateInWeek(period.StartDate.Date, culture).Date);
			var outerEnd = new DateOnly(DateHelper.GetLastDateInWeek(period.EndDate.Date, culture).Date);
			var fullWeekPeriod = new DateOnlyPeriod(outerStart, outerEnd);

			_brush = new PdfSolidBrush(Color.DimGray);
            _greyBrush = new PdfSolidBrush(Color.Gray);
            _pen = new PdfPen(Color.Gray, 1);
            _rtl = rightToLeft;

            individualPages(rightToLeft, persons, firstDayOfWeek, fullWeekPeriod, stateHolder, timeZoneInfo,
                            details, owner, singleFile, path, culture);
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

		public static void ExportShiftsPerDay(TimeZoneInfo timeZoneInfo, CultureInfo culture, IDictionary<IPerson, string> persons,
            DateOnlyPeriod period, ISchedulingResultStateHolder stateHolder, ScheduleReportDetail details, bool publicNote, Control owner, string path)
        {
            var shiftsPerDayToPdfManager = new ShiftsPerDayToPdfManager();
            PdfDocument doc = shiftsPerDayToPdfManager.Export(timeZoneInfo, culture, persons, period, stateHolder, details, publicNote);
            openDocument(doc, owner, path);

        }

		public void ExportTeam(TimeZoneInfo timeZoneInfo, CultureInfo culture, IDictionary<IPerson, string> persons,
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
            _greyBrush = new PdfSolidBrush(Color.Gray);
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
            TimeZoneInfo timeZoneInfo, ScheduleReportDetail details, Control owner, bool singleFile, string path, CultureInfo culture)
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

		private void teamPages(bool rightToLeft, PdfDocument doc, IDictionary<IPerson, string> persons, DayOfWeek firstDayOfWeek, DateOnlyPeriod fullWeekPeriod, ISchedulingResultStateHolder stateHolder, TimeZoneInfo timeZoneInfo, ScheduleReportDetail details, CultureInfo cultureInfo)
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
                Process.Start(fullPath);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageDialogs.ShowError(owner, ex.Message, Resources.Scheduling);
            }
        }

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
            ISchedulingResultStateHolder stateHolder, TimeZoneInfo timeZoneInfo, ScheduleReportDetail details, CultureInfo culture)
        {
            IList<IPdfScheduleTemplate> weekList = new List<IPdfScheduleTemplate>();
        	var dayCollection = fullWeekPeriod.DayCollection();
            for (int i = 0; i < 7; i++)
            {
                DateOnly date = dayCollection[i + offset];
                IScheduleDay part = stateHolder.Schedules[person].ScheduledDay(date);
                SchedulePartView significantPart = part.SignificantPartForDisplay();
                
                var personPeriod = person.Period(date);
                if (personPeriod == null)
                    significantPart = SchedulePartView.None;

                switch (significantPart)
                {
					case SchedulePartView.Overtime:
                    case SchedulePartView.MainShift:
                        IPdfScheduleTemplate schedule = new PdfScheduleAssignment(_scheduleColumnWidth,
                                                                        part,
                                                                        timeZoneInfo, rtl, details, culture);
                        weekList.Add(schedule);
                        break;

                    case SchedulePartView.DayOff:
                		var assignment = part.PersonAssignment();
						
						if (assignment != null && assignment.OvertimeActivities().Any())
						{
                            schedule = new PdfScheduleDayOffOvertime(_scheduleColumnWidth,
                                                                 part, part.PersonAssignment(),
                                                                 timeZoneInfo, rtl, details, culture);
                            weekList.Add(schedule);
                        }
                        else
                        {
                            IPdfScheduleTemplate dayOff = new PdfScheduleDayOff(_scheduleColumnWidth,
                                                                     part.PersonAssignment(), timeZoneInfo, rtl, culture);
                            weekList.Add(dayOff);
                        }
                        break;

					case SchedulePartView.ContractDayOff:
						IPdfScheduleTemplate fullDayAbsenceOnDayOff = new PdfScheduleFullDayAbsence(_scheduleColumnWidth,
                                                                                             part, timeZoneInfo,
                                                                                             rtl, culture, true);
                        weekList.Add(fullDayAbsenceOnDayOff);
                        break;

                    case SchedulePartView.FullDayAbsence:
                        IPdfScheduleTemplate fullDayAbsence = new PdfScheduleFullDayAbsence(_scheduleColumnWidth,
                                                                                             part, timeZoneInfo,
                                                                                             rtl, culture, false);
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
            var format = new CccPdfStringFormat {Alignment = PdfTextAlignment.Left, LineAlignment = PdfVerticalAlignment.Middle};
            const float fontSize = 8f;
            var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, cultureInfo);
            _graphics.DrawString(text, font, _brush, rect, format.PdfStringFormat);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private float drawReportHeader(float top, float width, string text, CultureInfo cultureInfo)
        {
            var format = new CccPdfStringFormat {Alignment = PdfTextAlignment.Left, LineAlignment = PdfVerticalAlignment.Middle};
            const float fontSize = 14f;
            var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, cultureInfo);
            var headerRect = new RectangleF(0, top + RowSpace, width, fontSize + 4);
            PdfBrush backGround = new PdfSolidBrush(Color.PowderBlue);
            _graphics.DrawRectangle(backGround, headerRect);
            _graphics.DrawString(text, font, _brush, headerRect, format.PdfStringFormat);
            return top + fontSize + 4 + RowSpace;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void AddFooter(PdfDocument doc, string footerText, CultureInfo cultureInfo)
        {
            var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 40);

            //Create a page template
            var footer = new PdfPageTemplateElement(rect);

            const float fontSize = 6f;
            var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, cultureInfo);

            var format = new CccPdfStringFormat {Alignment = PdfTextAlignment.Center, LineAlignment = PdfVerticalAlignment.Bottom};
            footer.Graphics.DrawString(footerText, font, _greyBrush, rect, format.PdfStringFormat);

            doc.Template.Bottom = footer;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void AddHeader(PdfDocument doc, CultureInfo cultureInfo)
        {
            var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            var header = new PdfPageTemplateElement(rect);

            const float fontSize = 6f;
            var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, cultureInfo);

            var format = new CccPdfStringFormat {LineAlignment = PdfVerticalAlignment.Top, Alignment = PdfTextAlignment.Right};
            var pageNumber = new PdfPageNumberField(font, _greyBrush);

            var count = new PdfPageCountField(font, _greyBrush);

            var createdDate = new PdfCreationDateField(font, _greyBrush);
            createdDate.DateFormatString = CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern;

            var compositeField = new PdfCompositeField(font, _greyBrush, Resources.CreatedPageOf, createdDate, pageNumber, count)
                                     {
                                         StringFormat = format.PdfStringFormat,
                                         Bounds = header.Bounds
                                     };
            compositeField.Draw(header.Graphics);
            doc.Template.Top = header;
        }
    }
}