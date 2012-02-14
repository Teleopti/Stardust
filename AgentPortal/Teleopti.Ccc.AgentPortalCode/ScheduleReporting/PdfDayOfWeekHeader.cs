using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.ScheduleReporting
{
    public class PdfDayOfWeekHeader
    {
        private readonly float _height = 100;
        private readonly PdfStringFormat _format;
        private readonly PdfBrush _brush;
        private readonly float _columnWidth;
        private readonly PdfGraphics _graphics;
        private readonly PdfTemplate _template;
        private const float RowSpace = 1;

        public PdfDayOfWeekHeader(float columnWidth, DateOnly dayOfWeek, bool rightToLeft)
        {
            _template = new PdfTemplate(columnWidth, _height);
            _graphics = _template.Graphics;
            _format = new PdfStringFormat {RightToLeft = rightToLeft};
            _columnWidth = columnWidth;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private float RenderDayName(DateOnly dayOfWeek, float top)
        {
            _format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 9f;
            PdfFont font = new PdfTrueTypeFont(new Font("Helvetica", fontSize, FontStyle.Bold), true);
            _graphics.DrawString(dayOfWeek.Date.ToString("dddd",CultureInfo.CurrentUICulture), font, _brush, new RectangleF(0, top + RowSpace, _columnWidth, fontSize + 2), _format);
            return top + fontSize + 2 + ((RowSpace + 1) * 2);
        }
    }
}