using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture]
    public class ColumnWidthConverterTest
    {
        private ColumnWidthConverter _target;

        [SetUp]
        public void Setup()
        {
            _target = new ColumnWidthConverter();
        }

        [Test]
        public void VerifyConvert()
        {
            object[] values = new object[2] { 10d, 20d };
            object value = _target.Convert(values, null, null, null);
            Assert.AreEqual(30, value);
        }

        [Test]
        public void VerifyConvertBack()
        {
            Assert.Throws<NotImplementedException>(() => _target.ConvertBack(null, null, null, null));
        }
    }
}
