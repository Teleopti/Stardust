using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.GuiHelpers
{
    [TestFixture]
    public class TimePixelRelationHelperTest
    {
        private TimePixelRelationHelper _tpr;
        private TimePeriod _period;
        private int _width;

        [SetUp]
        public void Setup()
        {
            _width = 675;
            _period = new TimePeriod(new TimeSpan(-1, 22, 0, 0), new TimeSpan(1, 2, 0, 0));
            _tpr = new TimePixelRelationHelper(_width, _period);
        }

        [Test]
        public void CanCalculateMinutesPerPixel()
        {
           Assert.AreEqual(_period.SpanningTime().TotalMinutes/_width,_tpr.MinutesPerPixel());
        }

        [Test]
        public void CanCalculateThePixelCorrespondingToAMinute()
        {
            double pixelFromMinute;
            int minute = 123;
            pixelFromMinute = _tpr.PixelFromTime(minute);
            Assert.AreEqual(Math.Round(243/_tpr.MinutesPerPixel(),0), pixelFromMinute);
        }

        [Test]
        public void CanCalculateThePixelCorrespondingToTimeSpan()
        {
            double pixelFromMinute;
            TimeSpan ts = new TimeSpan(-1, 23, 0, 0);
            pixelFromMinute = _tpr.PixelFromTime(ts);
            Assert.AreEqual(Math.Round(60 / _tpr.MinutesPerPixel(), 0), pixelFromMinute);
        }

        [Test]
        public void CanScale()
        {
            double pixelFromMinute;
            TimeSpan ts = new TimeSpan(-1, 23, 0, 0);
            _width = 1680;
            _tpr = new TimePixelRelationHelper(_width, _period);
            pixelFromMinute = _tpr.PixelFromTime(ts);
            Assert.AreEqual(60,pixelFromMinute);

            _width = 840;
            _tpr = new TimePixelRelationHelper(_width, _period);
            pixelFromMinute = _tpr.PixelFromTime(ts);
            Assert.AreEqual(30, pixelFromMinute);

            _width = 3360;
            _tpr = new TimePixelRelationHelper(_width, _period);
            pixelFromMinute = _tpr.PixelFromTime(ts);
            Assert.AreEqual(120, pixelFromMinute);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyLowLimit()
        {
            double pixelFromMinute;
            TimeSpan ts = new TimeSpan(-1, 21, 59, 0);
            pixelFromMinute = _tpr.PixelFromTime(ts);
            Assert.AreEqual(0, pixelFromMinute);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyHighLimit()
        {
            double pixelFromMinute;
            TimeSpan ts = new TimeSpan(1, 2, 1, 0);
            pixelFromMinute = _tpr.PixelFromTime(ts);
            Assert.AreEqual(0, pixelFromMinute);
        }

        [Test]
        public void VerifyWithinLimits()
        {
            double pixelFromMinute;
            TimeSpan ts = _period.StartTime;
            pixelFromMinute = _tpr.PixelFromTime(ts);
            Assert.AreEqual(0, pixelFromMinute);

            ts = _period.EndTime;
            pixelFromMinute = _tpr.PixelFromTime(ts);
            Assert.AreEqual(_width, pixelFromMinute);
        }
    }
}
