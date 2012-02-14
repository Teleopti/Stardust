using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.Time;

namespace Teleopti.Ccc.WinCodeTest.Common.Time
{
    [TestFixture]
    public class TimeSpanDataBoundItemTest
    {
        private TimeSpanDataBoundItem _target;

        [SetUp]
        public void Setup()
        {
            TimeSpan timeSpan = new TimeSpan(0, 2, 15, 0);
            _target = new TimeSpanDataBoundItem(timeSpan);
        }

        [Test]
        public void VerifyInstance()
        {
            Assert.IsNotNull(_target);
            Assert.IsNotNull(_target.TimeSpan);
        }

        [Test]
        public void VerifyFormattedText()
        {
            Assert.AreEqual("2:15", _target.FormattedText);
            TimeSpanDataBoundItem timeSpanDataBoundItem = new TimeSpanDataBoundItem(new TimeSpan(2, 5, 0));
            Assert.AreEqual("2:05", timeSpanDataBoundItem.FormattedText);
            timeSpanDataBoundItem = new TimeSpanDataBoundItem(new TimeSpan(10, 5, 0));
            Assert.AreEqual("10:05", timeSpanDataBoundItem.FormattedText);
        }
    }
}
