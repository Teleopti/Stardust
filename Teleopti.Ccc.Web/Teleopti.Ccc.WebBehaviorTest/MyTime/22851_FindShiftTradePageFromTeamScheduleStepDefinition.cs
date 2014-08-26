using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Scope(Feature = "22851 - Find Shift Trade Page from Team Schedule")]
	[Binding]
	public class _22851_FindShiftTradePageFromTeamScheduleStepDefinition
	{
		[When(@"I initialize a shift trade")]
		public void WhenIInitializeAShiftTrade()
		{
			Browser.Interactions.Click(".hidden-sm .initialize-shift-trade");
		}

		[Then(@"I should not be able to initialize a shift trade")]
		public void ThenIShouldNotBeAbleToInitializeAShiftTrade()
		{
			Browser.Interactions.AssertNotExists(".navbar", ".initialize-shift-trade");
		}
	}
}
