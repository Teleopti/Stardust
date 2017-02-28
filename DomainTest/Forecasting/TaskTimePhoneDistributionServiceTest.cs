﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class TaskTimePhoneDistributionServiceTest
    {
        private TaskTimePhoneDistributionService target;

        [SetUp]
        public void Setup()
        {
            target = new TaskTimePhoneDistributionService();
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
            Assert.AreEqual(DistributionType.ByPercent,target.DistributionType);
        }
    }
}
