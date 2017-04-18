using System;
using System.Windows;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters;

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

        [Test]
        public void ShouldReturnNullOnMinValue()
        {
            _target.Convert(DateTime.MinValue, null, null, null).Should().Be.Null();
        }

        [Test]
        public void ShouldReturnNullOnMoreThanOneDay()
        {
            _target.Convert(DateTime.UtcNow.AddDays(-1).AddMinutes(-1), null, null, null).Should().Be.Null();
        }
    }
}
