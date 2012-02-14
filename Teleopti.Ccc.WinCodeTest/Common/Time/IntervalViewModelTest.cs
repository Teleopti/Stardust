using System;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.Time;
using Teleopti.Ccc.WinCodeTest.Helpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Time
{
    [TestFixture]
    public class IntervalViewModelTest
    {
        private IntervalViewModel _target;
        private DateTime _start;
        private TimeSpan _interval;
        private DateTimePeriod _period;

        [SetUp]
        public void Setup()
        {
            _start = new DateTime(2001, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            _interval = TimeSpan.FromHours(1);
            _period = new DateTimePeriod(_start, _start.Add(_interval));
            _target = new IntervalViewModel();
        }


        [Test]
        public void VerifyContentChanges()
        {
            _target.Period = _period;
            Assert.AreEqual(_target.Label, _start);
            DateTimePeriod newPeriod = new DateTimePeriod(_start.Add(_interval), _start.Add(_interval).Add(_interval));
            PropertyChangedListener listener = new PropertyChangedListener().ListenTo(_target);
            _target.Period = newPeriod;
            Assert.IsTrue(listener.HasOnlyFired("Label"));
            Assert.AreEqual(_target.Period.StartDateTime, _target.Label);
        }


    }
}
