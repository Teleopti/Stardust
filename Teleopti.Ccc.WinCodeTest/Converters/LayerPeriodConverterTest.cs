using System.Windows;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Converters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture]
    public class LayerPeriodConverterTest
    {
        private LayerPeriodConverter _target;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new LayerPeriodConverter();
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
            var layer = _mocks.StrictMock<ILayer<IActivity>>();
            DateTimePeriod period = new DateTimePeriod();
            Expect.Call(layer.Period).Return(period);
            _mocks.ReplayAll();

            object value = _target.Convert(layer, null, null, null);
            _mocks.VerifyAll();

            Assert.AreEqual(period.LocalStartDateTime, value);
        }

        [Test]
        public void VerifyConvertBack()
        {
            object value = _target.ConvertBack(null, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);
        }
    }
}
