using System;
using System.Globalization;
using NUnit.Framework;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
    [TestFixture]
    public class StringWidthHandlerTest
    {
        private StringWidthHandler _target;
        private const float FontSize = 9f;
        private CultureInfo _cultureInfo;

        [SetUp]
        public void Setup()
        {
            _cultureInfo = CultureInfo.CurrentCulture;
            _target = new StringWidthHandler(FontSize, PdfFontStyle.Regular, 25f, _cultureInfo);
        }

        [Test]
        public void ShouldGetWholeStringWhenWidthLessThanMaxWidth()
        {
            var font = PdfFontManager.GetFont(FontSize, PdfFontStyle.Regular, _cultureInfo);
            var text = UserTexts.Resources.Ok;
            var returnValue = _target.GetString(text);

            Assert.IsTrue(font.MeasureString(text).Width < 100f);
            Assert.AreEqual(text, returnValue);
        }

        [Test]
        public void ShouldGetChoppedOffStringWhenWidthGreaterThanMaxWidth()
        {
            var font = PdfFontManager.GetFont(FontSize, PdfFontStyle.Regular, _cultureInfo);
            var text = UserTexts.Resources.ChooseCancelIfYouWouldLikeToCorrectTheErrorsOrCheckEachErrorToOverrideTheBusinessRules;
            var returnValue = _target.GetString(text);

            Assert.IsTrue(font.MeasureString(text).Width > 100f);
            Assert.IsTrue(font.MeasureString(returnValue).Width < font.MeasureString(text).Width);
            Assert.IsTrue(returnValue.EndsWith(UserTexts.Resources.ThreeDots, StringComparison.CurrentCulture));
            Assert.AreNotEqual(text, returnValue);
        }
    }
}
