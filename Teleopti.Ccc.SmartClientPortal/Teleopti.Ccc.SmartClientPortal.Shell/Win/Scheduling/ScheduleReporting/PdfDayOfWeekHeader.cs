using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.ScheduleReporting
{
    public class PdfDayOfWeekHeader
    {
        private readonly float _height = 100;
        private readonly PdfStringFormat _format;
        private readonly PdfBrush _brush;
        private readonly float _columnWidth;
        private readonly CultureInfo _cultureInfo;
        private readonly PdfGraphics _graphics;
        private readonly PdfTemplate _template;
        private const float RowSpace = 1;

        public PdfDayOfWeekHeader(float columnWidth, int dayOfWeek, bool rightToLeft, CultureInfo cultureInfo)
        {
            _template = new PdfTemplate(columnWidth, _height);
            _graphics = _template.Graphics;
            _format = new PdfStringFormat {RightToLeft = rightToLeft};
            _columnWidth = columnWidth;
            _cultureInfo = cultureInfo;
            _brush = new PdfSolidBrush(Color.DimGray);

            _height = renderDayName(dayOfWeek, 0);
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
        private float renderDayName(int dayOfWeek, float top)
        {
            _format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 9f;
            var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, _cultureInfo);
            var info = CultureInfo.CurrentUICulture;
            var daynames = info.DateTimeFormat.DayNames;
            _graphics.DrawString(daynames[dayOfWeek], font, _brush, new RectangleF(0, top + RowSpace, _columnWidth, fontSize + 2), _format);
            return top + fontSize + 2 + ((RowSpace + 1) * 2);
        }
    }
}