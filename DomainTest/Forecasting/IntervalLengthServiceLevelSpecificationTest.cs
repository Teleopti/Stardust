using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class IntervalLengthServiceLevelSpecificationTest
    {
        private IntervalLengthServiceLevelSpecification _target;

        [SetUp]
        public void Setup()
        {
            _target = new IntervalLengthServiceLevelSpecification(15);
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnTrue()
        {
            Assert.IsTrue(_target.IsSatisfiedBy((TimeSpan.FromHours(2).TotalSeconds)));
        }

        [Test]
        public void VerifyIsSatisfiedByCanReturnFalse()
        {
            Assert.IsFalse(_target.IsSatisfiedBy(TimeSpan.FromHours(24).TotalSeconds));
        }
    }
}
