using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting
{
	public class ScheduleReportGraphicalDrawNameDate
	{
		private readonly PdfPage _page;
		private float _top;
		private readonly string _text;
		private readonly bool _rightToLeft;
		private readonly CultureInfo _cultureInfo;
		private readonly PdfSolidBrush _brush;
		private const int Padding = 5;
		private float _columnWidth;
		private const int RowSpace = 2;

		public ScheduleReportGraphicalDrawNameDate(PdfPage page,float top, string text, bool rightToLeft, CultureInfo cultureInfo)
		{
			_top = top;
			_text = text;
			_rightToLeft = rightToLeft;
			_cultureInfo = cultureInfo;
			_page = page;
			_brush = new PdfSolidBrush(Color.DimGray);
		}

		public float DrawHeader(string value)
		{
			_top = _top + Padding;
		    var format = new CccPdfStringFormat();
			const float fontSize = 10f;
			var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, _cultureInfo);
			_columnWidth = font.MeasureString(value).Width + Padding;
			var left = 0;

			if(_rightToLeft)
				left = (int)(_page.GetClientSize().Width - _columnWidth);

			_page.Graphics.DrawString(_text, font, _brush, new RectangleF(left, _top, left + _columnWidth, font.Height + Padding), format.PdfStringFormat);
			return _top + font.Height + Padding;	
		}

		public float DrawData(int width)
		{
		    var format = new CccPdfStringFormat {WordWrapType = PdfWordWrapType.None};
			const float fontSize = 9f;
			var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, _cultureInfo);
			var left = 0;

			if(_rightToLeft)
				left = (int)(_page.GetClientSize().Width - width);
	
			_page.Graphics.DrawString(_text, font, _brush, new RectangleF(left, _top + RowSpace, left + width, font.Height + RowSpace), format.PdfStringFormat);
			return _top + font.Height + RowSpace;
		}

		public float ColumnWidth
		{
			get { return _columnWidth; }
		}
	}
}
