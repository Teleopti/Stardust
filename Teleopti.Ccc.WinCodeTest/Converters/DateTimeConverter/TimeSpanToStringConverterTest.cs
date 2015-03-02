using System;
using System.Windows;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Converters.DateTimeConverter;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.WinCodeTest.Converters.DateTimeConverter
{
    [TestFixture]
    public class TimeSpanToStringConverterTest
    {
        private TimeSpanToStringConverter _target;

        [SetUp]
        public void Setup()
        {
            _target = new TimeSpanToStringConverter();
        }

        [Test]
        public void VerifyConvert()
        {
            TimeSpan timeSpan = new TimeSpan(0, 5, 30, 25);
            object value = _target.Convert(timeSpan, null, null, TeleoptiPrincipal.CurrentPrincipal.Regional.Culture);

            Assert.AreEqual("5:30", value);
        }

        [Test]
        public void VerifyConvertBack()
        {
            object value = _target.ConvertBack(null, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);
        }
    }
}
