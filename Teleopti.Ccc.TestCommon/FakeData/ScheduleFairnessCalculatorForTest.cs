using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public class ScheduleFairnessCalculatorForTest : IScheduleFairnessCalculator
    {
        public double PersonFairness(IPerson person)
        {
            return 1;
        }

        public double ScheduleFairness()
        {
            return 5;
        }
    }
}
