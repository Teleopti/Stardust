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
		// I click 'add full day absence' -> css class
		[When(@"I click '(.*)'")]
		public void WhenIClass(string classText)
		{
			var className = classText.ToLower().Replace(" ", "-");
			Browser.Current.Element(Find.BySelector(string.Format(".{0}", className))).EventualClick();
		}

		// I click agent 'mathias stenbom' -> css class, text
		[When(@"I click (.*) '(.*)'")]
		public void WhenIClick(string className, string text)
		{
			Browser.Current.Element(Find.BySelector(string.Format(".{0}:contains('{1}')", className, text))).EventualClick();
		}

		// I should see message 'an error message' -> class, resource
		[Then(@"I should see the (.*) '(.*)'")]
		public void ThenIShouldSeeTheMessageInvalidEndDate(string className, string text)
		{
			var resourceKey = new CultureInfo("en-US").TextInfo.ToTitleCase(text).Replace(" ", "");
			var localizedText = Resources.ResourceManager.GetString(resourceKey) ?? text;
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector("." + className)).Text, Is.StringContaining(localizedText));
		}

	}
}