using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingOptionsCreatorTest
    {
        private SchedulingOptionsSyncronizer _target;
        private IOptimizationPreferences _optimizationPreferences;
        private ISchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _optimizationPreferences = new OptimizationPreferences();
            _schedulingOptions = new SchedulingOptions();
            _target = new SchedulingOptionsSyncronizer();
        }

        [Test]
        public void ShouldCreateNewSchedulingOption()
        {
            _target.SyncronizeSchedulingOption(_optimizationPreferences, _schedulingOptions);
        }
    }
}
