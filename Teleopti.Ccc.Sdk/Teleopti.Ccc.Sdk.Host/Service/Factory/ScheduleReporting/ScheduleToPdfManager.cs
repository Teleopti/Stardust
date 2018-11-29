using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory.ScheduleReporting
{
    internal class ScheduleToPdfManager
    {
        private PdfGraphics _graphics;
        private PdfSolidBrush _brush;
        private PdfPen _pen;
        private float _scheduleColumnWidth;
        private bool _rtl;
        private string _reportTitle;

        private const float ROW_SPACE = 1;

        public IList<PersonWithScheduleStream> ExportIndividual(TimeZoneInfo timeZoneInfo, IEnumerable<IPerson> persons,
                                     DateOnlyPeriod period, IScheduleDictionary scheduleDictionary, ScheduleReportDetail details)
        {
            _brush = new PdfSolidBrush(Color.DimGray);
            _pen = new PdfPen(Color.Gray, 1);

            return IndividualPages(persons, period, scheduleDictionary, timeZoneInfo, details);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private IList<PersonWithScheduleStream> IndividualPages(IEnumerable<IPerson> persons, DateOnlyPeriod period, IDictionary<IPerson, IScheduleRange> scheduleDictionary, TimeZoneInfo timeZoneInfo, ScheduleReportDetail details)
        {
            IList<PersonWithScheduleStream> scheduleStreams = new List<PersonWithScheduleStream>();
            
            foreach (IPerson person in persons)
            {
                var culture = person.PermissionInformation.Culture();
                _rtl = person.PermissionInformation.UICulture().TextInfo.IsRightToLeft;
                DayOfWeek firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
                DateOnly outerStart = new DateOnly(DateHelper.GetFirstDateInWeek(period.StartDate.Date, culture).Date);
                DateOnly outerEnd = new DateOnly(DateHelper.GetLastDateInWeek(period.EndDate.Date, culture).Date);
                DateOnlyPeriod fullWeekPeriod = new DateOnlyPeriod(outerStart, outerEnd);

                PdfDocument doc = new PdfDocument();
                doc.PageSettings.Orientation = PdfPageOrientation.Landscape;

                _reportTitle = person.Name.ToString(NameOrderOption.FirstNameLastName);
                float top;
                PdfPage page;
                top = newPage(doc, firstDayOfWeek, 10, true,culture, out page);

                int offset = 0;
                while (offset < fullWeekPeriod.DayCount())
                {
                    //skapa en PdfSchedule per dag hela veckan
                    IList<IPdfScheduleTemplate> weekList = CreateWeek(_rtl, person, fullWeekPeriod, offset,
                                                                      scheduleDictionary, timeZoneInfo, details, culture);
                    float height = GetHeight(weekList);

                    if (top + height > page.GetClientSize().Height)
                    {
                        top = newPage(doc, firstDayOfWeek, 0, true,culture, out page);
                    }

                    for (int i = 0; i < 7; i++)
                    {
                        float left = (i*_scheduleColumnWidth);
                        drawLine(top, page.GetClientSize().Width);
                        weekList[i].Template.Draw(page, left, top);
                    }

                    top += height + 3;
                    offset += 7;
                }

                AddHeader(doc);
                AddFooter(doc, UserTexts.Resources.PoweredByTeleoptCCC);

                MemoryStream memoryStream = new MemoryStream();
                doc.Save(memoryStream);
                scheduleStreams.Add(new PersonWithScheduleStream{Person = person,SchedulePdf = memoryStream});
            }

            return scheduleStreams;
        }

        private static float GetHeight(IEnumerable<IPdfScheduleTemplate> templateList)
        {
            float ret = 0;
            foreach (IPdfScheduleTemplate template in templateList)
            {
                if(template.Height > ret)
                    ret = template.Height;
            }

            return ret;
        }

        private IList<IPdfScheduleTemplate> CreateWeek(bool rtl, IPerson person, DateOnlyPeriod fullWeekPeriod, int offset, IDictionary<IPerson, IScheduleRange> scheduleDictionary, TimeZoneInfo timeZoneInfo, ScheduleReportDetail details, CultureInfo culture)
        {

            IList<IPdfScheduleTemplate> weekList = new List<IPdfScheduleTemplate>();
            for (int i = 0; i < 7; i++)
            {
                DateOnly date = fullWeekPeriod.DayCollection()[i + offset];
                IScheduleDay part = scheduleDictionary[person].ScheduledDay(date);
                SchedulePartView significantPart = part.SignificantPart();
                IVirtualSchedulePeriod virtualSchedulePeriod = person.VirtualSchedulePeriod(date);
                if (!virtualSchedulePeriod.IsValid)
                    significantPart = SchedulePartView.None;

                switch (significantPart)
                {
                    case SchedulePartView.MainShift:
                        IPdfScheduleTemplate schedule = new PdfScheduleAssignment(_scheduleColumnWidth, part,
                                                                                  timeZoneInfo, rtl, details, culture);
                        weekList.Add(schedule);
                        break;

                    case SchedulePartView.DayOff:
                        IPdfScheduleTemplate dayOff = new PdfScheduleDayOff(_scheduleColumnWidth,
                                                                            part.PersonAssignment(), timeZoneInfo, rtl, culture);
                        weekList.Add(dayOff);
                        break;

                    case SchedulePartView.FullDayAbsence:
                        IPdfScheduleTemplate fullDayAbsence = new PdfScheduleFullDayAbsence(_scheduleColumnWidth,
                                                                                            part, timeZoneInfo, rtl,culture);
                        weekList.Add(fullDayAbsence);
                        break;

                    default:
                        IPdfScheduleTemplate empty = new PdfScheduleNotScheduled(_scheduleColumnWidth, date, rtl,culture);
                        weekList.Add(empty);
                        break;
                }
            }

            return weekList;
        }

        private float newPage(PdfDocument doc, DayOfWeek firstDayOfWeek, float top, bool newReport, CultureInfo culture, out PdfPage page)
        {
            page = doc.Pages.Add();
            _graphics = page.Graphics;

            _scheduleColumnWidth = (page.GetClientSize().Width) / 7;
            if (newReport)
                top = drawReportHeader(top, page.GetClientSize().Width, _reportTitle) + 3;

            drawColumns(top, page.GetClientSize().Height);
            top = drawWeekHeaders(firstDayOfWeek, top, _rtl,culture);
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
                float left = (i * _scheduleColumnWidth);
                _graphics.DrawLine(_pen, left, top, left, height);
            }
        }

        private float drawWeekHeaders(DayOfWeek firstDayOfWeek, float top, bool rtl, CultureInfo culture)
        {
            float height = new PdfDayOfWeekHeader(_scheduleColumnWidth, (int)firstDayOfWeek, rtl,culture).Height;  //dynamic
            for (int i = 0; i < 7; i++)
            {
                DayOfWeek dow;
                if ((int)firstDayOfWeek + i < 7)
                {
                    dow = (DayOfWeek) i + (int) firstDayOfWeek;
                }
                else
                {
                    dow = (DayOfWeek)i + (int)firstDayOfWeek - 7;
                }
                float left = (i * _scheduleColumnWidth);

                PdfDayOfWeekHeader header = new PdfDayOfWeekHeader(_scheduleColumnWidth, (int)dow, rtl,culture);
                header.Template.Draw(_graphics, left, top);
            }

            return top + height;
        }

        private float drawReportHeader(float top, float width, string text)
        {
	        var format = new PdfStringFormat
	        {
		        RightToLeft = _rtl,
		        Alignment = PdfTextAlignment.Left,
		        LineAlignment = PdfVerticalAlignment.Middle
	        };

	        float fontSize = 14f;
            Font f = new Font("Helvetica", fontSize, FontStyle.Bold);
            PdfFont font = new PdfTrueTypeFont(f, true);
            RectangleF headerRect = new RectangleF(0, top + ROW_SPACE, width, fontSize + 4);
            PdfBrush backGround = new PdfSolidBrush(Color.PowderBlue);
            _graphics.DrawRectangle(backGround, headerRect);
            _graphics.DrawString(text, font, _brush, headerRect, format);
            return top + fontSize + 4 + ROW_SPACE;
        }

        private void AddFooter(PdfDocument doc, string footerText)
        {
            RectangleF rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            //Create a page template
            PdfPageTemplateElement footer = new PdfPageTemplateElement(rect);
            PdfSolidBrush brush = new PdfSolidBrush(Color.Gray);

            Font f = new Font("Helvetica", 6, FontStyle.Bold);
            PdfFont font = new PdfTrueTypeFont(f, true);
            PdfStringFormat format = new PdfStringFormat();
            format.RightToLeft = _rtl;
            format.Alignment = PdfTextAlignment.Center;
            format.LineAlignment = PdfVerticalAlignment.Bottom;
            footer.Graphics.DrawString(footerText, font, brush, rect, format);

            //Add the footer template at the bottom
            doc.Template.Bottom = footer;
        }

        private void AddHeader(PdfDocument doc)
        {
            RectangleF rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            //Create page template
            PdfPageTemplateElement header = new PdfPageTemplateElement(rect);
            PdfSolidBrush brush = new PdfSolidBrush(Color.Gray);

            Font f = new Font("Helvetica", 6, FontStyle.Bold);
            PdfFont font = new PdfTrueTypeFont(f, true);
            PdfStringFormat format = new PdfStringFormat();
            format.RightToLeft = _rtl;
            format.LineAlignment = PdfVerticalAlignment.Top;
            format.Alignment = PdfTextAlignment.Right;
            
            //Create page number field
            PdfPageNumberField pageNumber = new PdfPageNumberField(font, brush);

            var createdDate = new PdfCreationDateField(font, brush);
            createdDate.DateFormatString = CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern;
           
            //Create page count field
            PdfPageCountField count = new PdfPageCountField(font, brush);

            PdfCompositeField compositeField = new PdfCompositeField(font, brush, UserTexts.Resources.CreatedPageOf, createdDate, pageNumber, count);
            compositeField.StringFormat = format;
            compositeField.Bounds = header.Bounds;
            compositeField.Draw(header.Graphics);
            //Add header template at the top.
            doc.Template.Top = header;
        }
    }
}