using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;
using ShiftConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.ShiftConfigurable;

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
			DataMaker.ApplyFromTable<ShiftExchangeOfferConfigurable>(person, table);
		}
	}
}