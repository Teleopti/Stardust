using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting.ForecastFormattedFile;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Forecasting.ForecastsFormattedFile
{
    [TestFixture]
    public class ForecastsFileTest
    {
        private MockRepository _mocks;
        private ForecastsFile _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            var file = _mocks.StrictMock<byte[]>();
            _target = new ForecastsFile(file);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

    }
}
