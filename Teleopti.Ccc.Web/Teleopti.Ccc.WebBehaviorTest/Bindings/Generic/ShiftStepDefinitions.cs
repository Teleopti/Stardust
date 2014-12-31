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
		[Given(@"'?(.*)'? have a shift exchange for bulletin")]
		public void GivenAgentHasAShiftExchangeForBulletin(string person, Table table)
		{
			DataMaker.ApplyFromTable<ShiftExchangeOfferConfigurable>(person, table);
		}

		[Then(@"I should see an announcement in request list")]
		public void ThenIShouldSeeAnAnnouncementInRequestList()
		{
			ScenarioContext.Current.Pending();
		}

	}
}