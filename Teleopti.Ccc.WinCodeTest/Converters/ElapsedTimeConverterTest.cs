using System;
using System.Windows;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Converters;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture]
    public class ElapsedTimeConverterTest
    {
        private ElapsedTimeConverter _target;

        [SetUp]
        public void Setup()
        {
            _target = new ElapsedTimeConverter();
        }

        [Test]
        public void VerifyConvertNull()
        {
            var value = _target.Convert(null, null, null, null);
            Assert.IsNull(value);
        }

        [Test]
        public void VerifyConvert()
        {
            var period = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
            var value = _target.Convert(period, null, null, null);
            Assert.That(value, Is.EqualTo(new TimeSpan(1, 0, 0)));
        }

        [Test]
        public void VerifyConvertBack()
        {
            object value = _target.ConvertBack(null, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);
        }
    }
}
