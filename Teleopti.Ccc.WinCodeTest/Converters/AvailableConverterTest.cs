using System.Windows;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Converters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture]
    public class AvailableConverterTest
    {
        private AvailableConverter _target;

        [SetUp]
        public void Setup()
        {
            _target = new AvailableConverter();
        }

        [Test]
        public void VerifyConvert()
        {
            object value = _target.Convert(false, null, null, null);
            Assert.AreEqual("Not available", value);
            value = _target.Convert(true, null, null, null);
            Assert.AreEqual("Available", value);
        }

        [Test]
        public void VerifyConvertBack()
        {
            object value = _target.ConvertBack(null, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);
        }
    }
}
