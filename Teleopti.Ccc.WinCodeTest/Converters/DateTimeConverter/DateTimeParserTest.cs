using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter;

namespace Teleopti.Ccc.WinCodeTest.Converters.DateTimeConverter
{
    [TestFixture]
    public class DateTimeParserTest
    {
        private DateTimeParser _target;
        private DateTime _dateTime1;
        private DateTime _dateTime2;

        [SetUp]
        public void Setup()
        {
            _dateTime1 = new DateTime(2001,12,12,2,1,5,0,DateTimeKind.Utc);
            _dateTime2 = new DateTime(2001,1,12,13,4,52,0,DateTimeKind.Utc);
            _target = new DateTimeParser();
        }

       

        #region Parse
        [Test]
        public void VerifyParseFromTime()
        {
            Assert.AreEqual(_dateTime1,_target.Parse("not ok", _dateTime1, DateTimeParseMode.Time),"Not ok returns same value");
            DateTime newDateTime = _target.Parse(_dateTime1.ToShortTimeString(), _dateTime2,
                                                 DateTimeParseMode.Time);

            Assert.AreEqual(_dateTime2.Date, newDateTime.Date);
            Assert.AreEqual(_dateTime1.Hour, newDateTime.Hour);
            Assert.AreEqual(_dateTime1.Minute, newDateTime.Minute);
        }

        [Test]
        public void VerifyParseFromTimeSecondsAndMillisecondsAreTruncated()
        {
            const int days = 0;
            const int hours = 2;
            const int minutes = 44;
            const int seconds = 19;
            const int milliseconds = 23;

            var t =new TimeSpan(days,hours,minutes,seconds,milliseconds);
            var newDateTime = _target.Parse(t.ToString(), _dateTime2, DateTimeParseMode.Time);

            Assert.AreEqual(_dateTime2.Date, newDateTime.Date);
            Assert.AreEqual(hours, newDateTime.Hour);
            Assert.AreEqual(minutes, newDateTime.Minute);
            Assert.AreEqual(0, newDateTime.Second);  //Will be zero
            Assert.AreEqual(0, newDateTime.Millisecond); //Will be zero
        }

        [Test]
        public void VerifyParseFromDate()
        {
            Assert.AreEqual(_dateTime1, _target.Parse("not ok", _dateTime1, DateTimeParseMode.Date), "Not ok returns same value");
            DateTime newDateTime = _target.Parse(_dateTime1.ToShortDateString(), _dateTime2, DateTimeParseMode.Date);
            Assert.AreEqual(_dateTime1.Date, newDateTime.Date);
            Assert.AreEqual(_dateTime2.Hour, newDateTime.Hour);
            Assert.AreEqual(_dateTime2.Minute, newDateTime.Minute);
        }

        [Test]
        public void VerifyParseFromDateSecondsAndMillisecondsAreTruncated()
        {
            _dateTime1.AddMilliseconds(12);
            var newDateTime = _target.Parse(_dateTime1.ToString(), _dateTime2, DateTimeParseMode.Date);

            Assert.AreEqual(_dateTime1.Date, newDateTime.Date);
            Assert.AreEqual(_dateTime2.Hour, newDateTime.Hour);
            Assert.AreEqual(_dateTime2.Minute, newDateTime.Minute);
            Assert.AreEqual(0, newDateTime.Second);
            Assert.AreEqual(0, newDateTime.Millisecond);
        }

        [Test]
        public void VerifyParseFromDateTime()
        {
            Assert.AreEqual(_dateTime1, _target.Parse("not ok", _dateTime1, DateTimeParseMode.DateTime), "Not ok returns same value");
             DateTime newDateTime = _target.Parse(_dateTime1.ToString(), _dateTime2,
                                                DateTimeParseMode.DateTime);

            Assert.AreEqual(_dateTime1.Date, newDateTime.Date);
            Assert.AreEqual(_dateTime1.Hour, newDateTime.Hour);
            Assert.AreEqual(_dateTime1.Minute, newDateTime.Minute);

        }
        #endregion

        #region ToString
        [Test]
        public void VerifyToGuiTime()
        {
            string result =_target.ToGuiText(_dateTime1, DateTimeParseMode.Time);
            Assert.AreEqual(_dateTime1.ToShortTimeString(),result);
        }

        [Test]
        public void VerifyToGuiDate()
        {
            string result = _target.ToGuiText(_dateTime1, DateTimeParseMode.Date);
            Assert.AreEqual(_dateTime1.ToShortDateString(), result);


        }

        [Test]
        public void VerifyToGuiDateTime()
        {
            string result = _target.ToGuiText(_dateTime1, DateTimeParseMode.DateTime);
            Assert.IsTrue(result.Contains(_dateTime1.ToShortDateString()));
            Assert.IsTrue(result.Contains(_dateTime1.ToShortTimeString()));
        }

       
        #endregion
    }
}
