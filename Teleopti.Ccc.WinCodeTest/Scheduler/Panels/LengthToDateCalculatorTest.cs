using NUnit.Framework;

using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;

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
