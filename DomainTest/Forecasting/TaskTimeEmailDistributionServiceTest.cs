using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class TaskTimeEmailDistributionServiceTest
    {
        private TaskTimeEmailDistributionService target;

        [SetUp]
        public void Setup()
        {
            target = new TaskTimeEmailDistributionService();
        }

        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyInstanceImplementsCorrectInterface()
        {
            Assert.IsInstanceOf<ITaskTimeDistributionService>(target);
        }

        [Test]
        public void VerifyHasCorrectDistributionType()
        {
            Assert.AreEqual(DistributionType.Even,target.DistributionType);
        }
    }
}
