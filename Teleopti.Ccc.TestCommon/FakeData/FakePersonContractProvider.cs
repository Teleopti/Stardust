using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakePersonContractProvider 
	{
		public PersonContract GetPersonContractWithSpecifiedNightRest(string name, TimeSpan nightRest)
		{

			var contract = new Contract(name)
			{
				WorkTimeDirective = new WorkTimeDirective(
					new TimeSpan(40, 0, 0),
					new TimeSpan(48, 0, 0),
					nightRest,
					new TimeSpan(60, 0, 0)
				)
			};

			return new PersonContract(contract, new PartTimePercentage("Fake"), new ContractSchedule("Fake"));
		}

	}
}
