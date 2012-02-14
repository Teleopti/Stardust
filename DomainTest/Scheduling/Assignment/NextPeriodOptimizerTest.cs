using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class NextPeriodOptimizerTest : FilterLayerBaseTest
    {
        protected override IFilterOnPeriodOptimizer CreateOptimizer()
        {
            return new NextPeriodOptimizer();
        }
    }
}
