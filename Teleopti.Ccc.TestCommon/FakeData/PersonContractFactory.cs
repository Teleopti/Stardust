using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: sumeda herath
    /// Created date: 2008-01-15
    /// </remarks>
    public static class PersonContractFactory
    {
        /// <summary>
        /// Creates the person contract.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-28
        /// </remarks>
        public static IPersonContract CreatePersonContract()
        {
            return CreatePersonContract("dummyContract", "dummyBasicSchedule", "dummyPartTime 75%");
        }

	    public static IPersonContract CreatePersonContract(IContract contract)
	    {
		    return CreatePersonContract(contract, "dummyBasicSchedule", "dummyPartTime 75%");
	    }


        /// <summary>
        /// Creates the person contract.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="contractScheduleName">Name of the contract schedule.</param>
        /// <param name="partTimePercentage">The part time percentage.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-31
        /// </remarks>
        public static IPersonContract CreatePersonContract(string contractName, string contractScheduleName, string partTimePercentage)
        {
	        return CreatePersonContract(new Contract(contractName), contractScheduleName, partTimePercentage);
        }

			public static IPersonContract CreatePersonContract(IContract contract, string contractScheduleName, string partTimePercentage)
			{
				IPartTimePercentage newPartTimePercentage = new PartTimePercentage(partTimePercentage);
				return CreatePersonContract(contract, contractScheduleName, newPartTimePercentage);
			}

			public static IPersonContract CreatePersonContract(IContract contract, string contractScheduleName, IPartTimePercentage partTimePercentage)
			{
				IContractSchedule contractSch = ContractScheduleFactory.CreateContractSchedule(contractScheduleName);

				return CreatePersonContract(contract, partTimePercentage, contractSch);
			}

        public static IPersonContract CreateFulltimePersonContractWithWorkingWeekContractSchedule()
        {
            IContract contract = ContractFactory.CreateContract("MyContract");
            IPartTimePercentage partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("Full time");
            IContractSchedule contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
            return CreatePersonContract(contract,partTimePercentage,contractSchedule);
        }

        public static  IPersonContract CreatePersonContract(IContract contract,IPartTimePercentage partTimePercentage,IContractSchedule contractSchedule)
        {
            return new PersonContract(contract, partTimePercentage, contractSchedule);
        }
    }
}
