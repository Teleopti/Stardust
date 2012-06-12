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
    	private CultureInfo _culture;

        private const float RowSpace = 1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void ExportIndividual(IList<PersonDto> persons, DateOnlyPeriod period, AgentScheduleStateHolder stateHolder, bool rightToLeft, ScheduleReportDetail details, Control owner, bool singleFile, string path, CultureInfo culture)
        {
            //DayOfWeek firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
            _headerColumnWidth = 0;
            _brush = new PdfSolidBrush(Color.DimGray);
            _pen = new PdfPen(Color.Gray, 1);
            _rtl = rightToLeft;
        	_culture = culture;

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

        private void IndividualPages(bool rightToLeft, IList<PersonDto> persons, DateOnlyPeriod fullWeekPeriod, AgentScheduleStateHolder stateHolder, ScheduleReportDetail details, Control owner, bool singleFile, string path)
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void OpenDocument(PdfDocument doc, Control owner, string fullPath)
        {
            bool success = false;
            try
            {
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
                		                                                          part, rtl, details, _culture);
                        weekList.Add(schedule);
                        break;

                    case SchedulePartView.DayOff:
                        if (part.PersonAssignmentCollection.Length == 0)
                        {
                            IPdfScheduleTemplate dayOff = new PdfScheduleDayOff(_scheduleColumnWidth,
																				part.PersonDayOff, rtl, _culture);
                            weekList.Add(dayOff);
                        }
                        else
                        {
                            IPdfScheduleTemplate dayOff = new PdfScheduleDayOffOvertime(_scheduleColumnWidth, part, rtl, _culture);
                            weekList.Add(dayOff);
                        }

                        break;

                    case SchedulePartView.FullDayAbsence:
                        IPdfScheduleTemplate fullDayAbsence = new PdfScheduleFullDayAbsence(_scheduleColumnWidth,
                                                                                             part, rtl, _culture);
                        weekList.Add(fullDayAbsence);
                        break;

                    default:
                        IPdfScheduleTemplate empty = new PdfScheduleNotScheduled(_scheduleColumnWidth, date, rtl, _culture);
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
            float height = new PdfDayOfWeekHeader(_scheduleColumnWidth, firstDayOfWeek, rtl, _culture).Height;  //dynamic
            for (int i = 0; i < 7; i++)
            {
                DateOnly dow = firstDayOfWeek.AddDays(i);
                float left = _headerColumnWidth + (i * _scheduleColumnWidth);

                PdfDayOfWeekHeader header = new PdfDayOfWeekHeader(_scheduleColumnWidth, dow, rtl, _culture);
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
			PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, _culture);
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
			const float fontSize = 6f;
			PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, _culture);
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
			const float fontSize = 6f;
			PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, _culture);
            var format = new PdfStringFormat
                             {
                                 RightToLeft = _rtl,
                                 LineAlignment = PdfVerticalAlignment.Top,
                                 Alignment = PdfTextAlignment.Right
                             };
            
            //Create page number field
            var pageNumber = new PdfPageNumberField(font, brush);
            var createdDate = new PdfCreationDateField(font, brush);
            createdDate.DateFormatString = CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern;
           

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
