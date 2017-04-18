using System.Globalization;
using NUnit.Framework;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
	[TestFixture]
	public class ScheduleReportDrawHeaderTest
	{
		private ScheduleReportDrawHeader _drawHeader;
		private PdfFont _font;
		private CultureInfo _culture;

		[SetUp]
		public void Setup()
		{
			var doc = new PdfDocument();
			var page = doc.Pages.Add();
			_culture = CultureInfo.GetCultureInfo("en-US");
			_font = PdfFontManager.GetFont(14f, PdfFontStyle.Regular, _culture );
			_drawHeader = new ScheduleReportDrawHeader(page, UserTexts.Resources.Test, CultureInfo.GetCultureInfo("en-US"));
		}

		[Test]
		public void ShouldDrawAndReturnTop()
		{
			var top =_drawHeader.Draw();
			Assert.AreEqual(_font.Height + 10 + 5, top);
		}
	}
}
