using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting
{
	public class ScheduleReportDrawHeader
	{
		private const int TopPos = 10;
		private const int LeftPos = 0;
		private const int Padding = 5;
		private const float FontSize = 14f;
		private readonly PdfPage _page;
		private readonly string _text;
		private readonly bool _rightToLeft;
		private readonly CultureInfo _cultureInfo;
		private readonly PdfSolidBrush _brush;
		

		public ScheduleReportDrawHeader(PdfPage page, string text, bool rightToLeft, CultureInfo cultureInfo)
		{
			_page = page;
			_text = text;
			_rightToLeft = rightToLeft;
			_cultureInfo = cultureInfo;
			_brush = new PdfSolidBrush(Color.DimGray);
		}

		public float Draw()
		{
			var format = new PdfStringFormat { RightToLeft = _rightToLeft, Alignment = PdfTextAlignment.Left, LineAlignment = PdfVerticalAlignment.Middle };
			var font = PdfFontManager.GetFont(FontSize, PdfFontStyle.Regular, _cultureInfo);
			var rect = new RectangleF(LeftPos, TopPos, (int)(_page.GetClientSize().Width), font.Height + Padding);
			PdfBrush backGround = new PdfSolidBrush(Color.PowderBlue);
			_page.Graphics.DrawRectangle(backGround, rect);
			_page.Graphics.DrawString(_text, font, _brush, rect, format);
			return TopPos + font.Height + Padding;	
		}
	}
}
