using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for Contract domain object
    /// </summary>
    public static class ContractFactory
    {
        /// <summary>
        /// Creates a contract aggregate.
        /// </summary>
        /// <returns></returns>
        public static IContract CreateContract(string name)
        {
            return new Contract(name);
        }
    }
}