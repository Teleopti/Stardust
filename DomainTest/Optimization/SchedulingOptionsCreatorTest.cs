using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingOptionsCreatorTest
    {
        private SchedulingOptionsSyncronizer _target;
        private IOptimizationPreferences _optimizationPreferences;

        [SetUp]
        public void Setup()
        {
            _optimizationPreferences = new OptimizationPreferences();
            _target = new SchedulingOptionsSyncronizer();
        }

        [Test]
        public void ShouldCreateNewSchedulingOption()
        {
            ISchedulingOptions result = _target.CreateSchedulingOption(_optimizationPreferences);
            Assert.IsNotNull(result);
        }
    }
}
