using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftStepDefinitions
	{
		[Given(@"(I) have a shift with")]
		[Given(@"'?(.*)'? has a shift with")]
		public void GivenIHaveAShiftWith(string person, Table table)
		{
			DataMaker.ApplyFromTable<ShiftConfigurable>(person, table);
		}

		[Given(@"'?(.*)'? has a shift exchange for bulletin")]
		public void GivenOtherAgentHasAShiftExchangeForBulletin(string person, Table table)
		{
			table.CreateInstance<ShiftExchangeOfferFields>();
		}
	}
}