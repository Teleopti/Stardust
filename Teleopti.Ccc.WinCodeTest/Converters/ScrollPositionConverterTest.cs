using System;
using System.Windows;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Converters;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture]
    public class ScrollPositionConverterTest
    {
        private ScrollPositionConverter _target;

        [SetUp]
        public void Setup()
        {
            _target = new ScrollPositionConverter();
        }

        [Test]
        public void VerifyConvertUnsetValue()
        {
            object[] values = new object[2] { DependencyProperty.UnsetValue, 1 };
            object value = _target.Convert(values, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);

            values = new object[2] { 1, DependencyProperty.UnsetValue };
            value = _target.Convert(values, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);
        }

        [Test]
        public void VerifyConvert()
        {
            object[] values = new object[2] { 1.2, 2.0 };
            object value = _target.Convert(values, null, null, null);
            Assert.AreEqual(-2.4, value);
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void VerifyConvertBack()
        {
            _target.ConvertBack(null, null, null, null);
        }
    }
}
