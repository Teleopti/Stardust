using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Meetings.Overview;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Overview
{
    [TestFixture]
    public class MeetingTimeZoneHelperTest
    {
        private TimeZoneInfo _timeZone;
        private TimeZoneInfo _userZone;
        private MeetingTimeZoneHelper _target;

        [SetUp]
        public void Setup()
        {
            //finland +2
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));
            _userZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
            _target = new MeetingTimeZoneHelper(_userZone);
        }

        [Test]
        public void ShouldConvertToMeetingTimeZone()
        {
            var meetingStart = new DateTime(2011, 3, 26, 3, 30, 0);
            Assert.That(_target.ConvertToUserTimeZone(meetingStart, _timeZone), Is.EqualTo(meetingStart.AddHours(-1)));
        }

        [Test]
        public void ShouldAddOneHourWhenConvertingToNotValidTime()
        {
            // 3:30 in Finland becomes 2:30 in sweden = invalid
            // should add one more hour to 3:30
            var meetingStart = new DateTime(2011, 3, 27, 3, 30, 0);
            Assert.That(_target.ConvertToUserTimeZone(meetingStart,_timeZone),Is.EqualTo(meetingStart));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfTimeZoneIsNull()
        {
            var meetingStart = new DateTime(2011, 3, 27, 3, 30, 0);
            Assert.That(_target.ConvertToMeetingTimeZone(meetingStart, null), Is.EqualTo(meetingStart));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowHereTooIfTimeZoneIsNull()
        {
            var meetingStart = new DateTime(2011, 3, 27, 3, 30, 0);
            Assert.That(_target.ConvertToUserTimeZone(meetingStart, null), Is.EqualTo(meetingStart));
        }

        [Test]
        public void ShouldAddOneHourWhenConvertingToNotValidTimeToMeeting()
        {
            _timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
            _userZone = (TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));

            // 3:30 in Finland becomes 2:30 in sweden = invalid
            // should add one more hour to 3:30
            var meetingStart = new DateTime(2011, 3, 27, 3, 30, 0);
            Assert.That(_target.ConvertToMeetingTimeZone(meetingStart, _timeZone), Is.EqualTo(meetingStart));
        }

        [Test]
        public void ShouldConvertCorrectFromOtherSideOfAtlantic()
        {
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time")); // -8
            _userZone = TimeZoneInfoFactory.StockholmTimeZoneInfo(); // +1

            // 26:e 17:30 in Alaska becomes 27:e 2:30 in sweden = invalid
            // should add one more hour to 3:30
            var meetingStart = new DateTime(2011, 3, 26, 17, 30, 0);
            Assert.That(_target.ConvertToUserTimeZone(meetingStart, _timeZone), Is.EqualTo(new DateTime(2011, 3, 27, 3, 30, 0)));
        }
    }

}