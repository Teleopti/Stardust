﻿using System.Drawing;
using System.Linq;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting
{
    public class ScheduleReportGraphicalDrawDayOff
    {
        private readonly IScheduleDay _scheduleDay;
        private DateTimePeriod _timelinePeriod;
        private Rectangle _projectionRectangle;
        private readonly bool _rightToLeft;
        private readonly PdfGraphics _pdfGraphics;

        public ScheduleReportGraphicalDrawDayOff(IScheduleDay scheduleDay, DateTimePeriod timelinePeriod, Rectangle projectionRectangle, bool rightToLeft, PdfGraphics pdfGraphics)
        {
            _scheduleDay = scheduleDay;
            _timelinePeriod = timelinePeriod;
            _projectionRectangle = projectionRectangle;
            _rightToLeft = rightToLeft;
            _pdfGraphics = pdfGraphics;
        }

        public Rectangle Draw()
        {
            var personDayOff = PersonDayOff();
            if (personDayOff == null) return Rectangle.Empty;

            var rectangle = ViewBaseHelper.GetLayerRectangle(_timelinePeriod, _projectionRectangle, Period(), _rightToLeft);

            if (rectangle.IsEmpty) return rectangle;

						DrawLayer(rectangle, personDayOff.DisplayColor);
						DrawDescription(personDayOff.Description.ShortName, rectangle);

            return rectangle;
        }

        private void DrawLayer(Rectangle rectangle, Color color)
        {
            var brush = GradientBrush(rectangle, color);
            _pdfGraphics.DrawRectangle(brush, rectangle); 
        }

        private void DrawDescription(string text, Rectangle rectangle)
        {
            var culture = TeleoptiPrincipal.Current.Regional.Culture;
            var brush = new PdfSolidBrush(Color.Gray);
            var format = new PdfStringFormat { RightToLeft = _rightToLeft, WordWrap = PdfWordWrapType.None };
            var font = PdfFontManager.GetFont(9f, PdfFontStyle.Regular, culture);
            var width = font.MeasureString(text).Width;
            var left = rectangle.Width / 2 + rectangle.Left - width / 2;
            var descRectangle = new RectangleF(left, rectangle.Top, left + width, rectangle.Height);
            _pdfGraphics.DrawString(text, font, brush, descRectangle, format);
        }

        private static PdfLinearGradientBrush GradientBrush(Rectangle destinationRectangle, Color color)
        {
            var rect = new Rectangle(destinationRectangle.X, destinationRectangle.Y - 1, destinationRectangle.Width, destinationRectangle.Height + 2);
            return new PdfLinearGradientBrush(rect, Color.WhiteSmoke, color, 90);
        }

        public PdfTilingBrush TilingBrush()
        {
            var rect = new RectangleF(0, 0, _projectionRectangle.Height, _projectionRectangle.Height);
            var tilingBrush = new PdfTilingBrush(rect);
            var pen = new PdfPen(Color.LightGray, 0.5f);
            tilingBrush.Graphics.DrawLine(pen, 0, _projectionRectangle.Height, _projectionRectangle.Height, 0);
            return tilingBrush;   
        }

        public IDayOff PersonDayOff()
        {
            if(_scheduleDay.SignificantPartForDisplay() != SchedulePartView.DayOff || !_scheduleDay.HasDayOff()) return null;

            var overtime = (from p in _scheduleDay.PersonAssignmentCollectionDoNotUse()
                            where p.OvertimeLayers().Any()
                            select p).ToList();


            return overtime.Count > 0 ? null : _scheduleDay.PersonAssignment().DayOff();
        }

        public DateTimePeriod Period()
        {
            var startTime = _timelinePeriod.StartDateTime;
            var endTime = _timelinePeriod.EndDateTime;

            if (startTime < _scheduleDay.Period.StartDateTime)
                startTime = _scheduleDay.Period.StartDateTime;

            if (endTime > _scheduleDay.Period.EndDateTime)
                endTime = _scheduleDay.Period.EndDateTime;

            return new DateTimePeriod(startTime, endTime);
        }
    }
}
