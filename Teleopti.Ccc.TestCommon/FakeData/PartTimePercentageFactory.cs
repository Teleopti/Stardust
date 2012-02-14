using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for PartTimePercentage domain object
    /// </summary>
    public static class PartTimePercentageFactory
    {
        /// <summary>
        /// Creates a PartTimePercentage aggregate.
        /// </summary>
        /// <returns></returns>
        public static IPartTimePercentage CreatePartTimePercentage(string name)
        {
            return new PartTimePercentage(name);
        }
    }
}