using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.ScheduleReporting
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void ExportIndividual(IList<PersonDto> persons,
            DateOnlyPeriod period, AgentScheduleStateHolder stateHolder, bool rightToLeft, ScheduleReportDetail details, 
            Control owner, bool singleFile, string path)
        {
            //DayOfWeek firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
            _headerColumnWidth = 0;
            _brush = new PdfSolidBrush(Color.DimGray);
            _pen = new PdfPen(Color.Gray, 1);
            _rtl = rightToLeft;

            try
            {
                IndividualPages(rightToLeft, persons, period, stateHolder, details, owner, singleFile, path);
            }
            catch (Exception ex)
            {
                // this is ok here. We swallow the error, but the application should not break. 
                // if the logic changes, remove the fx cop warning from the top of the method
                MessageDialogs.ShowError(owner, ex.Message, Resources.ExportToPDF);
            }

        }


        //public void ExportTeam(ICccTimeZoneInfo timeZoneInfo, CultureInfo culture, IList<PersonDto> persons,
        //    DateOnlyPeriod period, AgentScheduleStateHolder stateHolder, bool rightToLeft, ScheduleReportDetail details, Control owner, string path)
        //{
        //    _headerColumnWidth = 70;
        //    DayOfWeek firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
        //    DateOnly outerStart = new DateOnly(DateHelper.GetFirstDateInWeek(period.StartDate.Date, culture).Date);
        //    DateOnly outerEnd = new DateOnly(DateHelper.GetLastDateInWeek(period.EndDate.Date, culture).Date);
        //    DateOnlyPeriod fullWeekPeriod = new DateOnlyPeriod(outerStart, outerEnd);

        //    _brush = new PdfSolidBrush(Color.DimGray);
        //    _pen = new PdfPen(Color.Gray, 1);
        //    _rtl = rightToLeft;

        //    //Create a new document.
        //    PdfDocument doc = new PdfDocument();
        //    doc.PageSettings.Orientation = PdfPageOrientation.Landscape;

        //    TeamPages(rightToLeft, doc, persons, firstDayOfWeek, fullWeekPeriod, stateHolder, timeZoneInfo, details);

        //    AddHeader(doc);
        //    AddFooter(doc, Resources.PoweredByTeleoptCCC);

            
        //    OpenDocument(doc, owner, path);

        //}

        private void IndividualPages(bool rightToLeft, IList<PersonDto> persons, DateOnlyPeriod fullWeekPeriod, AgentScheduleStateHolder stateHolder, 
            ScheduleReportDetail details, Control owner, bool singleFile, string path)
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
            
            foreach (PersonDto person in persons)
            {
                if (!singleFile)
                {
                    doc = new PdfDocument
                              {
                                  PageSettings = {Orientation = PdfPageOrientation.Landscape}
                              };
                }
                _reportTitle = person.Name;
                float top;
                top = NewPage(doc, fullWeekPeriod.DayCollection()[0], 10, true, out page);

                int offset = 0;   
                while (offset < fullWeekPeriod.DayCount())
                {
                    //skapa en PdfSchedule per dag hela veckan
                    IList<IPdfScheduleTemplate> weekList = CreateWeek(rightToLeft, fullWeekPeriod, offset, stateHolder, details);
                    float height = GetHeight(weekList);

                    if (top + height > page.GetClientSize().Height)
                    {
                        top = NewPage(doc, fullWeekPeriod.DayCollection()[0], 0, true, out page);
                    }

                    for (int i = 0; i < 7; i++)
                    {
                        float left = _headerColumnWidth + (i * _scheduleColumnWidth);
                        DrawLine(top, page.GetClientSize().Width);
                        weekList[i].Template.Draw(page, left, top);
                    }

                    top += height + 3;
                    offset += 7;
                }

                if (!singleFile)
                {
                    CreateOpenDocument(doc, owner, path);
                } 
            }

            if (singleFile)
            {
                CreateOpenDocument(doc, owner, path);
            } 
        }

        private void CreateOpenDocument(PdfDocument doc, Control owner, string path)
        {
            AddHeader(doc);
            AddFooter(doc, Resources.PoweredByTeleoptCCC);
            string fileName = Path.GetRandomFileName();
            fileName = fileName.Replace(".", "");
            OpenDocument(doc, owner, path + "\\" + fileName + ".PDF");
        }

        //private void TeamPages(bool rightToLeft, PdfDocument doc, IList<PersonDto> persons, DayOfWeek firstDayOfWeek, DateOnlyPeriod fullWeekPeriod, AgentScheduleStateHolder stateHolder, ICccTimeZoneInfo timeZoneInfo, ScheduleReportDetail details)
        //{
        //    PdfPage page;

        //    int offset = 0;
        //    while (offset < fullWeekPeriod.DayCount())
        //    {
        //        _reportTitle = string.Concat(fullWeekPeriod.DayCollection()[offset].ToShortDateString(), " -- ",
        //                                     fullWeekPeriod.DayCollection()[offset + 6].ToShortDateString());
        //        float top;
        //        top = NewPage(doc, firstDayOfWeek, 10, true, out page);

        //        foreach (PersonDto person in persons)
        //        {
        //            //skapa en PdfSchedule per dag hela veckan
        //            IList<IPdfScheduleTemplate> weekList;
        //            weekList = CreateWeek(rightToLeft, fullWeekPeriod, offset, stateHolder,details);
        //            float height = GetHeight(weekList);

        //            if (top + height > page.GetClientSize().Height)
        //            {
        //                top = NewPage(doc, firstDayOfWeek, 0, false, out page);
        //            }

        //            RectangleF rect = new RectangleF(new PointF(0, top), new SizeF(_headerColumnWidth, height));
        //            DrawRowHeader(rect, person.Name);

        //            for (int i = 0; i < 7; i++)
        //            {
        //                float left = _headerColumnWidth + (i*_scheduleColumnWidth);
        //                DrawLine(top, page.GetClientSize().Width);
        //                weekList[i].Template.Draw(page, left, top);
        //            }

        //            top += height + 3;
                    
        //        }
        //        offset += 7;
        //    }
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void OpenDocument(PdfDocument doc, Control owner, string fullPath)
        {
            bool success = false;
            try
            {
                //doc.ViewerPreferences.HideMenubar = true;
                doc.Save(fullPath);
                success = true;
            }
            catch (IOException ex)
            {
                MessageDialogs.ShowError(owner, ex.Message, Resources.Scheduling);
            }

            if (!success)
                return;

            //Launching the PDF file using the default Application.[Acrobat Reader]
            try
            {
                Process.Start(fullPath);
            }
            catch (Exception)
            {
                MessageDialogs.ShowError(owner,
                                         Resources.TheFileCouldNotBeOpenedPleaseCheckIfPdfFilesAreAssociatedWithAProgram,
                                         Resources.OpenFile);
            }
        }

        //private static void OpenDocument(PdfDocument doc, Control owner, string path)
        //{
        //    string fileName = "16887";
        //    OpenDocument(doc, owner, fileName);
        //}

        private static float GetHeight(IList<IPdfScheduleTemplate> templateList)
        {
            float ret = 0;
            foreach (IPdfScheduleTemplate template in templateList)
            {
                if(template.Height > ret)
                    ret = template.Height;
            }

            return ret;
        }

        private IList<IPdfScheduleTemplate> CreateWeek(bool rtl, DateOnlyPeriod fullWeekPeriod, int offset,
            AgentScheduleStateHolder stateHolder, ScheduleReportDetail details)
        {
            IList<IPdfScheduleTemplate> weekList = new List<IPdfScheduleTemplate>();
            for (int i = 0; i < 7; i++)
            {
                DateOnly date = fullWeekPeriod.DayCollection()[i + offset];
                SchedulePartDto part = stateHolder.AgentSchedulePartDictionary[date];
                SchedulePartView significantPart = SchedulePartView.None;

                if (part.PersonAssignmentCollection.Length > 0)
                    significantPart = SchedulePartView.PersonalShift;

                if (part.ProjectedLayerCollection.Length > 0)
                    significantPart = SchedulePartView.MainShift;

                if (part.PersonDayOff != null)
                    significantPart = SchedulePartView.DayOff;

                if (part.IsFullDayAbsence)
                    significantPart = SchedulePartView.FullDayAbsence;

                

                switch (significantPart)
                {
                    case SchedulePartView.MainShift:
                        IPdfScheduleTemplate schedule = new PdfScheduleAssignment(_scheduleColumnWidth,
                                                                        part,rtl, details);
                        weekList.Add(schedule);
                        break;

                    case SchedulePartView.DayOff:
                        if (part.PersonAssignmentCollection.Length == 0)
                        {
                            IPdfScheduleTemplate dayOff = new PdfScheduleDayOff(_scheduleColumnWidth,
                                                                                part.PersonDayOff, rtl);
                            weekList.Add(dayOff);
                        }
                        else
                        {
                            IPdfScheduleTemplate dayOff = new PdfScheduleDayOffOvertime(_scheduleColumnWidth, part, rtl);
                            weekList.Add(dayOff);
                        }

                        break;

                    case SchedulePartView.FullDayAbsence:
                        IPdfScheduleTemplate fullDayAbsence = new PdfScheduleFullDayAbsence(_scheduleColumnWidth,
                                                                                             part, rtl);
                        weekList.Add(fullDayAbsence);
                        break;

                    default:
                        IPdfScheduleTemplate empty = new PdfScheduleNotScheduled(_scheduleColumnWidth, date, rtl);
                        weekList.Add(empty);
                        break;
                }
            }

            return weekList;
        }

        private float NewPage(PdfDocument doc, DateOnly firstDayOfWeek, float top, bool newReport, out PdfPage page)
        {
            page = doc.Pages.Add();
            _graphics = page.Graphics;

            _scheduleColumnWidth = (page.GetClientSize().Width - _headerColumnWidth) / 7;
            if (newReport)
                top = DrawReportHeader(top, page.GetClientSize().Width, _reportTitle) + 3;

            DrawColumns(top, page.GetClientSize().Height);
            top = DrawWeekHeaders(firstDayOfWeek, top, _rtl);
            return top;
        }

        private void DrawLine(float top, float width)
        {
            _graphics.DrawLine(_pen, 0, top, width, top);
        }

        private void DrawColumns(float top, float height)
        {
            for (int i = 0; i < 7; i++)
            {
                float left = _headerColumnWidth + (i * _scheduleColumnWidth);
                _graphics.DrawLine(_pen, left, top, left, height);
            }

        }

        private float DrawWeekHeaders(DateOnly firstDayOfWeek, float top, bool rtl)
        {
            float height = new PdfDayOfWeekHeader(_scheduleColumnWidth, firstDayOfWeek, rtl).Height;  //dynamic
            for (int i = 0; i < 7; i++)
            {
                //if ((int)firstDayOfWeek + i < 7)
                //{
                //    dow = (DayOfWeek) i + (int) firstDayOfWeek;
                //}
                //else
                //{
                //    dow = (DayOfWeek)i + (int)firstDayOfWeek - 7;
                //}
                DateOnly dow = firstDayOfWeek.AddDays(i);
                float left = _headerColumnWidth + (i * _scheduleColumnWidth);

                PdfDayOfWeekHeader header = new PdfDayOfWeekHeader(_scheduleColumnWidth, dow, rtl);
                header.Template.Draw(_graphics, left, top);
            }

            return top + height;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private float DrawReportHeader(float top, float width, string text)
        {
            PdfStringFormat format = new PdfStringFormat();
            format.RightToLeft = _rtl;
            format.Alignment = PdfTextAlignment.Left;
            format.LineAlignment = PdfVerticalAlignment.Middle;
            const float fontSize = 14f;
            //PdfFont font = new PdfTrueTypeFont("Arial", fontSize, PdfFontStyle.Bold);
            PdfFont font = new PdfTrueTypeFont(new Font("Helvetica", fontSize, FontStyle.Bold), true);
            var headerRect = new RectangleF(0, top + RowSpace, width, fontSize + 4);
            PdfBrush backGround = new PdfSolidBrush(Color.PowderBlue);
            _graphics.DrawRectangle(backGround, headerRect);
            _graphics.DrawString(text, font, _brush, headerRect, format);
            return top + fontSize + 4 + RowSpace;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void AddFooter(PdfDocument doc, string footerText)
        {
            var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            //Create a page template
            var footer = new PdfPageTemplateElement(rect);

            var brush = new PdfSolidBrush(Color.Gray);

            PdfFont font = new PdfTrueTypeFont(new Font("Helvetica", 6, FontStyle.Bold), true);
            var format = new PdfStringFormat();
            format.RightToLeft = _rtl;
            format.Alignment = PdfTextAlignment.Center;
            format.LineAlignment = PdfVerticalAlignment.Bottom;
            footer.Graphics.DrawString(footerText, font, brush, rect, format);

            //Add the footer template at the bottom
            doc.Template.Bottom = footer;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void AddHeader(PdfDocument doc)
        {
            var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            //Create page template
            var header = new PdfPageTemplateElement(rect);
            var brush = new PdfSolidBrush(Color.Gray);

            PdfFont font = new PdfTrueTypeFont(new Font("Helvetica", 6, FontStyle.Bold), true);
            var format = new PdfStringFormat
                             {
                                 RightToLeft = _rtl,
                                 LineAlignment = PdfVerticalAlignment.Top,
                                 Alignment = PdfTextAlignment.Right
                             };
            //format.Alignment = PdfTextAlignment.Left;

            //Create page number field
            var pageNumber = new PdfPageNumberField(font, brush);
            var createdDate = new PdfCreationDateField(font, brush);
            createdDate.DateFormatString = CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern.ToString();
           

            //Create page count field
            var count = new PdfPageCountField(font, brush);

            var compositeField = new PdfCompositeField(font, brush, Resources.CreatedPageOf, createdDate, pageNumber, count);
            compositeField.StringFormat = format;
            compositeField.Bounds = header.Bounds;
            compositeField.Draw(header.Graphics); 

            //Add header template at the top.
            doc.Template.Top = header;

        }
    }
}
