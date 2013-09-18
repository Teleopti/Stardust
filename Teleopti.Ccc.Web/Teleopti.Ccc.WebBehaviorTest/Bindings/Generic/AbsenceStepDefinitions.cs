using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AbsenceStepDefinitions
	{
		[Given(@"there is an absence with")]
		public void GivenThereIsAnActivity(Table table)
		{
			DataMaker.ApplyFromTable<AbsenceConfigurable>(table);
		}

		[Given(@"'?(I)'? have a absence with")]
		[Given(@"'?(.*)'? has an absence with")]
		public void GivenHaveAAbsenceWith(string userName, Table table)
		{
			DataMaker.ApplyFromTable<PersonAbsenceConfigurable>(userName, table);
		}

		[When(@"'(.*)' adds an absence for '(.*)' with")]
		public void WhenAddsAnAbsenceForWith(string adder, string person, Table scenarioData)
		{
			DataMaker.ApplyFromTable<PersonAbsenceConfigurable>(person, scenarioData);
		}

	}
}