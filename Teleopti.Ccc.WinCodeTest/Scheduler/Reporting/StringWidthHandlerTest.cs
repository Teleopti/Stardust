using System;
using System.Globalization;
using NUnit.Framework;
using Syncfusion.Pdf.Graphics;
using System.Linq;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
    [TestFixture]
    public class StringWidthHandlerTest
    {
        private StringWidthHandler _target;
        private const float FontSize = 9f;
        private CultureInfo _cultureInfo;
        private PdfFont _font;
        private const float MaxWidth = 25f;

        [SetUp]
        public void Setup()
        {
            _cultureInfo = CultureInfo.CurrentCulture;
            _font = PdfFontManager.GetFont(FontSize, PdfFontStyle.Regular, _cultureInfo);
            _target = new StringWidthHandler(_font, MaxWidth);
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

        [Test]
        public void ShouldNotCharWrapWhenNotExceedingMaxWidth()
        {
            var text = UserTexts.Resources.Ok;
            var lines = _target.CharWrap(text);

            Assert.AreEqual(1, lines.Count());
            Assert.AreEqual(text, lines[0]);
        }

        [Test]
        public void ShouldNotWordWrapWhenNotExceedingMaxWidth()
        {
            var text = UserTexts.Resources.Ok;
            var lines = _target.WordWrap(text);

            Assert.AreEqual(1, lines.Count());
            Assert.AreEqual(text, lines[0]);
        }

        [Test]
        public void ShouldCharWrapWhenExceedingMaxWidth()
        {
            var text = UserTexts.Resources.ChooseCancelIfYouWouldLikeToCorrectTheErrorsOrCheckEachErrorToOverrideTheBusinessRules;
            var lines = _target.CharWrap(text);

            Assert.IsTrue(lines.Count() > 1);

            var result = string.Empty;

            for(int i = 0; i < lines.Count(); i++)
            {
                Assert.IsTrue(_font.MeasureString(lines[i]).Width <= MaxWidth);
                result += lines[i];
            }

            Assert.AreEqual(text, result);
        }

        [Test]
        public void ShouldWordWrapWhenExceedingMaxWidth()
        {
            var text = UserTexts.Resources.ChooseCancelIfYouWouldLikeToCorrectTheErrorsOrCheckEachErrorToOverrideTheBusinessRules;
            var lines = _target.WordWrap(text);

            Assert.IsTrue(lines.Count() > 1);

            var result = string.Empty;

            for (int i = 0; i < lines.Count(); i++)
            {
                Assert.IsTrue(_font.MeasureString(lines[i]).Width <= MaxWidth);
                result += lines[i];
            }

            Assert.AreEqual(text, result);   
        }
    }
}
