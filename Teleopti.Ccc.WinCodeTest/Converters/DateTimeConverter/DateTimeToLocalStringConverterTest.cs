using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter;

namespace Teleopti.Ccc.WinCodeTest.Converters.DateTimeConverter
{
    [TestFixture]
    public class DateTimeToLocalStringConverterTest
    {
        private DateTimeToLocalStringConverter _target;
        private TimeZoneInfo _timeZone;

        [SetUp]
        public void Setup()
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            _target = new DateTimeToLocalStringConverter();
        }

        [Test]
        public void VerifyDefaultDateTime()
        {
            //There should be a default datetime (UtcNow).
            Assert.IsTrue(DateTime.Now.AddMinutes(-1) < _target.LatestConvertedDateTime
                && _target.LatestConvertedDateTime < DateTime.Now.AddMinutes(1));
        }

        [Test]
        public void VerifyConvertBackReturnsDateTime()
        {
            //Create a string that can be parsed:
            DateTime origin = new DateTime(2001, 1, 1, 1, 0, 0, DateTimeKind.Utc);
            var resultFromConvert = _target.Convert(new object[] {origin, _timeZone}, typeof (DateTime), DateTimeParseMode.DateTime,
                                                    CultureInfo.CurrentCulture);
            var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(origin, _timeZone);
            Assert.AreEqual(string.Concat(localDateTime.ToShortDateString(), " ", localDateTime.ToShortTimeString()),
                            resultFromConvert);
            var result = _target.ConvertBack(resultFromConvert, new[] { typeof(string) }, null,
                                             CultureInfo.CurrentCulture);
            Assert.AreEqual(2,result.Length);
            Assert.AreEqual(origin, result[0]);
            Assert.AreEqual(_timeZone, result[1]);
            Assert.AreEqual(localDateTime, _target.LatestConvertedDateTime, "A successful parse should change the LatestConverted");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "LatestConverted"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "NUnit.Framework.Assert.AreEqual(System.Object,System.Object,System.String)"), Test]
        public void VerifyConvertBackReturnsActualDateTime()
        {
            //Create a string that can be parsed:
            DateTime origin = new DateTime(2001, 1, 1, 1, 0, 0, DateTimeKind.Utc);
            var resultFromConvert = _target.Convert(new object[] { origin, _timeZone }, typeof(DateTime), DateTimeParseMode.DateTime,
                                                    CultureInfo.CurrentCulture);
            var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(origin, _timeZone);
            Assert.AreEqual(string.Concat(localDateTime.ToShortDateString(), " ", localDateTime.ToShortTimeString()),
                            resultFromConvert);
            var result = _target.ConvertBack(null, new[] { typeof(string) }, null,
                                             CultureInfo.CurrentCulture);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(origin, result[0]);
            Assert.AreEqual(_timeZone, result[1]);
            Assert.AreEqual(localDateTime, _target.LatestConvertedDateTime, "A successful parse should change the LatestConverted");
        } 


        [Test]
        public void VerifyConvertChecksInParameter()
        {
            Assert.Throws<ArgumentException>(() => _target.Convert(new object[]{ "not datetime" }, typeof (DateTime), null, CultureInfo.CurrentCulture));
        }

        [Test]
        public void VerifyConvertUsesParser()
        {
            var mocks = new MockRepository();
            IDateTimeParser parser = mocks.StrictMock<IDateTimeParser>();
            DateTime utcDateTime = new DateTime(2001,1,1,1,1,0,DateTimeKind.Utc);
            Type targetType = typeof (DateTime);
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            const string guiString = "12:00";
            string result;
            _target = new DateTimeToLocalStringConverter();
            _target.Parser = parser;
         
            using (mocks.Record())
            {
                //Verify that the result is passed to the parser
                Expect.Call(parser.ToGuiText(utcDateTime,DateTimeParseMode.DateTime)).Return(guiString);
            }
            using(mocks.Playback())
            {
                result = _target.Convert(new object[]{ utcDateTime, TimeZoneInfo.Utc}, targetType, null, cultureInfo).ToString();
            }

            Assert.AreEqual(guiString,result,"The result should be the result of the parser");
        }
    }
}
