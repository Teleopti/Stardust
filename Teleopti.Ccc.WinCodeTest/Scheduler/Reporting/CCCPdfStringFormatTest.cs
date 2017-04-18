using Syncfusion.Pdf.Graphics;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
    [TestFixture]
    public class CccPdfStringFormatTest
    {
        private CccPdfStringFormat _cccPdfStringFormat;

        [SetUp]
        public void Setup()
        {
            _cccPdfStringFormat = new CccPdfStringFormat();
        }

        [Test]
        public void ShouldSetDefaultValues()
        {
            Assert.AreEqual(PdfWordWrapType.Word, _cccPdfStringFormat.WordWrapType);
            Assert.AreEqual(PdfTextAlignment.Left, _cccPdfStringFormat.Alignment);
            Assert.AreEqual(PdfVerticalAlignment.Top, _cccPdfStringFormat.LineAlignment);
            Assert.IsTrue(_cccPdfStringFormat.PdfStringFormat.RightToLeft);       
        }

        [Test]
        public void ShouldReturnProperties()
        {
            _cccPdfStringFormat.WordWrapType = PdfWordWrapType.None;
            _cccPdfStringFormat.Alignment = PdfTextAlignment.Center;
            _cccPdfStringFormat.LineAlignment = PdfVerticalAlignment.Bottom;

            Assert.AreEqual(PdfWordWrapType.None, _cccPdfStringFormat.WordWrapType);
            Assert.AreEqual(PdfTextAlignment.Center, _cccPdfStringFormat.Alignment);
            Assert.AreEqual(PdfVerticalAlignment.Bottom, _cccPdfStringFormat.LineAlignment);
            Assert.IsTrue(_cccPdfStringFormat.PdfStringFormat.RightToLeft);
        }
    }
}
