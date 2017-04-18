using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
    [TestFixture]
    public class PdfFontManagerTest
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldReturnSinoTypeSongLightOnSimplifiedChinese()
        {
            var cultureInfo = CultureInfo.GetCultureInfo("zh-CN");
            var font = new PdfCjkStandardFont(PdfCjkFontFamily.SinoTypeSongLight, 10, PdfFontStyle.Regular);
            Assert.That(PdfFontManager.GetFont(10,PdfFontStyle.Regular, cultureInfo).Name, Is.EqualTo(font.Name));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldReturnMonotypeSungLightOnTraditionalChinese()
        {
            var cultureInfo = CultureInfo.GetCultureInfo("zh-tw");
            var font = new PdfCjkStandardFont(PdfCjkFontFamily.MonotypeSungLight, 10, PdfFontStyle.Regular);
            Assert.That(PdfFontManager.GetFont(10, PdfFontStyle.Regular, cultureInfo).Name, Is.EqualTo(font.Name));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Kaku"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldReturnHeiseiKakuGothicW5OnJapanese()
        {
            var cultureInfo = CultureInfo.GetCultureInfo("ja");
            var font = new PdfCjkStandardFont(PdfCjkFontFamily.HeiseiKakuGothicW5, 10, PdfFontStyle.Regular);
            Assert.That(PdfFontManager.GetFont(10, PdfFontStyle.Regular, cultureInfo).Name, Is.EqualTo(font.Name));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hanyang"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldReturnHanyangSystemsGothicMediumOnKorean()
        {
            var cultureInfo = CultureInfo.GetCultureInfo("ko");
            var font = new PdfCjkStandardFont(PdfCjkFontFamily.HanyangSystemsGothicMedium, 10, PdfFontStyle.Regular);
            Assert.That(PdfFontManager.GetFont(10, PdfFontStyle.Regular, cultureInfo).Name, Is.EqualTo(font.Name));
        }

		[Test]
		public void ShouldReturnTahomaOnPersian()
		{
			var cultureInfo = CultureInfo.GetCultureInfo("fa-IR");
			var font = PdfFontManager.GetFont(9, PdfFontStyle.Regular, cultureInfo);
			Assert.AreEqual("Tahoma", font.Name);
		}

		[Test]
		public void ShouldReturnDownSizedFontOnBoldPersian()
		{
			var cultureInfo = CultureInfo.GetCultureInfo("fa-IR");
			var font = PdfFontManager.GetFont(10, PdfFontStyle.Bold, cultureInfo);
			Assert.AreEqual(9f, font.Size);
		}

		[Test]
		public void ShouldReturnDownSizedFontOnSize9Persian()
		{
			var cultureInfo = CultureInfo.GetCultureInfo("fa-IR");
			var font = PdfFontManager.GetFont(9, PdfFontStyle.Regular, cultureInfo);
			Assert.AreEqual(8f, font.Size);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldReturnHelveticaOnOthers()
        {
            var cultureInfo = CultureInfo.GetCultureInfo("sv");
            var theFont = new Font("Helvetica", 10, FontStyle.Regular);
            var font = new PdfTrueTypeFont(theFont, true);
           
            Assert.That(PdfFontManager.GetFont(10, PdfFontStyle.Regular, cultureInfo).Name, Is.EqualTo(font.Name));
        }
    }


}