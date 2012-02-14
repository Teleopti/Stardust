using System.Globalization;
using Syncfusion.Pdf.Graphics;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting
{
    public class StringWidthHandler
    {
        private readonly float _fontSize;
        private readonly PdfFontStyle _fontStyle;
        private readonly float _maxWidth;
        private readonly CultureInfo _cultureInfo;

        public StringWidthHandler(float fontSize, PdfFontStyle fontStyle, float maxWidth, CultureInfo cultureInfo)
        {
            _fontSize = fontSize;
            _fontStyle = fontStyle;
            _maxWidth = maxWidth;
            _cultureInfo = cultureInfo;
        }

        public string GetString(string text)
        {
            var font = PdfFontManager.GetFont(_fontSize, _fontStyle, _cultureInfo);

            if (font.MeasureString(text).Width > _maxWidth)
            {
                while (font.MeasureString(text + UserTexts.Resources.ThreeDots).Width > 130)
                {
                    text = text.Substring(0, text.Length - 1);
                }

                text = text + UserTexts.Resources.ThreeDots;
            }

            return text;
        }
    }
}
