using System;
using NUnit.Framework;

using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Panels
{

    /// <summary>
    /// Tests for LengthToTimeCalculator
    /// </summary>
    [TestFixture]
    public class LengthToTimeCalculatorTest
    {
        private LengthToTimeCalculator _target;
        private DateTimePeriod _period;
        private readonly DateTime _baseDateTime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private int _days = 10;
        private double _length = 100;

        [SetUp]
        public void Setup()
        {
            _period = new DateTimePeriod(_baseDateTime, _baseDateTime.AddDays(_days));
            _target = new LengthToTimeCalculator(_period, _length);
        }

        [Test]
        public void VerifyCalculatorDateTimes()
        {
            Assert.AreEqual(_period.StartDateTime, _target.DateTimeFromPosition(0));
            Assert.AreEqual(_period.EndDateTime, _target.DateTimeFromPosition(100));
            Assert.AreEqual(_period.StartDateTime.AddDays(_days / 5), _target.DateTimeFromPosition(_length / 5));

            //Outside Boundries
            Assert.AreEqual(_period.StartDateTime.AddDays(-1*_days),_target.DateTimeFromPosition(-1*_length));
            Assert.AreEqual(_period.StartDateTime.AddDays(3*_days),_target.DateTimeFromPosition(3*_length));
        }


        [Test]
        public void VerifyDateTimeKindIsUtc()
        {
           Assert.Throws<ArgumentException>(() => _target.PositionFromDateTime(new DateTime(2001, 1, 1, 8, 0, 0, DateTimeKind.Local)));
        }

        [Test]
        public void VerifyCanGetPositionFromDateTime()
        {
            Assert.AreEqual(0,_target.PositionFromDateTime(_period.StartDateTime));
            Assert.AreEqual(_length, _target.PositionFromDateTime(_period.EndDateTime));
            Assert.AreEqual(_length/2, _target.PositionFromDateTime(_baseDateTime.AddDays(_days/2)));
            
            //Outside Boundries
            Assert.AreEqual(-100,_target.PositionFromDateTime(_baseDateTime.Subtract(TimeSpan.FromDays(_days))));
            Assert.AreEqual(200,_target.PositionFromDateTime(_baseDateTime.AddDays(_days*2)));
        }

        [Test]
        public void VerifyCanGetPositionFromDateTimeRightToLeft()
        {
            Assert.AreEqual(70, _target.PositionFromDateTime(_baseDateTime.AddDays(3), true));
        }

        [Test]
        public void VerifyCanGetDateTimeFromPositionRightToLeft()
        {
            Assert.AreEqual(_baseDateTime.AddDays(3), _target.DateTimeFromPosition(70, true));
        }

        [Test]
        public void VerifyCanCreateRectangle()
        {
            Rectangle rect = _target.RectangleFromDateTimePeriod(new DateTimePeriod(_baseDateTime.AddDays(1), _baseDateTime.AddDays(3)),new Point(11, 7), 20, false);
            Assert.AreEqual(21, rect.X);
            Assert.AreEqual(7, rect.Y);
            Assert.AreEqual(20, rect.Height);
            Assert.AreEqual(20, rect.Width);
        }

        [Test]
        public void VerifyCanCreateRectangleWithRounding()
        {
            Rectangle rect = _target.RectangleFromDateTimePeriod(new DateTimePeriod(_baseDateTime.AddDays(1), _baseDateTime.AddDays(3).AddHours(13)), new Point(11, 7), 20, false);
            Assert.AreEqual(25, rect.Width);
            rect = _target.RectangleFromDateTimePeriod(new DateTimePeriod(_baseDateTime.AddDays(1), _baseDateTime.AddDays(3).AddHours(14)), new Point(11, 7), 20, false);
            Assert.AreEqual(26, rect.Width);
        }

        [Test]
        public void VerifyCanCreateRectangleRightToLeft()
        {
            Rectangle rect = _target.RectangleFromDateTimePeriod(new DateTimePeriod(_baseDateTime.AddDays(1), _baseDateTime.AddDays(3)), new Point(11, 7), 20, true);
            Assert.AreEqual(81, rect.X);
            Assert.AreEqual(7, rect.Y);
            Assert.AreEqual(20, rect.Height);
            Assert.AreEqual(20, rect.Width);
        }
    }
}
