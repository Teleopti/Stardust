using System;
using System.Windows;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Converters;
using Teleopti.Interfaces.Domain;

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
            object value = _target.Convert(null, null, null, null);
            Assert.IsNull(value);
        }

        [Test]
        public void VerifyConvert()
        {
            DateTimePeriod period = new DateTimePeriod(
                new DateTime(2008, 1, 1,0,0,0,DateTimeKind.Utc),
                new DateTime(2008, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            object value = _target.Convert(period, null, null, null);
            Assert.AreEqual(TimeSpan.FromDays(1), value);
        }

        [Test]
        public void VerifyConvertBack()
        {
            object value = _target.ConvertBack(null, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);
        }
    }
}
