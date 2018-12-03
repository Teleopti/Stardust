using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class ServiceAgreementTest
    {
        private ServiceAgreement _target;
        private ServiceAgreement _targetEmail;

        [SetUp]
        public void Setup()
        {
            _target = ServiceAgreement.DefaultValues();
            _targetEmail = ServiceAgreement.DefaultValuesEmail();
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(new Percent(0.8), _target.ServiceLevel.Percent);
            Assert.AreEqual(20d, _target.ServiceLevel.Seconds);
            Assert.AreEqual(new Percent(0.30), _target.MinOccupancy);
            Assert.AreEqual(new Percent(0.90), _target.MaxOccupancy);
            Assert.AreEqual(new Percent(1), _targetEmail.ServiceLevel.Percent);
            Assert.AreEqual(7200d, _targetEmail.ServiceLevel.Seconds);
            Assert.AreEqual(new Percent(0), _targetEmail.MinOccupancy);
            Assert.AreEqual(new Percent(0), _targetEmail.MaxOccupancy);
        }

        [Test]
        public void VerifyConstructor()
        {
            ServiceLevel sl = new ServiceLevel(new Percent(0.90), 15);
            Percent minOcc = new Percent(0.40);
            Percent maxOcc = new Percent(0.80);
            _target = new ServiceAgreement(sl, minOcc, maxOcc);
            Assert.AreEqual(sl, _target.ServiceLevel);
            Assert.AreEqual(minOcc, _target.MinOccupancy);
            Assert.AreEqual(maxOcc, _target.MaxOccupancy);
        }
    }
}
