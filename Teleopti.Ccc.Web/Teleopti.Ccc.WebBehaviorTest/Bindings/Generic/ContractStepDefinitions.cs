using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

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
			DataMaker.Data().Apply(contractSchedule);
		}

		[Given(@"there is a contract schedule named '(.*)'")]
		public void GivenThereIsAContractScheduleNamed(string name)
		{
			DataMaker.Data().Apply(new ContractScheduleConfigurable {Name = name});
		}

		[Given(@"there is a contract with")]
		public void GivenThereIsAContractWith(ContractConfigurable contract)
		{
			DataMaker.Data().Apply(contract);
		}

		[Given(@"there is a contract named '(.*)'")]
		public void GivenThereIsAContractNamed(string name)
		{
			DataMaker.Data().Apply(new ContractConfigurable {Name = name});
		}

	}
}