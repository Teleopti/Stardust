using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory.ScheduleReporting
{
    internal abstract class PdfScheduleTemplate : IPdfScheduleTemplate
    {
        private float _height = 50;
        private PdfTemplate _template;
        private PdfStringFormat _format;
        private PdfGraphics _graphics;
        private PdfBrush _brush;
        private float _columnWidth;

        protected const float RowSpace = 1;

        #region Implementation of IPdfScheduleTemplate

        public float Height
        {
            get { return _height; }
            protected set { _height = value;}
        }

        public PdfTemplate Template
        {
            get { return _template; }
            protected set { _template = value;}
        }

        protected PdfStringFormat Format
        {
            get { return _format; }
            set { _format = value; }
        }

        protected PdfGraphics Graphics
        {
            get { return _graphics; }
            set { _graphics = value; }
        }

        protected PdfBrush Brush
        {
            get { return _brush; }
            set { _brush = value; }
        }

        protected float ColumnWidth
        {
            get { return _columnWidth; }
            set { _columnWidth = value; }
        }

        #endregion

        protected float RenderDate(DateTime startDateTime, float top, CultureInfo culture)
        {
            _format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 9f;
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, fontSize);
            Graphics.DrawString(startDateTime.ToString(culture.DateTimeFormat.ShortDatePattern, culture), font, Brush,
                                new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2), _format);
            return top + fontSize + 2 + RowSpace;
        }

        protected float RenderText(string text, float top)
        {
            _format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 9f;
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, fontSize, PdfFontStyle.Bold);
            Graphics.DrawString(text, font, Brush, new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2), _format);
            return top + fontSize + 2 + RowSpace;
        }

        protected float RenderContractTime(TimeSpan contractTime, float top)
        {
            _format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 8f;
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, fontSize, PdfFontStyle.Bold);
			Graphics.DrawString(TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentCulture), font, Brush, new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2), _format);
            return top + fontSize + 2 + RowSpace;
        }

        protected float RenderSplitter(Color color, float top, float height)
        {
            PdfBrush b = new PdfSolidBrush(color);
            PdfPen pen = new PdfPen(b, height);
            top = top + RowSpace + (height/2);
            Graphics.DrawLine(pen, 5, top, ColumnWidth - 5, top);
            return top + pen.Width + RowSpace;
        }
    }
}