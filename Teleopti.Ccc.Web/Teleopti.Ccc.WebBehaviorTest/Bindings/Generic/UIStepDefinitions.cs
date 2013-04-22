using System.Globalization;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class UIStepDefinitions
	{
		[StepArgumentTransformation]
		public CssClass ToClassName(string textToBeClassName)
		{
			var className = textToBeClassName.ToLower().Replace(" ", "-");
			return new CssClass {Name = className};
		}

		public class CssClass
		{
			public string Name { get; set; }
		}

		[StepArgumentTransformation]
		public LocalizedText To(string textToBeResourceKey)
		{
			var resourceKey = new CultureInfo("en-US").TextInfo.ToTitleCase(textToBeResourceKey).Replace(" ", "");
			var localizedText = Resources.ResourceManager.GetString(resourceKey) ?? textToBeResourceKey;
			return new LocalizedText { Text = localizedText };
		}

		public class LocalizedText
		{
			public string Text { get; set; }
		}

		// I click 'add full day absence'
		// *NOT* I click 'remove' on absence named 'Vacation'
		[When(@"I click '([a-z|\s]*)'")]
		public void WhenIClickClass(CssClass cssClass)
		{
			Browser.Current.Element(Find.BySelector(string.Format(".{0}", cssClass.Name))).EventualClick();
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
			Browser.Current.Element(Find.BySelector(string.Format(".{0}:contains('{1}')", cssClass.Name, text))).EventualClick();
		}

		// I should see the message 'an error message'
		// I should see the error message 'an error message'
		// *NOT* I should see the preference Late on '2012-06-20'
		// *NOT* I should see the time indicator at time '2030-01-01 11:00'
		[Then(@"I should see the ([a-z]*|[a-z]* [a-z]*) '(.*)'")]
		public void ThenIShouldSeeTheMessage(CssClass cssClass, LocalizedText text)
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector("." + cssClass.Name)).Text, Is.StringContaining(text.Text));
		}

	}
}