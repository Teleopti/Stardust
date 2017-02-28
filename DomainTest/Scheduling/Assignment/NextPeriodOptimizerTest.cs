using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

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
