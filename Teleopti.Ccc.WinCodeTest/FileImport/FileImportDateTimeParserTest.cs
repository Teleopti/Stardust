using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.FileImport;

namespace Teleopti.Ccc.WinCodeTest.FileImport
{
    [TestFixture]
    public class FileImportDateTimeParserTest
    {
        private IFileImportDateTimeParser _target;
        private TimeZoneInfo _timeZone;
        private DateTime _utcDateTime;


        [SetUp]
        public void Setup()
        {
            _utcDateTime = new DateTime(2001, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            _target = new FileImportDateTimeParser();
            _timeZone = TimeZoneInfo.CreateCustomTimeZone("--", TimeSpan.FromMinutes(12), "--", "--");

        }

        [Test]
        public void VerifyParseToDateTime()
        {
            
            DateTime convertedDateTime = _target.UtcDateTime("20010101", "08:00");
            Assert.AreEqual(_utcDateTime, convertedDateTime, "Can parse");

            _target.TimeZone(_timeZone);
            convertedDateTime = _target.UtcDateTime("20010101", "08:12");
            Assert.AreEqual(_utcDateTime, convertedDateTime, "Parses and converts with the TimeZone");
        }

        [Test]
        public void VerifyReturnsTime()
        {
           
            Assert.AreEqual("08:00", _target.UtcTime("20010101", "08:00"));
        }

        [Test]
        public void VerifyReturnsTimeAndConvert()
        {
            _target.TimeZone(_timeZone);
            Assert.AreEqual("22:00", _target.UtcTime("20010101", "22:12"), "Parses and converts with the TimeZone");
        }

        [Test]
        public void VerifyInParameterDate()
        {
            Assert.Throws<FormatException>(() => _target.UtcDateTime("Hej","08:00"));
        }

        [Test]
        public void VerifyInParameterTime()
        {
            Assert.Throws<FormatException>(() => _target.UtcDateTime("20090101", "08:hej"));
        }

        [Test]
        public void VerifyDateTimeIsValid()
        {
            TimeZoneInfo swedish = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            _target.TimeZone(swedish);
            Assert.IsTrue(_target.DateTimeIsValid("20100328", "01:45"));
            Assert.IsFalse(_target.DateTimeIsValid("20100328", "02:00"));
        }
    }
}
