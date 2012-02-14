using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling.Panels;
using Teleopti.Interfaces.Domain;
using System.Drawing;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Panels
{
    [TestFixture]
    public class LengthToDateCalculatorTest
    {
        private LengthToDateCalculator _target;
        private DateOnlyPeriod _period;
        private readonly DateOnly _baseDate = new DateOnly(2001, 1, 1);
        private int _days = 10;
        private double _length = 100;

        [SetUp]
        public void Setup()
        {
            _period = new DateOnlyPeriod(_baseDate, _baseDate.AddDays(_days));
            _target = new LengthToDateCalculator(_period, _length);
        }

        [Test]
        public void VerifyCalculatorDateTimes()
        {
            Assert.AreEqual(_period.StartDate, _target.DateTimeFromPosition(0));
            Assert.AreEqual(_period.EndDate, _target.DateTimeFromPosition(100));
            Assert.AreEqual(_period.StartDate.AddDays(_days / 5), _target.DateTimeFromPosition(_length / 5));

            //Outside Boundries
            Assert.AreEqual(_period.StartDate.AddDays(-1*_days),_target.DateTimeFromPosition(-1*_length));
            Assert.AreEqual(_period.StartDate.AddDays(3*_days),_target.DateTimeFromPosition(3*_length));
        }

        [Test]
        public void VerifyCanGetPositionFromDateTime()
        {
            Assert.AreEqual(0,_target.PositionFromDateTime(_period.StartDate));
            Assert.AreEqual(_length, _target.PositionFromDateTime(_period.EndDate));
            Assert.AreEqual(_length/2, _target.PositionFromDateTime(_baseDate.AddDays(_days/2)));
            
            //Outside Boundries
            Assert.AreEqual(-100,_target.PositionFromDateTime(_baseDate.AddDays(-_days)));
            Assert.AreEqual(200,_target.PositionFromDateTime(_baseDate.AddDays(_days*2)));
        }

        [Test]
        public void VerifyCanGetPositionFromDateTimeRightToLeft()
        {
            Assert.AreEqual(70, _target.PositionFromDateTime(_baseDate.AddDays(3), true));
        }

        [Test]
        public void VerifyCanGetDateTimeFromPositionRightToLeft()
        {
            Assert.AreEqual(_baseDate.AddDays(3), _target.DateTimeFromPosition(70, true));
        }

        [Test]
        public void VerifyCanCreateRectangle()
        {
            Rectangle rect = _target.RectangleFromDateTimePeriod(new DateOnlyPeriod(_baseDate.AddDays(1), _baseDate.AddDays(3)),new Point(11, 7), 20, false);
            Assert.AreEqual(21, rect.X);
            Assert.AreEqual(7, rect.Y);
            Assert.AreEqual(20, rect.Height);
            Assert.AreEqual(20, rect.Width);
        }

        [Test]
        public void VerifyCanCreateRectangleRightToLeft()
        {
            Rectangle rect = _target.RectangleFromDateTimePeriod(new DateOnlyPeriod(_baseDate.AddDays(1), _baseDate.AddDays(3)), new Point(11, 7), 20, true);
            Assert.AreEqual(81, rect.X);
            Assert.AreEqual(7, rect.Y);
            Assert.AreEqual(20, rect.Height);
            Assert.AreEqual(20, rect.Width);
        }
    }
}
