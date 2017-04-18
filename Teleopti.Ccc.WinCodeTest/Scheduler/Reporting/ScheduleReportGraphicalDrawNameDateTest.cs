using System.Globalization;
using NUnit.Framework;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
	[TestFixture]
	public class ScheduleReportGraphicalDrawNameDateTest
	{
		private ScheduleReportGraphicalDrawNameDate _drawNameDate;
		private PdfFont _fontHeader;
		private PdfFont _fontData;
		private CultureInfo _culture;

		[SetUp]
		public void Setup()
		{
			var doc = new PdfDocument();
			var page = doc.Pages.Add();
			_culture = CultureInfo.GetCultureInfo("en-US");
			_fontHeader = PdfFontManager.GetFont(10f, PdfFontStyle.Regular, _culture);
			_fontData = PdfFontManager.GetFont(9f, PdfFontStyle.Regular, _culture);
			_drawNameDate = new ScheduleReportGraphicalDrawNameDate(page, 0, UserTexts.Resources.Test, false, _culture);	
		}

		[Test]
		public void ShouldDrawHeader()
		{
			var top = _drawNameDate.DrawHeader(UserTexts.Resources.Test);
			Assert.AreEqual(_fontHeader.Height + 5 + 5, top);
			Assert.AreEqual(_fontHeader.MeasureString(UserTexts.Resources.Test).Width + 5, _drawNameDate.ColumnWidth);
		}

		[Test]
		public void ShouldDrawData()
		{
			var top = _drawNameDate.DrawData(10);
			Assert.AreEqual(_fontData.Height + 2, top);
		}
	}
}
