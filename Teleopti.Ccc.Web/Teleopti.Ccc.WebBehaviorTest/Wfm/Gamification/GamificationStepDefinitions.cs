using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Gamification
{
	[Binding]
	public sealed class GamificationStepDefinitions
	{
		[Given(@"there is a gamification setting with")]
		public void GivenThereIsAGamificationSettingWith(Table table)
		{
			DataMaker.ApplyFromTable<GamificationConfigurable>(table);
		}

		[Then(@"I should see a gamification setting '(.*)' in the dropdown list")]
		public void ThenIShouldSeeAGamificationSettingInTheDropdownList(string name)
		{
			Browser.Interactions.AssertAnyContains("div.md-text span.ng-binding", name);
		}

		[Then(@"I should not see a gamification setting '(.*)' in the dropdown list")]
		public void ThenIShouldNotSeeAGamificationSettingInTheDropdownList(string name)
		{
			Browser.Interactions.AssertNoContains("div.md-text", "span.ng-binding", name);
		}

		[When(@"I select a gamification setting '(.*)' to remove from the dropdown list")]
		[Then(@"I select a gamification setting '(.*)' to remove from the dropdown list")]
		public void WhenISelectAGamificationSettingToRemoveFromTheDropdownList(string name)
		{
			Browser.Interactions.Click("md-select-value span.md-select-icon");
			Browser.Interactions.ClickContaining("div.md-text span.ng-binding", name);
		}

		[When(@"I click remove button")]
		[Then(@"I click remove button")]
		public void WhenIClickRemoveButton()
		{
			Browser.Interactions.Click("button.wfm-fab.material-depth-1.ng-scope");
		}

		[Then(@"I should see a popup confirm dialog before removing a gamification setting '(.*)'")]
		public void ThenIShouldSeeAPopupConfirmDialogBeforeRemovingAGamificationSetting(string name)
		{
			Browser.Interactions.AssertExists("div.md-dialog-container");
			Browser.Interactions.AssertExists("div.md-dialog-content-body.ng-scope p.ng-binding", name);
		}

		[When(@"I click cancel button of confirm dialog")]
		[Then(@"I click cancel button of confirm dialog")]
		public void WhenIClickCancelButtonOfConfirmDialog()
		{
			Browser.Interactions.Click("md-dialog-actions button.md-cancel-button");
		}

		[When(@"I click ok button of confirm dialog")]
		[Then(@"I click ok button of confirm dialog")]
		public void WhenIClickOkButtonOfConfirmDialog()
		{
			Browser.Interactions.Click("md-dialog-actions button.md-confirm-button");
		}
	}
}
