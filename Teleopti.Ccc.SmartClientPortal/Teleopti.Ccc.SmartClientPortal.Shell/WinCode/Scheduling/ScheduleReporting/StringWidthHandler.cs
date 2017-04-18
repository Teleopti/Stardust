using System;
using System.Linq;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting
{
    public class StringWidthHandler
    {
        private readonly float _maxWidth;
        private readonly PdfFont _font;

        public StringWidthHandler(PdfFont font, float maxWidth)
        {
            _font = font;
            _maxWidth = maxWidth;
        }

        public string GetString(string text)
        {
            if (_font.MeasureString(text).Width > _maxWidth)
            {
                while (_font.MeasureString(text + UserTexts.Resources.ThreeDots).Width > 130)
                {
                    text = text.Substring(0, text.Length - 1);
                }

                text = text + UserTexts.Resources.ThreeDots;
            }

            return text;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Pdf.Graphics.PdfFont.MeasureString(System.String)")]
        public string[] WordWrap(string text)
        {
            if(text == null) throw new ArgumentNullException("text");

            text = text.Replace(Environment.NewLine, " ");
            if (_font.MeasureString(text).Width < _maxWidth) return new[] { text };

            var words = text.Split(' ');
            var wrappedString = string.Empty;
            var currentLine = string.Empty;

            foreach (var t in words)
            {
                currentLine = currentLine + " " + t;
                currentLine = currentLine.TrimStart();
               
                if (_font.MeasureString(currentLine).Width <= _maxWidth) continue;
                var lines = CharWrap(currentLine);
                for (var j = 0; j < lines.Count() - 1; j++)
                {
                    if (wrappedString.Length > 0) wrappedString += Environment.NewLine;
                    wrappedString += lines[j];
                }

                currentLine = lines[lines.Count() - 1];
            }

            if (wrappedString.Length > 0) wrappedString += Environment.NewLine;
            wrappedString += currentLine;
            return wrappedString.Split(Environment.NewLine);
        }

        public string[] CharWrap(string line)
        {
            if(line == null) throw new ArgumentNullException("line");

            var currentLine = string.Empty;
            var wrappedString = string.Empty;

            if (_font.MeasureString(line).Width > _maxWidth)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    var currentChar = line[i];
                    currentLine += currentChar;

                    if (_font.MeasureString(currentLine).Width <= _maxWidth) continue;
                    currentLine = currentLine.Remove(currentLine.Length - 1);
                    if (wrappedString.Length > 0) wrappedString += Environment.NewLine;
                    wrappedString += currentLine;
                    currentLine = string.Empty;
                    i--;
                }

                wrappedString += Environment.NewLine + currentLine;
            }

            if (wrappedString.Length == 0) wrappedString = line;
            return wrappedString.Split(Environment.NewLine);
        }
    }
}
