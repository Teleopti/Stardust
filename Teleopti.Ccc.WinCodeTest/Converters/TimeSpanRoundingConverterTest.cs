using System;
using System.Windows;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Converters;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture]
    public class TimeSpanRoundingConverterTest
    {
        private TimeSpanRoundingConverter _target;

        [SetUp]
        public void Setup()
        {
            _target = new TimeSpanRoundingConverter();
        }

        [Test]
        public void VerifyConvertNull()
        {
            object value = _target.Convert(null, null, null, null);
            Assert.IsNull(value);
        }

        [Test]
        public void VerifyConvert()
        {
            TimeSpan timeSpan = new TimeSpan(1, 12, 59, 59, 501);
            object value = _target.Convert(timeSpan, null, null, null);
            Assert.AreEqual(new TimeSpan(1, 13, 00, 00), value);
        }

        [Test]
        public void VerifyConvertBack()
        {
            object value = _target.ConvertBack(null, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);
        }
    }
}
