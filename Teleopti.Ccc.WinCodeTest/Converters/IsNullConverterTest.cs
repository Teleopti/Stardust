using System.Windows;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture]
    public class IsNullConverterTest
    {
        private IsNullConverter _target;

        [SetUp]
        public void Setup()
        {
            _target = new IsNullConverter();
        }

        [Test]
        public void VerifyConvert()
        {
            object value = _target.Convert(null, null, null, null);
            Assert.AreEqual(true, value);
            value = _target.Convert(new object(), null, null, null);
            Assert.AreEqual(false, value);
        }

        [Test]
        public void VerifyConvertBack()
        {
            object value = _target.ConvertBack(null, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);
        }
    }
}
