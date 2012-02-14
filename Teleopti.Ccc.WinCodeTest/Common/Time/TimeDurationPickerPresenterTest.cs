using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Common.Time;

namespace Teleopti.Ccc.WinCodeTest.Common.Time
{
    [TestFixture]
    public class TimeDurationPickerPresenterTest
    {
        private MockRepository _mocks;
        private ITimeDurationPickerView _mockedTimeDurationPickerView;
        private TimeDurationPickerPresenter _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _mockedTimeDurationPickerView = _mocks.StrictMock<ITimeDurationPickerView>();
            _target = new TimeDurationPickerPresenter(_mockedTimeDurationPickerView);
        }

        [Test]
        public void CanSetNewInterval()
        {
            Assert.AreEqual(30, _target.Interval.Minutes);
            _target.Interval = new TimeSpan(0, 10, 0);
            Assert.AreEqual(10, _target.Interval.Minutes);
        }

        [Test]
        public void CanCreateTimeList()
        {
            Assert.AreEqual(30, _target.Interval.Minutes);
            IList<TimeSpanDataBoundItem> timeSpans = _target.CreateTimeList(TimeSpan.Zero, new TimeSpan(23, 59, 59));
            Assert.AreEqual(48, timeSpans.Count);
        }
    }
}
