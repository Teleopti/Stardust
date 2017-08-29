using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
        	var contract = new Contract(name);
			contract.SetId(Guid.Empty);
        	return contract;
        }
    }
}