using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.RestrictionSummary
{
    [TestFixture]
    public class WeekHeaderCellDataTest
    {
        private IWeekHeaderCellData _target;
        private TimeSpan _minPerWeek;
        private TimeSpan _maxPerWeek;
        private bool _alert;
        private int _weekNumber;

        [SetUp]
        public void Setup()
        {
            _weekNumber = 1;
            _alert = false;
            _minPerWeek = new TimeSpan(155,0,0);
            _maxPerWeek = new TimeSpan(165,0,0);
            _target = new WeekHeaderCellData(_minPerWeek, _maxPerWeek, _alert, _weekNumber);
        }
        [Test]
        public void CanCreateInstance()
        {
            Assert.IsNotNull(_target);
            _target = new WeekHeaderCellData(true);
            Assert.IsNotNull(_target);
            _target = new WeekHeaderCellData();
            Assert.IsNotNull(_target);
        }
        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_minPerWeek, _target.MinimumWeekWorkTime);
            Assert.AreEqual(_maxPerWeek, _target.MaximumWeekWorkTime);
            Assert.AreEqual(false, _target.Invalid);
            Assert.AreEqual(_weekNumber, _target.WeekNumber);
            Assert.AreEqual(true, _target.Validated);
            Assert.AreEqual(_alert, _target.Alert);
        }
    }
}
