using System.Windows;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Converters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture]
    public class LayerDescriptionConverterTest
    {
        private LayerDescriptionConverter _target;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new LayerDescriptionConverter();
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
            ILayer layer = _mocks.CreateMock<ILayer>();
            IPayload payload = _mocks.CreateMock<IPayload>();
            Description description = new Description("fsdf");
            Expect.Call(layer.Payload).Return(payload);
            Expect.Call(payload.ConfidentialDescription()).Return(description);
            _mocks.ReplayAll();

            object value = _target.Convert(layer, null, null, null);
            _mocks.VerifyAll();

            Assert.AreEqual(description, value);
        }

        [Test]
        public void VerifyConvertBack()
        {
            object value = _target.ConvertBack(null, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);
        }
    }
}
