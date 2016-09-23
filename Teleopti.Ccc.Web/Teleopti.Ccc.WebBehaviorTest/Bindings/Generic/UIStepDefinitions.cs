using System.Globalization;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{

	[Binding]
	public class UIStepDefinitions
	{
		
		// I click 'add full day absence'
		// *NOT* I click 'remove' on absence named 'Vacation'
		[When(@"I click '([a-z|\s]*)'")]
		[When(@"I initiate '([a-z|\s]*)'")]
		public void WhenIClickButtonWithClass(CssClass cssClass)
		{
			Browser.Interactions.ClickUsingJQuery(string.Format("button.{0}", cssClass.Name));
		}

		// I click agent 'mathias stenbom'
		// I click the agent 'mathias stenbom'
		// I click super agent 'james bond'
		// *NOT* I click the radiobutton with caption 'Probably not'
		// *NOT* I click the extended preference indication on '2012-06-20'
		// *NOT* I click on the day symbol area for date '2013-10-03'
		[When(@"I click( the)? ([a-z-]*|[a-z]* [a-z]*) '(.*)'")]
		public void WhenIClickClassWithText(string the, CssClass cssClass, string text)
		{
			Browser.Interactions.ClickContaining("." + cssClass.Name, text);
		}

		// I should see the message 'an error message'
		// I should see the error message 'an error message'
		// *NOT* I should see the preference Late on '2012-06-20'
		// *NOT* I should see the time indicator at time '2030-01-01 11:00'
		[Then(@"I should see the ([a-z]*|[a-z]* [a-z]*) '(.*)'")]
		public void ThenIShouldSeeTheMessage(CssClass cssClass, LocalizedText text)
		{
			Browser.Interactions.AssertAnyContains("." + cssClass.Name, text.Text);
		}

		[Then(@"I should see the '(.*)' '(.*)'")]
		public void ThenIShouldSeeTheMessageWithQuote(CssClass cssClass, LocalizedText text)
		{
			Browser.Interactions.AssertAnyContains("." + cssClass.Name, text.Text);
		}

		[Then(@"I should see status on '(.*)'")]
		public void ThenIShouldSeeStatusOn(CssClass cssClass)
		{
			Browser.Interactions.AssertExists("." + cssClass.Name);
		}

	}
}