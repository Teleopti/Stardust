using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PartTimePercentageStepDefinitions
	{
		[Given(@"there is a part time percentage with")]
		public void GivenThereIsAPartTimePercentageWith(Table table)
		{
			DataMaker.ApplyFromTable<PartTimePercentageConfigurable>(table);
		}

		[Given(@"there is a part time percentage named '(.*)'")]
		public void GivenThereIsAPartTimePercentageWith(string name)
		{
			DataMaker.Data().Apply(new PartTimePercentageConfigurable {Name = name});
		}

	}
}