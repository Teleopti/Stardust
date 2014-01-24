using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class RealTimeAdherencePageStepDefinitions
	{
		[Then(@"I should see a message that I have no permission for this function")]
		public void ThenIShouldSeeAMessageThatIHaveNoPermissionForThisFunction()
		{
			Browser.Interactions.AssertExists(".alert-error");
		}

		[Then(@"I should see Real time adherence overview in the menu")]
		public void ThenIShouldSeeRealTimeAdherenceOverviewInTheMenu()
		{
			Browser.Interactions.AssertExists("#link-realtimeadherence");
		}
	}
}