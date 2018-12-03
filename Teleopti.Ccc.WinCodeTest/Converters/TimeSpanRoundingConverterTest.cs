using System;
using System.Windows;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters;


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
		 public void VerifyDateTimePeriod()
		 {
		 	var startdate = new DateTime(2000, 1, 1, 1, 1, 1,10,DateTimeKind.Utc);
			var endDate = new DateTime(2000, 1, 1, 1, 1, 2, 0, DateTimeKind.Utc);
		 	_target.Convert(new DateTimePeriod(startdate, endDate), null, null, null)
				.Should().Be.EqualTo(TimeSpan.FromSeconds(1));
		 }

        [Test]
        public void VerifyConvertBack()
        {
            object value = _target.ConvertBack(null, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);
        }

        [Test]
        public void ShouldThrowOnWrongType()
        {
	        Assert.Throws<ArgumentException>(() => _target.Convert(33, null, null, null));
        }
    }
}
