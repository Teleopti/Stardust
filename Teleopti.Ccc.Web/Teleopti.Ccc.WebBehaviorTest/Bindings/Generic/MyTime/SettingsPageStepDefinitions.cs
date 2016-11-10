using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class SettingsPageStepDefinitions
	{

		[Then(@"I should see '(.*)' active")]
		public void ThenIShouldSeeActive(CssClass cssClass)
		{
			Browser.Interactions.AssertExists(string.Format(".{0}.active", cssClass.Name));
		}

		[Then(@"I should see '(.*)' inactive")]
		public void ThenIShouldSeeInactive(CssClass cssClass)
		{
			Browser.Interactions.AssertNotExists(string.Format(".{0}", cssClass.Name), ".share-my-calendar.active");
		}

		[Then(@"I should see a sharing link")]
		public void ThenIShouldSeeASharingLink()
		{
			Browser.Interactions.AssertExists(".calendar-url");
		}

		[Then(@"I should see a warning message")]
		public void ThenIShouldSeeAWarningMessage()
		{
			Browser.Interactions.AssertExists("#Setting-MissingWorkflowControlSet");
		}


		[Then(@"I should not see a sharing link")]
		public void ThenIShouldNotSeeASharingLink()
		{
			Browser.Interactions.AssertNotExists(".share-my-calendar", ".calendar-url");
		}

		[Then(@"I should not see '(.*)' in settings")]
		public void ThenIShouldNotSeeInSettings(CssClass cssClass)
		{
			Browser.Interactions.AssertNotExists("#settings", "." + cssClass.Name);
		}
	}
}