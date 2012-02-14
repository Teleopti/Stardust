using System;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.WinCodeTest.Common.GuiHelpers
{
    
    [TestFixture]
    public class TimeSpanSecondsTest
    {
        private TimeSpanSeconds target;
        private TimeSpan _timeSpan;
       
        [SetUp]
        public void Setup()
        {
            _timeSpan = new TimeSpan(0,2,12,23,45);
            target = new TimeSpanSeconds(_timeSpan);    
        }

        [Test]
        public void CanCreateFromTimeSpan()
        {
            target = new TimeSpanSeconds(_timeSpan);
            Assert.IsNotNull(target);
        } 

        [Test]
        public void CanSetAndGetTimeSpanProperty()
        {
            TimeSpan ts = new TimeSpan(0, 2, 12, 21, 33);
            target.TimeSpan = ts;

            Assert.AreEqual(target.TimeSpan, ts);
        }

        [Test]
        public void CanSetAndGetSecondsProperty()
        {
            target.Seconds = 55.23;
            Assert.AreEqual(55.23,target.Seconds);
            target.Seconds = 23.231;
            Assert.AreEqual(23.231, target.Seconds);
            target.Seconds = 2343434.23;
            Assert.AreEqual(2343434.23, target.Seconds);
            target.Seconds = 23433.231;
            Assert.AreEqual(23433.231, target.Seconds);
            target.Seconds = 238938249238.231;
            Assert.AreEqual(238938249238.231, target.Seconds);
        }

        [Test]
        public void CanSetGetCorrectSeconds()
        {
            //7943.045 The sum of targets seconds
            Assert.AreEqual(7943.045, target.Seconds);
        }

        [Test]
        public void ToStringWorks()
        {

            double seconds = 56.23;
            target.Seconds = seconds;
            Assert.AreEqual(seconds.ToString(CultureInfo.CurrentCulture),target.ToString());
        }

        [Test]
        public void CanSetString()
        {
            string gnagare = (12.34).ToString(CultureInfo.CurrentCulture);
            target = gnagare;
            Assert.AreEqual(12.34, target.Seconds);
        }


        [Test]
        public void CanGetTimeSpan()
        {
            TimeSpan timeSpan = target;
            Assert.AreEqual(target.TimeSpan, timeSpan);
        }

        [Test]
        public void CanGetStringImplicitOperator()
        {
            target.Seconds = 1;
            string test = target;
            Assert.AreEqual("1", test);
        }

        [Test]
        public void CanGetDoubleImplicitOperator()
        {
            target.Seconds = 123.456;
            double test = target;
            Assert.AreEqual(123.456, test);
        }

        [Test]
        public void CanGetFromTimeSpan()
        {
            target = TimeSpan.FromSeconds(123.456);
            TimeSpan test = target;
            Assert.AreEqual(TimeSpan.FromSeconds(123.456), test);
        }
    }
}
