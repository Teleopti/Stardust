﻿using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;

namespace Teleopti.Ccc.Sdk.WcfService.Factory.ScheduleReporting
{
    public class PdfDayOfWeekHeader
    {
        private float _height = 100;
        private PdfStringFormat _format;
        private PdfBrush _brush;
        private float _columnWidth;
        private readonly CultureInfo _uiCulture;
        private PdfGraphics _graphics;
        private PdfTemplate _template;
        private const float ROW_SPACE = 1;

        public PdfDayOfWeekHeader(float columnWidth, int dayOfWeek, bool rightToLeft, CultureInfo uiCulture)
        {
            _template = new PdfTemplate(columnWidth, _height);
            _graphics = _template.Graphics;
            _format = new PdfStringFormat();
            _format.RightToLeft = rightToLeft;
            _columnWidth = columnWidth;
            _uiCulture = uiCulture;
            _brush = new PdfSolidBrush(Color.DimGray);
            _height = RenderDayName(dayOfWeek, 0);
        }

        public float Height
        {
            get { return _height; }
        }

        public PdfTemplate Template
        {
            get { return _template; }
        }

        private float RenderDayName(int dayOfWeek, float top)
        {
            _format.Alignment = PdfTextAlignment.Center;
            float fontSize = 9f;
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, fontSize, PdfFontStyle.Bold);
            string[] daynames = _uiCulture.DateTimeFormat.DayNames;
            _graphics.DrawString(daynames[dayOfWeek], font, _brush, new RectangleF(0, top + ROW_SPACE, _columnWidth, fontSize + 2), _format);
            return top + fontSize + 2 + ((ROW_SPACE + 1) * 2);
        }
    }
}