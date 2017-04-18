using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    /// <summary>
    /// This Conveterer is needed when TwoWay-Databinding to TimeSpans Totalxxx (minutes etc) because:
    /// a. TimeSpanConverter does not implement IValueConverter
    /// b. Totalxxx is readonly
    /// </summary>
    /// <remarks>
    /// henrika 2008-08-15
    /// </remarks>
    [TestFixture]
    public class TimeSpanConverterTest
    {
        private TimeSpanConverter _target;

        [SetUp]
        public void Setup()
        {
            _target = new TimeSpanConverter();
        }
	

        [Test]
        public void VerifyConvertsToTotalMinutesByDefault()
        {
            _target = new TimeSpanConverter();
            TimeSpan t = new TimeSpan(1,1,3);
            Assert.AreEqual(t.TotalMinutes, (double)_target.Convert(t, typeof(string), null, null));
        }

        [Test]
        public void VerifyConvertsBackToTotalMinutesByDefault()
        {
            _target = new TimeSpanConverter();
            TimeSpan t = new TimeSpan(1, 1, 3);
            Assert.AreEqual(t, (TimeSpan)_target.ConvertBack(t.TotalMinutes, typeof(string), null, null));
        }

        [Test]
        public void VerifyOnlyTakesTimeSpanAsArgument()
        {
	        Assert.Throws<ArgumentException>(() =>
	        {
				_target = new TimeSpanConverter();
				Assert.AreEqual(0d, (double)_target.Convert(DateTime.Now, typeof(string), null, null));
	        });
        }

        [Test]
        public void VerifyOnlyTakesDoubleAsConvertBackArgument()
        {
	        Assert.Throws<ArgumentException>(() =>
	        {
				_target = new TimeSpanConverter();
				Assert.AreEqual(TimeSpan.FromMinutes(15), (TimeSpan)_target.ConvertBack(DateTime.Now, typeof(string), null, null));
	        });
        }
    }
}
