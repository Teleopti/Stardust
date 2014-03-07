using System.Globalization;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{

	public class CssClass
	{
		public string Name { get; set; }
	}

	public class LocalizedText
	{
		public string Text { get; set; }
	}

	[Binding]
	public class UIStepDefinitions
	{
		[StepArgumentTransformation]
		public CssClass ToClassName(string textToBeClassName)
		{
			var className = textToBeClassName.ToLower().Replace(" ", "-");
			return new CssClass {Name = className};
		}

		[StepArgumentTransformation]
		public LocalizedText To(string textToBeResourceKey)
		{
			var resourceKey = new CultureInfo("en-US").TextInfo.ToTitleCase(textToBeResourceKey).Replace(" ", "");
			var localizedText = Resources.ResourceManager.GetString(resourceKey, DataMaker.Data().MyCulture) ?? textToBeResourceKey;
			return new LocalizedText { Text = localizedText };
		}

		// I click 'add full day absence'
		// *NOT* I click 'remove' on absence named 'Vacation'
		[When(@"I click '([a-z|\s]*)'")]
		[When(@"I initiate '([a-z|\s]*)'")]
		public void WhenIClickButtonWithClass(CssClass cssClass)
		{
			// enforcing button because of :enabled selector.
			// if its clickable, it has to be enabled after initialization for robustness
			// probably have to reevaluate this decision later
			Browser.Interactions.Click(string.Format("button.{0}:enabled", cssClass.Name));
		}


		// I click agent 'mathias stenbom'
		// I click the agent 'mathias stenbom'
		// I click super agent 'james bond'
		// *NOT* I click the radiobutton with caption 'Probably not'
		// *NOT* I click the extended preference indication on '2012-06-20'
		// *NOT* I click on the day symbol area for date '2013-10-03'
		[When(@"I click( the)? ([a-z]*|[a-z]* [a-z]*) '(.*)'")]
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

	}
}