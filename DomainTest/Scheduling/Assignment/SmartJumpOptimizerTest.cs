using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    //missing tests here for the actual optimization
    //add if needed
    [TestFixture]
    public class SmartJumpOptimizerTest : FilterLayerBaseTest
    {
        protected override IFilterOnPeriodOptimizer CreateOptimizer()
        {
            return new SmartJumpOptimizer(5);
        }


        [Test]
        public void VerifyEndSize()
        {
            Assert.AreEqual(5, ((SmartJumpOptimizer)Optimizer).EndSize);
        }

        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void CannotSetEndSizeLowerThanOne()
        {
            new SmartJumpOptimizer(0);
        }
    }
}
