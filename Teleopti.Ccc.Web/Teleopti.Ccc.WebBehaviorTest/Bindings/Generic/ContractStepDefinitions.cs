using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ContractStepDefinitions
	{
		[StepArgumentTransformation]
		public static ContractScheduleConfigurable ContractScheduleConfigurableTransform(Table table)
		{
			return table.CreateInstance<ContractScheduleConfigurable>();
		}

		[StepArgumentTransformation]
		public static ContractConfigurable ContractConfigurableTransform(Table table)
		{
			return table.CreateInstance<ContractConfigurable>();
		}

		[Given(@"there is a contract schedule with")]
		public void GivenThereIsAContractScheduleWith(ContractScheduleConfigurable contractSchedule)
		{
			DataMaker.Data().Setup(contractSchedule);
		}

		[Given(@"there is a contract with")]
		public void GivenThereIsAContractWith(ContractConfigurable contract)
		{
			DataMaker.Data().Setup(contract);
		}
	}
}