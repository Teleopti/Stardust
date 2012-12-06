using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class AdvanceSchedulingServiceTest
    {
        private MockRepository _mocks;
        private IAdvanceSchedulingService _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new AdvanceSchedulingService();
        }

        [Test]
        public void ShouldVerifyBlockIntradayAggregation()
        {
            Assert.That(_target.BlockIntradayAggregation(),Is.Not.Null  );
        }

        [Test]
        public void ShouldVerifyEffectiveRestrictionAggregation()
        {
            Assert.That(_target.EffectiveRestrictionAggregation(), Is.Not.Null);
        }

        [Test]
        public void ShouldVerifyRunScheduling()
        {
            Assert.That(_target.RunScheduling(), Is.Not.Null);
        }
    }

    
}
