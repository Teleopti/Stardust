using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
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

		[When(@"I change the shift trade post value with")]
		public void WhenIChangeTheShiftTradePostValueWith(Table table)
		{
			var exchangeOffer = table.CreateInstance<ShiftExchangeOfferFields>();

			Browser.Interactions.Javascript(string.Format("$('.shift-exchange-offer-start-time').timepicker('setTime', '{0}');", exchangeOffer.StartTime));
			Browser.Interactions.Javascript(string.Format("$('.shift-exchange-offer-end-time').timepicker('setTime', '{0}');", exchangeOffer.EndTime));
		}

		[Then(@"I should see an announcement in request list")]
		public void ThenIShouldSeeAnAnnouncementInRequestList()
		{
			ScenarioContext.Current.Pending();
		}

	}
}